using System;
using System.Collections.Generic;
using System.Linq;
using Behavior.Events.Interfaces;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Sensor {
    [RequireComponent(typeof(Collider))]
    public class TriggerArea : MonoBehaviour, IRequireEntityColliderInteractionChannel {
        [Header("Event Cast")]
        [SerializeField] EntityType targetEntityType;
        [SerializeField] EMessageType messageType;
        [SerializeField] bool multiApply = true;
        [SerializeField] bool useEventChannel = true;
        [ShowIf("@!useEventChannel")]
        public UnityEvent onCollisionEvent;
        
        [Header("Player Specific")]
        [Title("Weapon Upgrade")]
        [ShowIf("@targetEntityType == EntityType.Player")]
        [SerializeField] bool weaponUpgrade;
        [ShowIf("@targetEntityType == EntityType.Player && weaponUpgrade")]
        [SerializeField] Player.Weapon.WeaponSO weaponSO;
        [Title("Quest")]
        [ShowIf("@targetEntityType == EntityType.Player")]
        [SerializeField] bool quest;
        [ShowIf("@targetEntityType == EntityType.Player && quest")]
        [SerializeField] Player.Quest.QuestSO questSO;
        [ShowIf("@targetEntityType == EntityType.Player && quest")]
        [SerializeField] [EnumToggleButtons] QuestState questState;
        enum QuestState { QuestBegin, QuestComplete }

        [ShowIf("@targetEntityType == EntityType.Player && quest && questState == QuestState.QuestBegin")]
        [SerializeField] UnityEvent onQuestBeginEvent;
        [ShowIf("@targetEntityType == EntityType.Player && quest && questState == QuestState.QuestComplete")]
        [SerializeField] UnityEvent onQuestCompleteEvent;

        
        [Title("Bonfire")]
        [ShowIf("@targetEntityType == EntityType.Player")]
        [SerializeField] bool bonfire;
        [ShowIf("@targetEntityType == EntityType.Player && bonfire")]
        [SerializeField] Transform bonfireRespawnPoint;
        

        bool _hasApplied;
        
        Collider _collider;
        List<Entity> _targetEntities;
        
        EntityColliderInteractionChannel _entityColliderInteractionChannel;
        bool _isInitialized;
        
        void Start() {
            _collider = GetComponent<Collider>();
            
            // TODO: Find a way to get the target entity without using FindObjectsByType
            var entities = FindObjectsByType<Entity>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            _targetEntities = entities.Where(entity => entity.EntityType == targetEntityType).ToList();

            if(_targetEntities.Count == 0) {
                Debug.LogError($"No entity of type {targetEntityType} found.");
                return;
            }

            if (!useEventChannel) {
                _isInitialized = true;
            }
        }

        public EntityColliderInteractionChannel AssignEventChannel(EntityColliderInteractionChannel entityColliderInteractionChannel) {
            if (!useEventChannel) {
                Debug.LogError("We are not using Event Channel via Bootstrapper");
                return null;
            }
            
            _isInitialized = true;
            if (_entityColliderInteractionChannel == null) {
                _entityColliderInteractionChannel = entityColliderInteractionChannel;
                return _entityColliderInteractionChannel;
            }
            
            // Channel was already assigned, by a different AssignEventChannel call
            return _entityColliderInteractionChannel;
        }
        void OnTriggerEnter(Collider other) {
            if(!_isInitialized) { return; }
            if (messageType != EMessageType.Enter) { return; }
            
            if(_hasApplied) { return; }
                        
            if(!other.TryGetComponent(out Entity entity)) { return; }
            if(!_targetEntities.Contains(entity)) { return; }
            if(other.TryGetComponent(out Rigidbody rigidbody)) {
                if (rigidbody.isKinematic) {
                    // If the other Object is Kinematic, check for intersection first
                    if (IsColliderIntersecting(other)) {
                        if (!multiApply) { _hasApplied = true; }
                        
                        FireEvent();
                        FireSpecificAction(entity);
                        FirePlayerSpecificAction(entity);
                    }
                    return;
                }
            }
            
            if (IsColliderIntersecting(other)) {
                if (!multiApply) { _hasApplied = true; }
                
                FireEvent();
                FireSpecificAction(entity);
                FirePlayerSpecificAction(entity);
            }
        }
        
        bool IsColliderIntersecting(Collider other) {
            return _collider.bounds.Intersects(other.bounds);
        }

        void FireEvent() {
            if (useEventChannel) {
                _entityColliderInteractionChannel.SendEventMessage();
            }
            else {
                onCollisionEvent?.Invoke();
            }
        }
        
        // Unclean, does very specific thing.
        void FirePlayerSpecificAction(Entity entity) {
            if(targetEntityType != EntityType.Player) { return; }

            if (weaponUpgrade) {
                UpgradeWeapon(entity);
            }

            if (bonfire) {
                SaveBonfireProgress(entity);
            }
            
            if (quest && questSO != null) {
                TriggerQuest(entity);
            }
        }
        void UpgradeWeapon(Entity entity) {
            if (entity.TryGetComponent(out Player.Weapon.WeaponManager weaponManager)) {
                weaponManager.AddWeapon(weaponSO);
            }
            
            var saveManager = Game.Save.SaveManager.Instance;
            if (saveManager == null) {
                Debug.LogError("SaveManager is null"); 
                return;
            }
            
            saveManager.RegisterWeapon(weaponSO.name);
        }
        
        async void TriggerQuest(Entity entity) {
            if (!entity.TryGetComponent(out Player.Quest.QuestManager questManager)) {
                Debug.LogError("QuestManager not found on Player");
                return;
            }
            
            switch (questState) {
                    case QuestState.QuestBegin:
                        var questStarted = questManager.StartQuest(questSO);
                        if (questStarted) {
                            onQuestBeginEvent?.Invoke();
                        }
                        break;
                    case QuestState.QuestComplete:
                        var questCompleted = await questManager.CompleteQuest(questSO);
                        if(questCompleted) {
                            onQuestCompleteEvent?.Invoke();
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
            }
        }
        
        void SaveBonfireProgress(Entity entity) {
            var saveManager = Game.Save.SaveManager.Instance;
            if (saveManager == null) {
                Debug.LogError("SaveManager is null"); 
                return;
            }
            
            saveManager.RegisterObject(entity.name, bonfireRespawnPoint.position);
        }
        protected virtual void FireSpecificAction(Entity entity) { }
        void OnTriggerExit(Collider other) {
            if(!_isInitialized) { return; }
            if (messageType != EMessageType.Exit) { return; }
            
            if(_hasApplied) { return; }
            
            if(!other.TryGetComponent(out Entity entity)) { return; }
            if(!_targetEntities.Contains(entity)) { return; }
            
            if(other.TryGetComponent(out Rigidbody rigidbody)) {
                if (rigidbody.isKinematic) {
                    if (!IsColliderIntersecting(other)) {
                        if (!multiApply) { _hasApplied = true; }
                        
                        FireEvent();
                        FireSpecificAction(entity);
                        FirePlayerSpecificAction(entity);
                    }
                    return;
                }
            }
            // Doublecheck if is really outside of the collider
            if (!IsColliderIntersecting(other)) {
                if (!multiApply) { _hasApplied = true; }
                
                FireEvent();
                FireSpecificAction(entity);
                FirePlayerSpecificAction(entity);
            }
        }
    
        public enum EMessageType {
            Enter,
            Exit
        }
    }
}