using System.IO;
using UnityEngine;
using FileLoaders;

public class SUPExternalLoader
{
    [Header("Configuration")]
    [Tooltip("The folder on your PC where the JSONs are stored.")]
    public string externalFolderPath = @"C:\ExternalAnimations\";
    [Tooltip("The .txt file listing the animation names.")]
    public string manifestFileName = "animations.txt";
    public AnimationListAsset_TXT animationListAsset_TXT;

    // Custructor to use in case of non-MonoBehaviour usage
    public SUPExternalLoader(string folderPath, string manifestName, AnimationListAsset_TXT asset) {
        externalFolderPath = folderPath;
        manifestFileName = manifestName;
        animationListAsset_TXT = asset;
        
    }

    [ContextMenu("Load Animations")]
    public void LoadExternalAnimations()
    {
        string txtPath = Path.Combine(externalFolderPath, manifestFileName);

        if (!File.Exists(txtPath))
        {
            Debug.LogError($"Manifest not found at: {txtPath}");
            return;
        }

        // Read the .txt file
        string[] lines = File.ReadAllLines(txtPath);
        animationListAsset_TXT.animationAssetGroups.Clear();

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            AnimationAssetGroup_TXT group = new AnimationAssetGroup_TXT();
            string[] fileNames = line.Trim().Split(' ');

            foreach (string fileName in fileNames)
            {
                string fullPath = Path.Combine(externalFolderPath, fileName.EndsWith(".json") ? fileName : fileName + ".json");

                if (File.Exists(fullPath))
                {
                    // Read the actual JSON text from the hard drive
                    string rawJson = File.ReadAllText(fullPath);
                    group.jsonEntries.Add(rawJson);
                }
                else
                {
                    Debug.LogWarning($"File missing: {fullPath}");
                }
            }

            if (group.jsonEntries.Count > 0)
            {
                animationListAsset_TXT.animationAssetGroups.Add(group);
            }
        }

        Debug.Log($"Successfully loaded {animationListAsset_TXT.animationAssetGroups.Count} groups from external storage.");
    }

    
}