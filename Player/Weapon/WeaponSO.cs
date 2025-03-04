using Player.Animation.MotionWarp;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Player.Weapon {
    [CreateAssetMenu(fileName = "Weapon", menuName = "Player/Weapon")]
    public class WeaponSO : ScriptableObject {
        [Title("User Interface")]
        public string weaponName;
        [Tooltip("The speed multiplier for the ground locomotion animations, e.g. Katana Ground Locomotion was too slow, thats how to speed the animation up or slow it down")]
        [Range(0.7f, 2f)] public float groundLocomotionSpeedMultiplier = 1;
        [TextArea(2, 4)] public string weaponDescription;
        public Sprite weaponSprite;
        public GameObject weaponPrefab;
        public Vector3 weaponOffset;
        public Vector3 weaponRotation;
        
        [Title("Animation")]
        [InfoBox("Don't forget to set the WeaponPositionData in the References" +
                 "for Equip, UnEquip and Hit Detection, also add e.g. Weapon & Scripts to the Equipped Weapon for Input tracking")]
        public AnimatorOverrideController animatorOverrideController;

        [Title("Attack Data")]
        [HideLabel] public AttackData lightAttack;
        [HideLabel] public AttackData heavyAttack;
        
        [Title("Finisher")]
        [HideLabel] public WarpData finisherData;
    }
}