using UnityEngine;

namespace Extensions {
    /// <summary>
    /// Generic Singleton template. Inherits from MonoBehaviour and ensures that only one instance exists
    /// and that it persists across scene loads.
    /// </summary>
    /// <typeparam name="T">The type of the Singleton.</typeparam>
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour {
        static T instance;
        static readonly object Lock = new();

        /// <summary>
        /// Gets the Singleton instance.
        /// </summary>
        public static T Instance {
            get {
                if (applicationIsQuitting) {
                    Debug.LogWarning("[Singleton] Instance '" + typeof(T) +
                                     "' already destroyed on application quit. " +
                                     "Won't create again - returning null.");
                    return null;
                }
                lock (Lock) {
                    if (instance != null) return instance;
                    // Search for an existing instance
                    instance = (T)FindFirstObjectByType(typeof(T));

                    // Create a new instance if one doesn't already exist
                    if (instance != null) { return instance; }
                    GameObject singletonObject = new GameObject();
                    instance = singletonObject.AddComponent<T>();
                    singletonObject.name = typeof(T) + " (Singleton)";
                    DontDestroyOnLoad(singletonObject);
                    return instance;
                }
            }
        }

        static bool applicationIsQuitting;

        /// <summary>
        /// Called when the object is destroyed.
        /// Marks the application as quitting to prevent new Singleton instances from being created.
        /// </summary>
        protected virtual void OnDestroy() {
            applicationIsQuitting = true;
        }

        /// <summary>
        /// Initializes the Singleton instance.
        /// </summary>
        protected virtual void Awake() {
            // If no instance exists, assign this one and mark it as persistent.
            if (instance == null) {
                instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            // If an instance already exists and it's not this one, destroy this object.
            else if (instance != this) {
                Destroy(gameObject);
            }
        }
    }
}
