using UnityEngine;

namespace Extensions {
    public static class Vector2Extensions {
        public static Vector2 GetLocalDirectionToPoint(Transform modelRoot, Vector3 hitPosition) {
            // Calculate the direction from the model root to the hit position
            Vector3 modelToHit = hitPosition - modelRoot.position;
    
            // Convert this direction to the local space of the model root
            Vector3 localHitDirection = modelRoot.InverseTransformDirection(modelToHit);
    
            // We only care about the X and Z components for horizontal direction
            return new Vector2(localHitDirection.x, localHitDirection.z).normalized;
        }
        public static Vector2 GetClosestDirectionVectorToDirection(Vector2 direction, Vector2[] directions) {
            if (directions.Length == 0) {
                Debug.LogError("No directions passed in");
                return Vector2.zero;
            }
            
            Vector2 bestMatch = directions[0];
            float bestDotProduct = Vector2.Dot(direction, directions[0]);

            foreach (Vector2 dir in directions) {
                float dotProduct = Vector2.Dot(direction, dir); // Measure similarity
                if (dotProduct > bestDotProduct) {
                    bestDotProduct = dotProduct;
                    bestMatch = dir;
                }
            }

            return bestMatch; // Snap to the closest direction
        }
    }
}