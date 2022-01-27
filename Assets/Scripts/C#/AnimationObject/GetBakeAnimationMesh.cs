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
    [SerializeField] bool weld = false;
    static Mesh flameMesh;
    float flameCount = new float();
    [SerializeField] private string name = "skin";

    public InputField inputField;
    public InputField inputField1;
    public Text text;

    [SerializeField] bool isDissect = false;
    [SerializeField, Range(1, 4)] protected int details = 1;
    static Vector2[] uvs;
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
        if (isDissect)
        {
            mesh = SubdivisionSurface.Subdivide(SubdivisionSurface.Weld(mesh, float.Epsilon, mesh.bounds.size.x), details, weld);
            int i = 0;
            uvs = new Vector2[mesh.vertices.Length];
            foreach (var item in mesh.vertices)
            {
                uvs[i] = new Vector2(item.x, item.y);
                i++;
            }

            mesh.uv = uvs;
        }
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
