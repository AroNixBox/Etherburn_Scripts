using Player.Animation.MotionWarp;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Player.Weapon {
    [CreateAssetMenu(fileName = "Weapon", menuName = "Player/Weapon")]
    public class WeaponSO : ScriptableObject {
        [Title("User Interface")]
        public string weaponName;
        [TextArea(2, 4)] public string weaponDescription;
        public Sprite weaponSprite;
        public GameObject weaponPrefab;
        public Vector3 weaponOffset;
        public Vector3 weaponRotation;
        
        [Title("Stats")]
        public float dodgeStaminaCost = 10f;
        
        [Title("Animation")]
        [InfoBox("Don't forget to set the WeaponPositionData in the References" +
                 "for Equip, UnEquip and Hit Detection, also add e.g. Weapon & Scripts to the Equipped Weapon for Input tracking")]
        public AnimatorOverrideController animatorOverrideController;
        [Tooltip("The speed multiplier for the ground locomotion animations, e.g. Katana Ground Locomotion was too slow, thats how to speed the animation up or slow it down")]
        [Range(0.7f, 2f)] public float groundLocomotionSpeedMultiplier = 1;

        [Header("Attack Data")]
        [Title("Light Attack")]
        [HideLabel] public AttackData lightAttack;
        
        [Title("Heavy Attack")]
        [HideLabel] public AttackData heavyAttack;
        
        [Title("Finisher")]
        [HideLabel] public WarpData finisherData;
    }
}