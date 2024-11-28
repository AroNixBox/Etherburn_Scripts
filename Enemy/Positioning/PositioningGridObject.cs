using UnityEngine;

namespace Enemy.Positioning {
    // When the grid is created, each cell is initialized with a GridObject
    public class PositioningGridObject {
        public readonly int X;
        public readonly int Z;
        public bool IsWalkable { get; private set; }
        public bool IsOccupied { get; private set; }
        public Vector3 NavMeshSamplePosition { get; private set; }
        
        // locally store this x, z and the grid reference
        public PositioningGridObject(int x, int z, bool isWalkable, Vector3 navMeshSamplePosition) {
            X = x;
            Z = z;
            IsWalkable = isWalkable;
            NavMeshSamplePosition = navMeshSamplePosition;
        }
            
        // Assign a building to this cell
        public void SetOccupyState(bool isOccupied) {
            IsOccupied = isOccupied;
        }
        
        // Override the ToString method to print the x, z of the cell whenever it is called
        public override string ToString() {
            return X + ", " + Z + "\n";
        }
    }
}