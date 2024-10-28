using UnityEngine;

namespace Attributes {
    public class UltimateEnergy : EnergyBase {
        [SerializeField] float startEnergy;
        void Awake() {
            CurrentEnergy = startEnergy;
        }
    }
}