using Unity.Behavior;
using UnityEngine;

namespace Behavior {
    public class BehaviorAgentBootstrapper : MonoBehaviour {
        [SerializeField] BehaviorGraphAgent behaviorGraphAgent;
        [RequireInterface(typeof(IRequireAttributeEventChannel))]
        [SerializeField] MonoBehaviour[] healthRelatedChannels;
        [SerializeField] string healthChannelBbvName = "HealthChanged";
        EnergyValueChanged _energyValueEventChannelInstance;

        void Start() {
            if(behaviorGraphAgent == null) {
                Debug.LogError("Behavior Agent is not set in the inspector");
                return;
            }
                
            if (!behaviorGraphAgent.BlackboardReference.GetVariableValue(healthChannelBbvName, out _energyValueEventChannelInstance)) {
                Debug.LogError($"Blackboard variable: {healthChannelBbvName} could not be set, the variable name is incorrect or the variable does not exist in the blackboard");
                return;
            }
        
            foreach (var component in healthRelatedChannels) {
                if (component is IRequireAttributeEventChannel channel) {
                    channel.InitializeEnergyChannel(_energyValueEventChannelInstance);
                }
            }
        }
    }
}
