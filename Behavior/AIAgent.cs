using Unity.Behavior;
using UnityEngine;

namespace Behavior {
    public class AIAgent : MonoBehaviour {
        [SerializeField] BehaviorGraphAgent behaviorGraphAgent;
        EnergyValueChanged _energyValueEventChannelInstance;

        void Start() {
            if(behaviorGraphAgent == null) {
                Debug.LogError("Behavior Agent is not set in the inspector");
                return;
            }
                
            if (!behaviorGraphAgent.BlackboardReference.GetVariableValue("Hunger changed", out _energyValueEventChannelInstance)) {
                Debug.LogError("Blackboard variable: HungerAttribute could not be set, the variable name is incorrect or the variable does not exist in the blackboard");
                return;
            }
        
            foreach (var component in GetComponentsInChildren<IEnergyValueChannelModifier>()) {
                component.InitializeEnergyChannel(_energyValueEventChannelInstance);
            }
        }
    }
}
