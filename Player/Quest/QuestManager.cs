using System.Collections.Generic;
using System.Linq;
using Game.Save;
using ShaderControl;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Player.Quest {
    public class QuestManager : MonoBehaviour {
        [SerializeField, Required] Transform playerQuestItemParent;
        [SerializeField] List<QuestSO> availableQuests = new();

        readonly List<QuestSO> _activeQuests = new();
        readonly List<QuestSO> _completedQuests = new();

        void Start() {
            LoadQuestProgress();
        }
        
        public void StartQuest(QuestSO quest) {
            if (!_activeQuests.Contains(quest) && !_completedQuests.Contains(quest)) {
                var questItem = Instantiate(quest.collectiblePrefab, playerQuestItemParent);
                questItem.transform.localScale = quest.collectibleScaleFixedOnPlayer;
                questItem.transform.localPosition = quest.collectiblePositionFixedOnPlayer;
                questItem.transform.localRotation = quest.collectibleRotationFixedOnPlayer;
                var questItemDissolveControl = questItem.GetComponent<DissolveControl>();
                if (questItemDissolveControl != null) {
                    _ = questItemDissolveControl.ChangeDissolveMode(DissolveControl.DissolveMode.Materialize);
                }
                
                _activeQuests.Add(quest);
                SaveQuestProgress();
                
                // TODO: Add ScreenUI Hint to show where to bring the Item
                // TODO: How do we target the ScreenUI Hint? The Reciever is a trigger area that has a bool enabled..
            }
        }

        public async void CompleteQuest(QuestSO quest) {
            if (!_activeQuests.Contains(quest)) { return; }
            
            // End the quest Data
            _activeQuests.Remove(quest);
            _completedQuests.Add(quest);
            SaveQuestProgress();
            
            // End the quest visuals
            var questItem = playerQuestItemParent
                .GetComponentsInChildren<QuestItem>()
                .FirstOrDefault(item => item.associatedQuest == quest);
            if (questItem == null) { return; }
            var dissolveControl = questItem.GetComponent<DissolveControl>();
            if (dissolveControl != null) {
                await dissolveControl.ChangeDissolveMode(DissolveControl.DissolveMode.Dissolve);
            }
            
            Destroy(questItem.gameObject);
                
            // TODO: Remove ScreenUI Hint
        }

        void SaveQuestProgress() {
            foreach (var quest in _activeQuests) {
                SaveManager.Instance.RegisterObject("Quest_" + quest.questId, false);
            }
            
            foreach (var quest in _completedQuests) {
                SaveManager.Instance.RegisterObject("Quest_" + quest.questId, true);
            }
        }

        void LoadQuestProgress() {
            _activeQuests.Clear();
            _completedQuests.Clear();
            
            foreach (var quest in availableQuests) {
                var state = SaveManager.Instance.GetObjectState("Quest_" + quest.questId);
                
                switch (state) {
                    case null:
                        // Quest not started yet
                        continue;
                    case true:
                        // Completed quest
                        _completedQuests.Add(quest);
                        break;
                    default:
                        // Active quest
                        StartQuest(quest);
                        break;
                }
            }
        }
    }
}