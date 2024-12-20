using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Sensor {
    public class RaycastInBetweenTransformsSensor : MonoBehaviour {
        // Material, Collision Point, Collision Normal
        public UnityEvent<Transform, PhysicsMaterial, Vector3, Vector3> collisionEvent = new();
        [SerializeField] Vector3 castSize = new (0.05f, 0.05f, 0.05f);
        [SerializeField] protected LayerMask excludedLayers = 1 << 2; // Default to Ignore Raycast layer
        [SerializeField] Transform rayStart;
        [SerializeField] Transform rayEnd;
        
        Vector3 _lastRayStartPosition;
        Vector3 _lastRayEndPosition;
        bool _isFirstCast = true;

        readonly HashSet<Collider> _hitObjects = new();
        bool _cast;

        void Start() {
            if(fallbackPositionsBetweenStartAndEnd.Count == 0) {
                GenerateFallbackCollisionTrackingPositions();
                Debug.LogError($"Fallback Positions for {gameObject.name} are not generated, please generate them in the Editor", transform);
            }
        }

        [FoldoutGroup("Fallback Positions")]
        [SerializeField] [ReadOnly] List<Transform> fallbackPositionsBetweenStartAndEnd = new();

        [FoldoutGroup("Fallback Positions")]
        [Button, GUIColor(0.4f, 0.8f, 1f)]
        void GenerateFallbackCollisionTrackingPositions() {
            var direction = rayEnd.position - rayStart.position;
            var distance = direction.magnitude;
            var normalizedDirection = direction.normalized;
            var stepSize = 0.15f;

            int totalSteps = Mathf.FloorToInt(distance / stepSize);

            // Create a parent GameObject for fallback positions
            var fallbackParent = new GameObject("FallbackCollisionDetectionPositions").transform;
            fallbackParent.SetParent(transform);

            for (int i = 1; i <= totalSteps; i++) {
                var fallbackPosition = rayStart.position + normalizedDirection * (i * stepSize);

                var fallbackTransform = new GameObject($"FallbackPosition_{i}").transform;
                fallbackTransform.position = fallbackPosition;

                // Set the fallback position as a child of the fallback parent
                fallbackTransform.SetParent(fallbackParent);

                fallbackPositionsBetweenStartAndEnd.Add(fallbackTransform);
            }
        }

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

            // Initialize the first cast
            if (_isFirstCast) {
                _lastRayStartPosition = currentRayStart;
                _lastRayEndPosition = currentRayEnd;
                _isFirstCast = false;
            }

            // Calculate movement delta
            var deltaStart = currentRayStart - _lastRayStartPosition;
            var deltaEnd = currentRayEnd - _lastRayEndPosition;

            // Adjust step size based on cast size to avoid gaps
            var stepSize = Mathf.Max(castSize.x, castSize.y, castSize.z) * 0.9f; // Ensures overlap
            var totalCastsStart = Mathf.CeilToInt(deltaStart.magnitude / stepSize);
            var totalCastsEnd = Mathf.CeilToInt(deltaEnd.magnitude / stepSize);
            var totalCasts = Mathf.Max(totalCastsStart, totalCastsEnd);

            var hitList = new List<RaycastHit>();

            for (int i = 0; i <= totalCasts; i++) {
                // Interpolate start and end points for the current cast
                var lerpStart = Vector3.Lerp(_lastRayStartPosition, currentRayStart, (float)i / totalCasts);
                var lerpEnd = Vector3.Lerp(_lastRayEndPosition, currentRayEnd, (float)i / totalCasts);

                // Calculate direction and distance
                var direction = lerpEnd - lerpStart;
                var maxDistance = direction.magnitude;
                var orientation = direction.sqrMagnitude > 0 ? Quaternion.LookRotation(direction) : Quaternion.identity;

                // Perform BoxCast
                var hitResults = new RaycastHit[100];
                var hitCount = Physics.BoxCastNonAlloc(
                    lerpStart,
                    castSize,
                    direction.normalized,
                    hitResults,
                    orientation,
                    maxDistance,
                    ~excludedLayers,
                    QueryTriggerInteraction.Ignore
                );

                // Process hits
                for (var j = 0; j < hitCount; j++) {
                    var hitInfo = hitResults[j];

                    if (hitInfo.point == Vector3.zero) {
                        // Calculate the closest point from fallback positions, rayStart, and rayEnd
                        var closestPoint = rayStart.position;
                        var closestDistance = (hitInfo.collider.ClosestPoint(rayStart.position) - rayStart.position).sqrMagnitude;

                        foreach (var fallbackPosition in fallbackPositionsBetweenStartAndEnd) {
                            var distance = (hitInfo.collider.ClosestPoint(fallbackPosition.position) - fallbackPosition.position).sqrMagnitude;
                            if (distance < closestDistance) {
                                closestPoint = fallbackPosition.position;
                                closestDistance = distance;
                            }
                        }

                        var endPointDistance = (hitInfo.collider.ClosestPoint(rayEnd.position) - rayEnd.position).sqrMagnitude;
                        if (endPointDistance < closestDistance) {
                            closestPoint = rayEnd.position;
                        }

                        hitInfo.point = closestPoint;
                    }
                    
                    if (CollisionWithSelfOrParent(hitInfo.collider)) continue;
                    if (!_hitObjects.Add(hitInfo.collider)) continue;

                    hitList.Add(hitInfo);
                    // Debug and collision events
                    collisionEvent?.Invoke(hitInfo.transform, hitInfo.collider.sharedMaterial, hitInfo.point, hitInfo.normal);
                }
            }

            // Update last positions
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
