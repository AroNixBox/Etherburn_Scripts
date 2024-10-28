using Extensions.FSM;
using Player.Weapon;
using UnityEngine;

namespace Player.States {
    /* @ Exit Condition
     * 
     * Every Unequip Animation needs to fire the UnEquipEnd Event
     */
    public class WeaponUnEquipState : IState {
        // Initial References
        readonly References _references;
        readonly WeaponManager _weaponManager;
        readonly Animation.AnimationController _animationController;
        
        // Dynamic Reference
        WeaponPositionData _weaponPositionData;
        public WeaponUnEquipState(References references) {
            _references = references;
            _animationController = _references.animationController;
            _weaponManager = _references.weaponManager;
        }
        public void OnEnter() {
            PlayAnimation();
            _weaponPositionData = _weaponManager.WeaponPositionData;

            if (_weaponPositionData.hasHolster) {
                _references.GrabHolster += GrabHolster;
                _references.ReleaseHolster += ReleaseHolster;
            }
            _references.ReleaseWeapon += ReleaseWeapon;
        }

        void PlayAnimation() {
            _animationController.ChangeAnimationState(Animation.AnimationParameters.UnEquip, 
                Animation.AnimationParameters.GetAnimationDuration(Animation.AnimationParameters.UnEquip), 
                0);
        }
        #region Anim Event Methods from Equip Animation
        
        void GrabHolster() {
            // On Body
            _weaponPositionData.weaponHolsterOnBody.gameObject.SetActive(false);
            
            // In Hand
            _weaponPositionData.weaponHolsterInHand.gameObject.SetActive(true);
        }
        
        void ReleaseHolster() {
            // Weapon
            _weaponPositionData.weaponInOtherHand.gameObject.SetActive(false);
            _weaponPositionData.weaponOnBody.gameObject.SetActive(true);
            
            // Holster
            _weaponPositionData.weaponHolsterInHand.gameObject.SetActive(false);
            _weaponPositionData.weaponHolsterOnBody.gameObject.SetActive(true);
        }
        void ReleaseWeapon() {
            // Disable the Equipped Weapon
            _weaponPositionData.equippedWeapon.gameObject.SetActive(false);
        
            // Either enable the Weapon in the other hand for two arm UnEquip
            // or directly on the body
            if (_weaponPositionData.twoArmEquip) {
                _weaponPositionData.weaponInOtherHand.gameObject.SetActive(true);
            } else {
                _weaponPositionData.weaponOnBody.gameObject.SetActive(true);
            }
        }
        
        #endregion
        public void Tick() { }

        public void FixedTick() { }

        public void OnExit() {
            // Reset the Trigger + Param
            _references.UnEquipEnded = false;
            _weaponManager.ResetWeaponPositionData();
            
            _references.GrabHolster -= GrabHolster;
            _references.ReleaseHolster -= ReleaseHolster;
            _references.ReleaseWeapon -= ReleaseWeapon;
        }
    }
}