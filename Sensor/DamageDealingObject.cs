using UnityEngine;

namespace Sensor {
    public class DamageDealingObject : BaseMeeleWeaponHitSensor {
        [SerializeField] float damageAmount;
        
        protected void Awake() {
            InitializeSensor(damageAmount);
        }
    }
}