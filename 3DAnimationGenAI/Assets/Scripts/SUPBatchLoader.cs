using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor; 
using FileLoaders; 
public class SUPBatchLoader : MonoBehaviour
{
    [Header("Settings")]
    public TextAsset listFile;
    public AnimationListAsset animationListAsset;

    [ContextMenu("Populate Animation List")]
    public void PopulateList()
    {
        if (listFile == null || animationListAsset == null)
        {
            Debug.LogError("Assign the TXT list and the Target Asset first!");
            return;
        }
        
        string[] lines = listFile.text.Split(new[] { "\r\n", "\r", "\n" }, System.StringSplitOptions.RemoveEmptyEntries);
        
        // Reset current animations
        animationListAsset.animationAssetGroups = new List<AnimationAssetGroup>();

        foreach (string line in lines)
        {
            // Split the line by spaces (for paired animations on one line)
            string[] fileNames = line.Trim().Split(' ');
            
            AnimationAssetGroup newGroup = new AnimationAssetGroup();
            newGroup.assets = new List<TextAsset>();

            foreach (string name in fileNames)
            {
                string cleanName = Path.GetFileNameWithoutExtension(name);
                TextAsset foundAsset = FindTextAssetByName(cleanName);

                if (foundAsset != null)
                {
                    newGroup.assets.Add(foundAsset);
                }
                else
                {
                    Debug.LogWarning($"Could not find JSON asset named: {cleanName}");
                }
            }

            if (newGroup.assets.Count > 0)
            {
                animationListAsset.animationAssetGroups.Add(newGroup);
            }
        }
        
        EditorUtility.SetDirty(animationListAsset);
        AssetDatabase.SaveAssets();
        
        Debug.Log($"Finished! Loaded {animationListAsset.animationAssetGroups.Count} groups.");
    }

    private TextAsset FindTextAssetByName(string name)
    {
        string[] guids = AssetDatabase.FindAssets($"{name} t:TextAsset");
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (Path.GetFileNameWithoutExtension(path) == name)
            {
                return AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            }
        }
        return null;
    }
}