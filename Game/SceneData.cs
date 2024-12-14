using System.Collections.Generic;
using Eflatun.SceneReference;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game {
    [CreateAssetMenu(menuName = "Game/SceneData")]
    public class SceneData : ScriptableObject {
        public SceneReference playerScene;
        public ScenePackage[] levels;
        public NavMeshScenePackage[] navMeshes;
        [FormerlySerializedAs("agressionManagers")] public GridManagerPackage[] aggressionManagers;
        
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