using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
//[RequireComponent(typeof(SkinnedMeshRenderer))]
public class GetBakeAnimationMesh : MonoBehaviour
{
    [SerializeField] GameObject animatorObject;
    private Animator selfAnimator;
    [SerializeField] private  SkinnedMeshRenderer _skin;
    static Mesh flameMesh;
    float flameCount = new float();
    [SerializeField] private string name = "skin";

    public InputField inputField;
    public InputField inputField1;
    public Text text;

    int flame;
    private void Awake()
    {
        if (animatorObject != null)
        {
            Debug.Log("Complete setAnimator");
            selfAnimator = animatorObject.GetComponent<Animator>();
        }
    }
    private void Start()
    {
        flameMesh = new Mesh();
    }
    public void SetAnimationFlame()
    {
        Debug.Log("SetAnimation");
        flameCount = flame;
        var clipList = selfAnimator.GetCurrentAnimatorClipInfo(0);
        var clip = clipList[0].clip;
        float time = (float)flame / clip.frameRate;
        var stateInfo = selfAnimator.GetCurrentAnimatorStateInfo(0);
        var animationHashID = stateInfo.shortNameHash;
        selfAnimator.Play(animationHashID, 0, time);
    }
    public void UsedBakeMeshAsMake()
    {
        Debug.Log("Bake");
        var mesh = new Mesh();
        _skin.BakeMesh(mesh);
        AssetDatabase.CreateAsset(mesh, "Assets/prefabs/mekemeshskin_"+ name + "_flame_" + flameCount + "s.mesh");
    }
    public void SetName()
    {
        text.text = inputField.text;
        name = text.text;
    }
    public void SetFlame()
    {
        flame = int.Parse(inputField1.text);
    }
}
