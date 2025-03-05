using System.Collections.Generic;
using System.IO;
using Extensions;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Game.Save {
    public class SaveManager : Singleton<SaveManager> {
        Dictionary<string, bool> _objectStateData = new();
        Dictionary<string, SerializableVector3> _objectPositionData = new();
        
        const string SaveName = "objectSaveData.json";
        protected override bool ShouldPersist => true;

        protected override void Awake() {
            LoadSaveData();
        }

        void LoadSaveData() {
            var savePath = Path.Combine(Application.persistentDataPath, SaveName);
            if (File.Exists(savePath)) {
                string json = File.ReadAllText(savePath);
                var saveData = JsonConvert.DeserializeObject<SaveData>(json) 
                               ?? new SaveData();
        
                _objectStateData = saveData.ObjectStateData;
                _objectPositionData = saveData.ObjectPositionData;
            } else {
                Debug.Log("No Save Data found, creating new Save Data");
                _objectStateData = new Dictionary<string, bool>();
                _objectPositionData = new Dictionary<string, SerializableVector3>();
            }
        }
        [Button]
        void PrintSaveFilePath() {
            var savePath = Path.Combine(Application.persistentDataPath, SaveName);
            Debug.Log("Save file path: " + savePath);
        }

        public void RegisterObject(string itemName, bool objectState) {
            _objectStateData[itemName] = objectState;

            var saveData = new SaveData {
                ObjectStateData = _objectStateData,
                ObjectPositionData = _objectPositionData
            };

            string json = JsonConvert.SerializeObject(saveData, Formatting.Indented);
            var savePath = Path.Combine(Application.persistentDataPath, SaveName);
            File.WriteAllText(savePath, json);
        }

        public void RegisterObject(string itemName, Vector3 objectPosition) {
            _objectPositionData[itemName] = new SerializableVector3(objectPosition);

            var saveData = new SaveData {
                ObjectStateData = _objectStateData,
                ObjectPositionData = _objectPositionData
            };

            string json = JsonConvert.SerializeObject(saveData, Formatting.Indented);
            var savePath = Path.Combine(Application.persistentDataPath, SaveName);
            File.WriteAllText(savePath, json);
        }

        // If Object state is null, then the object is not registered -> Call RegisterObject
        // If Object state is not null, use the state!
        public bool? GetObjectState(string itemName) {
            return _objectStateData.TryGetValue(itemName, out var state) ? state : null;
        }
        
        public Vector3? GetObjectPosition(string itemName) {
            return _objectPositionData.TryGetValue(itemName, out var position) ? position.ToVector3() : null;
        }
        
        [Button]
        // TODO: Call when Reset Button is clicked
        public void ClearSaveData() {
            // Clear the dictionary
            _objectStateData.Clear();
            _objectPositionData.Clear();
    
            var savePath = Path.Combine(Application.persistentDataPath, SaveName);
            // Delete the JSON file
            if (File.Exists(savePath)) {
                File.Delete(savePath);
            }
        }
        [System.Serializable]
        public class SaveData {
            public Dictionary<string, bool> ObjectStateData = new();
            public Dictionary<string, SerializableVector3> ObjectPositionData = new();
        }
        
        [System.Serializable]
        public class SerializableVector3 {
            public float x, y, z;

            public SerializableVector3(Vector3 vector) {
                x = vector.x;
                y = vector.y;
                z = vector.z;
            }

            public Vector3 ToVector3() {
                return new Vector3(x, y, z);
            }
        }
    }
}