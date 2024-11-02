using System;
using Unity.Behavior.GraphFramework;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/NPC State Changed")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(name: "NPC State Changed", message: "NPC State has changed to [Value]", category: "Events", id: "296b2c3feac0c09efa0da34cad491797")]
public partial class NpcStateChanged : EventChannelBase
{
    public delegate void NpcStateChangedEventHandler(EnemyState Value);
    public event NpcStateChangedEventHandler Event; 

    public void SendEventMessage(EnemyState Value)
    {
        Event?.Invoke(Value);
    }

    public override void SendEventMessage(BlackboardVariable[] messageData)
    {
        BlackboardVariable<EnemyState> ValueBlackboardVariable = messageData[0] as BlackboardVariable<EnemyState>;
        var Value = ValueBlackboardVariable != null ? ValueBlackboardVariable.Value : default(EnemyState);

        Event?.Invoke(Value);
    }

    public override Delegate CreateEventHandler(BlackboardVariable[] vars, System.Action callback)
    {
        NpcStateChangedEventHandler del = (Value) =>
        {
            BlackboardVariable<EnemyState> var0 = vars[0] as BlackboardVariable<EnemyState>;
            if(var0 != null)
                var0.Value = Value;

            callback();
        };
        return del;
    }

    public override void RegisterListener(Delegate del)
    {
        Event += del as NpcStateChangedEventHandler;
    }

    public override void UnregisterListener(Delegate del)
    {
        Event -= del as NpcStateChangedEventHandler;
    }
}

