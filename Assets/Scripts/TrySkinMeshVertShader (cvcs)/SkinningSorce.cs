using UnityEngine;

[AddComponentMenu("Skinning/SkinRead")]
[RequireComponent(typeof(SkinnedMeshRenderer))]
public class SkinningSorce : MonoBehaviour
{
    [SerializeField] SkinningSorceModel m_model;

    #region value
    //���_�x�C�N���邽�߂̑�փV�F�[�_�[
    [SerializeField] Shader p_replacementShader;
    [SerializeField] Shader p_replacementShaderPosition;
    [SerializeField] Shader p_replacementShaderNormal;
    [SerializeField] Shader p_replacementShaderTangent;
    //�`�悵�Ȃ��p�}�e���A��
    [SerializeField] Material p_placeMaterial;
    //���_�o�b�t�@
    RenderTexture p_positionBuffer0;
    RenderTexture p_positionBuffer1;
    RenderTexture p_normalBuffer;
    RenderTexture p_tangentBuffer;
    int p_flameCount;
    //�}���`�����_�����O���̃^�[�Q�b�g
    RenderBuffer[] _mrt0;
    RenderBuffer[] _mrt1;
    bool swapFlag;
    //���_�x�C�N�̃J����
    Camera p_camera;
    #endregion

    #region Public value
    /// <summary>
    /// ���f���̒��_��
    /// </summary>
    public int vertexCount
    {
        get
        {
            return m_model != null ? m_model.vertexCount : 0;
        }
    }
    public bool isReady
    {
        get
        {
            return p_flameCount > 1;
        }
    }
    public RenderTexture positionBuffer
    {
        get
        {
            return swapFlag ? p_positionBuffer1 : p_positionBuffer0;
        }
    }
    public RenderTexture prePositionBuffer
    {
        get
        {
            return swapFlag ? p_positionBuffer0 : p_positionBuffer1;
        }
    }
    public RenderTexture normalBuffer
    {
        get
        {
            return p_normalBuffer;
        }
    }
    public RenderTexture tangentBuffer
    {
        get
        {
            return p_tangentBuffer;
        }
    }

    #endregion

    #region �֐�
    /// <summary>
    /// �o�b�t�@���쐬
    /// </summary>
    RenderTexture CreateBuffer()
    {
        RenderTextureFormat format = SkinningRendingSystem.supportedBufferFormat;
        RenderTexture renderTexture = new RenderTexture(m_model.vertexCount, 1, 0, format);
        renderTexture.filterMode = FilterMode.Point;
        return renderTexture;
    }
    /// <summary>
    /// skinmesh�̃I�[�o�[���C�h
    /// </summary>
    void OverRideRenderer()
    {
        SkinnedMeshRenderer skin = GetComponent<SkinnedMeshRenderer>();
        skin.sharedMesh = m_model.mesh;
        skin.material = p_placeMaterial;
        skin.receiveShadows = false;
        //���_�x�C�N�J�����̓K���I��
        skin.enabled = false;
    }
    /// <summary>
    /// �x�C�N�J�����̍쐬
    /// </summary>
    void CreateBakeCamera()
    {
        GameObject go = new GameObject("Camera");
        go.hideFlags = HideFlags.HideInHierarchy;
        Transform tf = go.transform;
        tf.parent = transform;
        tf.localPosition = Vector3.zero;
        tf.localRotation = Quaternion.identity;
        p_camera = go.AddComponent<Camera>();
        p_camera.renderingPath = RenderingPath.Forward;
        p_camera.clearFlags = CameraClearFlags.SolidColor;
        p_camera.depth = -10000;
        p_camera.nearClipPlane = -100;
        p_camera.farClipPlane = 100;
        p_camera.orthographic = true;
        p_camera.orthographicSize = 100;
        p_camera.enabled = false;
        CullingStateController cull = go.AddComponent<CullingStateController>();
        cull.target = GetComponent<SkinnedMeshRenderer>();
    }
    #endregion

    #region Start Update
    private void Start()
    {
        p_positionBuffer0 = CreateBuffer();
        p_positionBuffer1 = CreateBuffer();
        p_normalBuffer = CreateBuffer();
        p_tangentBuffer = CreateBuffer();
        _mrt0 = new[] { p_positionBuffer0.colorBuffer, p_normalBuffer.colorBuffer, p_tangentBuffer.colorBuffer };
        _mrt1 = new[] { p_positionBuffer1.colorBuffer, p_normalBuffer.colorBuffer, p_tangentBuffer.colorBuffer };
        OverRideRenderer();
        CreateBakeCamera();
        swapFlag = true;
    }
    private void OnDestroy()
    {
        if (p_positionBuffer0 != null)
        {
            Destroy(p_positionBuffer0);
        }
        if (p_positionBuffer1 != null)
        {
            Destroy(p_positionBuffer1);
        }
        if (p_normalBuffer != null)
        {
            Destroy(p_normalBuffer);
        }
        if (p_tangentBuffer != null)
        {
            Destroy(p_tangentBuffer);
        }
    }
    private void LateUpdate()
    {
        swapFlag = !swapFlag;
        //VR�ɂ͑Ή����Ă܂���
        if (swapFlag)
        {
            p_camera.targetTexture = p_positionBuffer1;
            p_camera.RenderWithShader(p_replacementShaderPosition, "Skinning");
            p_camera.targetTexture = p_positionBuffer1;
            p_camera.RenderWithShader(p_replacementShaderNormal, "Skinning");
            p_camera.targetTexture = p_positionBuffer1;
            p_camera.RenderWithShader(p_replacementShaderTangent, "Skinning");
        }
        else
        {
            p_camera.targetTexture = p_positionBuffer0;
            p_camera.RenderWithShader(p_replacementShaderPosition, "Skinning");
            p_camera.targetTexture = p_positionBuffer1;
            p_camera.RenderWithShader(p_replacementShaderNormal, "Skinning");
            p_camera.targetTexture = p_positionBuffer1;
            p_camera.RenderWithShader(p_replacementShaderTangent, "Skinning");
        }
        GetComponent<SkinnedMeshRenderer>().enabled = false;
        p_flameCount++;
    }
    #endregion
}

