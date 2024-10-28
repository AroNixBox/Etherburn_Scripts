using System;
using UnityEngine;

namespace Effects.VFX {
    [Serializable]
    public class EffectInstance {
        /* @ Explanation
         * 
         * Add a AnimationEvent to the AnimationClip where you want the particle to spawn.
         * MethodName: SpawnParticle {@ref EventForward.cs}
         * Make sure you subscribe to the SpawnParticles UnityEvent in {@ref References.cs}
         */
        
        public ParticleSystem particleSystem;
        public Vector3 spawnPosition;
        public Vector3 spawnRotation;
        public AudioClip spawnSound;
    }
}
