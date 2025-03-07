using Effects;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Player.Audio {
    public class PlayerSoundEffects : MonoBehaviour {
        [Header("References")]
        [SerializeField, Required] References references;
        [SerializeField, Required] Mover mover;
        [SerializeField, Required] AudioSource effectAudioSource;
        
        [Title("Footsteps")]
        [Tooltip("Animation Events are captured through his component")]
        [SerializeField, Required] AudioEffect footstepEffectAsset;
        [Space(10f)]
        [SerializeField] float footStepVolume = .05f;
        
        [Title("Land")]
        [SerializeField, Required] AudioEffect landEffectAsset;
        [Space(10f)]
        [SerializeField] float landStepVolume = .15f;
        
        [Title("Dodge")]
        [SerializeField] AudioClip[] dodgeSounds;
        [SerializeField] float dodgeVolume = .2f;

        void Start() {
            references.OnFootstepPerformed += PlayFootstepSound;
            references.OnLandPerformed += PlayLandSound;
            references.OnDodgeStarted += PlayDodgeSound;
        }
        
        void PlayFootstepSound() {
            PlayGroundContactSound(EGroundContactType.Footstep);
        }
        void PlayLandSound() {
            PlayGroundContactSound(EGroundContactType.Land);
        }

        void PlayGroundContactSound(EGroundContactType groundContactType) {
            mover.RecalibrateSensor();
            mover.GroundedSensor.Cast();
            
            if (mover.GroundedSensor.HasDetectedHit()) {
                // Check the physics material of the object we hit
                var hit = mover.GroundedSensor.GetHit();
                var audioEffect = groundContactType == EGroundContactType.Footstep 
                    ? footstepEffectAsset.GetEffectData(hit.collider.sharedMaterial)
                    : landEffectAsset.GetEffectData(hit.collider.sharedMaterial);
                
                if (audioEffect == null) {
                    Debug.LogWarning("No SFX found for this surface, not even fallback.");
                }
                
                effectAudioSource.PlayOneShot(audioEffect, footStepVolume);
            }
            else
            {
                Debug.LogWarning("Sound event detected, but no ground Material found.");
            }
        }

        void PlayDodgeSound() {
            if (dodgeSounds.Length > 0) {
                effectAudioSource.PlayOneShot(dodgeSounds[UnityEngine.Random.Range(0, dodgeSounds.Length)], dodgeVolume);
            }
        }

        void OnDestroy() {
            references.OnFootstepPerformed -= PlayFootstepSound;
            references.OnLandPerformed -= PlayLandSound;
            references.OnDodgeStarted -= PlayDodgeSound;
        }

        enum EGroundContactType {
            Footstep,
            Land
        }
    }
}
