using Sirenix.OdinInspector;
using UnityEngine;

namespace Player.Cam {
    public class DistanceRaycaster : MonoBehaviour {
        [SerializeField, Required] Transform cameraTransform;
        [SerializeField, Required] Transform cameraTargetTransform;
        
        // Not zero
        public LayerMask layerMask = Physics.AllLayers;
        public float minimumDistanceFromObstacles = 0.1f;
        [Tooltip("How smoothly the camera adjusts when an obstacle is detected")]
        public float smoothingFactor = 25f;
        
        Transform _transform;
        // Distance to Camera
        float _currentDistance;

        void Awake() {
            _transform = transform;
            
            // Exclude the Ignore Raycast layer, so it doesnt intersect with any raycasthing/ Spherecasting that is about to come
            layerMask &= ~(1 << LayerMask.NameToLayer("Ignore Raycast"));
            _currentDistance = (cameraTargetTransform.position - cameraTransform.position).magnitude;
        }
        
        void LateUpdate() {
            AvoidObstacles();
        }

        void AvoidObstacles() {
            Vector3 castDirection = cameraTargetTransform.position - _transform.position;

            float distance = GetCameraDistance(castDirection);
            
            _currentDistance = Mathf.Lerp(_currentDistance, distance, Time.deltaTime * smoothingFactor);
            cameraTransform.position = _transform.position + castDirection.normalized * _currentDistance;
        }

        float GetCameraDistance(Vector3 castDirection) {
            // Calculate distance from the current position to the targets position plus buffer to make sure the camera doesnt get too close to any obstacles
            float distance = castDirection.magnitude + minimumDistanceFromObstacles;
            
            float sphereRadius = 0.5f;
            if(Physics.SphereCast(new Ray(_transform.position, castDirection), sphereRadius, out RaycastHit hit, distance, layerMask, QueryTriggerInteraction.Ignore)) {
                // If we hit anything
                // Calculate distance to the obstacle minus the buffer, {Mathf.Max} to only return positive values
                return Mathf.Max(0f, hit.distance - minimumDistanceFromObstacles);
            }
            // if we didnt hit anything, return full distance
            return castDirection.magnitude;
        }
    }
}