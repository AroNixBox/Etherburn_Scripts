using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TriggerExitAction : MonoBehaviour, IRequireEntityColliderInteractionChannel {
    [SerializeField] EntityType targetEntityType;
    [SerializeField] EMessageType messageType;
    Transform _target;
    
    EntityColliderInteractionChannel _entityColliderInteractionChannel;
    bool IsInitialized => _entityColliderInteractionChannel != null;

    void Start() {
        var entities = FindObjectsByType<Entity>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        var targetEntities = entities.Where(entity => entity.EntityType == targetEntityType).ToList();

        if(targetEntities.Count == 0) {
            Debug.LogError($"No entity of type {targetEntityType} found.");
            return;
        }
        _target = targetEntities.First().transform;
    }

    public void AssignEventChannel(EntityColliderInteractionChannel entityColliderInteractionChannel) {
        _entityColliderInteractionChannel = entityColliderInteractionChannel;
    }
    void OnTriggerEnter(Collider other) {
        if(!IsInitialized) { return; }
        if (messageType != EMessageType.Enter) { return; }
        if(other.transform != _target) { return; }
        _entityColliderInteractionChannel.SendEventMessage();
    }
    void OnTriggerExit(Collider other) {
        if(!IsInitialized) { return; }
        if (messageType != EMessageType.Exit) { return; }
        if(other.transform != _target) { return; }
        _entityColliderInteractionChannel.SendEventMessage();
    }
    
    public enum EMessageType {
        Enter,
        Exit
    }
}