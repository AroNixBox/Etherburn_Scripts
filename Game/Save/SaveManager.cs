using System.Collections.Generic;
using System.IO;
using Extensions;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Game.Save {
    public class SaveManager : Singleton<SaveManager> {
        Dictionary<string, bool> _saveData = new();
        const string SaveName = "objectSaveData.json";
        protected override bool ShouldPersist => true;

        protected override void Awake() {
            LoadSaveData();
        }

        void LoadSaveData() {
            var savePath = Path.Combine(Application.persistentDataPath, SaveName);
            if (File.Exists(savePath)) {
                string json = File.ReadAllText(savePath);
                _saveData = JsonConvert.DeserializeObject<Dictionary<string, bool>>(json) 
                            // Json File Empty
                            ?? new Dictionary<string, bool>();
            } else {
                Debug.Log("No Save Data found, creating new Save Data");
                _saveData = new Dictionary<string, bool>();
            }
        }
        [Button]
        void PrintSaveFilePath() {
            var savePath = Path.Combine(Application.persistentDataPath, SaveName);
            Debug.Log("Save file path: " + savePath);
        }

        public void RegisterObject(string itemName, bool objectState) {
            _saveData[itemName] = objectState;
            Save();
        }

        // If Object state is null, then the object is not registered -> Call RegisterObject
        // If Object state is not null, use the state!
        public bool? GetObjectState(string itemName) {
            return _saveData.TryGetValue(itemName, out var state) ? state : null;
        }

        void Save() {
            string json = JsonConvert.SerializeObject(_saveData, Formatting.Indented);
            
            var savePath = Path.Combine(Application.persistentDataPath, SaveName);
            File.WriteAllText(savePath, json);
        }
        
        [Button]
        void ClearSaveData() {
            // Clear the dictionary
            _saveData.Clear();
    
            var savePath = Path.Combine(Application.persistentDataPath, SaveName);
            // Delete the JSON file
            if (File.Exists(savePath)) {
                File.Delete(savePath);
            }
        }
    }
}