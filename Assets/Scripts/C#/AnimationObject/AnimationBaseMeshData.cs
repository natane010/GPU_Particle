using System.Collections.Generic;
using UnityEngine;

public class AnimationBaseMeshData : ScriptableObject
{
    const double PI = 3.1415926535;

    [SerializeField] public int m_group = 4;
    [SerializeField] public int m_segment = 32;
    [SerializeField] public Mesh m_targetMesh;

    List<Vector3> m_vertices = new List<Vector3>();
    List<int> m_indices = new List<int>();
    public void RecalcMesh()
    {
       
        for (int i = 0; i < m_segment; i++)
        {
            for (int t = 0; t < m_group; t++)
            {
                float phase = (float)PI * 2.0f * t / m_group;
                m_vertices.Add(new Vector3(phase, 0, i));
            }
        }

        int refill = 0;
        for (int i = 0; i < m_segment; i++)
        {
            for (int t = 0; t < m_group; t++)
            {
                m_indices.Add(refill);
                m_indices.Add(refill + 1);
                m_indices.Add(refill + 1 + m_group);
                m_indices.Add(refill + 1);
                m_indices.Add(refill + 2 + m_group);
                m_indices.Add(refill + 1 + m_group);

                refill++;
            }
            refill++;
        }
        m_targetMesh.SetVertices(m_vertices);
        m_targetMesh.SetIndices(m_indices.ToArray(), MeshTopology.Triangles, 0);
        m_targetMesh.UploadMeshData(true);
    }
    private void OnEnable()
    {
        if (m_targetMesh == null)
        {
            m_targetMesh = new Mesh();
            m_targetMesh.name = "Animation";
        }
    }
}
