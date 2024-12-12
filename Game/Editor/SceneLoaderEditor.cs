using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using System.Linq;
using UnityEngine.SceneManagement;

namespace Game.Editor {
    public class SceneLoaderEditor : EditorWindow {
        SceneData _sceneData; // Reference to SceneData ScriptableObject
        SceneData.ELevelType _selectedLevelType = SceneData.ELevelType.None;

        [MenuItem("Tools/Scene Loader")]
        public static void ShowWindow() {
            GetWindow<SceneLoaderEditor>("Scene Loader");
        }

        void OnGUI() {
            GUILayout.Label("Load Scenes by Level Type", EditorStyles.boldLabel);

            // Define custom styles for the buttons
            GUIStyle unloadAllButtonStyle = new GUIStyle(GUI.skin.button);
            unloadAllButtonStyle.normal.textColor = Color.white;
            unloadAllButtonStyle.fontStyle = FontStyle.Bold;
            unloadAllButtonStyle.fontSize = 14;
            unloadAllButtonStyle.normal.background = MakeTex(2, 2, Color.red);

            if (GUILayout.Button("Unload All Scenes", unloadAllButtonStyle)) {
                UnloadAllScenes();
            }

            GUILayout.Space(10); // Add some space

            // Field to select SceneData ScriptableObject
            _sceneData = (SceneData)EditorGUILayout.ObjectField("Scene Data", _sceneData, typeof(SceneData), false);

            if (_sceneData == null) {
                EditorGUILayout.HelpBox("Please assign a Scene Data object.", MessageType.Warning);
                return;
            }

            // Dropdown for selecting level type
            GUIStyle dropdownStyle = new GUIStyle(EditorStyles.popup);
            dropdownStyle.normal.textColor = Color.cyan;
            dropdownStyle.fontStyle = FontStyle.Bold;
            dropdownStyle.fontSize = 14;

            GUILayout.Space(10); // Add some space before the dropdown
            _selectedLevelType = (SceneData.ELevelType)EditorGUILayout.EnumPopup("Select Level Type", _selectedLevelType, dropdownStyle);

            GUIStyle loadButtonStyle = new GUIStyle(GUI.skin.button);
            loadButtonStyle.normal.textColor = Color.green;
            loadButtonStyle.fontStyle = FontStyle.Bold;
            loadButtonStyle.fontSize = 14;

            GUIStyle unloadButtonStyle = new GUIStyle(GUI.skin.button);
            unloadButtonStyle.normal.textColor = Color.yellow;
            unloadButtonStyle.fontStyle = FontStyle.Bold;
            unloadButtonStyle.fontSize = 14;

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Load Scenes", loadButtonStyle)) {
                LoadScenesByLevelType(_selectedLevelType);
            }
            if (GUILayout.Button("Unload Scenes", unloadButtonStyle)) {
                UnloadScenesByLevelType(_selectedLevelType);
            }
            GUILayout.EndHorizontal();
        }

        Texture2D MakeTex(int width, int height, Color col) {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++) {
                pix[i] = col;
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }
        void UnloadAllScenes() {
            for (int i = SceneManager.sceneCount - 1; i >= 0; i--) {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.isLoaded) {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
            Debug.Log("Unloaded all scenes.");
        }
        void LoadScenesByLevelType(SceneData.ELevelType levelType) {
            if (_sceneData == null) {
                Debug.LogError("Scene Data is not assigned.");
                return;
            }

            // Filter scenes by level type
            var filteredScenes = _sceneData.levels
                .Where(pkg => pkg.levelType == levelType)
                .SelectMany(pkg => pkg.levelScenes)
                .Concat(_sceneData.navMeshes.Where(nav => nav.levelType == levelType).Select(nav => nav.navMeshScene));

            foreach (var sceneRef in filteredScenes) {
                if (sceneRef.BuildIndex >= 0) { // Ensure BuildIndex is valid
                    EditorSceneManager.OpenScene(SceneUtility.GetScenePathByBuildIndex(sceneRef.BuildIndex), OpenSceneMode.Additive);
                } else {
                    Debug.LogWarning($"Invalid Build Index for scene: {sceneRef}");
                }
            }

            Debug.Log($"Loaded all scenes for level type: {levelType}");
        }
        void UnloadScenesByLevelType(SceneData.ELevelType levelType) {
            if (_sceneData == null) {
                Debug.LogError("Scene Data is not assigned.");
                return;
            }

            // Filter scenes by level type
            var filteredScenes = _sceneData.levels
                .Where(pkg => pkg.levelType == levelType)
                .SelectMany(pkg => pkg.levelScenes)
                .Concat(_sceneData.navMeshes.Where(nav => nav.levelType == levelType).Select(nav => nav.navMeshScene));

            foreach (var sceneRef in filteredScenes) {
                Scene scene = SceneManager.GetSceneByPath(SceneUtility.GetScenePathByBuildIndex(sceneRef.BuildIndex));
                if (scene.isLoaded) {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }

            Debug.Log($"Unloaded all scenes for level type: {levelType}");
        }
    }
}
