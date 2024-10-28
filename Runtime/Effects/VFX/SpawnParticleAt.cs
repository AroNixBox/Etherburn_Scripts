using Sensor;
using UnityEngine;

namespace Effects.VFX {
    // TODO: Rework for Pooling.
    public class SpawnParticleAt : MonoBehaviour {
        [SerializeField] FirstTriggerHitSensor sensor;
        [SerializeField] ParticleEffect particleData;

        void OnEnable() {
            sensor.collisionEvent.AddListener(SpawnParticle);
        }

        void SpawnParticle(Transform collisionTransform, PhysicsMaterial physicMaterial, Vector3 collisionPoint, Vector3 collisionNormal) {
            Quaternion rotation = collisionNormal == Vector3.zero 
                ? Quaternion.identity 
                : Quaternion.LookRotation(collisionNormal);
            var particleGameObject = particleData.GetEffectData(physicMaterial);
            
            var particleInstance = Instantiate(particleGameObject, collisionPoint, rotation);
            
            // If our GameObject that we spawn has a ParticleSystem On It Play it.
            if(particleInstance.TryGetComponent(out ParticleSystem particleSystem)) {
                particleSystem.Play();
                // TODO: Here we should use Pool!
            }
            
            // Is a decal effect on the GameObject?
            // Add DecalGroundSpawner.cs to the GameObject
        }

        void OnDestroy() {
            sensor.collisionEvent.RemoveListener(SpawnParticle);
        }
    }
}
