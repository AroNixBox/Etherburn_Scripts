using UnityEngine;
using Sirenix.OdinInspector;

namespace Player.Quest {
    [CreateAssetMenu(fileName = "New Quest", menuName = "Quest/Quest")]
    public class QuestSO : ScriptableObject {
        [BoxGroup("Quest Identity")]
        [Required("Quest ID must not assigned and unique")]
        public string questId;
        
        [BoxGroup("Quest Identity")]
        public string questName;
        
        [BoxGroup("Completion")]
        [Required]
        public QuestItem collectiblePrefab;
        public Vector3 collectiblePositionFixedOnPlayer;
        public Quaternion collectibleRotationFixedOnPlayer;
        public Vector3 collectibleScaleFixedOnPlayer;
    }
}