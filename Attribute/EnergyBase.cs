using System;
using Interfaces.Attribute;
using Unity.Behavior;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Attribute {
    public class EnergyBase : MonoBehaviour, IEnergy {
        [SerializeField] protected float maxEnergy = 100;
        protected float CurrentEnergy;
        
        EnergyValueChanged _energyValueChangedInstance;
        [SerializeField] bool hasBehaviorAgent;
        [SerializeField, ShowIf("hasBehaviorAgent"), Required] BehaviorGraphAgent behaviorAgent;
        public event Action<float> OnAttributeMaxValueSet;
        public event Action<float> OnAttributeValueIncreased;
        public event Action<float> OnAttributeValueDecresed;

        void Start() {
            CurrentEnergy = maxEnergy;
            
            OnAttributeMaxValueSet?.Invoke(maxEnergy);
            OnAttributeValueIncreased?.Invoke(CurrentEnergy);
            
            if(hasBehaviorAgent) {
                if(behaviorAgent == null) {
                    Debug.LogError("Behavior Agent is not set in the inspector");
                    return;
                }
                
                _energyValueChangedInstance = ScriptableObject.CreateInstance<EnergyValueChanged>();

                if (!behaviorAgent.BlackboardReference.SetVariableValue(GetBlackboardVariableName(), _energyValueChangedInstance)) {
                    Debug.LogError($"Blackboard variable: {GetBlackboardVariableName()} could not be set, the variable name is incorrect or the variable does not exist in the blackboard");
                }
            }
            
            _energyValueChangedInstance?.SendEventMessage(CurrentEnergy);
        }

        protected virtual string GetBlackboardVariableName() {
            return string.Empty;
        }
        public virtual void Increase(float amount) {
            CurrentEnergy += amount;
            CurrentEnergy = Mathf.Clamp(CurrentEnergy, 0, maxEnergy);
            OnAttributeValueIncreased?.Invoke(CurrentEnergy);
            
            _energyValueChangedInstance?.SendEventMessage(CurrentEnergy);
        }

        public virtual void Decrease(float amount, Vector3? hitPosition = null) {
            CurrentEnergy -= amount;
            CurrentEnergy = Mathf.Clamp(CurrentEnergy, 0, maxEnergy);
            OnAttributeValueDecresed?.Invoke(CurrentEnergy);
            
            _energyValueChangedInstance?.SendEventMessage(CurrentEnergy);
        }

        public bool HasEnough(float requiredAmount) {
            return CurrentEnergy >= requiredAmount;
        }
        
        public void ZeroCurrentEnergy() {
            CurrentEnergy = 0;
            OnAttributeValueDecresed?.Invoke(CurrentEnergy);
            
            _energyValueChangedInstance?.SendEventMessage(CurrentEnergy);
        }
    }
}
