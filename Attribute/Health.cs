using Interfaces.Attribute;
using UnityEngine;

namespace Attribute {
    public class Health : EnergyBase, IHealth {
        public Vector3 HitPosition { get; private set; }
        public bool HasTakenDamage { get; set; }
        public bool HasDied { get; set; }

        public override void Decrease(float amount, Vector3? hitPosition = null) {
            base.Decrease(amount, hitPosition);
            
            // Get the normalized hit direction
            if (hitPosition.HasValue) {
                // Get the direction
                HitPosition = hitPosition.Value;
            }
            
            // Flags for State Machines
            if (CurrentEnergy <= 0) {
                HasDied = true;
            }
            
            HasTakenDamage = true;
        }
    }
}