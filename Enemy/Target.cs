using UnityEngine;

namespace Enemy {
    public class Target{
        readonly Transform _parentTransform;
        readonly Transform _playerTransform;
        readonly Transform _targetTransform;
        readonly float _distanceFromParentOrigin;

        public Target(Transform targetTransform, Transform playerTransform, Transform parentTransform, float distanceFromParentOrigin) {
            _parentTransform = parentTransform;
            _playerTransform = playerTransform;
            _targetTransform = targetTransform;
            _distanceFromParentOrigin = distanceFromParentOrigin;
        }

        public Transform GetTransform() => _targetTransform;

        public void RotateAroundParent() {
            Vector3 parentToPlayer = _playerTransform.position - _parentTransform.position;
            _targetTransform.position = _parentTransform.position + parentToPlayer.normalized * _distanceFromParentOrigin;
        }

        public void LookAtParent() {
            Vector3 direction = (_parentTransform.position - _targetTransform.position).normalized;
            _targetTransform.rotation = Quaternion.LookRotation(direction);
        }
    }
}