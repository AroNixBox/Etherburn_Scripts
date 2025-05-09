using System;
using UnityEngine;

namespace Effects.Audio {
    // TODO: Check Script unused?
    [Serializable]
    public class AudioInstance {
        /* @ Explanation
         *
         * Currently we use the same event for Audio Trigger as for particle (SpawnPartice)
         * Change if needed
         */
        
        public AudioClip[] audioClips;
        public Vector3 spawnPosition;

        public void CreateSound() {
            if(audioClips.Length == 0) {
                Debug.LogWarning("AudioClip is null");
                return;
            }
            AudioClip randomClip = audioClips[UnityEngine.Random.Range(0, audioClips.Length)];
            Debug.Log($"Playing {randomClip.name}");
            AudioSource.PlayClipAtPoint(randomClip, spawnPosition);
        }
    }
}
