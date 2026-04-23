using System;
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
    public SUPExternalLoader fileLoader;
    public Models smplModels = default; 
    public PlaybackSettings playbackSettings = default;
    public BodySettings bodySettings = default;
    public DisplaySettings displaySettings = default;

    [Header("Configuration")]
    [Tooltip("The folder on your PC where the JSONs are stored.")]
    public string externalFolderPath = @"C:\ExternalAnimations\";
    [Tooltip("The .txt file listing the animation names.")]
    public string manifestFileName = "animations.txt";
    public AnimationListAsset_TXT animationListAsset_TXT = default;
    public Transform animationOrigin;
    public bool useCustomLoading = false; // Set to true to use the custom loading method instead of SUPLoader's built-in function  

    [Header("Output")]
    public List<List<AMASSAnimation>> loadedAnimations;

    private SUPPlayer SUPAnimPlayer;

    // --- EVENTS ---
    public static event Action OnLoadingFinished;
    public UnityEvent OnLoadingFinishedEvent;

    //----------------------------------------
    //--------- UNITY LIFECYCLE --------------
    //----------------------------------------
    void OnEnable()
    {
        if(animationOrigin == null) {
            SUPAnimPlayer = new SUPPlayer(playbackSettings, displaySettings, bodySettings);
        }
        else {
            SUPAnimPlayer = new SUPPlayer(playbackSettings, displaySettings, bodySettings, animationOrigin);
        }
        if(fileLoader == null) {    
            fileLoader = new SUPExternalLoader(externalFolderPath, manifestFileName, animationListAsset_TXT);
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

        SUPLoader_TXT.LoadFromListAssetAsync(animationListAsset_TXT, smplModels, animationListAsset_TXT.PlaybackSettings, (results) => {
            loadedAnimations = results;
            Debug.Log($"<color=cyan>Sequence Complete!</color> {results.Count} groups parsed and ready.");
            OnLoadingFinished?.Invoke(); // -1 indicates all animations
            OnLoadingFinishedEvent?.Invoke();     
        });
    }

    /// <summary>
    /// Loads the SMPLH animations using the SUPLoader
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
        SUPLoader.LoadAsync(fileRef, smplModels, playbackSettings, (results) => {
            Debug.Log("Loaded using SUPLoader built-in function");
            Debug.Log($"<color=cyan>Sequence Complete!</color> {results.Count} groups parsed and ready.");
            loadedAnimations = results;
            OnLoadingFinished?.Invoke(); // -1 indicates all animations
            OnLoadingFinishedEvent?.Invoke(); 
        });
    }



    /// <summary>
    /// Plays the SMPLH animations using the SUPPlayer.
    /// </summary>
    /// <param name="animIndex">Optional index to specify which animation group to play. If -1, all animations will be played in sequence.</param>
    public void PlayAnimation(List<List<AMASSAnimation>> loadedAnimations, int animIndex = -1)
    {
        if (loadedAnimations == null || loadedAnimations.Count == 0)
        {
            Debug.LogWarning("No animations loaded to play.");
            return;
        }

        if(animIndex >= 0)
        {
            SUPAnimPlayer.Play(loadedAnimations[animIndex]);
            Debug.Log($"Playing animation: {loadedAnimations[animIndex][0]}");
            return;
        }
        int currentPlayingIndex = 0;
        while(currentPlayingIndex < loadedAnimations.Count)
        {
            SUPAnimPlayer.Play(loadedAnimations[currentPlayingIndex]);
            Debug.Log($"Playing animation: {loadedAnimations[currentPlayingIndex][0]}");
            currentPlayingIndex++;
        }
    }
}