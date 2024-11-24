using UnityEngine;

namespace Extensions {
    public static class PositionExtensions {
        public static Vector3 GetMouseWorldPosition(Camera mainCamera) {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            if (groundPlane.Raycast(ray, out var rayLength)) {
                Vector3 point = ray.GetPoint(rayLength);
                point.y = 0;
                return point;
            }
            return Vector3.zero;
        }
    }
}
