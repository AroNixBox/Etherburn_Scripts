using UnityEngine;

namespace Extensions {
    public static class MathHelper {
        public static float LerpToZero(float value, float lerpSpeed) {
            return Mathf.Approximately(value, 0) 
                ? 0 
                : Mathf.Lerp(value, 0, lerpSpeed * Time.deltaTime);
        }
        public static Vector2 LerpToZero(Vector2 value, float lerpSpeed) {
            return Mathf.Approximately(value.magnitude, 0) 
                ? Vector2.zero 
                : Vector2.Lerp(value, Vector2.zero, lerpSpeed * Time.deltaTime);
        }
    }
}