using UnityEngine;

namespace Sensor {
    public class RaycastSensor {
        public float CastLength = 1.0f;
        public LayerMask Layermask = 255; // Default to everything
        
        Vector3 _origin = Vector3.zero;
        readonly Transform _target;

        /// <summary>
        /// Used to cast to different directions
        /// </summary>
        public enum CastDirection { Forward, Backward, Right, Left, Up, Down }
        CastDirection _castDirection;
        
        RaycastHit _hitInfo;
        
        public RaycastSensor(Transform playerTransform) {
            _target = playerTransform;
        }
        
        public void SetCastDirection(CastDirection direction) => _castDirection = direction;
        
        // Convert the world position to local position
        public void SetCastOrigin(Vector3 pos) => _origin = _target.InverseTransformPoint(pos);

        /// <summary>
        /// Cast the Sensor
        /// </summary>
        public void Cast() {
            // position of the origin in world space
            Vector3 worldOrigin = _target.TransformPoint(_origin);
            
            // The direction we want to shoot the ray
            Vector3 worldDirection = GetCastDirection();
            
            // Cast the ray
            Physics.Raycast(worldOrigin, worldDirection, out _hitInfo, CastLength, Layermask,
                QueryTriggerInteraction.Ignore);
            
            Debug.DrawRay(worldOrigin, worldDirection * CastLength, Color.red);
        }

        /// <returns>CastDirection of the Raycast</returns>
        Vector3 GetCastDirection() {
            return _castDirection switch {
                CastDirection.Forward => _target.forward,
                CastDirection.Backward => -_target.forward,
                CastDirection.Right => _target.right,
                CastDirection.Left => -_target.right,
                CastDirection.Up => _target.up,
                CastDirection.Down => -_target.up,
                _ => Vector3.zero
            };
        }
        public bool HasDetectedHit() => _hitInfo.collider != null;
        public float GetDistance() => _hitInfo.distance;
        public Vector3 GetNormal() => _hitInfo.normal;
        public Vector3 GetPosition() => _hitInfo.point;
        public Collider GetCollider() => _hitInfo.collider;
        public Transform GetTransform() => _hitInfo.transform;
    }
}
