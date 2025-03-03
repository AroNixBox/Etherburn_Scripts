using System.Data;
using System.Linq;
using Extensions;
using Player.Input;
using Sirenix.OdinInspector;
using UI;
using UnityEngine;

namespace Player.Cam {
    public class OrbitalController : MonoBehaviour {
        #region Fields
        [Title("References")]
        [SerializeField, Required] References references;
        [SerializeField, Required] Transform headHeight;
        [SerializeField, Required] UIOnscreenFocus uiOnScreenFocus;
        
        [Title("Settings")]
        [SerializeField] EntityType lockOnEntityType = EntityType.Enemy;

        [Range(0f, 90f)] [SerializeField] float upperVerticalLimit = 35f;
        [Range(0f, 90f)] [SerializeField] float lowerVerticalLimit = 35f;
        
        [SerializeField] bool invertVerticalAxis = true;
        [SerializeField] bool invertHorizontalAxis;

        [SerializeField] float cameraSpeed = 50f;
        [SerializeField] bool smoothCameraRotation;
        [SerializeField] [Range(1f, 50f)] float cameraSmoothingFactor = 25f;
        [SerializeField] float controllerSpeedMultiplier = 25f;
        [SerializeField] bool debug;
        
        public Entity LockedOnEnemyTarget { get; private set; }

        // Dynamic Values
        float _currentXAngle;
        float _currentYAngle;
        
        // Cached
        float _detectionRadius;
        
        // Directly Calling the Look Method from the Event doesn't work - InputAsset is broken, performed isnt called when value stays the same on controller but still isn't cancelled
        Vector2 _lookDirection;
        bool _isDeviceMouse;
        bool _isLooking;
        
        Transform _transform;

        InputReader _input;
        VisionTargetQuery<Entity> _visionEnemyWarpTargetQuery;


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
            _input.Look += ChangeLookValues;
            _input.IsLooking += ToggleLook;
            
            _detectionRadius = references.detectionRadius;
            var rayCheckOrigins = references.rayCheckOrigins;
            var maxTargets = references.maxTargetToCheckAround;
            var visionConeAngle = references.visionConeAngle;
            
            _visionEnemyWarpTargetQuery = new VisionTargetQuery<Entity>.Builder()
                .SetHead(headHeight)
                .SetRayCheckOrigins(rayCheckOrigins)
                .SetMaxTargets(maxTargets)
                .SetDetectionRadius(_detectionRadius)
                .SetVisionConeAngle(visionConeAngle)
                .SetDebug(debug)
                .Build<Entity>();
        }

        void ChangeLookValues(Vector2 lookDirection, bool isDeviceMouse) {
            _lookDirection = lookDirection;
            _isDeviceMouse = isDeviceMouse;
        }

        void ToggleLook(bool arg0) {
            _isLooking = arg0;
            if (_isLooking) { return; }
            // Reset on Cancel
            _lookDirection = Vector2.zero;
            _isDeviceMouse = false;
        }

        void ToggleLockOnTarget() {
            if (LockedOnEnemyTarget != null) {
                // Remove Lock on Visual
                uiOnScreenFocus.RemoveTarget();
                
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
                var entityManager = EntityManager.Instance;
                if (entityManager == null) {
                    Debug.LogError("Entity Manager ist not in the Scene!", transform);
                    return;
                }
            
                var allEnemies = entityManager.GetEntitiesOfType(lockOnEntityType, out _); // TODO: Use "_" if want to perform something when no more Enemies are alive                
                var allEntitiesInVisionCone = _visionEnemyWarpTargetQuery.GetAllTargetsInVisionConeSorted(allEnemies);
                if(allEntitiesInVisionCone.Count == 0) { return; }

                LockedOnEnemyTarget = allEntitiesInVisionCone.FirstOrDefault(entity => entity.EntityType == lockOnEntityType);
                
                // Add Lock on Visual
                if (LockedOnEnemyTarget == null) { return; }
                uiOnScreenFocus.SetTarget(LockedOnEnemyTarget.TryGetComponent(out EnemyBodyParts bodyParts)
                    ? bodyParts.head
                    : LockedOnEnemyTarget.transform);
                
            }
        }
        public bool IsLockedOnTarget() => LockedOnEnemyTarget != null;

        void Update() {
            if(_isLooking) {
                RotateCameraWithLookInput(_lookDirection, _isDeviceMouse);
            }
            
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
            if (LockedOnEnemyTarget != null) { return; }

            // Adjust look direction based on input device
            float adjustedCameraSpeed = isDeviceMouse ? cameraSpeed : cameraSpeed * controllerSpeedMultiplier;

            // Usual Camera Rotation based on input
            lookDirection.y = invertVerticalAxis ? -lookDirection.y : lookDirection.y;
            lookDirection.x = invertHorizontalAxis ? -lookDirection.x : lookDirection.x;

            // Smoothing
            if (smoothCameraRotation) {
                lookDirection.x = Mathf.Lerp(0, lookDirection.x, Time.deltaTime * cameraSmoothingFactor);
                lookDirection.y = Mathf.Lerp(0, lookDirection.y, Time.deltaTime * cameraSmoothingFactor);
            }

            // Adjust the Angles scaled by speed and time
            _currentXAngle += lookDirection.y * adjustedCameraSpeed * Time.deltaTime;
            _currentYAngle += lookDirection.x * adjustedCameraSpeed * Time.deltaTime;

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
