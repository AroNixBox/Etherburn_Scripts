using System;
using System.Collections.Generic;
using System.Linq;
using Drawing;
using UnityEngine;

namespace Extensions {
    public class VisionTargetQuery<T> : IDisposable where T : MonoBehaviour {
        readonly Transform _head;
        readonly Transform[] _rayCheckOrigins;
        readonly Transform _customForwardOrigin;
        readonly float _detectionRadius;
        readonly float _visionConeAngle;
        readonly bool _debug;

        readonly Collider[] _colliders;
        
        RedrawScope _redrawScope;
        VisionTargetQuery(Transform head, Transform[] rayCheckOrigins, Transform customForwardOrigin, int maxTargets, float detectionRadius, float visionConeAngle, bool debug) {
            _head = head;
            _rayCheckOrigins = rayCheckOrigins;
            _customForwardOrigin = customForwardOrigin;
            _detectionRadius = detectionRadius;
            _visionConeAngle = visionConeAngle;
            
            _colliders = new Collider[maxTargets];
            
#if UNITY_EDITOR
            _debug = debug;
            _redrawScope = new RedrawScope();
#endif
        }
        
        public List<T> GetAllTargetsInVisionConeSorted() {
            if(_head == null || _rayCheckOrigins.Length == 0 || _colliders.Length == 0 || _detectionRadius == 0f || _visionConeAngle == 0f) {
                Debug.LogError("VisionTargetQuery was not built for GetAllTargetsInVisionConeSorted().");
                return null;
            }
            
            var targetsFound = Physics.OverlapSphereNonAlloc(_head.position, _detectionRadius, _colliders);

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
            if (_debug) {
                _redrawScope.Rewind();
            
                DrawDetectionRadius();
                DrawVisionCone();
                if (validTargets.Count > 0) {
                    DrawLineToTarget(validTargets.First().transform);
                }
            }
#endif
            
            return validTargets;
        }
        
        public List<T> GetAllTargetsInVisionConeSorted(List<T> targetsToQuery) {
            if(_head == null || _rayCheckOrigins.Length == 0 || _detectionRadius == 0f || _visionConeAngle == 0f) {
                Debug.LogError("VisionTargetQuery was not built for GetAllTargetsInVisionConeSorted(List<T> targetsToQuery).");
                return null;
            }
            
            var validTargets = targetsToQuery
                .Where(item => 
                    item.TryGetComponent(out Collider collider) &&
                               IsInDetectionRadius(collider.transform.position) &&
                               IsInVisionCone(collider.ClosestPoint(_head.position)) &&
                               _rayCheckOrigins.Any(origin => {
                                   Vector3 direction = collider.ClosestPoint(origin.position) - origin.position;
                                   return Physics.Raycast(origin.position, direction, out var hit, _detectionRadius) 
                                          && hit.collider == item.GetComponent<Collider>();
                               })
                )
                .OrderBy(item => (_head.position - item.transform.position).sqrMagnitude)
                .Select(item => item)
                .ToList();
            
#if UNITY_EDITOR
            if (_debug) {
                _redrawScope.Rewind();
            
                DrawDetectionRadius();
                DrawVisionCone();
                if (validTargets.Count > 0) {
                    DrawLineToTarget(validTargets.First().transform);
                }
            }
#endif
            
            return validTargets;
        }
        
        public List<T> GetAllTargetsInRangeWithOutLineOfSightSorted(List<T> targetsToQuery) {
            if(_head == null || _detectionRadius == 0f) {
                Debug.LogError("VisionTargetQuery was not built for GetAllTargetsInRangeWithOutLineOfSight(List<T> targetsToQuery).");
                return null;
            }
            
            var validTargets = targetsToQuery
                .Where(item => IsInDetectionRadius(item.transform.position))
                .OrderBy(item => (_head.position - item.transform.position).sqrMagnitude)
                .Select(item => item)
                .ToList();
            
#if UNITY_EDITOR
            if (_debug) {
                _redrawScope.Rewind();
            
                DrawDetectionRadius();
                if (validTargets.Count > 0) {
                    DrawLineToTarget(validTargets.First().transform);
                }
            }
#endif
            
            return validTargets;
        }
        
