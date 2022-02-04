using System;
using UnityEngine;

/// <summary>
/// system�v���̊m�FRenderTextureFormat�̊m�F
/// </summary>
public static class SkinningRendingSystem
{
    //�J�[�l���o�b�t�@�Ŏg�p�\�ȃt�H�[�}�b�g��Ԃ��B
    public static RenderTextureFormat supportedBufferFormat
    {
        get
        {
            #if UNITY_IOS || UNITY_TVOS || UNITY_ANDROID
            return RenderTextureFormat.ARGBHalf;
            #else
            return SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBFloat) ? RenderTextureFormat.ARGBFloat : RenderTextureFormat.ARGBHalf;
            #endif
        }
    }
}
/// <summary>
/// �J�[�l���@�o�b�t�@�@�Ǘ�
/// </summary>
internal class AnimKernelSet<KernelEnum, BufferEnum> where KernelEnum : struct where BufferEnum : struct
{
    //�񋓂𐮐��փR���o�[�g
    public delegate int KernelEnumToInt(KernelEnum e);
    public delegate int BufferEnumToInt(BufferEnum e);

    #region �ʕϐ�
    KernelEnumToInt p_GetKernelIndex;
    BufferEnumToInt p_GetBufferIndex;
    Shader p_shader;
    Material p_material;
    RenderTexture[] p_buffer;
    bool p_swapFlag;
    bool p_isReady;
    public Material material
    {
        get
        {
            return p_material;
        }
    }
    public bool isReady
    {
        get
        {
            return p_isReady;
        }
    }
    /// <summary>
    /// �ŏI�t���[���̃o�b�t�@��Ԃ��B
    /// </summary>
    public RenderTexture GetLastBuffer(BufferEnum buffer)
    {
        int index = p_GetBufferIndex(buffer);
        return p_buffer[p_swapFlag ? index : index + p_buffer.Length / 2];
    }
    /// <summary>
    /// ���̃o�b�t�@��Ԃ��B
    /// </summary>
    public RenderTexture GetBuffer(BufferEnum buffer)
    {
        int index = p_GetBufferIndex(buffer);
        return p_buffer[p_swapFlag ? index : index + p_buffer.Length / 2];
    }
    #endregion
    #region �֐�
    /// <summary>
    /// �V�F�[�_�[���󂯎���Ă���ɑ΂��ď���������
    /// </summary>
    public AnimKernelSet(Shader shader, KernelEnumToInt k2i, BufferEnumToInt b2i)
    {
        p_shader = shader;
        p_GetKernelIndex = k2i;
        p_GetBufferIndex = b2i;
        //�o�b�t�@�̔z��K��
        int eCount = Enum.GetValues(typeof(BufferEnum)).Length;
        p_buffer = new RenderTexture[eCount * 2];
    }
    /// <summary>
    /// �J�[�l���ƃo�b�t�@�̊m�F�����ď���������
    /// </summary>
    public void Setup(int w, int h)
    {
        if (p_isReady)
        {
            return;
        }
        p_material = new Material(p_shader);
        RenderTextureFormat format = SkinningRendingSystem.supportedBufferFormat;
        for (int i = 0; i < p_buffer.Length; i++)
        {
            RenderTexture renderTexture = new RenderTexture(w, h, 0, format);
            renderTexture.filterMode = FilterMode.Point;
            renderTexture.wrapMode = TextureWrapMode.Clamp;
            p_buffer[i] = renderTexture;
        }
        p_swapFlag = false;
        p_isReady = true;
    }
    /// <summary>
    /// �J�[�l���ƃo�b�t�@�̃����[�X
    /// </summary>
    public void Release()
    {
        if (!p_isReady)
        {
            return;
        }
        UnityEngine.Object.Destroy(p_material);
        p_material = null;
        for (int i = 0; i < p_buffer.Length; i++)
        {
            UnityEngine.Object.Destroy(p_buffer[i]);
            p_buffer[i] = null;
        }
        p_isReady = false;
    }
    /// <summary>
    /// �J�[�l�����N�����ĕ\��
    /// </summary>
    public void Invoke(KernelEnum kernel, BufferEnum buffer)
    {
        Graphics.Blit(null, GetBuffer(buffer), p_material, p_GetKernelIndex(kernel));
    }
    /// <summary>
    /// �o�b�t�@�̓���ւ�
    /// </summary>
    public void SwapBuffers()
    {
        p_swapFlag = !p_swapFlag;
    }
    #endregion
}

/// <summary>
/// meshrenderer2meshFilter
/// </summary>
internal class M2M
{
    #region variables
    GameObject go;
    Material defaultMt;
    MaterialPropertyBlock propertyBlock;
    public MaterialPropertyBlock materialProperty
    {
        get
        {
            return propertyBlock;
        }
    }
    #endregion
    /// <summary>
    /// �^����ꂽ�l�œ����ϐ���������
    /// </summary>
    public M2M(GameObject gameObject, Material mt)
    {
        go = gameObject;
        defaultMt = mt;
        propertyBlock = new MaterialPropertyBlock();
    }
    /// <summary>
    /// Update MeshRenderer & MeshFilter
    /// </summary>
    public void Update(Mesh tempMesh)
    {
        MeshFilter meshFilter = go.GetComponent<MeshFilter>();
        //meshFilter = null�@�Ȃ�Βǉ�
        if (meshFilter == null)
        {
            meshFilter = go.AddComponent<MeshFilter>();
            meshFilter.hideFlags = HideFlags.NotEditable;
        }
        if (meshFilter.sharedMesh != tempMesh)
        {
            meshFilter.sharedMesh = tempMesh;
        }
        MeshRenderer meshRenderer = go.GetComponent<MeshRenderer>();
        //�}�e���A���̊m�F
        if (meshRenderer.sharedMaterial == null)
        {
            meshRenderer.sharedMaterial = defaultMt;
        }
        meshRenderer.SetPropertyBlock(propertyBlock);
    }
}
