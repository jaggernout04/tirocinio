using System;
using System.IO;
using System.Globalization;
using UnityEngine;
using FileLoaders;

public class SUPCustomLoader
{
    [Header("Configuration")]
    [Tooltip("The folder on your PC where the JSONs are stored.")]
    public string externalFolderPath = @"C:\ExternalAnimations\";
    [Tooltip("The .txt file listing the animation names.")]
    public string manifestFileName = "animations.txt";
    public AnimationListAsset_External animationListAsset_External;
    [Tooltip("If true, loader uses the custom transform found at the start of each line in the .txt file.")]
    public bool customTransform = false; 

    public SUPCustomLoader(string folderPath, string manifestName, AnimationListAsset_External asset, bool useCustomTransform = false) {
        externalFolderPath = folderPath;
        manifestFileName = manifestName;
        animationListAsset_External = asset;
        customTransform = useCustomTransform;
    }

    /// <summary>
    /// Load SMPLH json animations by manually scraping the folder 
    /// </summary>
    public void LoadExternalAnimations()
    {
        string txtPath = Path.Combine(externalFolderPath, manifestFileName);

        if (!File.Exists(txtPath))
        {
            Debug.LogError($"Manifest not found at: {txtPath}");
            return;
        }

        string[] lines = File.ReadAllLines(txtPath);
        animationListAsset_External.animationAssetGroups.Clear();

        int lineNumber = 0;
        foreach (string line in lines)
        {
            lineNumber++;
            if (string.IsNullOrWhiteSpace(line)) continue;

            AnimationAssetGroup_External group = new AnimationAssetGroup_External();
            
            string[] tokens = line.Trim().Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 0) continue;

            int fileStartIndex = 0;

            // Check if first token matches custom bracketed transform format
            if (TryParseTransformToken(tokens[0], out Vector3 pos, out Vector3 euler, out Quaternion rot))
            {
                fileStartIndex = 1; // Advance past transform token to reach JSON file names
                
                group.originPosition = pos;
                group.originEulerAngles = euler;
                group.originRotation = rot;
                group.hasCustomTransform = true;

                if (!customTransform)
                {
                    Debug.LogWarning($"[Line {lineNumber}] Transform '{tokens[0]}' was detected in '{manifestFileName}', but 'customTransform' boolean is set to FALSE. The transform was loaded into the asset group but will not be active.");
                }
            }

            // Read JSON file names
            for (int i = fileStartIndex; i < tokens.Length; i++)
            {
                string fileName = tokens[i];
                string fullPath = Path.Combine(externalFolderPath, fileName.EndsWith(".json") ? fileName : fileName + ".json");

                if (File.Exists(fullPath))
                {
                    string rawJson = File.ReadAllText(fullPath);
                    group.jsonEntries.Add(rawJson);
                }
                else
                {
                    Debug.LogWarning($"[Line {lineNumber}] File missing: {fullPath}");
                }
            }

            if (group.jsonEntries.Count > 0)
            {
                animationListAsset_External.animationAssetGroups.Add(group);
            }
        }

        Debug.Log($"Successfully loaded {animationListAsset_External.animationAssetGroups.Count} groups from external storage.");
    }

    /// <summary>
    /// Parses transform strings formatted like "{[12.53,42.12,1.2][3.1,78,0]}"
    /// </summary>
    private bool TryParseTransformToken(string token, out Vector3 position, out Vector3 eulerAngles, out Quaternion rotation)
    {
        position = Vector3.zero;
        eulerAngles = Vector3.zero;
        rotation = Quaternion.identity;

        // Verify token structure starts with '{[' and ends with ']}'
        if (!token.StartsWith("{[") || !token.EndsWith("]}"))
            return false;

        try
        {
            // Remove outer '{' and '}'
            string inner = token.Substring(1, token.Length - 2); // Result: "[12.53,42.12,1.2][3.1,78,0]"

            // Split by "]["
            string[] brackets = inner.Split(new[] { "][" }, StringSplitOptions.None);
            if (brackets.Length != 2) return false;

            // Clean leftover outer brackets
            string posString = brackets[0].TrimStart('[');
            string rotString = brackets[1].TrimEnd(']');

            string[] posParts = posString.Split(',');
            string[] rotParts = rotString.Split(',');

            if (posParts.Length == 3 && rotParts.Length == 3)
            {
                float px = float.Parse(posParts[0], CultureInfo.InvariantCulture);
                float py = float.Parse(posParts[1], CultureInfo.InvariantCulture);
                float pz = float.Parse(posParts[2], CultureInfo.InvariantCulture);

                float rx = float.Parse(rotParts[0], CultureInfo.InvariantCulture);
                float ry = float.Parse(rotParts[1], CultureInfo.InvariantCulture);
                float rz = float.Parse(rotParts[2], CultureInfo.InvariantCulture);

                position = new Vector3(px, py, pz);
                eulerAngles = new Vector3(rx, ry, rz);
                rotation = Quaternion.Euler(rx, ry, rz);
                return true;
            }
        }
        catch
        {
            return false;
        }

        return false;
    }
}