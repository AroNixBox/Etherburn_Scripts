using System;
using UnityEngine;

namespace Enemy.Positioning {
    /// <summary> Data class for PositioningGridObject </summary>
    [Serializable]
    public class PositioningGridObjectData {
        public int x;
        public int z;
        public bool isOnNavMesh;
        // The position on the navmesh that is reachable by the test object. From top to bottom,
        // since we only have a 2D Grid there is only one position per grid cell. And I chose the highest one.
        public Vector3 highestReachableNavMeshPosition;

        public PositioningGridObjectData(int x, int z, bool isOnNavMesh, Vector3 position) {
            this.x = x;
            this.z = z;
            this.isOnNavMesh = isOnNavMesh;
            highestReachableNavMeshPosition = position;
        }
    }
}