using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class LitFarTessellation : MonoBehaviour
{
    private const float TOLERANCE = 1E-2f;
    private Mesh mesh;
    private int[] meshTriangles;
    private Vector3[] meshVertices;
    private Vector2[] meshUV;
    [SerializeField]
    private RenderTexture renderTexture;
    [SerializeField]
    private Vector3 paintSize;

    private void Start()
    {
        MeshFilter meshfil = GetComponent<MeshFilter>();

        var mesh1 = meshfil.mesh;
        meshTriangles = mesh1.triangles;
        meshVertices = mesh1.vertices;
        meshUV = mesh1.uv;
        mesh = new Mesh();
        PolyMesh(0.1f, 10);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="radius">ラジアン</param>
    /// <param name="b"></param>
    void PolyMesh(float radius, int b)
    {
        //verticies
        List<Vector3> verticiesList = new List<Vector3> { };
        float x;
        float y;
        for (int i = 0; i < b; i++)
        {
            x = radius * Mathf.Sin((2 * Mathf.PI * i) / b);
            y = radius * Mathf.Cos((2 * Mathf.PI * i) / b);
            verticiesList.Add(new Vector3(x, y, 0f));
        }
        Vector3[] verticies = verticiesList.ToArray();

        //triangles
        List<int> trianglesList = new List<int> { };
        for (int i = 0; i < (b - 2); i++)
        {
            trianglesList.Add(0);
            trianglesList.Add(i + 1);
            trianglesList.Add(i + 2);
        }
        int[] triangles = trianglesList.ToArray();

        //normals
        List<Vector3> normalsList = new List<Vector3> { };
        for (int i = 0; i < verticies.Length; i++)
        {
            normalsList.Add(-Vector3.forward);
        }
        Vector3[] normals = normalsList.ToArray();

        //initialise
        mesh.vertices = verticies;
        mesh.triangles = triangles;
        mesh.normals = normals;
    }

    public void Paint(Vector3 worldPos, Camera renderCamera = null)
    {
        var material = new Material(Shader.Find("Unlit/Color"));
        material.SetColor("_Color", Color.red);
        Vector2 uv;
        if (renderCamera == null)
            renderCamera = Camera.main;

        Vector3 p = transform.InverseTransformPoint(worldPos);
        Matrix4x4 mvp = renderCamera.projectionMatrix * renderCamera.worldToCameraMatrix * transform.localToWorldMatrix;
        if (LocalPointToUV(p, mvp, out uv))
        {
            var cmd = new CommandBuffer(); // コマンドバッファを作る

            cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity); // 2Dとして描画する
            cmd.SetRenderTarget(renderTexture); // RenderTargetを設定
            uv = new Vector2(uv.x * -2 + 1.0f, uv.y * 2 - 1.0f);
            cmd.DrawMesh(mesh, Matrix4x4.TRS((uv), Quaternion.identity, paintSize), material, 0, 0, default); // 位置、回転、大きさを指定してMeshを描画
            Graphics.ExecuteCommandBuffer(cmd); // コマンドバッファを実行
        }
    }
    public bool LocalPointToUV(Vector3 localPoint, Matrix4x4 matrixMVP, out Vector2 uv)
    {
        int index0;
        int index1;
        int index2;
        Vector3 t1;
        Vector3 t2;
        Vector3 t3;
        Vector3 p = localPoint;

        for (var i = 0; i < meshTriangles.Length; i += 3)
        {
            index0 = i + 0;
            index1 = i + 1;
            index2 = i + 2;

            t1 = meshVertices[meshTriangles[index0]];
            t2 = meshVertices[meshTriangles[index1]];
            t3 = meshVertices[meshTriangles[index2]];

            if (!ExistPointInPlane(p, t1, t2, t3))
                continue;
            if (!ExistPointOnTriangleEdge(p, t1, t2, t3) && !ExistPointInTriangle(p, t1, t2, t3))
                continue;

            var uv1 = meshUV[meshTriangles[index0]];
            var uv2 = meshUV[meshTriangles[index1]];
            var uv3 = meshUV[meshTriangles[index2]];
            uv = TextureCoordinateCalculation(p, t1, uv1, t2, uv2, t3, uv3, matrixMVP);

            return true;
        }
        uv = default(Vector3);
        return false;
    }
    public static bool ExistPointInTriangle(Vector3 p, Vector3 t1, Vector3 t2, Vector3 t3)
    {
        var a = Vector3.Cross(t1 - t3, p - t1).normalized;
        var b = Vector3.Cross(t2 - t1, p - t2).normalized;
        var c = Vector3.Cross(t3 - t2, p - t3).normalized;

        var d_ab = Vector3.Dot(a, b);
        var d_bc = Vector3.Dot(b, c);

        if (1 - TOLERANCE < d_ab && 1 - TOLERANCE < d_bc)
            return true;
        return false;
    }
    public static bool ExistPointOnTriangleEdge(Vector3 p, Vector3 t1, Vector3 t2, Vector3 t3)
    {
        if (ExistPointOnEdge(p, t1, t2) || ExistPointOnEdge(p, t2, t3) || ExistPointOnEdge(p, t3, t1))
            return true;
        return false;
    }
    public static bool ExistPointOnEdge(Vector3 p, Vector3 v1, Vector3 v2)
    {
        return 1 - TOLERANCE < Vector3.Dot((v2 - p).normalized, (v2 - v1).normalized);
    }
    public static bool ExistPointInPlane(Vector3 p, Vector3 t1, Vector3 t2, Vector3 t3)
    {
        var v1 = t2 - t1;
        var v2 = t3 - t1;
        var vp = p - t1;

        var nv = Vector3.Cross(v1, v2);
        var val = Vector3.Dot(nv.normalized, vp.normalized);
        if (-TOLERANCE < val && val < TOLERANCE)
            return true;
        return false;
    }
    public static Vector2 TextureCoordinateCalculation(Vector3 p, Vector3 t1, Vector2 t1UV, Vector3 t2, Vector2 t2UV, Vector3 t3, Vector2 t3UV, Matrix4x4 transformMatrix)
    {
        Vector4 p1_p = transformMatrix * new Vector4(t1.x, t1.y, t1.z, 1);
        Vector4 p2_p = transformMatrix * new Vector4(t2.x, t2.y, t2.z, 1);
        Vector4 p3_p = transformMatrix * new Vector4(t3.x, t3.y, t3.z, 1);
        Vector4 p_p = transformMatrix * new Vector4(p.x, p.y, p.z, 1);
        Vector2 p1_n = new Vector2(p1_p.x, p1_p.y) / p1_p.w;
        Vector2 p2_n = new Vector2(p2_p.x, p2_p.y) / p2_p.w;
        Vector2 p3_n = new Vector2(p3_p.x, p3_p.y) / p3_p.w;
        Vector2 p_n = new Vector2(p_p.x, p_p.y) / p_p.w;
        var s = 0.5f * ((p2_n.x - p1_n.x) * (p3_n.y - p1_n.y) - (p2_n.y - p1_n.y) * (p3_n.x - p1_n.x));
        var s1 = 0.5f * ((p3_n.x - p_n.x) * (p1_n.y - p_n.y) - (p3_n.y - p_n.y) * (p1_n.x - p_n.x));
        var s2 = 0.5f * ((p1_n.x - p_n.x) * (p2_n.y - p_n.y) - (p1_n.y - p_n.y) * (p2_n.x - p_n.x));
        var u = s1 / s;
        var v = s2 / s;
        var w = 1 / ((1 - u - v) * 1 / p1_p.w + u * 1 / p2_p.w + v * 1 / p3_p.w);
        return w * ((1 - u - v) * t1UV / p1_p.w + u * t2UV / p2_p.w + v * t3UV / p3_p.w);
    }
}
