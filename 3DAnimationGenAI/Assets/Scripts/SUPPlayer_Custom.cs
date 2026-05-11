using System.Collections.Generic;
using System.Collections;
using JetBrains.Annotations;
using Settings;
using SMPLModel;
using UnityEngine;

namespace Playback {
    
    public class SUPPlayer_Custom {
        // Settings 
        readonly PlaybackSettings defaultPlaybackSettings;
        readonly DisplaySettings defaultDisplaySettings;
        readonly BodySettings defaultBodySettings;
        List<SMPLCharacter> currentCharacters;
        readonly Transform defaultOrigin;
        private MonoBehaviour coroutineRunner;


        /// <summary>
        /// Create a new player object by supplying default settings. These settings can be overriden when playing animations.
        /// </summary>
        /// <param name="defaultPlaybackSettings"></param>
        /// <param name="defaultDisplaySettings"></param>
        /// <param name="defaultBodySettings"></param>
        /// <param name="defaultOrigin">Optionally provide an origin transform to reposition the characters in the scene.</param>
        public SUPPlayer_Custom(PlaybackSettings defaultPlaybackSettings, DisplaySettings defaultDisplaySettings, BodySettings defaultBodySettings, Transform defaultOrigin = null) {
            this.defaultPlaybackSettings = defaultPlaybackSettings;
            this.defaultDisplaySettings = defaultDisplaySettings;
            this.defaultBodySettings = defaultBodySettings;
            this.defaultOrigin = defaultOrigin;
        }

        public SUPPlayer_Custom(PlaybackSettings defaultPlaybackSettings, DisplaySettings defaultDisplaySettings, BodySettings defaultBodySettings, MonoBehaviour coroutineRunner, Transform defaultOrigin = null) 
        : this(defaultPlaybackSettings, defaultDisplaySettings, defaultBodySettings, defaultOrigin) {
            this.coroutineRunner = coroutineRunner;
        }
        
        
        /// <summary>
        /// Play a set of multiple animations simultaneously
        /// </summary>
        /// <param name="animationGroup">The set to play</param>
        /// <param name="overrideDisplaySettings">Optionally override settings for this set only</param>
        /// <param name="overridePlaybackSettings">Optionally override settings for this set only</param>
        /// <param name="overrideBodySettings">Optionally override settings for this set only</param>
        /// <param name="overrideOrigin">Optionally provide an origin transform to reposition the characters in the scene.</param>
        [PublicAPI]
        public void Play(List<AMASSAnimation> animationGroup, 
            DisplaySettings overrideDisplaySettings = null, 
            PlaybackSettings overridePlaybackSettings = null, 
            BodySettings overrideBodySettings = null, 
            Transform overrideOrigin = null) {

            DisplaySettings playWithDisplaySettings = overrideDisplaySettings != null ? overrideDisplaySettings : defaultDisplaySettings;
            PlaybackSettings playWithPlaybackSettings = overridePlaybackSettings != null ? overridePlaybackSettings : defaultPlaybackSettings;
            BodySettings playWithBodySettings = overrideBodySettings != null ? overrideBodySettings : defaultBodySettings;
            Transform playWithOrigin = overrideOrigin != null ? overrideOrigin : defaultOrigin;
            
            List<SMPLCharacter> newCharacters = new List<SMPLCharacter>();

            for (int animationIndex = 0; animationIndex < animationGroup.Count; animationIndex++) {
				
                AMASSAnimation amassAnimation = animationGroup[animationIndex];
                amassAnimation.Reset();

                SMPLCharacter smplCharacter =
                    amassAnimation.Data.Model.CreateCharacter(amassAnimation, animationIndex);

                newCharacters.Add(smplCharacter);
                smplCharacter.StartAnimation(amassAnimation, playWithPlaybackSettings, playWithDisplaySettings,
                    playWithBodySettings);

                smplCharacter.SetOrigin(playWithOrigin);

            }
			

            if(currentCharacters == null) currentCharacters = new List<SMPLCharacter>();
            currentCharacters.AddRange(newCharacters);
        }
        
        
        /// <summary>
        /// Plays a single animation
        /// </summary>
        /// <param name="animation">The animation to play</param>
        /// <param name="overrideDisplaySettings">Optionally override settings for this animation only</param>
        /// <param name="overridePlaybackSettings">Optionally override settings for this animation only</param>
        /// <param name="overrideBodySettings">Optionally override settings for this animation only</param>
        /// <param name="overrideOrigin">Optionally provide an origin transform to reposition the character in the scene.</param>
        [PublicAPI]
        public void Play(AMASSAnimation animation, 
                        DisplaySettings overrideDisplaySettings = null, 
                        PlaybackSettings overridePlaybackSettings = null, 
                        BodySettings overrideBodySettings = null, 
                        Transform overrideOrigin = null) {
            List<AMASSAnimation> asList = new List<AMASSAnimation> {animation};
            
            Play(asList, 
                overrideDisplaySettings, 
                overridePlaybackSettings, 
                overrideBodySettings, 
                overrideOrigin);
        }
        
        /// <summary>
        /// Immediately stop all currently playing animations. Existing characters will be removed from the scene
        /// </summary>
        [PublicAPI]
        public void StopCurrentAnimations() {
            if (currentCharacters == null) return;
            foreach (SMPLCharacter character in currentCharacters) {
                if (character == null) continue;
                character.InterruptAnimation();
            }
        }
        
        public void PlaySequence(List<List<AMASSAnimation>> fullSequence) {
            if(fullSequence == null || fullSequence.Count == 0) {
                Debug.LogWarning("No animations provided in the sequence to play.");
                return;
            }
            if(coroutineRunner == null) {
                Debug.LogError("Coroutine runner not set. Cannot play sequence.");
                return;
            }
            if(defaultPlaybackSettings.Loop) {
                Debug.LogWarning("Looping is enabled in playback settings. Sequence will not end.");
                return;
            }
            for (int i = 0; i < fullSequence.Count; i++) {
                // Launch each batch in its own independent thread/coroutine
                coroutineRunner.StartCoroutine(BatchRoutine(fullSequence[i], i));
            }
        }
        private IEnumerator BatchRoutine(List<AMASSAnimation> batch, int batchIndex) {
            
            // Create Anchor for this batch to keep characters organized in the scene
            GameObject anchorObj = new GameObject($"Batch_{batchIndex}_Anchor");
            Transform batchAnchor = anchorObj.transform;
            
            Vector3 startPos = defaultOrigin != null ? defaultOrigin.position : Vector3.zero;
            batchAnchor.position = startPos + (defaultPlaybackSettings.OffSetSpacing * batchIndex);

            Debug.Log($"<color=cyan>Batch {batchIndex} started at {batchAnchor.position}</color>");

            // Play all animations in the current batch
            foreach (AMASSAnimation anim in batch) {             
                Play(anim, overrideOrigin: batchAnchor);

                while (!anim.Finished) {
                    yield return null; 
                }
                
                Debug.Log($"Batch {batchIndex} finished: {anim.AnimationName}");
            }

            // Cleanup the anchor when this specific batch is totally done
            Object.Destroy(anchorObj);
            Debug.Log($"<color=green>Batch {batchIndex} fully complete.</color>");
        }
    }
}