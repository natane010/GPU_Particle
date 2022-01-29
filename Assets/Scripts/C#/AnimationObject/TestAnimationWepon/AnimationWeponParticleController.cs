using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AnimationWeponParticleController : MonoBehaviour
{
    [SerializeField] private GPUParticleRootSystem _particleSystem = null;
    [SerializeField] private GPUParticleTargetGroups[] _groups = null;
    
    private GPUParticleTargetGroups CurrentGroup => _groups[_index];

    private int _index = 0;

    private InitData[] _initData = null;
    WeponType WType => TestAnimationPlayer.wt;
    int hash => TestAnimationPlayer.m_animHash;


    private IEnumerator Start()
    {
        Initialize();

        yield return new WaitForSeconds(0.5f);

        foreach (var g in _groups)
        {
            g.Initialize(_particleSystem);
        }

        _particleSystem.SetGroup(_groups[0]);
        _particleSystem.Play();

        
    }

    private void Update()
    {
        switch (WType)
        {
            case WeponType.Sword:
                _index = 0;
                break;
            case WeponType.Spier:
                _index = 1;
                break;
            case WeponType.Hand:
                _index = 2;
                break;
            case WeponType.Leg:
                break;
        }
        if (hash == 0)
        {
            KeepPosition();
        }
        else if (hash == 20)
        {
            KeepPosition();
        }
        else
        {
            UpdatePosition();
        }
    }
    private void Initialize()
    {
        _initData = new InitData[_particleSystem.ParticleCount];

        for (int i = 0; i < _initData.Length; i++)
        {
            _initData[i] = new InitData();
        }
    }

    private void Next()
    {
        _index = (_index + 1) % _groups.Length;
        _particleSystem.ChangeUpdateMethod(UpdateMethodType.Target);
        _particleSystem.SetGroup(CurrentGroup);
    }
    private void KeepPosition()
    {
        _particleSystem.ChangeUpdateMethodWithClear(UpdateMethodType.KeepPosition);
        _particleSystem.SetGroup(CurrentGroup);
    }

    private void Gravity()
    {
        _particleSystem.SetOrigin(Vector3.one);
        _particleSystem.ChangeUpdateMethodWithClear(UpdateMethodType.Gravity);
    }
    private void UpdatePosition()
    {
        _particleSystem.ChangeUpdateMethodWithClear(UpdateMethodType.UpdatePosition);
        _particleSystem.SetGroup(CurrentGroup);
    }
    private void Explosion()
    {
        for (int i = 0; i < _initData.Length; i++)
        {
            _initData[i].isActive = 1;
            _initData[i].scale = 2.0f;
            _initData[i].horizontal = Random.onUnitSphere;
            Vector3 v = Vector3.forward;
            float w = Random.Range(1f, 3f);

            float d = Vector3.Dot(v, _initData[i].horizontal);

            if (d < 0)
            {
                v = (v - _initData[i].horizontal);
            }
            else
            {
                v = (v - _initData[i].horizontal);
            }

            _initData[i].velocity = new Vector4(v.x, v.y, v.z, w);
        }

        _particleSystem.SetOrigin(Vector3.one);
        _particleSystem.UpdateInitData(_initData);
        _particleSystem.ChangeUpdateMethodWithClear(UpdateMethodType.Explode);
    }
}
