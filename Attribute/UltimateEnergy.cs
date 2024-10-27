using UnityEngine;

namespace Attribute {
    public class UltimateEnergy : EnergyBase {
        [SerializeField] float startEnergy;
        void Awake() {
            CurrentEnergy = startEnergy;
        }
    }
}