using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
//[RequireComponent(typeof(SkinnedMeshRenderer))]
public class GetBakeAnimationMesh : MonoBehaviour
{
    private Animator selfAnimator;
    [SerializeField] SkinnedMeshRenderer _skin;
    static Mesh flameMesh;
    [SerializeField] float flameCount;
    [SerializeField] private string name = "skin";
    private void Start()
    {
        flameMesh = new Mesh();
        selfAnimator = GetComponent<Animator>();
        string name1 = name;
        SetAnimationFlame(flameCount);

        _skin.BakeMesh(flameMesh ,true);
        AssetDatabase.CreateAsset(flameMesh, "Assets/prefabs/mekemeshskin" + name1 + "flame" + flameCount + ".mesh");
    }
    public void SetAnimationFlame(float flame)
    {
        var clipList = selfAnimator.GetCurrentAnimatorClipInfo(0);
        var clip = clipList[0].clip;
        float time = (float)flame / clip.frameRate;
        var stateInfo = selfAnimator.GetCurrentAnimatorStateInfo(0);
        var animationHashID = stateInfo.shortNameHash;
        selfAnimator.Play(animationHashID, 0, time);
    }


}
