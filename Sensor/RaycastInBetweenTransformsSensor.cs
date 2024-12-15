using System;
using System.Collections.Generic;
using Drawing;
using UnityEngine;
using UnityEngine.Events;

namespace Sensor {
    public class RaycastInBetweenTransformsSensor : MonoBehaviour {
        // Material, Collision Point, Collision Normal
        public UnityEvent<Transform, PhysicsMaterial, Vector3, Vector3> collisionEvent = new();
        [SerializeField] Vector3 castSize = new Vector3(0.05f, 0.05f, 0.05f);
        [SerializeField] protected LayerMask excludedLayers = 1 << 2; // Default to Ignore Raycast layer
        [SerializeField] Transform rayStart;
        [SerializeField] Transform rayEnd;
        
        Vector3 _lastRayStartPosition;
        Vector3 _lastRayEndPosition;
        bool _isFirstCast = true;

        readonly HashSet<Collider> _hitObjects = new();
        bool _cast;

        // FixedUpdate is not precise enough for this sensor
        void FixedUpdate() {
            if (_cast) {
                DetectHitsInSensor();
            }
        }

        public void CastForObjects(bool active) {
            _cast = active;

            if (!active) {
                _hitObjects.Clear(); 
                
                _lastRayEndPosition = Vector3.zero;
                _lastRayStartPosition = Vector3.zero;
                _isFirstCast = true;
            }
        }

        protected virtual List<RaycastHit> DetectHitsInSensor() {
            var currentRayStart = rayStart.position;
            var currentRayEnd = rayEnd.position;

            // Wenn es der erste Cast ist, speichere die Startpositionen
            if (_isFirstCast) {
                _lastRayStartPosition = currentRayStart;
                _lastRayEndPosition = currentRayEnd;
                _isFirstCast = false;
            }

            var deltaStart = currentRayStart - _lastRayStartPosition;
            
            var totalCasts = Mathf.CeilToInt(deltaStart.magnitude / 0.1f);

            var hitList = new List<RaycastHit>();

            for (int i = 0; i <= totalCasts; i++) {
                // Interpolate positions
                var lerpStart = Vector3.Lerp(_lastRayStartPosition, currentRayStart, (float)i / totalCasts);
                var lerpEnd = Vector3.Lerp(_lastRayEndPosition, currentRayEnd, (float)i / totalCasts);

                // BoxCast the interpolated positions
                var direction = lerpEnd - lerpStart;
                var maxDistance = direction.magnitude;
                var halfExtents = new Vector3(0.05f, 0.05f, 0.05f);
                var orientation = Quaternion.LookRotation(direction.normalized);
                

                RaycastHit[] hitResults = new RaycastHit[10];
                int hitCount = Physics.BoxCastNonAlloc(lerpStart, halfExtents, direction.normalized, hitResults, orientation, maxDistance, ~excludedLayers, QueryTriggerInteraction.Ignore);

                for (var j = 0; j < hitCount; j++) {
                    var hitInfo = hitResults[j];
                    if (CollisionWithSelfOrParent(hitInfo.collider)) continue;

                    if (!_hitObjects.Add(hitInfo.collider)) continue;

                    hitList.Add(hitInfo);

                    // Debug und Events
                    collisionEvent?.Invoke(hitInfo.transform, hitInfo.collider.sharedMaterial, hitInfo.point, hitInfo.normal);

                    #if UNITY_EDITOR
                    using (Draw.WithDuration(2f)) {
                        Draw.Arrow(hitInfo.point, hitInfo.point + hitInfo.normal, Color.red);
                    }
                    #endif
                }
            }

            // Aktualisiere die letzten Positionen
            _lastRayStartPosition = currentRayStart;
            _lastRayEndPosition = currentRayEnd;

            return hitList;
        }
        
        void OnDrawGizmosSelected() {
            if (rayStart == null || rayEnd == null) return;

            var direction = rayEnd.position - rayStart.position;
            var orientation = Quaternion.LookRotation(direction.normalized);

            Gizmos.color = Color.blue;

            // Draw the starting cube
            Gizmos.matrix = Matrix4x4.TRS(rayStart.position, orientation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, castSize);

            // Draw the ending cube
            Gizmos.matrix = Matrix4x4.TRS(rayEnd.position, orientation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, castSize);

            // Reset the matrix for drawing lines
            Gizmos.matrix = Matrix4x4.identity;

            // Draw lines connecting the corresponding corners of the two cubes
            Vector3[] cubeCorners = {
                new(-0.5f, -0.5f, -0.5f),
                new( 0.5f, -0.5f, -0.5f),
                new(-0.5f,  0.5f, -0.5f),
                new( 0.5f,  0.5f, -0.5f),
                new(-0.5f, -0.5f,  0.5f),
                new( 0.5f, -0.5f,  0.5f),
                new(-0.5f,  0.5f,  0.5f),
                new( 0.5f,  0.5f,  0.5f)
            };

            foreach (var corner in cubeCorners) {
                // Transform corners to world space for both cubes
                Vector3 startCorner = rayStart.position + orientation * Vector3.Scale(corner, castSize);
                Vector3 endCorner = rayEnd.position + orientation * Vector3.Scale(corner, castSize);

                // Draw a line between corresponding corners
                Gizmos.DrawLine(startCorner, endCorner);
            }
        }
        bool CollisionWithSelfOrParent(Collider other) => transform.IsChildOf(other.transform) || other.transform == transform;
    }
}
