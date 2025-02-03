using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using Player.Input;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game {
    public class SceneLoader : Singleton<SceneLoader> {
        [SerializeField] SceneData sceneData;
        [SerializeField] Canvas loadingCanvas;
        [SerializeField] Slider loadingSlider;
        readonly List<AsyncOperation> _asyncOperations = new ();
        
        protected override bool ShouldPersist => true;
        
        public SceneData.ELevelType CurrentLevelType { get; private set; }
        
        public IEnumerator LoadScenesAsync(SceneData.ELevelType levelType) {
            if(sceneData == null) {
                Debug.LogError("SceneData is not set in the inspector", transform);
            }
            
            // Set Current Level Type
            CurrentLevelType = levelType;
            
            // Save Bootstrapper Scene Build Index
            var currentBuildIndex = SceneManager.GetActiveScene().buildIndex;
            
            // Load Systems Scene
            var systemsSceneOperation = SceneManager.LoadSceneAsync(sceneData.systemsScene.BuildIndex, LoadSceneMode.Additive);
            systemsSceneOperation.allowSceneActivation = false;
            _asyncOperations.Add(systemsSceneOperation);
            
            // Load Player Scene
            var playerSceneOperation = SceneManager.LoadSceneAsync(sceneData.playerScene.BuildIndex, LoadSceneMode.Additive);
            playerSceneOperation.allowSceneActivation = false;
            _asyncOperations.Add(playerSceneOperation);
            
            // Load Navmesh Scene
            SceneData.NavMeshScenePackage[] navMeshPackages = sceneData.navMeshes.Where(navMesh => navMesh.levelType == levelType).ToArray();
            foreach (var navMeshPackage in navMeshPackages) {
                var navSceneOperation = SceneManager.LoadSceneAsync(navMeshPackage.navMeshScene.BuildIndex, LoadSceneMode.Additive);
                navSceneOperation.allowSceneActivation = false;
                _asyncOperations.Add(navSceneOperation);
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
            
            // Unload Bootstrapper Scene
            SceneManager.UnloadSceneAsync(currentBuildIndex);
        }

        /// <summary>
        /// Used to load one scene async at a time
        /// </summary>
        /// <returns></returns>
        public IEnumerator LoadSceneAsync(SceneData.EMenuType menuType) {
            if (sceneData == null) {
                Debug.LogError("SceneData is not set in the inspector", transform);
            }
    
            // Load Menu Scene
            var menuSceneOperation = SceneManager.LoadSceneAsync(sceneData.MenuScenes[menuType].BuildIndex);
            menuSceneOperation.allowSceneActivation = false;
    
            StartCoroutine(UpdateLoadingSlider(new List<AsyncOperation> {menuSceneOperation}));
    
            // Wait for all scenes to load
            while (menuSceneOperation.progress < 0.9f) {
                yield return null;
            }
    
            // Activate all scenes
            menuSceneOperation.allowSceneActivation = true;
            
            // Wait for scene to activate
            while (!menuSceneOperation.isDone) {
                yield return null;
            }
    
            if (menuType == SceneData.EMenuType.GameOver) {
                var gameOverWindow = FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                    .FirstOrDefault(go => go.name == "Game Over");
                var mainMenuWindow = FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                    .FirstOrDefault(go => go.name == "Main Menu");
                
                if (gameOverWindow != null) {
                    gameOverWindow.SetActive(true);
                } else {
                    Debug.LogError("Game Over object not found");
                }

                if (mainMenuWindow != null) {
                    mainMenuWindow.SetActive(false);
                } else {
                    Debug.LogError("Main Menu object not found");
                }
                
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
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