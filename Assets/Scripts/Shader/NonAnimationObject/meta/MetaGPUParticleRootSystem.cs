using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct TransformMetaBallParticle
{
    public int isActive;
    public int targetId;
    public Vector2 uv;

    public Vector3 targetPosition;

    public float speed;
    public Vector3 position;

    public int useTexture;
    public float scale;

    public Vector4 velocity;

    public Vector3 horizontal;
}
public struct InitializeMetaData
{
    public int isActive;
    public Vector3 targetPosition;

    public int targetId;
    public float scale;

    public Vector4 velocity;

    public Vector2 uv;
    public Vector3 horizontal;
}
[ExecuteAlways]
public class MetaGPUParticleRootSystem : MonoBehaviour
{
#nullable enable
    private class PropertyMetaDef
    {
        public int ParticleBufferID;
        public int InitDataListID;
        public int DeltaTimeID;
        public int TimeID;
        public int MatrixDataID;
        public int TexturesID;
        public int BaseScaleID;
        public int OffsetID;
        public int IndexBufferID;
        public int OriginID;
        public int GravityID;
        public int CountPID;

        public PropertyMetaDef()
        {
            ParticleBufferID = Shader.PropertyToID("_Particles");
            InitDataListID = Shader.PropertyToID("_InitDataList");
            DeltaTimeID = Shader.PropertyToID("_DeltaTime");
            TimeID = Shader.PropertyToID("_Time");
            MatrixDataID = Shader.PropertyToID("_MatrixData");
            TexturesID = Shader.PropertyToID("_Textures");
            BaseScaleID = Shader.PropertyToID("_BaseScale");
            OffsetID = Shader.PropertyToID("_Offset");
            IndexBufferID = Shader.PropertyToID("_IndexBuffer");
            OriginID = Shader.PropertyToID("_Origin");
            GravityID = Shader.PropertyToID("_Gravity");
            CountPID = Shader.PropertyToID("_CountParticle");
        }
    }
    [SerializeField] private int _metaCount = 10000;
    [SerializeField] private int _initMetaDataCount = 500000;
    [SerializeField] private ComputeShader _computeMetaShader = null;
    [SerializeField] private Mesh _metapParticleMesh = null;
    [SerializeField] private Material _particleMat = null;
    [SerializeField] private float _baseScale = 0.01f;
    [SerializeField] private float _gravity = -0.1f;
    [SerializeField] private Vector3 _offset = Vector3.zero;
    private readonly int THREAD_NUM = 64;

    public int MetaParticleCount => _metaCount;

    private ComputeBuffer _metaParticleBuffer = null;
    private ComputeBuffer _metaInitDataListBuffer = null;
    private ComputeBuffer _metaMatrixBuffer = null;
    private ComputeBuffer _metaArgsBuffer = null; //引数バッファ
    private ComputeBuffer _metaIndexBuffer = null;

    private uint[] _argsData = new uint[] { 0, 0, 0, 0, 0 };
    private uint[] _indices = null;

}
#nullable disable