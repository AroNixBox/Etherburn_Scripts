using UnityEngine;

namespace Effects {
    public abstract class GenericEffect<T> : ScriptableObject {
        [SerializeField] MaterialEffectPair<T>[] effectData;

        public T GetEffectData(PhysicsMaterial material) {
            if (effectData.Length == 0) {
                Debug.LogError("No effect data found in " + name);
                return default;
            }
            
            foreach (MaterialEffectPair<T> pair in effectData) {
                if (pair.material == material) {
                    return pair.associatedEffects[Random.Range(0, pair.associatedEffects.Length)];
                }
            }
            
            // If no effect is found, return from the first pair
            return effectData[0].associatedEffects[Random.Range(0, effectData[0].associatedEffects.Length)];
        }
    }
}