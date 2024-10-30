using Sensor;
using UnityEngine;

public class MeshSlicer : MonoBehaviour {
    [SerializeField] FirstTriggerHitSensor sensor;
    
    void Start() {
        Application.targetFrameRate = 60;
        sensor.collisionEvent.AddListener(SliceMesh);
    }

    void SliceMesh(Transform collisionTransform, PhysicsMaterial collisionMaterial, Vector3 collisionPoint, Vector3 collisionNormal) {
//#if ENABLE_OPENFRACTURE
        if (collisionTransform.TryGetComponent(out Slice sliceObj)) {
            sliceObj.ComputeSlice(transform.up, collisionPoint);
        } else if (collisionTransform.TryGetComponent(out LODSlice lodSliceObj)) {
            lodSliceObj.ComputeSlice(transform.up, collisionPoint);
        }
//#endif
    }
}
