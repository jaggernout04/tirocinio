using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using Playback;
using SMPLModel;
using Settings;
using FileLoaders;


public class SUPSequenceManager : MonoBehaviour
{
    [Header("References")]
    public SUPCustomLoader fileLoader;
    public Models smplModels = default; 
    public PlaybackSettings playbackSettings = default;
    public BodySettings bodySettings = default;
    public DisplaySettings displaySettings = default;

    // --- LOADING SETTINGS ---
    [Header("Loading Settings")]
    [Tooltip("The folder on your PC where the JSONs are stored.")]
    public string externalFolderPath = @"/yourdirectory";
    [Tooltip("The .txt file listing the animation names.")]
    public string manifestFileName = "animations.txt";
    // This one can either be set to a preloaded AnimationListAsset_External or automatically created with the settings in SUPSequenceManager. I
    public AnimationListAsset_External animationListAsset_External = default;
    public bool useCustomLoading = false; // Set to true to use the custom loading method instead of SUPLoader's built-in function  
    [Tooltip("Only work with custom loading.")]
    public bool useCustomTransform = false; // if true, loader expect a transform at the start of each animation inside the JSON. If false, loader will use the animationOrigin transform for all animations.


    // --- ANIMATION SETTINGS ---
    [Header("Animation Settings")]
    public Transform animationOrigin;
    public bool spawnAnimationSeperated = false; // If true, each animation will be spawned at a different position and in sequence. If false, all animations will be spawned at the same position.
    private SUPPlayer_Custom SUPAnimPlayer;

    // --- OUTPUT ---
    public List<List<AMASSAnimation>> loadedAnimations;

 

    // --- EVENTS ---
    public static event Action OnLoadingFinished;
    public UnityEvent OnLoadingFinishedEvent;
    


    //----------------------------------------
    //--------- UNITY LIFECYCLE --------------
    //----------------------------------------
    void OnEnable()
    {
        if(animationOrigin == null) {
            SUPAnimPlayer = new SUPPlayer_Custom(playbackSettings, displaySettings, bodySettings, this);
        }
        else {
            SUPAnimPlayer = new SUPPlayer_Custom(playbackSettings, displaySettings, bodySettings, this, animationOrigin);
        }
        if(fileLoader == null) {    
            fileLoader = new SUPCustomLoader(externalFolderPath, manifestFileName, animationListAsset_External, useCustomTransform);
        }
        loadedAnimations = new List<List<AMASSAnimation>>();
        SUPSequenceManager.OnLoadingFinished += LoadingFinished;
    }
        private void OnDisable()
    {
        SUPSequenceManager.OnLoadingFinished -= LoadingFinished;
    }

    void Start()
    {
        switch (useCustomLoading)
        {
            case true:
                CustomLoading();
                break;
            case false:
                SUPLoading(true);
                break;
        }
    }

    //-----------------------------------------------
    //-------- Event handlers and Callbacks ---------
    //-----------------------------------------------
    private void LoadingFinished()
    {
        PlayAnimation(loadedAnimations);
    }



    //----------------------------------------------
    //------------Core Functions--------------------
    //----------------------------------------------

    /// <summary>
    /// Load SMPLH animations with custom method
    /// </summary>
    public void CustomLoading()
    {
        fileLoader.LoadExternalAnimations(); 
        SUPLoader_External.LoadFromListAssetAsync(animationListAsset_External, smplModels, animationListAsset_External.PlaybackSettings, (results) => {
            loadedAnimations = results;
            Debug.Log($"<color=cyan>Sequence Complete!</color> {results.Count} groups parsed and ready.");
            OnLoadingFinished?.Invoke(); // -1 indicates all animations
            OnLoadingFinishedEvent?.Invoke();     
        });
    }

    /// <summary>
    /// Loads the SMPLH animations using the original SUPLoader
    /// </summary>
    /// <param name="useListTXT">Set to true to use a load order using the manifest filec</param>
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
        SUPLoader_External.LoadAsync(fileRef, smplModels, playbackSettings, (results) => {
            Debug.Log("Loaded using SUPLoader built-in function");
            Debug.Log($"<color=cyan>Sequence Complete!</color> {results.Count} groups parsed and ready.");
            loadedAnimations = results;
            OnLoadingFinished?.Invoke(); // -1 indicates all animations
            OnLoadingFinishedEvent?.Invoke(); 
        });
    }



    /// <summary>
    /// Plays the SMPLH animations using the SUPPlayer_Custom.
    /// </summary>
    /// <param name="animIndex">Optional index to specify which animation group to play. If -1, all animations will be played in sequence.</param>
    public void PlayAnimation(List<List<AMASSAnimation>> loadedAnimations, int animIndex = -1)
    {
        if (loadedAnimations == null || loadedAnimations.Count == 0)
        {
            Debug.LogWarning("No animations loaded to play.");
            return;
        }

        List<Vector3> customPositions = new List<Vector3>();
        List<Quaternion> customRotations = new List<Quaternion>();
        if(useCustomTransform)
        {
            // Extract custom positions and rotations from loaded external asset groups
            if (animationListAsset_External != null && animationListAsset_External.animationAssetGroups != null)
            {
                foreach (var group in animationListAsset_External.animationAssetGroups)
                {
                    customPositions.Add(group.originPosition);
                    customRotations.Add(group.originRotation);
                }
            }
        }
        // play all animations independently
        if(spawnAnimationSeperated)
        {
            Debug.Log($"Playing all animations independently with custom origins.");
            SUPAnimPlayer.PlaySequence(loadedAnimations, customPositions, customRotations, useCustomTransform);
            return;
        }




        // Play a specific animation group if animIndex is provided
        if (animIndex >= 0 && animIndex < loadedAnimations.Count)
        {
            Transform overrideOrigin = null;
            if (useCustomTransform && animIndex < customPositions.Count)
            {
                GameObject tempAnchor = new GameObject($"Anim_{animIndex}_CustomOrigin");
                tempAnchor.transform.position = customPositions[animIndex];
                tempAnchor.transform.rotation = customRotations[animIndex];
                overrideOrigin = tempAnchor.transform;
            }

            SUPAnimPlayer.Play(loadedAnimations[animIndex], overrideOrigin: overrideOrigin);
            Debug.Log($"Playing animation at index {animIndex}: {loadedAnimations[animIndex][0]}");
            return;
        }

        // Play all animations in sequence
        int currentPlayingIndex = 0;
        while (currentPlayingIndex < loadedAnimations.Count)
        {
            Transform overrideOrigin = null;
            if (useCustomTransform && currentPlayingIndex < customPositions.Count)
            {
                GameObject tempAnchor = new GameObject($"Anim_{currentPlayingIndex}_CustomOrigin");
                tempAnchor.transform.position = customPositions[currentPlayingIndex];
                tempAnchor.transform.rotation = customRotations[currentPlayingIndex];
                overrideOrigin = tempAnchor.transform;
            }

            SUPAnimPlayer.Play(loadedAnimations[currentPlayingIndex], overrideOrigin: overrideOrigin);
            Debug.Log($"Playing animation: {loadedAnimations[currentPlayingIndex][0]}");
            currentPlayingIndex++;
        }
    }
}