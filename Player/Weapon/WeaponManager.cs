using System.Collections.Generic;
using System.Linq;
using Player.Animation.MotionWarp;
using Sensor;
using ShaderControl;
using Sirenix.OdinInspector;
using UI;
using UnityEngine;

namespace Player.Weapon {
    public class WeaponManager : MonoBehaviour {
        [Header("Animation Overrides")]
        [SerializeField] AnimationClip emptyExecuteClip;
        [SerializeField] AnimationClip emptyAttackClip;
        [SerializeField] AnimationClip emptyAttackClip2;
        
        [Header("User Interface")]
        [SerializeField] RadialSelection radialSelection;
        
        [Title("Weapons")]
        [SerializeField] List<WeaponSO> weapons; 
        public DissolveControl CurrentSpawnedWeaponDissolver { get; set; }
        public PlayerMeeleWeaponSensor CurrentWeaponSensor { get; set; }
        WeaponSO _selectedWeapon;

        [SerializeField] int startSelectedIndex = 1;
        int _selectedIndex;
        
        /* @ Explanation
         *
         * The attack index is used to determine which attack we should perform next
         * Works for Light, Heavy, Finisher, etc.
         * So if we do 1 Light attack at Index 0, We perform index 0 Light Attack
         * Then the index is increased to 1, and we perform the next attack at index 1
         * If we would do the Heavy attack next, we would do the Heavy Index 1 attack
         */
        int _attackIndex;

        void Start() {
            // Ask SaveManager for the Weapons in our Inventory (additionally to our Katana, that is always there)
            var saveManager = Game.Save.SaveManager.Instance;
            var savedWeapons = saveManager.LoadWeapons();
            weapons.AddRange(savedWeapons);
            
            // Which weapon do we start with?
            radialSelection.InitializeRadialParts(weapons, _selectedIndex);
        }
        public void AddWeapon(WeaponSO newWeapon) {
            // Only add the weapon if we don't have it already
            if (weapons.Contains(newWeapon)) { return; }
            
            weapons.Add(newWeapon);
            radialSelection.ClearRadialParts();
            radialSelection.InitializeRadialParts(weapons, _selectedIndex);
        }
        public void ResetAttackIndex() {
            _attackIndex = 0;
        }
        public bool HasSelectedNewWeapon(int selectedIndex) {
            return _selectedIndex != selectedIndex;
        }
        public void SetSelectedWeapon(int newSelectedIndex) {
            // If the weapon is the one we had equipped before, return
            if (_selectedIndex == newSelectedIndex) { return; }
            
            // Set the new weapon
            _selectedIndex = newSelectedIndex;
            _selectedWeapon = weapons[_selectedIndex];
        }
        
        /// <returns>Normal: The selected weapon if its null means initial Equip: StartWeapon set by index</returns>
        public WeaponSO GetSelectedWeapon() {
            if (_selectedWeapon == null) {
                _selectedIndex = startSelectedIndex;
                return weapons[startSelectedIndex];
            }
            return _selectedWeapon;
        }
        
        void ReplaceClipFromOverrideController(AnimationClip oldClip, AnimationClip newClip) {
            var selectedWeapon = GetSelectedWeapon();
            var overrideController = selectedWeapon.animatorOverrideController;
            
            // Get the current overrides
            var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
            overrideController.GetOverrides(overrides);

            // Find and replace the specific override
            for (int i = 0; i < overrides.Count; i++) {
                if (overrides[i].Key == oldClip) {
                    overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(overrides[i].Key, newClip);
                    break;
                }
            }

            // Apply the modified overrides
            overrideController.ApplyOverrides(overrides);
        }

        public void ReplaceFinisherFromOverrideController(AnimationClip finisherClip) {
            ReplaceClipFromOverrideController(emptyExecuteClip, finisherClip);
        }

        public void ReplaceAttackFromOverrideController(AnimationClip attackClip, bool useFirstAttackClip) {
            ReplaceClipFromOverrideController(useFirstAttackClip 
                ? emptyAttackClip 
                : emptyAttackClip2, 
                attackClip);
        }
        // TODO:
        public void IncreaseAttackIndex() {
            if(GetSelectedWeapon().finisherData.WarpCount - 1 > _attackIndex) {
                _attackIndex++;
            } else {
                _attackIndex = 0;
            }
        }
        public WarpAnimation GetCurrentFinisher() {
            return GetSelectedWeapon().finisherData.GetFinisherWarpAnimation(_attackIndex);
        }
        public AnimationEffect GetCurrentLightAttack() {
            return GetSelectedWeapon().lightAttack.GetAttackData(_attackIndex);
        }
        public AnimationEffect GetCurrentHeavyAttack() {
            return GetSelectedWeapon().heavyAttack.GetAttackData(_attackIndex);
        }
        public WeaponSO[] GetAllWeapons() {
            return weapons.ToArray();
        }
        /// <summary> Identify the selected weapon by name</summary>
        public string GetSelectedWeaponName() {
            return GetSelectedWeapon().weaponName;
        }
    }
}