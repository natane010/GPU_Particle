using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TestPlayerActionState(current)
/// </summary>
public enum PAState
{
    Idle = 0,
    Sword = 1,
    Sword2 = 2,
    Spier = 3,
    Upper = 4,
    acrobatkick = 5,
    Guard = 10,
    Move = 20,
}
public enum WeponType
{
    Sword,
    Spier,
    Hand,
    Leg,
}
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CharacterController))]
public class TestAnimationPlayer : MonoBehaviour
{
    #region animation menber
    Animator m_selfAnimator;
    public static int m_animHash = Animator.StringToHash("State");
    public static PAState ps = default;
    public int m_currentHash;
    public int m_nextHash;
    public bool isAnimation;
    bool conbo = false;
    #endregion

    #region AttackState
    static public WeponType wt;
    int m_switchint = 0;
    WeponType[] wts = { WeponType.Sword, WeponType.Spier, WeponType.Hand, WeponType.Leg };
    #endregion

    #region Move
    CharacterController cc;
    [SerializeField] float speed;
    #endregion

    private void Start()
    {
        
        cc = GetComponent<CharacterController>();
        m_selfAnimator = GetComponent<Animator>();
        m_currentHash = 0;
        ps = PAState.Idle;
        m_animHash = m_currentHash;
        m_selfAnimator.SetInteger("State", m_animHash);
        wt = WeponType.Sword;
        isAnimation = false;
        
    }
    private void Update()
    {
        CheakInput();
        CheakState();
        //Debug.Log(m_animHash);
        m_animHash = m_currentHash;
        m_selfAnimator.SetInteger("State", m_animHash);
    }
    void CheakState()
    {
        if (isAnimation)
        {
            return;
        }
        switch (ps)
        {
            case PAState.Idle:
                m_nextHash = 0;
                conbo = false;
                break;
            case PAState.Sword:
                m_nextHash = 1;
                break;
            case PAState.Sword2:
                m_nextHash = 2;
                break;
            case PAState.Spier:
                m_nextHash = 3;
                break;
            case PAState.Upper:
                m_nextHash = 4;
                break;
            case PAState.acrobatkick:
                m_nextHash = 5;
                break;
            case PAState.Guard:
                m_nextHash = 10;
                break;
            case PAState.Move:
                m_nextHash = 20;
                break;
        }
        m_currentHash = m_nextHash;
        if (ps != PAState.Idle && ps != PAState.Move)
        {
            isAnimation = true;
        }
    }
    void SetIdle()
    {
        ps = PAState.Idle;
        isAnimation = false;
    }
    void SetState(PAState p)
    {
        ps = p;
    }
    public void AnimationEnd()
    {
        isAnimation = false;
    }
    void CheakInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Attack();
        }
        else if (Input.GetMouseButtonDown(1))
        {
            ChangeWepon();
        }
        var moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical"));
        moveDirection = transform.TransformDirection(moveDirection);
        if (moveDirection != Vector3.zero)
        {
            Move(moveDirection);
            ps = PAState.Move;
        }
        
    }
    void Attack()
    {
        switch (wt)
        {
            case WeponType.Sword:
                if (conbo)
                {
                    SetState(PAState.Sword2);
                    conbo = false;
                }
                else
                {
                    SetState(PAState.Sword);
                    conbo = true;
                }
                break;
            case WeponType.Spier:
                SetState(PAState.Spier);
                break;
            case WeponType.Hand:
                SetState(PAState.Upper);
                break;
            case WeponType.Leg:
                SetState(PAState.acrobatkick);
                break;
        }
    }
    void ChangeWepon()
    {
        m_switchint = (m_switchint + 1) % wts.Length;
        wt = wts[m_switchint];
        Debug.Log(wt);
    }
    void Move(Vector3 dir)
    {
        cc.Move(dir * speed * Time.deltaTime);
    }
}
