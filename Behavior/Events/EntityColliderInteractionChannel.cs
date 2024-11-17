using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/Entity Collider Interaction")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(name: "Entity Collider Interaction", message: "Entity Collider Interaction", category: "Events", id: "e39b95991deebf767479b54eb9720c98")]
public partial class EntityColliderInteractionChannel : EventChannelBase
{
    public delegate void EntityColliderInteractionEventHandler();
    public event EntityColliderInteractionEventHandler Event; 

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
        EntityColliderInteractionEventHandler del = () =>
        {
            callback();
        };
        return del;
    }


    public override void RegisterListener(Delegate del)
    {
        Event += del as EntityColliderInteractionEventHandler;
    }

    public override void UnregisterListener(Delegate del)
    {
        Event -= del as EntityColliderInteractionEventHandler;
    }
}

