using System;
using System.Linq;
using Player.Input;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Player.Cam {
    public class OrbitalController : MonoBehaviour {
        #region Fields
        [SerializeField, Required] References references;
        [SerializeField, Required] Transform headHeight;
        [SerializeField] EntityType lockOnEntityType = EntityType.Enemy;

        [Range(0f, 90f)] public float upperVerticalLimit = 35f;
        [Range(0f, 90f)] public float lowerVerticalLimit = 35f;
        
        public bool invertVerticalAxis = true;
        public bool invertHorizontalAxis;

        public float cameraSpeed = 50f;
        public bool smoothCameraRotation;
        [Range(1f, 50f)] public float cameraSmoothingFactor = 25f;
        
        public Entity LockedOnEnemyTarget { get; private set; }

        // Dynamic Values
        float _currentXAngle;
        float _currentYAngle;
        
        // Cached
        float _detectionRadius;
        
        Transform _transform;

        InputReader _input;
        Extensions.VisionTargetQuery<Entity> _visionEnemyWarpTargetQuery;


        #endregion
        
        public Vector3 GetUpDirection() => _transform.up;
        public Vector3 GetFacingDirection() => _transform.forward;

         void Awake() {
            _transform = transform;
            
            _currentXAngle = _transform.eulerAngles.x;
            _currentYAngle = _transform.eulerAngles.y;
        }
        void Start() {
            _input = references.input;
            
            _input.LockOnTarget += ToggleLockOnTarget;
            _input.Look += RotateCameraWithLookInput;
            
            _detectionRadius = references.detectionRadius;
            var rayCheckOrigins = references.rayCheckOrigins;
            var maxTargets = references.maxTargetToCheckAround;
            var visionConeAngle = references.visionConeAngle;
            
            _visionEnemyWarpTargetQuery = new (headHeight, rayCheckOrigins, maxTargets, _detectionRadius, visionConeAngle);
        }

        void ToggleLockOnTarget() {
            if (LockedOnEnemyTarget != null) {
                // No more target
                LockedOnEnemyTarget = null;
                
                // Keep the current angles, so no weird flip happens
                Vector3 currentEulerAngles = _transform.eulerAngles;
                _currentYAngle = currentEulerAngles.y;
                _currentXAngle = currentEulerAngles.x;
                
                // Ensure the angle is between -180 and 180
                if (_currentXAngle > 180) {
                    _currentXAngle -= 360;
                }
                
                _currentXAngle = Mathf.Clamp(_currentXAngle, -upperVerticalLimit, lowerVerticalLimit);
            } else {
                // Lock on target
                var allEntitiesInVisionCone = _visionEnemyWarpTargetQuery.GetAllTargetsInVisionConeSorted();
                if(allEntitiesInVisionCone.Count == 0) { return; }

                LockedOnEnemyTarget = allEntitiesInVisionCone.FirstOrDefault(entity => entity.EntityType == lockOnEntityType);
            }
        }
        public bool IsLockedOnTarget() => LockedOnEnemyTarget != null;

        void Update() {
            if (LockedOnEnemyTarget != null) {
                if (IsTooFarAwayFromTarget()) {
                    ToggleLockOnTarget();
                    return;
                }
                
                LookAtTarget();
            }
        }

        void LookAtTarget() {
            // Look at target
            Vector3 directionToTarget = LockedOnEnemyTarget.transform.position - _transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

            // Clamp vertical rotation
            float targetXAngle = Mathf.Clamp(targetRotation.eulerAngles.x, -upperVerticalLimit, lowerVerticalLimit);
            targetRotation = Quaternion.Euler(targetXAngle, targetRotation.eulerAngles.y, 0);

            // Smooth rotation
            _transform.rotation = Quaternion.Slerp(_transform.rotation, targetRotation, Time.deltaTime * cameraSpeed);

            // Aktuelle Winkel aktualisieren
            _currentXAngle = _transform.eulerAngles.x;
            _currentYAngle = _transform.eulerAngles.y;
        }

        void RotateCameraWithLookInput(Vector2 lookDirection, bool isDeviceMouse) {
            // Only Rotate when we are not locked on a target
            if (LockedOnEnemyTarget != null) { return;}
            
            // TODO: Maybe use the isDeviceMouse to control the rotation speed
            
            // Usual Camera Rotation based on input
            lookDirection.y = invertVerticalAxis ? -lookDirection.y : lookDirection.y;
            lookDirection.x = invertHorizontalAxis ? -lookDirection.x : lookDirection.x;
        
            // Smoothing
            if (smoothCameraRotation) {
                lookDirection.x = Mathf.Lerp(0, lookDirection.x, Time.deltaTime * cameraSmoothingFactor);
                lookDirection.y = Mathf.Lerp(0, lookDirection.y, Time.deltaTime * cameraSmoothingFactor);
            }
        
            // Adjust the Angles scaled by speed and time
            _currentXAngle += lookDirection.y * cameraSpeed * Time.deltaTime;
            _currentYAngle += lookDirection.x * cameraSpeed * Time.deltaTime;
        
            // Ensure we stay in the vertical limits
            _currentXAngle = Mathf.Clamp(_currentXAngle, -upperVerticalLimit, lowerVerticalLimit);
        
            // Apply the rotation to this game object
            _transform.rotation = Quaternion.Euler(_currentXAngle, _currentYAngle, 0);
        }
        
        bool IsTooFarAwayFromTarget() {
            // Compare Squared Distances to avoid the square root calculation
            return (LockedOnEnemyTarget.transform.position - headHeight.position).sqrMagnitude > _detectionRadius * _detectionRadius;
        }

        void OnDestroy() {
            _visionEnemyWarpTargetQuery.Dispose();
        }
    }
}
