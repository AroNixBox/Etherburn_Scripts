using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/Target Entities Unregistered Channel")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(name: "Target Entities Unregistered Channel", message: "Target Entities Unregistered", category: "Events", id: "15379aca3a593b516b4ba3c02d06d856")]
public partial class TargetEntitiesUnregisteredChannel : EventChannelBase
{
    public delegate void TargetEntitiesUnregisteredChannelEventHandler();
    public event TargetEntitiesUnregisteredChannelEventHandler Event; 

    public void SendEventMessage()
    {
        Event?.Invoke();
    }

    public override void SendEventMessage(BlackboardVariable[] messageData)
    {
        Event?.Invoke();
    }

    public override Delegate CreateEventHandler(BlackboardVariable[] vars, System.Action callback)
    {
        TargetEntitiesUnregisteredChannelEventHandler del = () =>
        {
            callback();
        };
        return del;
    }

    public override void RegisterListener(Delegate del)
    {
        Event += del as TargetEntitiesUnregisteredChannelEventHandler;
    }

    public override void UnregisterListener(Delegate del)
    {
        Event -= del as TargetEntitiesUnregisteredChannelEventHandler;
    }
}

