using Interfaces.Attribute;
using UnityEngine;

namespace Sensor {
    public class PlayerMeeleWeaponSensor : BaseMeeleWeaponHitSensor {
        IEnergy _ultimateAttribute;
        bool _isUltimate;
        float _ultAttributeAmount;
        
        protected override void Awake() {
            base.Awake();
            SetColliderEnabled(false);
        }
        // Overload for InitializeSensor() to include Ultimate Attribute for Player
        public void InitializeSensor(float damageAmount, bool isUltimate, float ultAttributeAmount, IEnergy ultimateAttribute) {
            base.InitializeSensor(damageAmount);
            
            _isUltimate = isUltimate;
            _ultAttributeAmount = ultAttributeAmount;
            _ultimateAttribute = ultimateAttribute;
        }
        
        protected override void ApplyHit(Collider other, IHealth health) {
            base.ApplyHit(other, health);
            

            if (_ultimateAttribute == null) {
                Debug.LogError("InitializeSensor() was not called, will not gain Ultimate Attribute");
                return;
            }
            // Method is called by Light/ Heavy Attack & Ultimate Attacks gains Ultimate Attribute, Ultimate Ability consumes it
            switch (_isUltimate) {
                case true:
                    _ultimateAttribute.Decrease(_ultAttributeAmount);
                    break;
                case false:
                    _ultimateAttribute.Increase(_ultAttributeAmount);
                    break;
            }
        }
    }
}