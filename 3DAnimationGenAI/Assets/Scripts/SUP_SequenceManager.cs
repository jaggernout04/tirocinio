using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Playback;
using SMPLModel;
using Settings;
using FileLoaders;


public class SUPSequenceManager : MonoBehaviour
{
    [Header("References")]
    public SUPExternalLoader fileLoader;
    public Models smplModels = default; 
    public PlaybackSettings playbackSettings = default;

    [Header("Configuration")]
    [Tooltip("The folder on your PC where the JSONs are stored.")]
    public string externalFolderPath = @"C:\ExternalAnimations\";
    [Tooltip("The .txt file listing the animation names.")]
    public string manifestFileName = "animations.txt";
    public AnimationListAsset_TXT animationListAsset_TXT;

    [Header("Output")]
    public List<List<AMASSAnimation>> loadedAnimations = new List<List<AMASSAnimation>>();


    void Start()
    {
        //CustomLoading();
        SUPLoading(true);
    }
    [ContextMenu("Start Sequence")]
    public void CustomLoading()
    {
        if(fileLoader == null) {

            fileLoader = new SUPExternalLoader(externalFolderPath, manifestFileName, animationListAsset_TXT);
        }
        // 1. Fill the ScriptableObject with strings using your existing code
        fileLoader.LoadExternalAnimations(); 

        // 2. Pass those strings to the SUPLoader_TXT to be parsed into AMASSAnimations
        // We use the SO that your loader just finished populating
        var asset = fileLoader.animationListAsset_TXT;

        SUPLoader_TXT.LoadFromListAssetAsync(asset, smplModels, asset.PlaybackSettings, (results) => {
            loadedAnimations = results;
            Debug.Log($"<color=cyan>Sequence Complete!</color> {results.Count} groups parsed and ready.");
            
            // You can now call your playback function here, e.g.:
            // PlayAnimation(results[0][0]); 
        });
    }

    public void SUPLoading(bool useListTXT = false) {
    AnimationFileReference fileRef;
    if (useListTXT)
    {
        fileRef = new AnimationFileReference(Path.Combine(externalFolderPath), Path.Combine(externalFolderPath, manifestFileName));
    }
    else
    {
        fileRef = new AnimationFileReference(Path.Combine(externalFolderPath));
    }

    // SUPLoader built-in function to load external files
    SUPLoader.LoadAsync(fileRef, smplModels, playbackSettings, (results) => {
        Debug.Log("Loaded using SUPLoader built-in function");
        loadedAnimations = results;
    });
}
}