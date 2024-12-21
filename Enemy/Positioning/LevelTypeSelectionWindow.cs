using Game;
using UnityEngine;

#if UNITY_EDITOR
namespace Enemy.Positioning {
    public class LevelTypeSelectionWindow : UnityEditor.EditorWindow {
        static SceneData.ELevelType SelectedLevelType { get; set; }
        static bool IsCancelled { get; set; }
        public static System.Action<SceneData.ELevelType, bool> OnSelectionMade;

        static string[] options;
        static int selectedIndex;

        public static void ShowWindow() {
            options = System.Enum.GetNames(typeof(SceneData.ELevelType));
            selectedIndex = 0;
            IsCancelled = false;
            GetWindow<LevelTypeSelectionWindow>("Select Level Type");
        }

        void OnGUI() {
            GUILayout.Label("Please select the level type for the grid data:", UnityEditor.EditorStyles.boldLabel);
            selectedIndex = UnityEditor.EditorGUILayout.Popup("Level Type", selectedIndex, options);

            GUILayout.Space(20);

            if (GUILayout.Button("OK")) {
                SelectedLevelType = (SceneData.ELevelType)selectedIndex;
                IsCancelled = false;
                OnSelectionMade?.Invoke(SelectedLevelType, IsCancelled);
                Close();
            }

            if (GUILayout.Button("Cancel")) {
                IsCancelled = true;
                OnSelectionMade?.Invoke(SelectedLevelType, IsCancelled);
                Close();
            }
        }
    }
}

#endif
