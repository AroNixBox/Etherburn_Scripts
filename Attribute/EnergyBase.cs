using System;
using Behavior;
using Interfaces.Attribute;
using UnityEngine;

namespace Attribute {
    public class EnergyBase : MonoBehaviour, IEnergy, IRequireAttributeEventChannel {
        [SerializeField] protected float maxEnergy = 100;
        protected float CurrentEnergy;
        
        EnergyValueChanged _energyValueChanged;
        
        public event Action<float> OnAttributeValueIncreased;
        public event Action<float> OnAttributeValueDecresed;

        void Start() {
            CurrentEnergy = maxEnergy;
            
            OnAttributeValueIncreased?.Invoke(CurrentEnergy);
        }
        public void InitializeEnergyChannel(EnergyValueChanged energyValueChannel) {
            _energyValueChanged = energyValueChannel;
        }
        public virtual void Increase(float amount) {
            CurrentEnergy += amount;
            CurrentEnergy = Mathf.Clamp(CurrentEnergy, 0, maxEnergy);
            OnAttributeValueIncreased?.Invoke(CurrentEnergy);
            
            _energyValueChanged?.SendEventMessage(CurrentEnergy);
        }

        public virtual void Decrease(float amount, Vector3? hitPosition = null) {
            CurrentEnergy -= amount;
            CurrentEnergy = Mathf.Clamp(CurrentEnergy, 0, maxEnergy);
            OnAttributeValueDecresed?.Invoke(CurrentEnergy);
            
            _energyValueChanged?.SendEventMessage(CurrentEnergy);
        }

        public bool HasEnough(float requiredAmount) {
            return CurrentEnergy >= requiredAmount;
        }

    }
}
