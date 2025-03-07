using UnityEngine;

namespace Sensor {
    public class SpherecastSensor {
        public float CastLength = 1.0f;
        public float Radius = 0.5f;
        public LayerMask Layermask = 255; // Default to everything
        
        Vector3 _origin = Vector3.zero;
        readonly Transform _target;

        public enum CastDirection { Forward, Backward, Right, Left, Up, Down }
        CastDirection _castDirection;
        
        RaycastHit _hitInfo;
        
        public SpherecastSensor(Transform playerTransform) {
            _target = playerTransform;
        }
        
        public void SetCastDirection(CastDirection direction) => _castDirection = direction;
        
        public void SetCastOrigin(Vector3 pos) => _origin = _target.InverseTransformPoint(pos);

        public void Cast() {
            Vector3 worldOrigin = _target.TransformPoint(_origin);
            Vector3 worldDirection = GetCastDirection();
            
            Physics.SphereCast(worldOrigin, Radius, worldDirection, out _hitInfo, CastLength, Layermask,
                QueryTriggerInteraction.Ignore);
            
            Debug.DrawRay(worldOrigin, worldDirection * CastLength, Color.red);
        }

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
        public RaycastHit GetHit() => _hitInfo;
    }
}