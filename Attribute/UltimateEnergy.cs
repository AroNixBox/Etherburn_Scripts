using UnityEngine;

namespace Attribute {
    public class UltimateEnergy : EnergyBase {
        [SerializeField] float startEnergy;

        public override void Awake() {
            CurrentEnergy = startEnergy;
        }
    }
}