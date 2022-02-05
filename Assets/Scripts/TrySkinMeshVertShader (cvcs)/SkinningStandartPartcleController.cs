using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Skinning/Skinning Particle")]
[RequireComponent(typeof(MeshRenderer))]
public class SkinningStandartPartcleController : MonoBehaviour
{
    #region particle source value
    [SerializeField] Shader _kernelShader;
    [SerializeField] Material _defaultMaterial;
    [SerializeField] SkinningSorce m_skinsorce;
    [SerializeField] SkinningStandartParticle m_skinningStandartParticle;
    [SerializeField] float m_speedLimit = 1.0f;
    [SerializeField, Range(0, 20)] float m_drag = 0.1f;
    [SerializeField] Vector3 m_gravity = Vector3.zero;
    [SerializeField] float m_seedLife = 5.0f;
    [SerializeField] float m_maxLife = 5.0f;
    [SerializeField] float m_spin = 90.0f;
    [SerializeField] float m_maxSpin = 20.0f;
    [SerializeField] float m_seedScale = 1.0f;
    [SerializeField] float m_maxScale = 2.0f;
    [SerializeField] float m_noise = 1.0f;
    [SerializeField] float m_noiseF = 0.2f;
    [SerializeField] float m_noiseMotion = 1.0f;
    [SerializeField] int m_rand = 0;
    bool m_reconfiguration;
    public SkinningSorce SkinSource
    {
        get
        {
            return m_skinsorce;
        }
        set
        {
            m_skinsorce = value;
            m_reconfiguration = true;
        }
    }
    public SkinningStandartParticle SkinningStandartParticle
    {
        get
        {
            return m_skinningStandartParticle;
        }
        set
        {
            m_skinningStandartParticle = value;
            m_reconfiguration = true;
        }
    }
    public float speedLimit
    {
        get
        {
            return m_speedLimit;
        }
        set
        {
            m_speedLimit = value;
        }
    }
    public float Drag
    {
        get
        {
            return m_drag;
        }
        set
        {
            m_drag = value;
        }
    }
    public Vector3 Gravity
    {
        get
        {
            return m_gravity;
        }
        set
        {
            m_gravity = value;
        }
    }
    public float SeedLife
    {
        get
        {
            return m_seedLife;
        }
        set
        {
            m_seedLife = value;
        }
    }
    public float MaxLife
    {
        get
        {
            return m_maxLife;
        }
        set
        {
            m_maxLife = value;
        }
    }
    public float Spin
    {
        get
        {
            return m_spin;
        }
        set
        {
            m_spin = value;
        }
    }
    public float MaxSpin
    {
        get
        {
            return m_maxSpin;
        }
        set
        {
            m_maxSpin = value;
        }
    }
    public float SeedScale
    {
        get
        {
            return m_seedScale;
        }
        set
        {
            m_seedScale = value;
        }
    }
    public float MaxScale
    {
        get
        {
            return m_maxScale;
        }
        set
        {
            m_maxScale = value;
        }
    }
    public float Noise
    {
        get
        {
            return m_noise;
        }
        set
        {
            m_noise = value;
        }
    }
    public float NoiseF
    {
        get
        {
            return m_noiseF;
        }
        set
        {
            m_noiseF = value;
        }
    }
    public float NoiseMotion
    {
        get
        {
            return m_noiseMotion;
        }
        set
        {
            m_noiseMotion = value;
        }
    }
    public int Rand
    {
        get
        {
            return m_rand;
        }
        set
        {
            m_rand = value;
            m_reconfiguration = true;
        }
    }
    #endregion

    #region メソッド・関数
    public void UpdateConfiguration()
    {
        m_reconfiguration = true;
    }
    enum Kernels
    {
        InitializePosition, 
        InitializeVelocity,
        InitializeRotation,
        UpdatePosition, 
        UpdateVelocity, 
        UpdateRotation,
    }
    enum Buffers 
    { 
        Position, 
        Velocity, 
        Rotation, 
    }
    AnimationKernelSet<Kernels, Buffers> m_kernel;

    #endregion
}
