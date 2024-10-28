using Extensions.FSM;
using Player.Weapon;


namespace Player.States {
    /* @ Exit Condition
     * Every Equip Animation needs to fire the EquipEnd Event
     */
    public class WeaponEquipState : IState {
        readonly References _references;
        readonly Animation.AnimationController _animationController;
        readonly WeaponManager _weaponManager;
        readonly UI.RadialSelection _radialSelection;
        WeaponPositionData _weaponPositionData;
        
        public WeaponEquipState(References references) {
            _references = references;
            _animationController = _references.animationController;
            _weaponManager = _references.weaponManager;
            _radialSelection = _references.radialSelection;
        }

        public void OnEnter() {
            EquipNewWeapon();

            if (_weaponPositionData.hasHolster) {
                _references.GrabHolster += GrabHolster;
                _references.ReleaseHolster += ReleaseHolster;
            }
            
            _references.GrabWeapon += GrabWeapon;
        }
        void EquipNewWeapon() {
            // Read the Radial Selection selected Weapon Index
            var selectedWeaponIndex = _radialSelection.GetSelectedIndex();
            // Pass it to the Weapon Manager, it will set the Weapon
            _weaponManager.SetSelectedWeapon(selectedWeaponIndex);
            
            // Get the new selected Weapon
            var selectedWeapon = _weaponManager.GetSelectedWeapon();
            
            // Set the Anim Controller
            _animationController.OverrideAnimatorController(selectedWeapon.animatorOverrideController);
                
            // Set the Weapon Position Data based on the selected Weapon
            _weaponManager.SetWeaponPositionData(selectedWeapon);
            // Get the Weapon Position Data
            _weaponPositionData = _weaponManager.WeaponPositionData;
            PlayAnimation();
        }
        void PlayAnimation() {
            _animationController.ChangeAnimationState(Animation.AnimationParameters.EquipWeapon, 
                Animation.AnimationParameters.GetAnimationDuration(Animation.AnimationParameters.EquipWeapon), 
                0);
        }

        #region Anim Event Methods from Equip Animation
        // Always Called
        void GrabWeapon() {
            _weaponPositionData.equippedWeapon.gameObject.SetActive(true);

            // If the Weapon has an Animation where the weapon is pulled out with the secondary hand 
            // And then put into the main hand we need to set the weapon from the other hand false
            // If it is a base Equip with only a grab, we need to set the weapon on the body false
            if (_weaponPositionData.twoArmEquip) {
                _weaponPositionData.weaponInOtherHand.gameObject.SetActive(false);
            } else {
                _weaponPositionData.weaponOnBody.gameObject.SetActive(false);
            }
        }
        // Only Called if the HasHolster Checkbox is checked
        void GrabHolster() {
            // Weapon
            _weaponPositionData.weaponOnBody.gameObject.SetActive(false);
            _weaponPositionData.weaponInOtherHand.gameObject.SetActive(true);
            
            // Holster
            _weaponPositionData.weaponHolsterOnBody.gameObject.SetActive(false);
            _weaponPositionData.weaponHolsterInHand.gameObject.SetActive(true);
        }
        void ReleaseHolster() {
            // Weapon
            _weaponPositionData.weaponOnBody.gameObject.SetActive(false);
            _weaponPositionData.weaponInOtherHand.gameObject.SetActive(false);
            
            // Holster
            _weaponPositionData.weaponHolsterInHand.gameObject.SetActive(false);
            _weaponPositionData.weaponHolsterOnBody.gameObject.SetActive(true);
        }

        #endregion
        
        public void Tick() { }

        public void FixedTick() { }

        public void OnExit() {
            // Reset the Param
            _references.EquipEnded = false;
            
            _references.GrabHolster -= GrabHolster;
            _references.ReleaseHolster -= ReleaseHolster;
            _references.GrabWeapon -= GrabWeapon;
        }
    }
}