using System.Collections.Generic;
using Eflatun.SceneReference;
using UnityEngine;

namespace Game {
    [CreateAssetMenu(menuName = "Game/SceneData")]
    public class SceneData : ScriptableObject {
        public SceneReference playerScene;
        public SceneReference systemsScene;
        public ScenePackage[] levels;
        public NavMeshScenePackage[] navMeshes;
        
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
        public class GridManagerPackage {
            public SceneReference gridManagerScene;
            public ELevelType levelType;
        }
        
        public enum ELevelType {
            None,
            Nixon_Testing,
            Level_One
        }
    }
}