using System;
using Behavior;
using Behavior.Events.Interfaces;
using Interfaces.Attribute;
using UnityEngine;
using UnityEngine.Events;

namespace Attribute {
    public class EnergyBase : MonoBehaviour, IEnergy, IRequireAttributeEventChannel {
        [SerializeField] protected float maxEnergy = 100;
        [SerializeField] UnityEvent onEnergyHitZero;
        protected float CurrentEnergy;
        
        EnergyValueChanged _energyValueChanged;

        public virtual void Awake() {
            CurrentEnergy = maxEnergy;
        }

        public void InitializeEnergyChannel(EnergyValueChanged energyValueChannel, ref Action allChannelsInitialized) {
            _energyValueChanged = energyValueChannel;
            
            allChannelsInitialized += () => _energyValueChanged.SendEventMessage(CurrentEnergy, null);
        }
        public virtual void Increase(float amount) {
            CurrentEnergy += amount;
            CurrentEnergy = Mathf.Clamp(CurrentEnergy, 0, maxEnergy);
            
            // TODO: IncreasePosition can be used to determine the position from where the effect comes, should be directional vector
            Vector3? increaseDirection = null;
            _energyValueChanged?.SendEventMessage(CurrentEnergy, increaseDirection);
        }

        public virtual void Decrease(float amount, Vector3? hitPosition = null) {
            CurrentEnergy -= amount;
            CurrentEnergy = Mathf.Clamp(CurrentEnergy, 0, maxEnergy);
            
            _energyValueChanged?.SendEventMessage(CurrentEnergy, hitPosition);
            
            if (CurrentEnergy <= 0) {
                onEnergyHitZero.Invoke();
            }
        }

        public bool HasEnough(float requiredAmount) {
            return CurrentEnergy >= requiredAmount;
        }
    }
}
