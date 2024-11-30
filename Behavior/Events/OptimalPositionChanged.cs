using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/Optimal Position Changed")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(name: "Optimal Position Changed", message: "Optimal [Position]", category: "Events", id: "26053e3afca9bb4905d570530692540c")]
public partial class OptimalPositionChanged : EventChannelBase
{
    public delegate void OptimalPositionChangedEventHandler(Vector3 Position);
    public event OptimalPositionChangedEventHandler Event;

    public void SendEventMessage(Vector3 Position) {
        Event?.Invoke(Position);
    }

    public override void SendEventMessage(BlackboardVariable[] messageData)
    {
        BlackboardVariable<Vector3> PositionBlackboardVariable = messageData[0] as BlackboardVariable<Vector3>;
        var Position = PositionBlackboardVariable != null ? PositionBlackboardVariable.Value : default(Vector3);

        Event?.Invoke(Position);
    }

    public override Delegate CreateEventHandler(BlackboardVariable[] vars, System.Action callback)
    {
        OptimalPositionChangedEventHandler del = Position =>
        {
            BlackboardVariable<Vector3> var0 = vars[0] as BlackboardVariable<Vector3>;
            if (var0 != null)
                var0.Value = Position;

            callback();
        };
        return del;
    }
    public override void RegisterListener(Delegate del)
    {
        Event += del as OptimalPositionChangedEventHandler;
    }

    public override void UnregisterListener(Delegate del)
    {
        Event -= del as OptimalPositionChangedEventHandler;
    }
}

