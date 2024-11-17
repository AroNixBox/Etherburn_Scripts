using Sirenix.OdinInspector;
using Unity.Behavior;
using UnityEngine;

namespace Behavior {
    public class BehaviorAgentBootstrapper : MonoBehaviour {
        [SerializeField] BehaviorGraphAgent behaviorGraphAgent;
        [Title("Attributes")]
        [RequireInterface(typeof(IRequireAttributeEventChannel))]
        [SerializeField] MonoBehaviour[] healthRelatedChannels;
        [SerializeField] string healthChannelBbvName = "HealthChanged";
        
        [Title("Collision")]
        [RequireInterface(typeof(IRequireEntityColliderInteractionChannel))]
        [SerializeField] MonoBehaviour[] collisionRelatedChannels;
        [SerializeField] string entityCollisionChannelBbvName = "AgressionAreaChannel";

        void Start() {
            if(behaviorGraphAgent == null) {
                Debug.LogError("Behavior Agent is not set in the inspector");
                return;
            }

            AssignHealthChannel();
            AssignEntityCollisionChannel();
        }

        void AssignEntityCollisionChannel() {
            if (!behaviorGraphAgent.BlackboardReference.GetVariableValue(entityCollisionChannelBbvName, out EntityColliderInteractionChannel entityColliderInteractionChannel)) {
                Debug.LogError($"Blackboard variable: {entityCollisionChannelBbvName} could not be set, the variable name is incorrect or the variable does not exist in the blackboard");
                return;
            }

            foreach (var component in collisionRelatedChannels) {
                if (component is IRequireEntityColliderInteractionChannel channel) {
                    channel.AssignEventChannel(entityColliderInteractionChannel);
                }
                else {
                    Debug.LogError($"Component: {component.name} does not implement the IRequireEntityColliderInteractionChannel interface");
                }
            }
        }

        void AssignHealthChannel() {
            if (!behaviorGraphAgent.BlackboardReference.GetVariableValue(healthChannelBbvName, out EnergyValueChanged energyValueChanged)) {
                Debug.LogError($"Blackboard variable: {healthChannelBbvName} could not be set, the variable name is incorrect or the variable does not exist in the blackboard");
                return;
            }

            foreach (var component in healthRelatedChannels) {
                if (component is IRequireAttributeEventChannel channel) {
                    channel.InitializeEnergyChannel(energyValueChanged);
                }
            }
        }
    }
}
