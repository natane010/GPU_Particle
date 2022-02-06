using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("")]
public class CullingController : MonoBehaviour
{
    public Renderer Target
    {
        get;
        set;
    }
    private void OnPreCull()
    {
        Target.enabled = true;
    }
    private void OnPostRender()
    {
        Target.enabled = false;
    }
}
