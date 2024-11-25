using System;
using System.Collections.Generic;
using System.Linq;
using Drawing;
using UnityEngine;

namespace Extensions {
    public class VisionTargetQuery<T> : IDisposable where T : MonoBehaviour {
        readonly Transform _head;
        readonly Transform[] _rayCheckOrigins;
        readonly float _detectionRadius;
        readonly float _visionConeAngle;

        readonly Collider[] _colliders;
        
        RedrawScope _redrawScope;
        public VisionTargetQuery(Transform head, Transform[] rayCheckOrigins, int maxTargets, float detectionRadius, float visionConeAngle) {
            _head = head;
            _rayCheckOrigins = rayCheckOrigins;
            _detectionRadius = detectionRadius;
            _visionConeAngle = visionConeAngle;
            
            _colliders = new Collider[maxTargets];
            
#if UNITY_EDITOR
            _redrawScope = new RedrawScope();
#endif
        }
        
        public List<T> GetAllTargetsInVisionConeSorted() {
            int targetsFound = Physics.OverlapSphereNonAlloc(_head.position, _detectionRadius, _colliders);

            if (targetsFound == 0) {
                return new List<T>();
            }

            var validTargets = _colliders
                .Take(targetsFound)
                .Select(col => (targetProvider: col.GetComponent<T>(), collider: col))
                .Where(item => 
                    item.targetProvider != null &&
                    IsInVisionCone(item.collider.ClosestPoint(_head.position)) &&
                    _rayCheckOrigins.Any(origin => {
                        Vector3 direction = item.collider.ClosestPoint(origin.position) - origin.position;
                        return Physics.Raycast(origin.position, direction, out var hit, _detectionRadius) 
                               && hit.collider == item.collider;
                    })
                )
                .OrderBy(item => (_head.position - item.targetProvider.transform.position).sqrMagnitude)
                .Select(item => item.targetProvider)
                .ToList();
            
#if UNITY_EDITOR
            _redrawScope.Rewind();
            
            DrawDetectionRadius();
            DrawVisionCone();
            if (validTargets.Count > 0) {
                DrawLineToTarget(validTargets.First().transform);
            }
#endif
            
            return validTargets;
        }

        // TODO: Make Vision Cone also check horizontal angle and let it start from the head position, currently it is drawn from the head, but fired from the root.
        bool IsInVisionCone(Vector3 targetPosition) {
            Vector3 directionToTarget = (targetPosition - _head.position).normalized;
            float angle = Vector3.Angle(_head.forward, directionToTarget);
            return angle <= _visionConeAngle / 2f;
        }
        
        #if UNITY_EDITOR
        #region Gizmos
        void DrawDetectionRadius() {
            using var builder = DrawingManager.GetBuilder(_redrawScope);
            builder.WireSphere(_head.position, _detectionRadius, new Color(1f, 0.55f, 0f));
        }

        void DrawVisionCone() {
            using var builder = DrawingManager.GetBuilder(_redrawScope);
            Vector3 origin = _head.position;
            Vector3 direction = _head.forward;

            float radius = Mathf.Tan(_visionConeAngle * 0.5f * Mathf.Deg2Rad) * _detectionRadius;
            Vector3 farCenter = origin + direction * _detectionRadius;

            int segments = 20;
            Quaternion rotation = Quaternion.LookRotation(direction);

            Vector3 previousPoint = Vector3.zero;

            for (int i = 0; i <= segments; i++) {
                float progress = (float)i / segments;
                float currentRadian = progress * 2 * Mathf.PI;

                // Calculate point on the circle (at the end of the vision cone)
                Vector3 currentPoint = new Vector3(
                    Mathf.Cos(currentRadian) * radius,
                    Mathf.Sin(currentRadian) * radius,
                    _detectionRadius
                );

                // Adjust rotation to the direction and shift to world position
                currentPoint = rotation * currentPoint + origin;

                // Draw lines from the tip of the cone to the segments
                builder.Line(origin, currentPoint, Color.red * 0.7f);

                // Draw lines that close the circle
                if (i > 0) {
                    builder.Line(previousPoint, currentPoint, Color.red);
                }

                // Draw lines from the center of the circle to the segments
                if (i % 4 == 0) {
                    builder.Line(farCenter, currentPoint, Color.red * 0.5f);
                }

                previousPoint = currentPoint;
            }

            // Draw circle at the base end of the cone
            builder.Circle(farCenter, _head.forward, radius, Color.red);
        }

        void DrawLineToTarget(Transform target) {
            using var builder = DrawingManager.GetBuilder(_redrawScope);
            builder.Arrow(_head.position, target.position, Color.green);
            builder.SolidBox(_head.position, 0.2f, Color.green);
        }

        #endregion
        #endif
        public void Dispose() {
#if UNITY_EDITOR
            bool isDefault = _redrawScope.Equals(default(RedrawScope));
            if (isDefault) { return; }
            
            _redrawScope.Dispose();
#endif
        }
    }
}