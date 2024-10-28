using Sensor;
using UnityEngine;

namespace Effects.Audio {
    [RequireComponent(typeof(AudioSource))]
    public class PlayAudio : MonoBehaviour {
        [SerializeField] AudioEffect audioEffect;
        [SerializeField] FirstTriggerHitSensor sensor;
        AudioSource _audioSource;

        void Awake() {
            _audioSource = GetComponent<AudioSource>();
        }

        void OnEnable() {
            // Can discard position and normal, because this object wont move
            sensor.collisionEvent.AddListener(PlaySound);
        }

        void PlaySound(Transform collisionTransform, PhysicsMaterial physicMaterial, Vector3 position, Vector3 normal) {
            AudioClip randomClip = audioEffect.GetEffectData(physicMaterial);
            
            // TODO: Could also play it at a position an din a direction...
            _audioSource.PlayOneShot(randomClip);
        }

        void OnDestroy() {
            sensor.collisionEvent.RemoveListener(PlaySound);
        }
    }
}
