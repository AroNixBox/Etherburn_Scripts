using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game {
    public class SceneLoader : MonoBehaviour {
        [SerializeField] SceneData sceneData;
        [SerializeField] RectTransform loadingElement;
        [SerializeField] Slider loadingSlider;
        readonly List<AsyncOperation> _asyncOperations = new ();
        
        public SceneData.ELevelType CurrentLevelType { get; private set; }
        
        public static SceneLoader Instance { get; private set; }

        void Awake() {
            if(Instance == null) {
                Instance = this;
            } else {
                Destroy(gameObject);
            }
        }
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
            
            StartCoroutine(UpdateLoadingSlider());

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
        IEnumerator UpdateLoadingSlider() {
            loadingElement.gameObject.SetActive(true);
            
            while(_asyncOperations.Count > 0) {
                loadingSlider.value = _asyncOperations.Average(op => op.progress);
                yield return null;
            }
            
            loadingElement.gameObject.SetActive(false);
        }
    }
}