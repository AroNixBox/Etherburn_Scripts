using UnityEngine;

namespace Extensions {
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour {
        static T instance;
        static readonly object Lock = new();

        public static T Instance {
            get {
                lock (Lock) {
                    if (instance != null) return instance;

                    instance = (T)FindFirstObjectByType(typeof(T));

                    if (instance != null) return instance;
                    
                    GameObject singletonObject = new GameObject();
                    instance = singletonObject.AddComponent<T>();
                    singletonObject.name = typeof(T) + " (Singleton)";

                    return instance;
                }
            }
        }

        /// <summary>
        /// Determines whether the Singleton should persist across scene loads.
        /// Override this property in derived classes to change the behavior.
        /// </summary>
        protected virtual bool ShouldPersist => false;

        protected virtual void Awake() {
            if (instance == null) {
                instance = this as T;

                if (ShouldPersist) { 
                    DontDestroyOnLoad(gameObject);
                }
            } else if (instance != this) {
                Debug.Log("Instance already exists, destroying object", transform);
                Destroy(gameObject);
            }
        }

        public static void Deinitialize() {
            if (instance != null) {
                Destroy(instance.gameObject);
                instance = null;
            }
        }
    }
}