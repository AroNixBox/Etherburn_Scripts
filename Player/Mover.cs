using System.Linq;
using Enemy;
using Player.Animation.MotionWarp;
using Player.Cam;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Player {
    [RequireComponent(typeof(References))]
    public class Mover : MonoBehaviour {
        [Header("Rotation")]
        [Tooltip("ModelRoot is what we rotate to face the camera direction, to not rotate the camera with the player")]
        [SerializeField, Required] Transform modelRoot;
        [SerializeField] float turnSpeed = 200f;
        public bool CanApplyModelRotationInCameraForward { get; set; }
        
        [Header("Physics")]
        [SerializeField] float groundedCastLength = 0.25f;
        [SerializeField, Range(0f, 1f)] float airResistance = 0.05f;
        [SerializeField, Range(-1, -100f)] float gravity = -20f;
        [SerializeField] float maxSlopeAngle = 45f;
        
        [Header("Warping")]
        [Tooltip("The GameObject that has the Animatior directly on it, child from the Player and Child from the ModelRoot")]
        [SerializeField] Transform rootAnimatedGameObject;
        public RootMotionWarpingController RootMotionWarpingControllerController { get; private set; }
        
        References _references;
        Sensor.RaycastSensor _sensor;
        Rigidbody _rb;
        OrbitalController _orbitalController;
        Transform _transform;
        CapsuleCollider _collider;
        
        bool _isGrounded;
        float _currentYRotation;
        const float FallOffAngle = 90f;
        
        Vector3 _velocity;
        
        void Awake() {
            _references = GetComponent<References>();
            _rb = GetComponent<Rigidbody>();
            _transform = transform;
            _collider = GetComponent<CapsuleCollider>();
            
            RootMotionWarpingControllerController = new RootMotionWarpingController(rootAnimatedGameObject, modelRoot, _references);
        }
        void Start() {
            _orbitalController = _references.orbitalController;
            _currentYRotation = modelRoot.localEulerAngles.y;
        }
        public void SetKinematic(bool isKinematic) {
            _rb.isKinematic = isKinematic;
        }
        
        // All OnAnimatorMove is done in this class, redirected from {Player.Animation.EventForward.cs}
        public void AnimatorMove(Vector3 deltaPosition, Quaternion deltaRotation) {
            if (RootMotionWarpingControllerController.IsWarpTargetAssigned) {
                ApplyWarpedMotion(deltaPosition);
            } else {
                ApplyNormalMotion(deltaPosition, deltaRotation);
            }
        }

        void ApplyNormalMotion(Vector3 deltaPosition, Quaternion deltaRotation) {
            const int maxIterations = 3; // Limit the number of collision adjustments
            var iteration = 0;

            while (iteration < maxIterations) {
                // Perform a SweepTest to detect potential collisions
                if (_rb.SweepTest(deltaPosition.normalized, out RaycastHit hit, deltaPosition.magnitude, QueryTriggerInteraction.Ignore)) {
                    // Adjust the deltaPosition to prevent penetration
                    deltaPosition = Vector3.ProjectOnPlane(deltaPosition, hit.normal);

                    // Reduce the magnitude of deltaPosition slightly to avoid edge-case clipping
                    deltaPosition *= 0.95f;
                } else {
                    // No collision detected, break out of the loop
                    break;
                }

                iteration++;
            }

            // If the iterations exceeded the maximum, block the movement entirely
            if (iteration >= maxIterations) {
                deltaPosition = Vector3.zero;
            }

            // Move the Rigidbody using the adjusted delta position
            _rb.MovePosition(_rb.position + deltaPosition);

            // Apply root motion rotation change to this transform
            RotateModelRoot(deltaRotation);
        }




        void ApplyWarpedMotion(Vector3 deltaPosition) {
            // Warping
            var posRot = RootMotionWarpingControllerController.GetTotalWarpedAndRootMotion(deltaPosition,
                _references.animationController.GetCurrentAnimationState(0));
            // Doesnt work with _rb.MovePosition.
            _transform.position = posRot.Item1;
            modelRoot.rotation = posRot.Item2;
        }

        void RotateModelRoot(Quaternion deltaRotation) {
            // This flag is set by a state individually and determines if the model should rotate towards the camera forward
            if (CanApplyModelRotationInCameraForward) {
                // Only rotate towards target if there's significant movement
                var cameraForward = _orbitalController.GetFacingDirection();
                // Camera forward projected on xz plane
                Vector3 desiredForward = Vector3.ProjectOnPlane(cameraForward, Vector3.up).normalized;
                // Angle difference between current forward and desired forward
                float angleDifference = Vector3.SignedAngle(modelRoot.forward, desiredForward, Vector3.up);

                // Calculate smooth rotation step
                float step = Mathf.Sign(angleDifference) * // Get Rotation direction
                             Mathf.InverseLerp(0f, FallOffAngle, Mathf.Abs(angleDifference)) * // Smooth the rotation
                             Time.deltaTime * turnSpeed;

                // Update rotation
                _currentYRotation += Mathf.Clamp(step, -Mathf.Abs(angleDifference), Mathf.Abs(angleDifference));
                
                // Apply the rotation
                modelRoot.localRotation = Quaternion.Euler(0, _currentYRotation, 0);
            }
            
            // Normal Rotation
            modelRoot.rotation *= deltaRotation;
        }

        #region Physics
        
        /// <summary>
        /// This Tells if we are grounded + big offset, dont use from falling state to grounded state
        /// </summary>
        /// <returns></returns>
        public bool IsGrounded() => _isGrounded;

        // TODO: This doesnt work with current falling, because the slope is still a ground
        // So even if the rb pulls us down with gravity, we stay on the slope
        public bool IsGroundTooSteep() {
            // Get the normal of the hit
            Vector3 normal = _sensor.GetNormal();
            // If there is no normal, return out early
            if (normal == Vector3.zero) { return false; }
            // Calculate the angle between the normal and the up vector
            float angle = Vector3.Angle(Vector3.up, normal);
            // If the angle is greater than the max slope angle, return true
            return angle > maxSlopeAngle;
        }
        
        void FixedUpdate() {
            CheckGrounded();
        }
        
        void CheckGrounded() {
            RecalibrateSensor();
            _sensor.Cast();
            
            _isGrounded = _sensor.HasDetectedHit();
        }

        void RecalibrateSensor() {
            // If the sensor is null, create a new one
            _sensor ??= new Sensor.RaycastSensor(_transform);
            
            // buffer to prevent clipping
            const float buffer = 0.1f;
            _sensor.SetCastOrigin(_transform.position + Vector3.up * buffer);            
            _sensor.SetCastDirection(Sensor.RaycastSensor.CastDirection.Down);
            
            // TODO Only cast on the ground layer
            _sensor.Layermask = Physics.AllLayers;
            
            // How far do we want to cast the ray?
            // So how much have we adjusted the collider height and incorporate the step height ratio
            _sensor.CastLength = groundedCastLength;
        }
        public void SetGravity(bool enabled) {
            _rb.useGravity = enabled;
        }
        
        public void HandleFalling() {
            _velocity = _rb.linearVelocity;
            
            // Apply gravity (corrected for negative gravity value)
            _velocity += Vector3.up * (gravity * Time.fixedDeltaTime);

            // Apply air resistance (optional, can be removed for faster falling)
            _velocity = Vector3.MoveTowards(_velocity, Vector3.zero, airResistance * Time.fixedDeltaTime);

            // Limit maximum fall speed
            float maxFallSpeed = Mathf.Abs(gravity * 2f); // Use absolute value for max speed
            _velocity = Vector3.ClampMagnitude(_velocity, maxFallSpeed);

            // Apply movement
            _rb.linearVelocity = _velocity;
        }

        // TODO:
        public void HandleSliding() {
            // Get the ground normal
            Vector3 groundNormal = _sensor.GetNormal();

            // Calculate the slide direction (down the slope)
            Vector3 slideDirection = (Vector3.ProjectOnPlane(_transform.up, groundNormal).normalized - _transform.up).normalized;

            // Calculate the slope angle
            float slopeAngle = Vector3.Angle(groundNormal, Vector3.up);

            // Calculate slide acceleration based on slope angle
            float slideAcceleration = gravity * Mathf.Sin(slopeAngle * Mathf.Deg2Rad);

            // Apply slide acceleration
            _velocity += slideDirection * (slideAcceleration * Time.fixedDeltaTime);

            // Project velocity onto the slope
            _velocity = Vector3.ProjectOnPlane(_velocity, groundNormal);

            // Remove any upward velocity relative to world up
            float upwardVelocity = Vector3.Dot(_velocity, Vector3.up);
            if (upwardVelocity > 0) {
                _velocity -= Vector3.up * upwardVelocity;
            }

            // Calculate max slide speed based on slope angle
            float maxSlideSpeed = gravity * 2f * Mathf.Sin(slopeAngle * Mathf.Deg2Rad);
            maxSlideSpeed = Mathf.Clamp(maxSlideSpeed, gravity * 0.5f, gravity * 2f);

            // Limit maximum slide speed
            _velocity = Vector3.ClampMagnitude(_velocity, maxSlideSpeed);

            // Apply movement
            _rb.MovePosition(_rb.position + _velocity * Time.fixedDeltaTime);

            // Update rigidbody velocity
            _rb.linearVelocity = _velocity;
        }
        #endregion
        
    }
}