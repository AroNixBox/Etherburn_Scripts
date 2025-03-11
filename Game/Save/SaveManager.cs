using System.Collections.Generic;
using System.IO;
using Extensions;
using Newtonsoft.Json;
using Player.Weapon;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Game.Save {
    public class SaveManager : Singleton<SaveManager> {
        [SerializeField] WeaponSO[] availableWeapons;

        Dictionary<string, bool> _objectStateData = new();
        Dictionary<string, SerializableVector3> _objectPositionData = new();
        List<string> _savedWeaponNames = new();

        const string SaveName = "objectSaveData.json";
        protected override bool ShouldPersist => true;
        
        protected override void Awake() {
            base.Awake();
            LoadSaveData();
        }

        public void LoadSaveData() {
            var savePath = Path.Combine(Application.persistentDataPath, SaveName);
            if (File.Exists(savePath)) {
                string json = File.ReadAllText(savePath);
                var saveData = JsonConvert.DeserializeObject<SaveData>(json)
                               ?? new SaveData();

                _objectStateData = saveData.ObjectStateData;
                _objectPositionData = saveData.ObjectPositionData;
                _savedWeaponNames = saveData.SavedWeaponNames;
            } else {
                Debug.Log("No Save Data found, creating new Save Data");
                _objectStateData = new Dictionary<string, bool>();
                _objectPositionData = new Dictionary<string, SerializableVector3>();
                _savedWeaponNames = new List<string>();
            }
        }
        [Button]
        void PrintSaveFilePath() {
            var savePath = Path.Combine(Application.persistentDataPath, SaveName);
            Debug.Log("Save file path: " + savePath);
        }

        public void RegisterObject(string itemName, bool objectState) {
            _objectStateData[itemName] = objectState;

            Save();
        }

        public void RegisterObject(string itemName, Vector3 objectPosition) {
            _objectPositionData[itemName] = new SerializableVector3(objectPosition);

            Save();
        }

        public void RegisterWeapon(string weaponName) {
            if (!_savedWeaponNames.Contains(weaponName)) {
                _savedWeaponNames.Add(weaponName);
            }

            Save();
        }

        public List<WeaponSO> LoadWeapons() {
            var loadedWeapons = new List<WeaponSO>();
            foreach (var weaponName in _savedWeaponNames) {
                var weapon = System.Array.Find(availableWeapons, w => w.weaponName == weaponName);
                if (weapon != null) {
                    loadedWeapons.Add(weapon);
                }
            }
            return loadedWeapons;
        }

        void Save() {
            var saveData = new SaveData {
                ObjectStateData = _objectStateData,
                ObjectPositionData = _objectPositionData,
                SavedWeaponNames = _savedWeaponNames
            };

            string json = JsonConvert.SerializeObject(saveData, Formatting.Indented);
            var savePath = Path.Combine(Application.persistentDataPath, SaveName);
            File.WriteAllText(savePath, json);
        }

        public bool? GetObjectState(string itemName) {
            return _objectStateData.TryGetValue(itemName, out var state) ? state : null;
        }

        public Vector3? GetObjectPosition(string itemName) {
            return _objectPositionData.TryGetValue(itemName, out var position) ? position.ToVector3() : null;
        }

        [Button]
        public void ClearSaveData() {
            _objectStateData.Clear();
            _objectPositionData.Clear();
            _savedWeaponNames.Clear();

            var savePath = Path.Combine(Application.persistentDataPath, SaveName);
            if (File.Exists(savePath)) {
                File.Delete(savePath);
            }
        }
        
        public bool HasSaveData() {
            return _objectStateData.Count > 0 || _objectPositionData.Count > 0 || _savedWeaponNames.Count > 0 || File.Exists(Path.Combine(Application.persistentDataPath, SaveName));
        }

        [System.Serializable]
        public class SaveData {
            public Dictionary<string, bool> ObjectStateData = new();
            public Dictionary<string, SerializableVector3> ObjectPositionData = new();
            public List<string> SavedWeaponNames = new();
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