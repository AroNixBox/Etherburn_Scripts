using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game {
    public class SceneLoader : Singleton<SceneLoader> {
        [SerializeField, Required] SceneData sceneData;
        [SerializeField, Required] Canvas loadingCanvas;
        [SerializeField, Required] Slider loadingSlider;
        readonly List<AsyncOperation> _asyncOperations = new ();
        
        protected override bool ShouldPersist => true;
        public SceneData.ELevelType CurrentLevelType { get; private set; }

        public IEnumerator LoadScenesAsync(SceneData.ELevelType levelType) {
            if(sceneData == null) {
                Debug.LogError("SceneData is not set in the inspector", transform);
            }
            
            // Set Current Level Type
            CurrentLevelType = levelType;
            
            // Load Systems Scene
            var systemsSceneOperation = SceneManager.LoadSceneAsync(sceneData.systemsScene.BuildIndex, LoadSceneMode.Additive);
            if (systemsSceneOperation != null) {
                systemsSceneOperation.allowSceneActivation = false;
                _asyncOperations.Add(systemsSceneOperation);
            }

            // Load Player Scene
            var playerSceneOperation = SceneManager.LoadSceneAsync(sceneData.playerScene.BuildIndex, LoadSceneMode.Additive);
            if (playerSceneOperation != null) {
                playerSceneOperation.allowSceneActivation = false;
                _asyncOperations.Add(playerSceneOperation);
            }

            // Load Navmesh Scene
            SceneData.NavMeshScenePackage[] navMeshPackages = sceneData.navMeshes.Where(navMesh => navMesh.levelType == levelType).ToArray();
            foreach (var navMeshPackage in navMeshPackages) {
                var navSceneOperation = SceneManager.LoadSceneAsync(navMeshPackage.navMeshScene.BuildIndex, LoadSceneMode.Additive);
                if (navSceneOperation != null) {
                    navSceneOperation.allowSceneActivation = false;
                    _asyncOperations.Add(navSceneOperation);
                }
            }
            
            SceneData.ScenePackage[] levelPackages = sceneData.levels.Where(level => level.levelType == levelType).ToArray();
            foreach (var levelPackage in levelPackages) {
                foreach (var scene in levelPackage.levelScenes) {
                    var environmentSceneOperation = SceneManager.LoadSceneAsync(scene.BuildIndex, LoadSceneMode.Additive);
                    environmentSceneOperation.allowSceneActivation = false;
                    _asyncOperations.Add(environmentSceneOperation);
                }
            }
            
            StartCoroutine(UpdateLoadingSlider(_asyncOperations));
            
            // Wait for all scenes to load
            while (_asyncOperations.Any(op => op.progress < 0.9f)) {
                yield return null;
            }
            
            // Activate all scenes
            foreach (var asyncOp in _asyncOperations) {
                asyncOp.allowSceneActivation = true;
            }
            
            // Wait for all scenes to activate
            foreach (var asyncOp in _asyncOperations) {
                while (!asyncOp.isDone) {
                    yield return null;
                }
            }
            
            // Clear Async Operations
            _asyncOperations.Clear();
            
            // Loading Scenes will always be a new Level...
            var gameBrain = GameBrain.Instance;
            if(gameBrain == null) {
                Debug.LogError("GameBrain is not set in the inspector", transform);
            }
            gameBrain.PlayTriggered = true;
        }

        // Loading UI Scene doesnt require a progress bar
        public void LoadSceneAsync(SceneData.EUISceneType uiSceneType) {
            if(sceneData == null) {
                Debug.LogError("SceneData is not set in the inspector", transform);
            }
            
            SceneData.UIScenePackage uiScenePackage = sceneData.uiScenePackages.FirstOrDefault(uiScene => uiScene.uiSceneType == uiSceneType);
            if (uiScenePackage.uiSceneType != uiSceneType) {
                Debug.LogError("UI Scene Type does not match the UI Scene Package", transform);
            }
            
            SceneManager.LoadSceneAsync(uiScenePackage.uiScene.BuildIndex, LoadSceneMode.Additive);
        }
        
        public void UnloadScene(SceneData.EUISceneType uiSceneType) {
            SceneData.UIScenePackage uiScenePackage = sceneData.uiScenePackages.FirstOrDefault(uiScene => uiScene.uiSceneType == uiSceneType);
            if (uiScenePackage != null && uiScenePackage.uiSceneType != uiSceneType) {
                Debug.LogError("UI Scene Type does not match the UI Scene Package", transform);
            }

            if (uiScenePackage != null) SceneManager.UnloadSceneAsync(uiScenePackage.uiScene.BuildIndex);
        }
        public bool IsInUIScene(SceneData.EUISceneType uiSceneType) {
            SceneData.UIScenePackage first = sceneData.uiScenePackages.FirstOrDefault(uiScene => uiScene.uiSceneType == uiSceneType);
            return first != null && SceneManager.GetSceneByBuildIndex(first.uiScene.BuildIndex).isLoaded;
        }
        
        public bool IsInLevel() {
            return CurrentLevelType != SceneData.ELevelType.None;
        }

        public void UnloadScenes(SceneData.ELevelType sceneLevelType) {
            if(sceneData == null) {
                Debug.LogError("SceneData is not set in the inspector", transform);
            }
            
            // Unload Systems Scene
            var systemsScene = sceneData.systemsScene;
            if (systemsScene != null) {
                SceneManager.UnloadSceneAsync(systemsScene.BuildIndex);
            }

            // Unload Player Scene
            var playerScene = sceneData.playerScene;
            if (playerScene != null) {
                SceneManager.UnloadSceneAsync(playerScene.BuildIndex);
            }

            // Unload Navmesh Scene
            var navMeshPackages = sceneData.navMeshes
                .Where(navMesh => navMesh.levelType == sceneLevelType).ToArray();
            foreach (var navMeshPackage in navMeshPackages) {
                var navMeshScene = navMeshPackage.navMeshScene;
                if (navMeshScene != null) {
                    SceneManager.UnloadSceneAsync(navMeshScene.BuildIndex);
                }
            }
            
            // Unload Level Package
            var levelPackages = sceneData.levels
                .Where(level => level.levelType == sceneLevelType).ToArray();
            foreach (var levelPackage in levelPackages) {
                foreach (var scene in levelPackage.levelScenes) {
                    SceneManager.UnloadSceneAsync(scene.BuildIndex);
                }
            }
        }
        
        IEnumerator UpdateLoadingSlider(List<AsyncOperation> asyncOperations) {
            loadingCanvas.gameObject.SetActive(true);
            
            while (asyncOperations.Any(op => !op.isDone)) {
                loadingSlider.value = asyncOperations.Average(op => op.progress);
                yield return null;
            }
            
            loadingCanvas.gameObject.SetActive(false);
        }
    }
}