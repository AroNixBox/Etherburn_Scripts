using System.Collections.Generic;
using Eflatun.SceneReference;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Game {
    [CreateAssetMenu(menuName = "Game/SceneData")]
    public class SceneData : ScriptableObject {
        [Title("Systems")]
        public SceneReference bootstrapperScene;
        public SceneReference playerScene;
        public SceneReference systemsScene;
        
        [Title("Levels")]
        public ScenePackage[] levels;
        public NavMeshScenePackage[] navMeshes;
        
        [Title("UI")]
        public UIScenePackage[] uiScenePackages;
        
        [System.Serializable]
        public class ScenePackage {
            public List<SceneReference> levelScenes;
            public ELevelType levelType;
        }
        
        [System.Serializable]
        public class NavMeshScenePackage {
            public SceneReference navMeshScene;
            public ELevelType levelType;
        }
        
        [System.Serializable]
        public class UIScenePackage {
            public SceneReference uiScene;
            public EUISceneType uiSceneType;
        }

        public enum ELevelType {
            None,
            Nixon_Testing,
            Level_One
        }
        
        public enum EUISceneType {
            None,
            MainMenu,
            GameOver
        }
    }
}