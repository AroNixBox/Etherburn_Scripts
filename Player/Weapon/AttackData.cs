using Attribute;
using Effects.VFX;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Player.Weapon {
    [System.Serializable]
    public class AttackData {
        public AnimationEffect[] attackClips;
        [HideLabel] public AttributeData attributeData;

        public AnimationEffect GetAttackData(int attackIndex) {
            if (attackIndex >= attackClips.Length) {
                Debug.LogError("Attack Index is out of bounds for Attacks");
                return null;
            }

            return attackClips[attackIndex];
        }
    }
}