using UnityEngine;

namespace Enemy.Positioning {
    // When the grid is created, each cell is initialized with a GridObject
    public class PositioningGridObject {
        readonly Grid<PositioningGridObject> _grid;
        readonly int _x;
        readonly int _z;
        public bool IsWalkable { get; private set; }
        public bool IsOccupied { get; private set; }
        public Vector3 NavMeshSamplePosition { get; private set; }
        
        // locally store this x, z and the grid reference
        public PositioningGridObject(Grid<PositioningGridObject> grid, int x, int z, bool isWalkable, Vector3 navMeshSamplePosition) {
            _grid = grid;
            _x = x;
            _z = z;
            IsWalkable = isWalkable;
            NavMeshSamplePosition = navMeshSamplePosition;
        }
            
        // Assign a building to this cell
        public void SetOccupyState(bool isOccupied) {
            IsOccupied = isOccupied;
        }
        
        // Center of the Cell, maybe to place world space UI
        public Vector3 GetCellCenterWorldPosition() {
            Vector3 worldPosition = _grid.GetWorldPosition(_x, _z);
            Vector3 tileCenterPosition = 
                new Vector3(
                    worldPosition.x + _grid.GetCellSize() / 2,
                    0, 
                    worldPosition.z + _grid.GetCellSize() / 2);
            return tileCenterPosition;
        }
        
        // Override the ToString method to print the x, z of the cell whenever it is called
        public override string ToString() {
            return _x + ", " + _z + "\n";
        }
    }
}