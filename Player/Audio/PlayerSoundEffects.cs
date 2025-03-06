using Effects;
using Player.Animation;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Player.Audio {
    public class PlayerSoundEffects : MonoBehaviour {
        [Header("References")]
        [SerializeField, Required] EventForward eventForward;
        [SerializeField, Required] AudioSource effectAudioSource;
        
        [Header("Ground Contact")]
        [Tooltip("The offset from the center feet position up to make the raycast hit the ground 100%")]
        [SerializeField] float castStartOffset = 0.5f;
        [SerializeField] float rayCastLength = 1f;
        
        
        [Title("Footsteps")]
        [SerializeField, Required] Transform centerFeetPosition;
        [Tooltip("Animation Events are captured through his component")]
        [SerializeField, Required] AudioEffect footstepEffectAsset;
        [SerializeField] LayerMask groundLayers;
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
            eventForward.OnFootstepPerformed += PlayFootstepSound;
            eventForward.OnLandPerformed += PlayLandSound;
            eventForward.OnDodgePerformed += PlayDodgeSound;
        }
        
        void PlayFootstepSound() {
            PlayGroundContactSound(EGroundContactType.Footstep);
        }
        void PlayLandSound() {
            PlayGroundContactSound(EGroundContactType.Land);
        }

        void PlayGroundContactSound(EGroundContactType groundContactType) {
            if (Physics.Raycast(centerFeetPosition.position + Vector3.up * castStartOffset, Vector3.down, out var hit, rayCastLength, groundLayers)) {
                // Check the physics material of the object we hit
                
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
            eventForward.OnFootstepPerformed -= PlayFootstepSound;
            eventForward.OnLandPerformed -= PlayLandSound;
            eventForward.OnDodgePerformed -= PlayDodgeSound;
        }

        enum EGroundContactType {
            Footstep,
            Land
        }
    }
}
