using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace UI.Menu {
    [RequireComponent(typeof(TMP_Dropdown))]
    public class FpsDropdown : MonoBehaviour {
        TMP_Dropdown _fpsDropdown;
        [SerializeField] List<int> fpsOptions = new() { -1, 30, 60, 120, 144, 240 };

        void Awake() {
            _fpsDropdown = GetComponent<TMP_Dropdown>();
        }

        void Start() {
            PopulateDropdown();
            SetInitialDropdownValue();
            _fpsDropdown.onValueChanged.AddListener(SetTargetFps);
        }

        void PopulateDropdown() {
            List<string> options = new List<string>();
            foreach (int fps in fpsOptions) {
                options.Add(fps == -1 ? "Unlimited" : fps.ToString());
            }
            _fpsDropdown.ClearOptions();
            _fpsDropdown.AddOptions(options);
        }

        void SetInitialDropdownValue() {
            int currentFps = Application.targetFrameRate;
            if (currentFps == -1) {
                currentFps = (int)Screen.currentResolution.refreshRateRatio.value;
            }
            int index = fpsOptions.IndexOf(currentFps);
            if (index == -1) {
                index = 0; // Default to the first option if the current FPS is not in the list
            }
            _fpsDropdown.value = index;
        }

        void SetTargetFps(int index) {
            int selectedFps = fpsOptions[index];
            Application.targetFrameRate = selectedFps == -1 ? (int)Screen.currentResolution.refreshRateRatio.value : selectedFps;
        }
    }
}