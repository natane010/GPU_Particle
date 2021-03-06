using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HumanState
{
    Idle = 0,
    Move = 1,
    Jump = 2,
    Explode = 3,
    None = 100,
}

public class GPUHumanTarget : MonoBehaviour
{
    [SerializeField] private GPUParticleRootSystem _particleSystem = null;
    [SerializeField] private GPUParticleTargetGroups[] _groups = null;

    private GPUParticleTargetGroups CurrentGroup => _groups[_index];

    private int _index = 0;

    private InitData[] _initData = null;

    public HumanState humanState;
    

    private IEnumerator Start()
    {
        Initialize();

        yield return new WaitForSeconds(0.5f);

        foreach (var g in _groups)
        {
            g.Initialize(_particleSystem);
        }

        _particleSystem.SetGroup(_groups[0]);
        humanState = HumanState.Idle;
        _particleSystem.Play();
    }

    private void Update()
    {
        switch (humanState)
        {
            case HumanState.Idle:
                KeepPosition();
                break;
            case HumanState.Move:
                UpdatePosition();
                break;
            case HumanState.Jump:
                KeepPosition();
                break;
            case HumanState.Explode:
                Gravity();
                break;
            case HumanState.None:
                break;
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            Next();
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            Explosion();
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
