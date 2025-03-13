using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Video;

namespace UI.Tutorial {
    [CreateAssetMenu(fileName = "TutorialPopup", menuName = "Game/Tutorial/Popup Data")]
    public class TutorialPopupSO : ScriptableObject {
        [Header("Content")]
        [Required] public string title;
        [TextArea(3, 10)]
        [InfoBox("Use <b><inputDevices></b> in the description to replace it with the input binding")]
        [Required] public string description;
        
        [Header("Media")]
        public VideoClip tutorialVideo;
        [Tooltip("Optional - Leave empty if no video needed")]
        public bool loopVideo = true;
        
        [Header("Input Binding")]
        public InputActionReference inputActionReference; 
    }
}