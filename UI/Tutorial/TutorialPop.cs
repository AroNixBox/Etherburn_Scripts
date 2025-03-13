using System;
using System.Text;
using Player.Input;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Video;

namespace UI.Tutorial {
    public class TutorialPop : MonoBehaviour {
        [Header("References")]
        [SerializeField, Required] InputReader inputReader;
        [SerializeField, Required] AudioSource audioSource;
        [SerializeField, Required] VideoPlayer videoPlayer;
        [SerializeField, Required] TutorialPopupSO tutorialPopupSO;
        [SerializeField, Required] Canvas popupCanvas;
        [SerializeField, Required] TMP_Text titleText;
        [SerializeField, Required] TMP_Text descriptionText;
        
        PlayerInputActions _playerInputActions;

        void Start() {
            _playerInputActions = inputReader.InputActions;
            if (!AreReferencesAssigned()) { return; }
            
            titleText.text = tutorialPopupSO.title;
            descriptionText.text = ReplaceControlText(tutorialPopupSO.description);
            videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
            videoPlayer.SetTargetAudioSource(0, audioSource);
            
            videoPlayer.gameObject.SetActive(false);
            popupCanvas.gameObject.SetActive(false);
        }
        
        public void OpenTutorialPopup() {
            if (!AreReferencesAssigned()) { return; }
            
            // Disable the Player Input Actions
            inputReader.SwitchActionMap(InputReader.ActionMapName.UI);
            
            // Hook into the Pause event to close the popup when the game is paused
            inputReader.Pause += ClosePopupForPauseMenu;
            
            // Show & Unlock Cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            // Open the popup
            ReplaceControlText(tutorialPopupSO.description);
            popupCanvas.gameObject.SetActive(true);
            videoPlayer.gameObject.SetActive(true);
            
            // Play the video if it exists
            if (tutorialPopupSO.tutorialVideo != null) {
                videoPlayer.clip = tutorialPopupSO.tutorialVideo;
                videoPlayer.isLooping = tutorialPopupSO.loopVideo;
                videoPlayer.Play();
            }
        }
        void ClosePopupForPauseMenu() {
            inputReader.Pause -= ClosePopupForPauseMenu;
            
            popupCanvas.gameObject.SetActive(false);
            videoPlayer.gameObject.SetActive(false);
            
            // Dont Switch the Actionmap back to Player since Pause Menu handles that.
            // Dont unlock the cursor since Pause Menu handles that.
        }
        
        public void ClosePopup() {
            inputReader.Pause -= ClosePopupForPauseMenu;
            
            // Close the popup
            popupCanvas.gameObject.SetActive(false);
            videoPlayer.gameObject.SetActive(false);
            
            // Hide & Lock Cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            // Disable the Player Input Actions
            inputReader.SwitchActionMap(InputReader.ActionMapName.Player);
        }

        bool AreReferencesAssigned() {
            if(inputReader == null) {
                Debug.LogError("Input Reader is not assigned");
                return false;
            }
            
            if(_playerInputActions == null) {
                Debug.LogError("Player Input Actions are not assigned");
                return false;
            }
            
            if (popupCanvas == null) {
                Debug.LogError("Popup Prefab is not assigned");
                return false;
            }

            if (tutorialPopupSO == null) {
                Debug.LogError("Popup Data is not assigned");
                return false;
            }
            
            if (titleText == null) {
                Debug.LogError("Title Text is not assigned");
                return false;
            }
            
            if (descriptionText == null) {
                Debug.LogError("Description Text is not assigned");
                return false;
            }
            if (videoPlayer == null) {
                Debug.LogError("Video Player is not assigned");
                return false;
            }
            if (audioSource == null) {
                Debug.LogError("Audio Source is not assigned");
                return false;
            }
            
            return true;
        }
        
        string ReplaceControlText(string text) {
            // If no special symbol or no action reference, return original text
            if (!text.Contains("<inputDevices>") || tutorialPopupSO.inputActionReference == null) {
                Debug.LogError("No input devices symbol found or no action reference");
                return text;
            }
        
            // Get the action from the reference
            InputAction action = _playerInputActions.FindAction(tutorialPopupSO.inputActionReference.name);
            if (action == null) {
                Debug.LogError("Action not found", transform);
                return text;
            }
            
            var allDeviceTypes = InputUtils.GetDeviceTypes();
            // Get the binding index for keyboard and gamepad
            int[] bindingIndexes = new int[allDeviceTypes.Length];
            for (int i = 0; i < allDeviceTypes.Length; i++) {
                bindingIndexes[i] = InputUtils.GetBindingIndex(tutorialPopupSO.inputActionReference, allDeviceTypes[i]);
            }
            
            // Get binding names for keyboard and gamepad
            StringBuilder allBindings = new StringBuilder();

            for (int i = 0; i < bindingIndexes.Length; i++) {
                int bindingIndex = bindingIndexes[i];
    
                // Get the binding display string
                _ = action.GetBindingDisplayString(
                    bindingIndex, 
                    out _, 
                    out var controlPath, 
                    InputBinding.DisplayStringOptions.DontUseShortDisplayNames);
    
                // Get the binding name
                string bindingName = InputUtils.GetBindingFancyName(action, bindingIndex, controlPath);
                
                allBindings.Append("<b>");
                // Append binding name
                allBindings.Append(bindingName);
                allBindings.Append("</b>");
    
                // Only add separator if not the last element
                if (i < bindingIndexes.Length - 1) {
                    allBindings.Append(" / ");
                }
            }
            
            // Replace the <inputDevices> symbol with the binding names
            return text.Replace("<inputDevices>", allBindings.ToString());
        }
    }
}