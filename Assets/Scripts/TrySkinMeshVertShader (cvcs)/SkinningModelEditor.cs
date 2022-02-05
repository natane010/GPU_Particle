using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SkinningSorceModel))]
public class SkinningModelEditor : Editor
{
    #region エディタ関数
    public override void OnInspectorGUI()
    {
        SkinningSorceModel model = (SkinningSorceModel)target;
        EditorGUILayout.LabelField("Vertex Count", model.vertexCount.ToString());
    }
    static Mesh[] SelectMeshAssets
    {
        get
        {
            Object[] assets = Selection.GetFiltered(typeof(Mesh), SelectionMode.Deep);
            return assets.Select(x => (Mesh)x).ToArray();
        }
    }
    static bool CheakSkinned(Mesh mesh)
    {
        if (mesh.boneWeights.Length > 0)
        {
            return true;
        }
        Debug.LogError("スキニングされていないもしくはできないモデルです。");
        return false;
    }
    [MenuItem("Assets/Skinning/Convert", true)]
    static bool ValidateAssets()
    {
        return SelectMeshAssets.Length > 0;
    }
    [MenuItem("Assets/Skinning/Convert")]
    static void ConvertAsset()
    {
        List<Object> converted = new List<Object>();
        foreach (var item in SelectMeshAssets)
        {
            if (!CheakSkinned(item))
            {
                continue;
            }
            string dirPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(item));
            string assetPath = AssetDatabase.GenerateUniqueAssetPath(dirPath + "/NewSkinnedModel.asset");
            SkinningSorceModel asset = ScriptableObject.CreateInstance<SkinningSorceModel>();
            asset.Initialize(item);
            AssetDatabase.CreateAsset(asset, assetPath);
            AssetDatabase.AddObjectToAsset(asset.mesh, asset);
            converted.Add(asset);
        }
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
        Selection.objects = converted.ToArray();
    }
    #endregion
}
[CanEditMultipleObjects]
[CustomEditor(typeof(SkinningSorce))]
public class SkinningSourceEditor : Editor
{
    SerializedProperty model;
    private void OnEnable()
    {
        model = serializedObject.FindProperty("model");
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(model);
        serializedObject.ApplyModifiedProperties();
    }
}
//[CustomEditor(typeof())]
//public class StandartSkinnerParticleEdtor : Editor
