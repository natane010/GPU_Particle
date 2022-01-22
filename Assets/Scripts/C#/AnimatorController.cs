using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorController : MonoBehaviour
{
    Animator selfAnim;
    private int animationStateHash = Animator.StringToHash("AnimationState");
    [SerializeField] GPUHumanTarget humantarget;
    // Start is called before the first frame update
    void Start()
    {
        selfAnim = this.gameObject.GetComponent<Animator>();
        //humantarget = this.gameObject.GetComponentInChildren<GPUHumanTarget>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.UpArrow))
        {
            selfAnim.SetInteger(animationStateHash, 1);
            humantarget.humanState = HumanState.Move;
            if (Input.GetKeyUp(KeyCode.UpArrow))
            {
                selfAnim.SetInteger(animationStateHash, 0);
                humantarget.humanState = HumanState.Idle;
            }
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            selfAnim.SetInteger(animationStateHash, 2);
            humantarget.humanState = HumanState.Jump;
            StartCoroutine(Jump());
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            humantarget.humanState = HumanState.Explode;
            selfAnim.SetInteger(animationStateHash, 0);
            StartCoroutine(Explode());
        }
    }
    IEnumerator Explode()
    {
        yield return null;
        while(!Input.GetKeyDown(KeyCode.U))
        {
            yield return null;
        }
        humantarget.humanState = HumanState.Idle;
    }
    IEnumerator Jump()
    {
        yield return null;
        humantarget.humanState = HumanState.Idle;
        selfAnim.SetInteger(animationStateHash, 0);

    }
}
