using System.Collections.Generic;
using UnityEngine;
using Playback;
using SMPLModel;

public class SUPSequenceManager : MonoBehaviour
{
    [Header("References")]
    public SUPExternalLoader fileLoader;
    public Models smplModels; 

    [Header("Output")]
    public List<List<AMASSAnimation>> loadedAnimations;

    [ContextMenu("Start Sequence")]
    public void StartSequence()
    {
        // 1. Fill the ScriptableObject with strings using your existing code
        fileLoader.LoadExternalAnimations(); 

        // 2. Pass those strings to the SUPLoader_txt to be parsed into AMASSAnimations
        // We use the SO that your loader just finished populating
        var asset = fileLoader.animationListAsset_TXT;

        SUPLoader_TXT.LoadFromListAssetAsync(asset, smplModels, asset.PlaybackSettings, (results) => {
            loadedAnimations = results;
            Debug.Log($"<color=cyan>Sequence Complete!</color> {results.Count} groups parsed and ready.");
            
            // You can now call your playback function here, e.g.:
            // PlayAnimation(results[0][0]); 
        });
    }
}