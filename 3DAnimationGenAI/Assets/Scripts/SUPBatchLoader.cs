using System.Collections.Generic;
using System.IO;
using UnityEngine;
using FileLoaders;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SUPBatchLoader : MonoBehaviour
{
    [Header("External Source")]
    [Tooltip("The folder on your PC where the JSONs are stored.")]
    [SerializeField] string externalFolderPath = @"C:\AnimationsExternal\";
    
    [Header("Unity Configuration")]
    [Tooltip("The .txt file listing the animation names.")]
    [SerializeField] TextAsset listFile;
    
    [Tooltip("The ScriptableObject that will hold the references.")]
    [SerializeField] AnimationListAsset animationListAsset;
    
    [Tooltip("Directory within the Unity project to store imported JSONs. Will be created if it doesn't exist.")]
    [SerializeField] string internalPath = "Assets/ImportedAnimations";

    [ContextMenu("Execute Full Import and Load")]
    [SerializeField] void ExecuteBatchLoad()
    {
#if UNITY_EDITOR
        if (listFile == null || animationListAsset == null)
        {
            Debug.LogError("Please assign the List File and the Target Asset.");
            return;
        }

        // Check internal path and create if it doesn't exist
        if (!Directory.Exists(internalPath))
        {
            Directory.CreateDirectory(internalPath);
        }


        animationListAsset.animationAssetGroups = new List<AnimationAssetGroup>();

        // Read and process the list file
        string[] lines = listFile.text.Split(new[] { "\r\n", "\r", "\n" }, System.StringSplitOptions.RemoveEmptyEntries);

        // Check every entry
        foreach (string line in lines)
        {
            string[] names = line.Trim().Split(' ');
            AnimationAssetGroup group = new AnimationAssetGroup { assets = new List<TextAsset>() };

            foreach (string rawName in names)
            {
                // Clean the name
                string fileName = rawName.EndsWith(".json") ? rawName : rawName + ".json";
                
                // Paths
                string sourcePath = Path.Combine(externalFolderPath, fileName);
                string destinationPath = Path.Combine(internalPath, fileName);

                if (File.Exists(sourcePath))
                {
                    // Copy to project if it's not already there or to update it
                    File.Copy(sourcePath, destinationPath, true);
                    
                    // Refresh the AssetDatabase for this specific file
                    AssetDatabase.ImportAsset(destinationPath);
                    
                    // Load as TextAsset
                    TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(destinationPath);
                    
                    if (asset != null)
                    {
                        group.assets.Add(asset);
                    }
                }
                else
                {
                    Debug.LogWarning($"File missing in external folder: {sourcePath}");
                }
            }

            if (group.assets.Count > 0)
            {
                animationListAsset.animationAssetGroups.Add(group);
            }
        }

        EditorUtility.SetDirty(animationListAsset);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log($"Batch Load Complete! {animationListAsset.animationAssetGroups.Count} groups imported and linked.");
        #else
            Debug.LogWarning("Batch Loading via AssetDatabase can only be done in the Unity Editor.");
        #endif
    }
}