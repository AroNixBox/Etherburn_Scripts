using Sensor;
using UnityEngine;

namespace Player.Weapon {
    public class ImpactHit : MonoBehaviour {
        [SerializeField] RaycastInBetweenTransformsSensor sensor;
        [SerializeField] float range = 1f;
        [SerializeField] float force = 1f;

        void OnEnable() {
            sensor.collisionEvent.AddListener(ApplyForceToAllCloseObjects);
        }

        void ApplyForceToAllCloseObjects(Transform collisionTransform, PhysicsMaterial physicMaterial,Vector3 hitPosition, Vector3 hitNormal) {
            var collidersInRange = Physics.OverlapSphere(transform.position, range);
            foreach (var col in collidersInRange) {
                var rb = col.attachedRigidbody;
                if (rb == null) { continue; }
                // Dont apply force to entities
                if (rb.GetComponent<Entity>()) { continue; }
                    
                Vector3 direction = (col.transform.position - transform.position).normalized;
                rb.AddForceAtPosition(direction * force, hitPosition, ForceMode.Impulse);
            }
        }
        
        void OnDestroy() {
            sensor.collisionEvent.RemoveListener(ApplyForceToAllCloseObjects);
        }
    }
}
