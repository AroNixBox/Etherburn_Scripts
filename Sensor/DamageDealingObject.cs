using UnityEngine;

namespace Sensor {
    public class DamageDealingObject : BaseMeeleWeaponHitSensor {
        [SerializeField] float damageAmount;
        
        protected override void Awake() {
            base.Awake();
            InitializeSensor(damageAmount);
        }
    }
}