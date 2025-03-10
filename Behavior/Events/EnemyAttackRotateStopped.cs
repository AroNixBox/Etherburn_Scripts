using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/Enemy Attack Rotate Stopped")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(name: "Enemy Attack Rotate Stopped", message: "Enemy Attack Rotate Stopped", category: "Events", id: "90ab2e160526c73abc00abfe538f679d")]
public partial class EnemyAttackRotateStopped : EventChannelBase
{
    public delegate void EnemyAttackRotateStoppedEventHandler();
    public event EnemyAttackRotateStoppedEventHandler Event; 

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
        EnemyAttackRotateStoppedEventHandler del = () =>
        {
            callback();
        };
        return del;
    }

    public override void RegisterListener(Delegate del)
    {
        Event += del as EnemyAttackRotateStoppedEventHandler;
    }

    public override void UnregisterListener(Delegate del)
    {
        Event -= del as EnemyAttackRotateStoppedEventHandler;
    }
}

