using System.Linq;
using UnityEngine;

namespace Extensions {
    public class VisionTargetQuery<T> where T : MonoBehaviour {
        readonly Transform _headPosition;
        readonly Transform[] _rayCheckOrigins;
        readonly float _detectionRadius;
        readonly float _visionConeAngle;

        readonly Collider[] _colliders;

        T _currentTarget;
        
        public VisionTargetQuery(Transform headPosition, Transform[] rayCheckOrigins, int maxTargets, float detectionRadius, float visionConeAngle) {
            _headPosition = headPosition;
            _rayCheckOrigins = rayCheckOrigins;
            _detectionRadius = detectionRadius;
            _visionConeAngle = visionConeAngle;
            
            _colliders = new Collider[maxTargets];
        }
        
        public T GetNearestTargetInVisionCone() {
            int targetsFound = Physics.OverlapSphereNonAlloc(_headPosition.position, _detectionRadius, _colliders);

            if (targetsFound == 0) {
                return null;
            }

            var potentialTargets = _colliders
                .Take(targetsFound)
                .Select(col => col.GetComponent<T>())
                .Where(targetProvider => targetProvider != null && IsInVisionCone(targetProvider.transform.position))
                .OrderBy(targetProvider => (_headPosition.position - targetProvider.transform.position).sqrMagnitude);

            foreach (var target in potentialTargets) {
                foreach (var origin in _rayCheckOrigins) {
                    if (Physics.Raycast(origin.position, target.transform.position - origin.position, out var hit, _detectionRadius)) {
                        if (hit.collider.TryGetComponent(out T enemyWarpTargetProvider)) {
                            _currentTarget = enemyWarpTargetProvider;
                            return _currentTarget;
                        }
                    }
                }
            }
            
            _currentTarget = null;
            return null;
        }

        // TODO: Make Vision Cone also check horizontal angle and let it start from the head position, currently it is drawn from the head, but fired from the root.
        bool IsInVisionCone(Vector3 targetPosition) {
            Vector3 directionToTarget = (targetPosition - _headPosition.position).normalized;
            float angle = Vector3.Angle(_headPosition.forward, directionToTarget);
            return angle <= _visionConeAngle / 2f;
        }

        public void DrawDetectionRadius() {
            Gizmos.color = new Color(1f, 0.7f, 0.07f, 0.75f); // Orange
            Gizmos.DrawWireSphere(_headPosition.position, _detectionRadius);
        }

        public void DrawVisionCone() {
            Vector3 origin = _headPosition.position;
            Vector3 direction = _headPosition.forward;

            float radius = Mathf.Tan(_visionConeAngle * 0.5f * Mathf.Deg2Rad) * _detectionRadius;
            Vector3 farCenter = origin + direction * _detectionRadius;

            int segments = 20;
            Quaternion rotation = Quaternion.LookRotation(direction);

            Vector3 previousPoint = origin;

            for (int i = 0; i <= segments; i++) {
                float progress = (float)i / segments;
                float currentRadian = progress * 2 * Mathf.PI;
                Vector3 currentPoint = new Vector3(Mathf.Cos(currentRadian) * radius, Mathf.Sin(currentRadian) * radius, _detectionRadius);
                currentPoint = rotation * currentPoint + origin;

                Gizmos.color = new Color(1f, 0f, 0.01f, 0.54f);
                Gizmos.DrawLine(origin, currentPoint);
                Gizmos.DrawLine(farCenter, currentPoint);

                if (i > 0) {
                    Gizmos.DrawLine(previousPoint, currentPoint);
                }

                Gizmos.color = new Color(1f, 0.16f, 0f, 0.75f);
                if (i % 4 == 0) {
                    Gizmos.DrawLine(origin, currentPoint);
                }

                previousPoint = currentPoint;
            }

            // Middle cone line
            Gizmos.color = new Color(1f, 0f, 0.07f);
            Gizmos.DrawLine(origin, farCenter);
        }
        
        /// <param name="nonCalculatedTarget">If theres a target which is not calculated by this class, e.g. a cached target by the camera</param>
        public void DrawLineToTarget(Transform nonCalculatedTarget = null) {
            if (nonCalculatedTarget == null && _currentTarget == null) { return; }
            var target = nonCalculatedTarget != null ? nonCalculatedTarget : _currentTarget.transform;
            
            // Draw rays to the selected target
            Gizmos.color = Color.green;
            Gizmos.DrawLine(target.transform.position, _headPosition.position);
            Gizmos.DrawSphere(_headPosition.transform.position, 0.2f);
        }
    }
}