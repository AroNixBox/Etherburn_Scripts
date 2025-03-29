using System;
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
        [SerializeField] [Range(1f, 50f)] float cameraSmoothingFactor = 25f;
        [SerializeField] float controllerSpeedMultiplier = 25f;
        [SerializeField] [Range(5f, 20f)] float lockOnSmoothSpeed = 10f;
        [SerializeField] bool debug;
        
        public Entity LockedOnEnemyTarget { get; private set; }

        // Target and current camera angles
        float _targetXAngle;
        float _targetYAngle;
        float _currentXAngle;
        float _currentYAngle;
        
        // For smoother input handling
        Vector2 _lookDirection;
        Vector2 _smoothedLookDirection;
        Vector2 _lookVelocity;
        
        bool _isDeviceMouse;
        bool _isLooking;
        
        // Cached references
        Transform _transform;
        float _detectionRadius;

        // Input handling
        InputReader _input;
        VisionTargetQuery<Entity> _visionEnemyWarpTargetQuery;

        #endregion
        
        public Vector3 GetUpDirection() => _transform.up;
        public Vector3 GetFacingDirection() => _transform.forward;

        void Awake() {
            _transform = transform;
            
            // Initialize angles from current rotation
            _currentXAngle = _transform.eulerAngles.x;
            _currentYAngle = _transform.eulerAngles.y;
            _targetXAngle = _currentXAngle;
            _targetYAngle = _currentYAngle;
            
            // Normalize angles for proper interpolation
            if (_currentXAngle > 180) _currentXAngle -= 360;
            if (_targetXAngle > 180) _targetXAngle -= 360;
        }
        
        void Start() {
            _input = references.input;
            
            // Subscribe to input events
            _input.LockOnTarget += ToggleLockOnTarget;
            _input.Look += ChangeLookValues;
            _input.IsLooking += ToggleLook;
            
            // Setup vision query for targeting
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

        void ToggleLook(bool isLooking) {
            _isLooking = isLooking;
            if (_isLooking) return;
            
            // Reset on cancel
            _lookDirection = Vector2.zero;
            _smoothedLookDirection = Vector2.zero;
            _lookVelocity = Vector2.zero;
            _isDeviceMouse = false;
        }

        void ToggleLockOnTarget() {
            if (LockedOnEnemyTarget != null) {
                // Remove lock-on visual
                uiOnScreenFocus.RemoveTarget();
                
                // Disable target
                LockedOnEnemyTarget = null;
                
                // Preserve current rotation as target to prevent jumps
                _targetXAngle = _currentXAngle;
                _targetYAngle = _currentYAngle;
                
                // Ensure angles are properly normalized
                if (_targetXAngle > 180) _targetXAngle -= 360;
                _targetXAngle = Mathf.Clamp(_targetXAngle, -upperVerticalLimit, lowerVerticalLimit);
            } else {
                // Find new target
                var entityManager = EntityManager.Instance;
                if (entityManager == null) {
                    Debug.LogError("Entity Manager is not in the Scene!", transform);
                    return;
                }
            
                var allEnemies = entityManager.GetEntitiesOfType(lockOnEntityType, out _); 
                var allEntitiesInVisionCone = _visionEnemyWarpTargetQuery.GetAllTargetsInVisionConeSorted(allEnemies);
                LockedOnEnemyTarget = allEntitiesInVisionCone.FirstOrDefault(entity => entity.EntityType == lockOnEntityType);
                
                // Set lock-on visual if target found
                if (LockedOnEnemyTarget == null) return;
                
                uiOnScreenFocus.SetTarget(LockedOnEnemyTarget.TryGetComponent(out EnemyBodyParts bodyParts)
                    ? bodyParts.head
                    : LockedOnEnemyTarget.transform);
            }
        }
        
        public bool IsLockedOnTarget() => LockedOnEnemyTarget != null;

        void FixedUpdate() {
            // 1. Smooth input values first
            SmoothInput();
            
            // 2. Calculate target angles based on mode
            UpdateTargetAngles();
            
            // 3. Smoothly interpolate to target angles
            InterpolateToTargetRotation();
        }

        void SmoothInput() {
            // Apply smoother input filtering with SmoothDamp
            _smoothedLookDirection = Vector2.SmoothDamp(
                _smoothedLookDirection,
                _lookDirection,
                ref _lookVelocity,
                1.0f / cameraSmoothingFactor);
        }

        void UpdateTargetAngles() {
            if (_isLooking && LockedOnEnemyTarget == null) {
                // Free camera mode
                UpdateFreeCameraAngles();
            }
            
            if (LockedOnEnemyTarget != null) {
                // Check if target is still valid
                if (IsTooFarAwayFromTarget()) {
                    ToggleLockOnTarget();
                    return;
                }
                
                // Lock-on mode
                UpdateLockOnAngles();
            }
        }

        void UpdateFreeCameraAngles() {
            // Apply different speeds based on input device
            float adjustedCameraSpeed = _isDeviceMouse ? cameraSpeed : cameraSpeed * controllerSpeedMultiplier;

            // Apply inversion settings
            Vector2 finalLookDirection = _smoothedLookDirection;
            finalLookDirection.y = invertVerticalAxis ? -finalLookDirection.y : finalLookDirection.y;
            finalLookDirection.x = invertHorizontalAxis ? -finalLookDirection.x : finalLookDirection.x;

            // Update target angles for interpolation
            _targetXAngle += finalLookDirection.y * adjustedCameraSpeed * Time.deltaTime;
            _targetYAngle += finalLookDirection.x * adjustedCameraSpeed * Time.deltaTime;

            // Apply vertical limits
            _targetXAngle = Mathf.Clamp(_targetXAngle, -upperVerticalLimit, lowerVerticalLimit);
        }

        void UpdateLockOnAngles() {
            // Calculate direction to target
            Vector3 directionToTarget = LockedOnEnemyTarget.transform.position - _transform.position;
            Quaternion lookRotation = Quaternion.LookRotation(directionToTarget);
            
            // Extract Euler angles
            Vector3 targetEulerAngles = lookRotation.eulerAngles;
            
            // Normalize X angle to -180 to 180 range
            if (targetEulerAngles.x > 180) targetEulerAngles.x -= 360;
            
            // Apply vertical limits
            targetEulerAngles.x = Mathf.Clamp(targetEulerAngles.x, -upperVerticalLimit, lowerVerticalLimit);
            
            // Update target angles
            _targetXAngle = targetEulerAngles.x;
            _targetYAngle = targetEulerAngles.y;
        }

        void InterpolateToTargetRotation() {
            // Select appropriate smoothing speed based on mode
            float smoothingSpeed = LockedOnEnemyTarget != null ? lockOnSmoothSpeed : cameraSmoothingFactor;
            
            // Calculate smooth factor for this frame
            float smoothFactor = 1.0f - Mathf.Exp(-smoothingSpeed * Time.deltaTime);
            
            // Apply exponential smoothing for more natural camera movement
            _currentXAngle = Mathf.Lerp(_currentXAngle, _targetXAngle, smoothFactor);
            _currentYAngle = Mathf.Lerp(_currentYAngle, _targetYAngle, smoothFactor);
            
            // Apply rotation
            _transform.rotation = Quaternion.Euler(_currentXAngle, _currentYAngle, 0);
        }
        
        bool IsTooFarAwayFromTarget() {
            return (LockedOnEnemyTarget.transform.position - headHeight.position).sqrMagnitude > 
                   _detectionRadius * _detectionRadius;
        }

        void OnDestroy() {
            _visionEnemyWarpTargetQuery?.Dispose();
            
            if (_input != null) {
                _input.LockOnTarget -= ToggleLockOnTarget;
                _input.Look -= ChangeLookValues;
                _input.IsLooking -= ToggleLook;
            }
        }
    }
}