using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/Change Aggression Channel")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(name: "Change Aggression Channel", message: "Change Agression [Level] Channel", category: "Events", id: "2bfb79367db190f4296dea3448b17c1d")]
public partial class ChangeAggressionChannel : EventChannelBase
{
    public delegate void ChangeAggressionChannelEventHandler(bool Level);
    public event ChangeAggressionChannelEventHandler Event; 

    public void SendEventMessage(bool Level)
    {
        Event?.Invoke(Level);
    }

    public override void SendEventMessage(BlackboardVariable[] messageData)
    {
        BlackboardVariable<bool> LevelBlackboardVariable = messageData[0] as BlackboardVariable<bool>;
        var Level = LevelBlackboardVariable != null ? LevelBlackboardVariable.Value : default(bool);

        Event?.Invoke(Level);
    }

    public override Delegate CreateEventHandler(BlackboardVariable[] vars, System.Action callback)
    {
        ChangeAggressionChannelEventHandler del = (Level) =>
        {
            BlackboardVariable<bool> var0 = vars[0] as BlackboardVariable<bool>;
            if(var0 != null)
                var0.Value = Level;
            callback();
        };
        return del;
    }

    public override void RegisterListener(Delegate del)
    {
        Event += del as ChangeAggressionChannelEventHandler;
    }

    public override void UnregisterListener(Delegate del)
    {
        Event -= del as ChangeAggressionChannelEventHandler;
    }
}

