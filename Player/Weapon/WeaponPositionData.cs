using System;
using Sensor;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Player.Weapon {
    [Serializable]
    public class WeaponPositionData {
        public WeaponSO associatedWeapon;
        
        [Header("Holster")]
        public bool hasHolster;
        [ShowIf("hasHolster")]
        public Transform weaponHolsterOnBody;

        [ShowIf("hasHolster")]
        public Transform weaponHolsterInHand;

        [Header("Weapon")] 
        [Tooltip("If the Equip Animation puts the weapon in the other hand," +
                 "before the weapon is equipped and then puts it into the correct hand")]
        public bool twoArmEquip;
        public Transform equippedWeapon;
        public Transform weaponOnBody;
        [ShowIf("twoArmEquip")]
        public Transform weaponInOtherHand;
        
        [Title("Hit Detection")]
        public bool usesHitSensor;
        [ShowIf("usesHitSensor")]
        public FirstTriggerHitSensor hitDetectionSensor;
    }
}