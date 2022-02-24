using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class CreateMeshDataStart : MonoBehaviour
{
    [SerializeField, Range(1, 4)] protected int details = 1;
    [SerializeField] bool weld = false;
    [SerializeField] int count = default;
    [SerializeField] Mesh sphere = default;
    static Vector2[] uvs;
    private void Awake()
    {
        
    }
    void Start()
    {
        //SubdivisionSurface.count = count;
        Debug.Log("made mesh");
        var filter = GetComponent<MeshFilter>();
        var source = filter.mesh;
        var mesh = SubdivisionSurface.Subdivide(SubdivisionSurface.Weld(source, float.Epsilon, source.bounds.size.x), details, weld);
        int i = 0;
        uvs = new Vector2[mesh.vertices.Length];
        foreach (var item in mesh.vertices)
        {
            uvs[i] = new Vector2(item.x, item.y);
            i++;
        }
        
        mesh.uv = uvs;
        filter.sharedMesh = mesh;
        filter.mesh = mesh;
        AssetDatabase.CreateAsset(mesh, "Assets/prefabs/mekemesh" + count + ".mesh");
        Debug.Log("Complete");
    }
}
