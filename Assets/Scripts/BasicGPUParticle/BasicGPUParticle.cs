using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicGPUParticle : MonoBehaviour
{
    [SerializeField] ComputeShader m_computeShader;
    ComputeBuffer m_particleBuffer;
    private void Awake()
    {
        Init();
    }
    void Init()
    {

    }
}
