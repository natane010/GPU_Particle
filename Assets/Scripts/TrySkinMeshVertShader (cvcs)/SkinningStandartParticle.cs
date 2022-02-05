using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SkinningStandartParticle : ScriptableObject
{
    #region StandartParticleValue
    [SerializeField] Mesh[] m_shapes = new Mesh[1];
    [SerializeField] int m_maxInstanceCount = 8192;
    [SerializeField] int m_InstanceCount;
    [SerializeField] Mesh m_mesh;
    [SerializeField] Mesh p_defaultShape;

    public Mesh[] shapes
    {
        get
        {
            return m_shapes;
        }
    }
    public int maxInstanceCount
    {
        get
        {
            return m_maxInstanceCount;
        }
    }
    public int instanceCount
    {
        get
        {
            return m_InstanceCount;
        }
    }
    public Mesh mesh
    {
        get
        {
            return m_mesh;
        }
    }
    Mesh GetMesh(int index)
    {
        if (m_shapes == null || m_shapes.Length == 0)
        {
            return p_defaultShape;
        }
        Mesh mesh = m_shapes[index % m_shapes.Length];
        return mesh == null ? p_defaultShape : mesh;
    }
    #endregion

    #region UnityEditor
    #if UNITY_EDITOR
    public void RebuildMesh()
    {
        List<Vector3> vertout = new List<Vector3>();
        List<Vector3> normalout = new List<Vector3>();
        List<Vector4> tanout = new List<Vector4>();
        List<Vector2> uv0out = new List<Vector2>();
        List<Vector2> uv1out = new List<Vector2>();
        List<int> indexout = new List<int>();
        int vertexcount = 0;
        m_InstanceCount = 0;
        while (m_InstanceCount < m_maxInstanceCount)
        {
            Mesh mesh = GetMesh(m_InstanceCount);
            Vector3[] vec3s = mesh.vertices;
            if (vertexcount + vec3s.Length > 65535)
            {
                break;
            }
            vertout.AddRange(vec3s);
            normalout.AddRange(mesh.normals);
            tanout.AddRange(mesh.tangents);
            uv0out.AddRange(mesh.uv);
            Vector2 uv1 = new Vector2(m_InstanceCount + 0.5f, 0);
            uv1out.AddRange(Enumerable.Repeat(uv1, vec3s.Length));
            indexout.AddRange(mesh.triangles.Select(i => i + vertexcount));
            vertexcount += vec3s.Length;
            m_InstanceCount++;
        }
        uv1out = uv1out.Select(x => x / instanceCount).ToList();
        m_mesh.Clear();
        m_mesh.SetVertices(vertout);
        m_mesh.SetNormals(normalout);
        m_mesh.SetUVs(0, uv0out);
        m_mesh.SetUVs(1, uv1out);
        m_mesh.SetIndices(indexout.ToArray(), MeshTopology.Triangles, 0);
        m_mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 1000);
        m_mesh.UploadMeshData(true);
    }
#endif
    #endregion

    #region ä÷êî
    private void OnValidate()
    {
        m_maxInstanceCount = Mathf.Clamp(maxInstanceCount, 4, 8129);
    }
    private void OnEnable()
    {
        if (m_mesh == null)
        {
            m_mesh = new Mesh();
            m_mesh.name = "SkinningStandartParticle";
        }
    }
    #endregion
}
