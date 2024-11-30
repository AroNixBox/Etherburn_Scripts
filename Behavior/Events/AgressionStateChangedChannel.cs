using System;
using Unity.Behavior.GraphFramework;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/Agression State Changed Channel")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(name: "Agression State Changed Channel", message: "[AgressionState] Changed Channel", category: "Events", id: "09746e4958c3861d46b6e621fff1f256")]
public partial class AgressionStateChangedChannel : EventChannelBase
{
    public delegate void AgressionStateChangedChannelEventHandler(NPCAggressionState AgressionState);
    public event AgressionStateChangedChannelEventHandler Event; 

    public void SendEventMessage(NPCAggressionState AgressionState)
    {
        Event?.Invoke(AgressionState);
    }

    public override void SendEventMessage(BlackboardVariable[] messageData)
    {
        BlackboardVariable<NPCAggressionState> AgressionStateBlackboardVariable = messageData[0] as BlackboardVariable<NPCAggressionState>;
        var AgressionState = AgressionStateBlackboardVariable != null ? AgressionStateBlackboardVariable.Value : default(NPCAggressionState);

        Event?.Invoke(AgressionState);
    }

    public override Delegate CreateEventHandler(BlackboardVariable[] vars, System.Action callback)
    {
        AgressionStateChangedChannelEventHandler del = (AgressionState) =>
        {
            BlackboardVariable<NPCAggressionState> var0 = vars[0] as BlackboardVariable<NPCAggressionState>;
            if(var0 != null)
                var0.Value = AgressionState;
            
            callback();
        };
        return del;
    }

    public override void RegisterListener(Delegate del)
    {
        Event += del as AgressionStateChangedChannelEventHandler;
    }

    public override void UnregisterListener(Delegate del)
    {
        Event -= del as AgressionStateChangedChannelEventHandler;
    }
}

