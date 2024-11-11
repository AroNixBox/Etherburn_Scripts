using System;
using Unity.Behavior.GraphFramework;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/NPC combat state changed")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(name: "NPC combat state changed", message: "[NPCCombatState] has changed", category: "Events", id: "7422214a94822c51fa9185dedf0d04cf")]
public partial class NpcCombatStateChanged : EventChannelBase
{
    public delegate void NpcCombatStateChangedEventHandler(NPCCombatState NPCCombatState);
    public event NpcCombatStateChangedEventHandler Event; 

    public void SendEventMessage(NPCCombatState NPCCombatState)
    {
        Event?.Invoke(NPCCombatState);
    }

    public override void SendEventMessage(BlackboardVariable[] messageData)
    {
        BlackboardVariable<NPCCombatState> NPCCombatStateBlackboardVariable = messageData[0] as BlackboardVariable<NPCCombatState>;
        var NPCCombatState = NPCCombatStateBlackboardVariable != null ? NPCCombatStateBlackboardVariable.Value : default(NPCCombatState);

        Event?.Invoke(NPCCombatState);
    }

    public override Delegate CreateEventHandler(BlackboardVariable[] vars, System.Action callback)
    {
        NpcCombatStateChangedEventHandler del = (NPCCombatState) =>
        {
            BlackboardVariable<NPCCombatState> var0 = vars[0] as BlackboardVariable<NPCCombatState>;
            if(var0 != null)
                var0.Value = NPCCombatState;

            callback();
        };
        return del;
    }

    public override void RegisterListener(Delegate del)
    {
        Event += del as NpcCombatStateChangedEventHandler;
    }

    public override void UnregisterListener(Delegate del)
    {
        Event -= del as NpcCombatStateChangedEventHandler;
    }
}

