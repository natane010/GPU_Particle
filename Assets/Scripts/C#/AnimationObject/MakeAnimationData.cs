using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public class MakeAnimationData : Editor
{
    [MenuItem("Assets/prefabs/Object", true)]
    static void Create()
    {
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (string.IsNullOrEmpty(path))
        {
            path = "Assets";
        }
        else if (Path.GetExtension(path) != "")
        {
            path = path.Replace(Path.GetFileName(path), "");
        }
        string assetPathName = AssetDatabase.GenerateUniqueAssetPath(path + "/Animation.asset");
        AnimationBaseMeshData asset = ScriptableObject.CreateInstance<AnimationBaseMeshData>();
        AssetDatabase.CreateAsset(asset, assetPathName);
        AssetDatabase.AddObjectToAsset(asset.m_targetMesh, asset);

        asset.RecalcMesh();
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
    }
}
