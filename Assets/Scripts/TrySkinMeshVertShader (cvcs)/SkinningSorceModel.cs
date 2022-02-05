using System.Linq;
using System.Collections.Generic;
using UnityEngine;

//データをより多く格納して置くためにスクリプタブルオブジェクトを継承（メモリ節約のため）
public class SkinningSorceModel : ScriptableObject
{
    [SerializeField] int m_VertexCount;
    [SerializeField] Mesh m_Mesh;
    #region 読み取り用のデータ
    public int vertexCount
    {
        get
        {
            return m_VertexCount;
        }
    }
    public Mesh mesh
    {
        get
        {
            return m_Mesh;
        }
    }
    #endregion
    #region 初期化などほかで使うための関数
    //初期化はエディタ依存のため
    #if UNITY_EDITOR
    public void Initialize(Mesh origin)
    {
        Vector3[] vertices = origin.vertices;
        Vector3[] normals = origin.normals;
        Vector4[] tangents = origin.tangents;
        BoneWeight[] boneWeights = origin.boneWeights;

        List<Vector3> putoutVertices = new List<Vector3>();
        List<Vector3> putOutNormals = new List<Vector3>();
        List<Vector4> putoutTangents = new List<Vector4>();
        List<BoneWeight> putoutBoneWeights = new List<BoneWeight>();

        for (int i = 0; i < vertices.Length; i++)
        {
            if (!putoutVertices.Any(_ => _ == vertices[i]))
            {
                putoutVertices.Add(vertices[i]);
                putOutNormals.Add(normals[i]);
                putoutTangents.Add(tangents[i]);
                putoutBoneWeights.Add(boneWeights[i]);
            }
        }
        //それぞれに別のUVつける。（ここで）
        List<Vector2> putoutUVs = Enumerable.Range(0, putoutVertices.Count).Select(i => Vector2.right * (i + 0.5f) / putoutVertices.Count).ToList();
        //インデックスを管理するために格納
        int[] indices = Enumerable.Range(0, putoutVertices.Count).ToArray();

        //キャッシュの問題を解決するためにクローンの生成
        m_Mesh = Instantiate<Mesh>(origin);
        m_Mesh.name = m_Mesh.name.Substring(0, m_Mesh.name.Length - 7);

        //使ってないものを消すが必要なので、作っておく。
        m_Mesh.colors = null;
        m_Mesh.uv2 = null;
        m_Mesh.uv3 = null;
        m_Mesh.uv4 = null;
        //頂点の上書き＋Meshに設定
        m_Mesh.subMeshCount = 0;
        m_Mesh.SetVertices(putoutVertices);
        m_Mesh.SetNormals(putOutNormals);
        m_Mesh.SetTangents(putoutTangents);
        m_Mesh.SetUVs(0, putoutUVs);
        m_Mesh.bindposes = origin.bindposes;
        m_Mesh.boneWeights = putoutBoneWeights.ToArray();
        m_Mesh.UploadMeshData(true);
        //頂点の格納
        m_VertexCount = putoutVertices.Count;
    }
    #endif
    #endregion
}
