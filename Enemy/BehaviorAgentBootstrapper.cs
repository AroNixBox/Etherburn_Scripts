using Behavior.Events.Interfaces;
using Sirenix.OdinInspector;
using Unity.Behavior;
using UnityEngine;
using Action = System.Action;

namespace Enemy {
    public class BehaviorAgentBootstrapper : MonoBehaviour {
        [SerializeField] BehaviorGraphAgent behaviorGraphAgent;
        [Title("Attributes")]
        [RequireInterface(typeof(IRequireAttributeEventChannel))]
        [SerializeField] MonoBehaviour[] healthRelatedChannels;
        [SerializeField] string healthChannelBbvName = "HealthChanged";
        public event Action AllHealthChannelsInitialized = delegate { };
        
        [Title("Collision")]
        [RequireInterface(typeof(IRequireEntityColliderInteractionChannel))]
        [SerializeField] MonoBehaviour[] collisionRelatedChannels;
        [SerializeField] string entityCollisionChannelBbvName = "AgressionAreaChannel";
        
        [Title("Get Executed")]
        [RequireInterface(typeof(IRequireNPCStateChannel))]
        [SerializeField] MonoBehaviour[] npcStateRelatedChannels;
        [SerializeField] string npcStateChannelBbvName = "NPCStateChanged";

        void Start() {
            if(behaviorGraphAgent == null) {
                Debug.LogError("Behavior Agent is not set in the inspector");
                return;
            }
        
            AssignNpcStateChannel();
            AssignHealthChannel();
            AssignEntityCollisionChannel();
        }

        void AssignNpcStateChannel() {
            if (!behaviorGraphAgent.BlackboardReference.GetVariableValue(npcStateChannelBbvName, out NpcStateChanged npcStateChanged)) {
                Debug.LogError($"Blackboard variable: {npcStateChannelBbvName} could not be set, the variable name is incorrect or the variable does not exist in the blackboard");
                return;
            }

            foreach (var component in npcStateRelatedChannels) {
                if (!component.isActiveAndEnabled) { continue; }
                
                if (component is IRequireNPCStateChannel channel) {
                    channel.AssignEventChannel(npcStateChanged);
                }
                else {
                    Debug.LogError($"Component: {component.name} does not implement the IRequireNPCStateChannel interface");
                }
            }
        }

        void AssignEntityCollisionChannel() {
            if (!behaviorGraphAgent.BlackboardReference.GetVariableValue(entityCollisionChannelBbvName, out EntityColliderInteractionChannel entityColliderInteractionChannel)) {
                Debug.LogError($"Blackboard variable: {entityCollisionChannelBbvName} could not be set, the variable name is incorrect or the variable does not exist in the blackboard");
                return;
            }

            foreach (var component in collisionRelatedChannels) {
                // If Entry is empty
                if (component == null) {
                    Debug.LogError("CollisionRelatedChannel is set in the inspector, but the Related channel is not set");
                }
                
                if (!component.isActiveAndEnabled) { continue; }
                
                if (component is IRequireEntityColliderInteractionChannel channel) {
                    var assignedChannel = channel.AssignEventChannel(entityColliderInteractionChannel);
                    
                    // Check if the assigned channel is the same as the one we passed in
                    if (assignedChannel == entityColliderInteractionChannel) {
                        // Noop, we assigned the channel.
                    }
                    else if(assignedChannel == null) {
                        Debug.LogError($"The channel was not assigned, an Error Occurred");
                    }
                    else {
                        // The channel was assigned by someone else, we should use the already assigned one for our behavior graph
                        if(!behaviorGraphAgent.BlackboardReference.SetVariableValue(entityCollisionChannelBbvName, assignedChannel)) {
                            Debug.LogError($"Could not find the variable: {entityCollisionChannelBbvName} in the blackboard");
                        }
                    }
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
                if (!component.isActiveAndEnabled) { continue; }
                
                if (component is IRequireAttributeEventChannel channel) {
                    channel.InitializeEnergyChannel(energyValueChanged, ref AllHealthChannelsInitialized);
                }
            }
            AllHealthChannelsInitialized?.Invoke();
        }
    }
}
