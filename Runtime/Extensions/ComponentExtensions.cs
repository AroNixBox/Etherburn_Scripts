namespace Extensions {
    using UnityEngine;

    public static class ComponentExtensions {
        public static bool TryGetComponentInParent<T>(this Component component, out T result) where T : class {
            result = component.GetComponentInParent<T>();
            return result != null;
        }
    }
}