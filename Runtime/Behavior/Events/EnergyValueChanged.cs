using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/Energy Value Changed")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(name: "Energy Value Changed", message: "[EnergyValue] changed", category: "Events", id: "ab947ce737c97b8c133f5190746b4818")]
public partial class EnergyValueChanged : EventChannelBase
{
    public delegate void EnergyValueChangedEventHandler(float EnergyValue);
    public event EnergyValueChangedEventHandler Event; 

    public void SendEventMessage(float EnergyValue) {
        Event?.Invoke(EnergyValue);
    }

    public override void SendEventMessage(BlackboardVariable[] messageData) {
        BlackboardVariable<float> EnergyValueBlackboardVariable = messageData[0] as BlackboardVariable<float>;
        var EnergyValue = EnergyValueBlackboardVariable != null ? EnergyValueBlackboardVariable.Value : default(float);

        Event?.Invoke(EnergyValue);
    }

    public override Delegate CreateEventHandler(BlackboardVariable[] vars, System.Action callback)
    {
        EnergyValueChangedEventHandler del = (EnergyValue) =>
        {
            BlackboardVariable<float> var0 = vars[0] as BlackboardVariable<float>;
            if(var0 != null)
                var0.Value = EnergyValue;

            callback();
        };
        return del;
    }

    public override void RegisterListener(Delegate del)
    {
        Event += del as EnergyValueChangedEventHandler;
    }

    public override void UnregisterListener(Delegate del)
    {
        Event -= del as EnergyValueChangedEventHandler;
    }
}

