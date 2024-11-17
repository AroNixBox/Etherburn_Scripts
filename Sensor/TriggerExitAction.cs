using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TriggerExitAction : MonoBehaviour, IRequireEntityColliderInteractionChannel {
    [SerializeField] Transform target;
    [SerializeField] EMessageType messageType;
    
    EntityColliderInteractionChannel _entityColliderInteractionChannel;
    bool IsInitialized => _entityColliderInteractionChannel != null;
    public void AssignEventChannel(EntityColliderInteractionChannel entityColliderInteractionChannel) {
        _entityColliderInteractionChannel = entityColliderInteractionChannel;
    }
    void OnTriggerEnter(Collider other) {
        if(!IsInitialized) { return; }
        if (messageType != EMessageType.Enter) { return; }
        if(other.transform != target) { return; }
        _entityColliderInteractionChannel.SendEventMessage();
    }
    void OnTriggerExit(Collider other) {
        if(!IsInitialized) { return; }
        if (messageType != EMessageType.Exit) { return; }
        if(other.transform != target) { return; }
        _entityColliderInteractionChannel.SendEventMessage();
    }
    
    public enum EMessageType {
        Enter,
        Exit
    }
}
public interface IRequireEntityColliderInteractionChannel {
    void AssignEventChannel(EntityColliderInteractionChannel entityColliderInteractionChannel); 
}
