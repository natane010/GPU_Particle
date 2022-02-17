using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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
    #region consolidate
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
    #endregion
    #region Variable
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

    private uint[] _metaArgsData = new uint[] { 0, 0, 0, 0, 0 };
    private uint[] _indices = null;
    private uint[] _metaDefaultIndices = null;
    private Matrix4x4[] _metaMatrixData = new Matrix4x4[30];
    private TransformMetaBallParticle[] _metaParticleData = null;
    private InitializeMetaData[] _initializeMetaData = null;

    private PropertyMetaDef _propertyMetaDef = null;
    #endregion
    #region kernels
    private int? _maxCount = null;

    private int? _kernelSetupParticlesImmediately = null;
    private int? _kernelSetupParticles = null;
    private int? _kernelDisable = null;

    private int? _kernelUpdateAsTarget = null;
    private int? _kernelUpdateAsExplosion = null;
    private int? _kernelUpdateAsGravity = null;
    private int? _kernelUpdateKeepAsPosition = null;
    private int? _kernelUpdateAsPosition = null;

    private int? _currentUpdateKernel = null;
    private bool _isRunning = false;
    #endregion
    #region initialize
    void Initialize()
    {
        _propertyMetaDef = new PropertyMetaDef();
        //get kernel id
        _kernelSetupParticles = _computeMetaShader.FindKernel("SetupParticle");
        _kernelSetupParticlesImmediately = _computeMetaShader.FindKernel("SetupParticlesImmediately");
        _kernelDisable = _computeMetaShader.FindKernel("Disable");

        _kernelUpdateAsTarget = _computeMetaShader.FindKernel("UpdateAsTarget");
        _kernelUpdateAsExplosion = _computeMetaShader.FindKernel("UpdateAsExplosion");
        _kernelUpdateAsGravity = _computeMetaShader.FindKernel("UpdateAsGravity");
        _kernelUpdateKeepAsPosition = _computeMetaShader.FindKernel("UpdateKeepAsPosition");
        _kernelUpdateAsPosition = _computeMetaShader.FindKernel("UpdateAsTargetPosition");

        _currentUpdateKernel = _kernelUpdateAsTarget;

        
    }
    void CriateBuffers()
    {
        _maxCount = (_metaCount / THREAD_NUM) * THREAD_NUM;
        Debug.Log($"creating {_maxCount} particles");
        
        _metaParticleData =  new TransformMetaBallParticle[(int)_maxCount];
        for (int i = 0; i < _metaParticleData.Length; i++)
        {
            _metaParticleData[i] = new TransformMetaBallParticle
            {
                speed = Random.Range(1f, 10f),
                position = Vector3.zero,
                targetPosition = Vector3.zero,
                scale = 1f,
            };
        }
        Debug.Log($"complete set {_metaParticleData.Length} particle datas");
        _initializeMetaData = new InitializeMetaData[_initMetaDataCount];
        for (int i = 0; i < _initializeMetaData.Length; i++)
        {
            _initializeMetaData[i].targetPosition = Vector3.zero;
        }
        Debug.Log($"complete set {_initializeMetaData.Length} particle initializedatas");
        _indices = new uint[(int)_maxCount];

        _metaDefaultIndices = new uint[(int)_maxCount];
        for (int i = 0; i < _metaDefaultIndices.Length; i++)
        {
            _metaDefaultIndices[i] = (uint)i;
        }
        for (int i = 0; i < _metaMatrixData.Length; i++)
        {
            _metaMatrixData[i] = Matrix4x4.identity;
        }
        Debug.Log($"complete set {_metaMatrixData.Length} Martrix datas");
        _metaArgsBuffer = new ComputeBuffer(1, Marshal.SizeOf(typeof(uint)) * _metaArgsData.Length, ComputeBufferType.IndirectArguments);
        _metaParticleBuffer = new ComputeBuffer((int)_maxCount, Marshal.SizeOf(typeof(TransformMetaBallParticle)));
        _metaParticleBuffer.SetData(_metaParticleData);
        _metaInitDataListBuffer = new ComputeBuffer(_initializeMetaData.Length, Marshal.SizeOf(typeof(InitializeMetaData)));
        _metaMatrixBuffer = new ComputeBuffer(_metaMatrixData.Length, Marshal.SizeOf(typeof(Matrix4x4)));
        _metaIndexBuffer = new ComputeBuffer(_indices.Length, Marshal.SizeOf(typeof(uint)));

    }
    #endregion
}
#nullable disable