using Sensor;
using UnityEngine;

public class MeshSlicer : MonoBehaviour {
    [SerializeField] FirstTriggerHitSensor sensor;
    
    void Start() {
        sensor.collisionEvent.AddListener(SliceMesh);
    }

    void SliceMesh(Transform collisionTransform, PhysicsMaterial collisionMaterial, Vector3 collisionPoint, Vector3 collisionNormal) {
        if (collisionTransform.TryGetComponent(out Slice sliceObj)) {
            sliceObj.ComputeSlice(transform.up, collisionPoint);
        } else if (collisionTransform.TryGetComponent(out LODSlice lodSliceObj)) {
            lodSliceObj.ComputeSlice(transform.up, collisionPoint);
        }
    }
}
