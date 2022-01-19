using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CreateMeshDataStart : MonoBehaviour
{
    [SerializeField, Range(1, 4)] protected int details = 1;
    [SerializeField] bool weld = false;
    [SerializeField] int count = default;

    // Start is called before the first frame update
    void Start()
    {
        SubdivisionSurface.count = count;
        var filter = GetComponent<MeshFilter>();
        var source = filter.mesh;
        var mesh = SubdivisionSurface.Subdivide(SubdivisionSurface.Weld(source, float.Epsilon, source.bounds.size.x), details, weld);
        filter.mesh = mesh;
        AssetDatabase.CreateAsset(mesh, "Assets/prefabs/mekemesh" + count + ".mesh");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
