using System;
using Behavior;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Player {
    public class PlayerBootstrapper : MonoBehaviour {
        [Header("Attributes")]
        [Title("Health")]
        [RequireInterface(typeof(IRequireAttributeEventChannel))]
        [SerializeField] MonoBehaviour[] healthRelatedChannels;
        public event Action AllHealthListenersInitialized = delegate { };
        
        [Title("Stamina")]
        [RequireInterface(typeof(IRequireAttributeEventChannel))]
        [SerializeField] MonoBehaviour[] staminaRelatedChannels;
        public event Action AllStaminaListenersInitialized = delegate { };
        
        [Title("Ultimate Bar")]
        [RequireInterface(typeof(IRequireAttributeEventChannel))]
        [SerializeField] MonoBehaviour[] ultEnergyRelatedChannels;
        public event Action AllUltEnergyListenersInitialized = delegate { };
        void Start() {
            AssignHealthListeners();
            AssignStaminaListeners();
            AssignUltEnergyListeners();
        }

        void AssignUltEnergyListeners() {
            var ultEnergyChannel = ScriptableObject.CreateInstance<EnergyValueChanged>();
            ultEnergyChannel.name = "PlayerUltEnergyChangedChannel";
            
            foreach (var component in ultEnergyRelatedChannels) {
                if (!component.isActiveAndEnabled) {
                    Debug.LogWarning($"Component: {component.name} is not active and enabled, skipping assignment");
                    continue;
                }
                
                if (component is IRequireAttributeEventChannel channel) {
                    channel.InitializeEnergyChannel(ultEnergyChannel, ref AllUltEnergyListenersInitialized);
                }
                else {
                    Debug.LogError($"Component: {component.name} does not implement the IRequireAttributeEventChannel interface");
                }
            }
            
            AllUltEnergyListenersInitialized.Invoke();
        }

        void AssignStaminaListeners() {
            var staminaChannel = ScriptableObject.CreateInstance<EnergyValueChanged>();
            staminaChannel.name = "PlayerStaminaChangedChannel";
            
            foreach (var component in staminaRelatedChannels) {
                if (!component.isActiveAndEnabled) {
                    Debug.LogWarning($"Component: {component.name} is not active and enabled, skipping assignment");
                    continue;
                }
                
                if (component is IRequireAttributeEventChannel channel) {
                    channel.InitializeEnergyChannel(staminaChannel, ref AllStaminaListenersInitialized);
                }
                else {
                    Debug.LogError($"Component: {component.name} does not implement the IRequireAttributeEventChannel interface");
                }
            }
            
            AllStaminaListenersInitialized.Invoke();
        }

        void AssignHealthListeners() {
            var healthChannel = ScriptableObject.CreateInstance<EnergyValueChanged>();
            healthChannel.name = "PlayerHealthChangedChannel";
            
            foreach (var component in healthRelatedChannels) {
                if (!component.isActiveAndEnabled) {
                    Debug.LogWarning($"Component: {component.name} is not active and enabled, skipping assignment");
                    continue;
                }
                
                if (component is IRequireAttributeEventChannel channel) {
                    channel.InitializeEnergyChannel(healthChannel, ref AllHealthListenersInitialized);
                }
                else {
                    Debug.LogError($"Component: {component.name} does not implement the IRequireAttributeEventChannel interface");
                }
            }
            
            AllHealthListenersInitialized.Invoke();
        }
    }
}