        public T GetTargetInRangeAndVisionCone(T targetToQuery) {
            if(_head == null || _visionConeAngle == 0f || _detectionRadius == 0f || _rayCheckOrigins.Length == 0) {
                Debug.LogError("VisionTargetQuery was not built for GetTargetInVisionCone(T targetToQuery).");
                return null;
            }
            
            var isValidTarget = false;
            
            if (targetToQuery.TryGetComponent(out Collider collider)) {
                if (IsInDetectionRadius(collider.transform.position) &&
                    IsInVisionCone(collider.ClosestPoint(_head.position))) {
                    isValidTarget = _rayCheckOrigins.Any(origin => {
                        Vector3 direction = collider.ClosestPoint(origin.position) - origin.position;
                        return Physics.Raycast(origin.position, direction, out var hit, _detectionRadius)
                                        && hit.collider == collider;
                    });
                }
            }

        #if UNITY_EDITOR
            if (_debug) {
                _redrawScope.Rewind();
                DrawDetectionRadius();
                DrawVisionCone();
                if (isValidTarget) {
                    DrawLineToTarget(targetToQuery.transform);
                }
            }
        #endif

            return isValidTarget ? targetToQuery : null;
        }
        
        public T GetTargetInRangeWithOutVisionCone(T targetToQuery) {
            if(_head == null || _detectionRadius == 0f) {
                Debug.LogError("VisionTargetQuery was not built for  GetTargetInRange(T targetToQuery).");
                return null;
            }
            
            var isValidTarget = false;
            
            if (targetToQuery.TryGetComponent(out Collider collider)) {
                if (IsInDetectionRadius(collider.transform.position)) {
                    isValidTarget = true;
                }
            }

#if UNITY_EDITOR
            if (_debug) {
                _redrawScope.Rewind();
                DrawDetectionRadius();
                DrawVisionCone();
                if (isValidTarget) {
                    DrawLineToTarget(targetToQuery.transform);
                }
            }
#endif
            return isValidTarget ? targetToQuery : null;
        }
        bool IsInVisionCone(Vector3 targetPosition) {
            Vector3 directionToTarget = (targetPosition - _head.position).normalized;
            Vector3 forwardDirection = _customForwardOrigin != null ? _customForwardOrigin.forward : _head.forward;            
            float angle = Vector3.Angle(forwardDirection, directionToTarget);
            return angle <= _visionConeAngle / 2f;
        }
        
        bool IsInDetectionRadius(Vector3 targetPosition) {
            return (_head.position - targetPosition).sqrMagnitude <= _detectionRadius * _detectionRadius;
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
            if (_debug) {
                bool isDefault = _redrawScope.Equals(default(RedrawScope));
                if (isDefault) { return; }
            
                _redrawScope.Dispose();
            }
#endif
        }
        public class Builder {
            Transform _head;
            Transform[] _rayCheckOrigins;
            Transform _customForwardOrigin;
            int _maxTargets;
            float _detectionRadius;
            float _visionConeAngle;
            bool _debug;

            public Builder SetHead(Transform head) {
                _head = head;
                return this;
            }

            public Builder SetRayCheckOrigins(Transform[] rayCheckOrigins) {
                _rayCheckOrigins = rayCheckOrigins;
                return this;
            }

            public Builder SetMaxTargets(int maxTargets) {
                _maxTargets = maxTargets;
                return this;
            }

            public Builder SetDetectionRadius(float detectionRadius) {
                _detectionRadius = detectionRadius;
                return this;
            }

            public Builder SetVisionConeAngle(float visionConeAngle) {
                _visionConeAngle = visionConeAngle;
                return this;
            }

            public Builder SetDebug(bool debug) {
                _debug = debug;
                return this;
            }
            
            public Builder SetCustomForwardOrigin(Transform customForwardOrigin) {
                _customForwardOrigin = customForwardOrigin;
                return this;
            }

            public VisionTargetQuery<U> Build<U>() where U : MonoBehaviour {
                return new VisionTargetQuery<U>(_head, _rayCheckOrigins, _customForwardOrigin, _maxTargets, _detectionRadius, _visionConeAngle, _debug);
            }
        }
    }
}