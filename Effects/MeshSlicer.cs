using Sensor;
using UnityEngine;

namespace Effects {
    public class MeshSlicer : MonoBehaviour {
        [SerializeField] FirstTriggerHitSensor sensor;
    
        void Start() {
            sensor.collisionEvent.AddListener(SliceMesh);
        }

        void SliceMesh(Transform collisionTransform, PhysicsMaterial collisionMaterial, Vector3 collisionPoint, Vector3 collisionNormal) {
#if ENABLE_OPENFRACTURE
            if (collisionTransform.TryGetComponent(out Slice sliceObj)) {
                sliceObj.ComputeSlice(transform.up, collisionPoint);
            } else if (collisionTransform.TryGetComponent(out LODSlice lodSliceObj)) {
                lodSliceObj.ComputeSlice(transform.up, collisionPoint);
            }
#endif
        }

        void OnDestroy() {
            sensor.collisionEvent.RemoveListener(SliceMesh);
        }
    }
}
