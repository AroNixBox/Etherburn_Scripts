using UnityEngine;

namespace Enemy {
    public class Target {
        // The pivot point around which the target rotates
        readonly Transform _pivot;
        // The reference object that defines the rotational relationship
        readonly Transform _anchor;
        // The actual target we set
        readonly Transform _targetObject;
        readonly float _distanceFromParentOrigin;

        public Target(Transform targetObject, Transform anchor, Transform pivot, float distanceFromParentOrigin) {
            _pivot = pivot;
            _anchor = anchor;
            _targetObject = targetObject;
            _distanceFromParentOrigin = distanceFromParentOrigin;
        }

        public Transform GetTransform() => _targetObject;

        public void RotateAroundParent() {
            Vector3 parentToPlayer = _anchor.position - _pivot.position;
            _targetObject.position = _pivot.position + parentToPlayer.normalized * _distanceFromParentOrigin;
        }

        public void LookAtParent() {
            Vector3 direction = (_pivot.position - _targetObject.position).normalized;
            _targetObject.rotation = Quaternion.LookRotation(direction);
        }
    }
}