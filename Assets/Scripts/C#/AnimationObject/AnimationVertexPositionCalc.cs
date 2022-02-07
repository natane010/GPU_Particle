using System;
using System.Collections.Generic;
using UnityEngine;


public class AnimationVertexPositionCalc : MonoBehaviour
{
    [SerializeField] Shader positionExtractionShader;
    [SerializeField] Material placeHolder;
    //置換するシェーダー
    [SerializeField] Shader m_replacePosition;
    [SerializeField] Shader m_replaceNormal;
    [SerializeField] Shader m_replaceTan;
    //描画準備用のマテリアル交換タグを所有
    [SerializeField] Material m_placeHolderMaterial;

    private Camera camera;

    int m_flameCount;

    //skinnerからとってきた機能モデルが使えるように処理してくれるクラス
    [SerializeField] SkinningSorceModel m_processedModel;

    RenderTexture tanBuffer;
    RenderTexture currentPositionBuffer0;
    RenderTexture currentPositionBuffer1;
    RenderTexture currentNormalBuffer;
    bool m_skinFlag;

    RenderBuffer[] multiRenderertargets0;
    RenderBuffer[] multiRenderertargets1;

    [SerializeField] SkinnedMeshRenderer m_targetmesh = default;

    public RenderTexture CurrentPositionBuffer
    {
        get { return m_skinFlag ? currentPositionBuffer1 : currentPositionBuffer0; }
    }

    public RenderTexture PrevPositionBuffer
    {
        get { return m_skinFlag ? currentPositionBuffer0 : currentPositionBuffer1; }
    }

    public RenderTexture CurrNormalBuffer
    {
        get { return currentNormalBuffer; }
    }
    public RenderTexture CurrentTanBuffer
    {
        get { return tanBuffer; }
    }
    
    RenderTexture CreateBuffer()
    {
        //デバイスの確認　skinnerからとってきた。
        //RenderTextureFormat format = SkinnerInternals.supportedBufferFormat;
        //RenderTexture renderTexture = new RenderTexture(m_processedModel.vertexCount, 1, 0, format);
        //renderTexture.filterMode = FilterMode.Point;
        //return renderTexture;
        return null;
    }

    //-------------------------------------------------------------
    void ReCalcAndSetMesh()
    {
        Mesh mesh = new Mesh();
        Mesh orginMesh = m_targetmesh.sharedMesh;
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector4> tans = new List<Vector4>();
        List<BoneWeight> bones = new List<BoneWeight>(orginMesh.boneWeights);
        int[] indices = new int[orginMesh.vertexCount];
        List<Vector2> uvs = new List<Vector2>();

        for (int i = 0; i < orginMesh.vertexCount; i++)
        {
            uvs.Add(new Vector2(((float)i * 0.5f) / (float)orginMesh.vertexCount, 0));
            indices[i] = i;
        }
        mesh.subMeshCount = 1;
        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetTangents(tans);
        mesh.SetIndices(indices, MeshTopology.Points, 0);
        mesh.SetUVs(0, uvs);
        mesh.bindposes = orginMesh.bindposes;
        mesh.boneWeights = bones.ToArray();
        mesh.UploadMeshData(true);
        m_targetmesh.sharedMesh = mesh;
    }
    //-------------------------------------------------------------
    private void Start()
    {
        
        m_skinFlag = true;
    }
}

