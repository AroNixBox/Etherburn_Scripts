using Interfaces.Attribute;
using UnityEngine;

namespace Attribute {
    public class Health : EnergyBase, IHealth {
        public Vector3 HitPosition { get; private set; }
        public bool HasTakenDamage { get; set; }
        public bool HasDied { get; set; }
        public bool IsInvincible { get; set; }
        
        public override void Decrease(float amount, Vector3? hitPosition = null) {
            if(IsInvincible) { return; }
            
            base.Decrease(amount, hitPosition);
            
            // Get the normalized hit direction
            if (hitPosition.HasValue) {
                // Get the direction
                HitPosition = hitPosition.Value;
            }
            HasTakenDamage = true;
            
            // Flags for State Machines
            if (!(CurrentEnergy <= 0)) return;
            HasDied = true;

            if (EntityManager.Instance == null) { return; }
            if(!TryGetComponent(out Entity entity)) { return; }
            
            EntityManager.Instance.UnregisterEntity(entity);
        }
    }
}