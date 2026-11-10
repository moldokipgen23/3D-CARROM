using UnityEditor;
using UnityEngine;

/// <summary>
/// The runtime art code builds every material with Shader.Find("Standard").
/// Shaders that no serialized material references are stripped from the player
/// build, which silently fell back to the legacy Diffuse shader (no metallic,
/// no glossiness). This utility adds the Standard shader to the Always Included
/// Shaders list so it survives the build.
/// </summary>
public static class EnsureStandardShader
{
    [MenuItem("Tools/Carrom/Ensure Standard Shader Included")]
    public static void Apply()
    {
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset");
        if (assets == null || assets.Length == 0)
        {
            Debug.LogError("EnsureStandardShader: GraphicsSettings.asset not found");
            return;
        }

        Shader standard = Shader.Find("Standard");
        if (standard == null)
        {
            Debug.LogError("EnsureStandardShader: could not find the Standard shader");
            return;
        }

        SerializedObject so = new SerializedObject(assets[0]);
        SerializedProperty list = so.FindProperty("m_AlwaysIncludedShaders");
        if (list == null)
        {
            Debug.LogError("EnsureStandardShader: m_AlwaysIncludedShaders missing");
            return;
        }

        for (int i = 0; i < list.arraySize; i++)
        {
            if (list.GetArrayElementAtIndex(i).objectReferenceValue == standard)
            {
                Debug.Log("EnsureStandardShader: Standard shader already included");
                return;
            }
        }

        list.InsertArrayElementAtIndex(list.arraySize);
        list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = standard;
        so.ApplyModifiedProperties();
        AssetDatabase.SaveAssets();
        Debug.Log("EnsureStandardShader: added Standard to Always Included Shaders");
    }
}
