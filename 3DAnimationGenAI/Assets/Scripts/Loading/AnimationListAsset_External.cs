using System;
using System.Collections.Generic;
using Playback;
using Settings;
using SMPLModel;
using UnityEngine;

namespace FileLoaders {
    
    [CreateAssetMenu(fileName = "New Animation List Asset TXT" , menuName = Menu.AssetMenu + "New AnimationList_TXT Asset")]
    public class AnimationListAsset_External : ScriptableObject {
        
        public PlaybackSettings playbackSettings = default;
        public Models models = default;
        
        public List<AnimationAssetGroup_External> animationAssetGroups = default;

        public PlaybackSettings PlaybackSettings => playbackSettings;
        public Models Models => models;
        public List<AnimationAssetGroup_External> AnimationAssetGroups => animationAssetGroups;
    }

    [Serializable]
    public class AnimationAssetGroup_External {
        [SerializeField]
        public List<string> jsonEntries = new List<string>();
        
        public Vector3 originPosition = Vector3.zero;
        public Vector3 originEulerAngles = Vector3.zero; // Explicit Vector3 for rotation degrees (Rx, Ry, Rz)
        public Quaternion originRotation = Quaternion.identity;
        public bool hasCustomTransform = false;
    }
}