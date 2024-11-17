using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Sensor {
    [RequireComponent(typeof(Collider))]
    public class FirstTriggerHitSensor : MonoBehaviour{
        // Material, Collision Point, Collision Normal
        [FormerlySerializedAs("transformsAlongWeapon")]
        [SerializeField] Transform[] transformsAlongObject;
        public UnityEvent<Transform, PhysicsMaterial, Vector3, Vector3> collisionEvent = new ();
        Collider _collider;
        
        [SerializeField] protected LayerMask excludedLayers = 1 << 2; // Default to Ignore Raycast layer

        protected virtual void Awake() {
            _collider = GetComponent<Collider>();
            _collider.isTrigger = true;
        }
        
        public virtual void SetColliderEnabled(bool active) {
            _collider.enabled = active;
        }
        
        // Base Call starts here, should identify collision before calling base.OnTriggerEnter(other)
        protected virtual void OnTriggerEnter(Collider other) {
            if (((1 << other.gameObject.layer) & excludedLayers) != 0) {
                return;
            }
            
            // Get the closest point of the collision (approximate contact point in world space)
            Vector3 collisionPoint = GetClosestPoint(other);

            // Calculate the collision normal using the direction from the other object's center to the collision point
            Vector3 collisionNormal = (collisionPoint - transform.position).normalized;

            // Trigger the event and pass the collision normal and the world space position of the collision
            collisionEvent?.Invoke(other.transform, other.sharedMaterial, collisionPoint, collisionNormal);
        }
        
        protected Vector3 GetClosestPoint(Collider other) {
            Vector3 closestPoint = transformsAlongObject[0].position;
            float closestDistance = Vector3.Distance(other.ClosestPoint(closestPoint), closestPoint);

            foreach (var transformAlongWeapon in transformsAlongObject) {
                Vector3 point = transformAlongWeapon.position;
                float distance = (other.ClosestPoint(point) - point).sqrMagnitude;
                if (distance < closestDistance) {
                    closestPoint = point;
                    closestDistance = distance;
                }
            }

            return closestPoint;
        }
    }
}