using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SkinnedMeshRenderer))]
public class AnimationParticleTarget : MonoBehaviour, GPUAnimationParticleTarget
{
    [SerializeField] private GameObject _target;
    [SerializeField] private float _minScale = 0.5f;
    [SerializeField] private float _maxScale = 1.0f;
    [SerializeField] protected Texture2D _texture = null;

    

    
    public MaterialPropertyBlock _props = new MaterialPropertyBlock();
    protected GameObject Target => _target == null ? gameObject : _target;
    private Mesh _mesh = null;
    private SkinnedMeshRenderer _skin = null;
    public SkinnedMeshRenderer Skin => _skin ?? (_skin = GetSkin());
    public Mesh Mesh => _mesh ?? (_mesh = GetMesh());
    private Renderer _renderer = null;
    private Renderer Renderer => _renderer ?? (_renderer = Target.GetComponent<Renderer>());
    public virtual int VertexCount => Mesh.vertexCount;
    public virtual Vector3[] Vertices => Mesh.vertices;
    public virtual Vector2[] UV => Mesh.uv;
    public virtual Texture2D Texture => _texture != null ? _texture : Renderer.material.mainTexture as Texture2D;
    public Matrix4x4 WorldMatrix => Target.transform.localToWorldMatrix;
    public float MinScale => _minScale;
    public float MaxScale => _maxScale;
    private uint[] _indices = null;
    public uint[] SubGroupIndices => _indices;
    public virtual void Initialize() 
    {
        RecreateMesh();
    }
    public void SetStartIndex(int startIdx)
    {
        _indices = new uint[VertexCount];

        for (int i = 0; i < _indices.Length; i++)
        {
            _indices[i] = (uint)(i + startIdx);
        }
    }
    private Mesh GetMesh()
    {
        MeshFilter filter = Target.GetComponent<MeshFilter>();
        if (filter != null)
        {
            return filter.mesh;
        }
        SkinnedMeshRenderer skin = Target.GetComponent<SkinnedMeshRenderer>();
        if (skin != null)
        {
            return skin.sharedMesh;
        }
        return null;
    }
    private SkinnedMeshRenderer GetSkin()
    {
        SkinnedMeshRenderer skin = Target.GetComponent<SkinnedMeshRenderer>();
        if (skin != null)
        {
            return skin;
        }
        return null;
    }

    void RecreateMesh()
    {
        Mesh origMesh = Skin.sharedMesh;

        var vertices = new List<Vector3>(origMesh.vertices);
        var normals = new List<Vector3>(origMesh.normals);
        var tangents = new List<Vector4>(origMesh.tangents);
        var boneWeights = new List<BoneWeight>(origMesh.boneWeights);
        int[] indices = new int[origMesh.vertexCount];

        List<Vector2> uv = new List<Vector2>();
        for (int i = 0; i < origMesh.vertexCount; i++)
        {
            uv.Add(new Vector2(((float)i + 0.5f) / (float)origMesh.vertexCount, 0));
            indices[i] = i;
        }

        Mesh.subMeshCount = 1;
        Mesh.SetVertices(vertices);
        Mesh.SetNormals(normals);
        Mesh.SetTangents(tangents);
        Mesh.SetIndices(indices, MeshTopology.Points, 0);
        Mesh.SetUVs(0, uv);
        Mesh.bindposes = origMesh.bindposes;
        Mesh.boneWeights = boneWeights.ToArray();

        Mesh.UploadMeshData(true);

        Skin.sharedMesh = Mesh;
    }
}
