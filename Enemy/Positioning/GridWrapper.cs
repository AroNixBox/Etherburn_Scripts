using UnityEngine;

namespace Enemy.Positioning {
    public class GridWrapper : ScriptableObject {
        public int gridWidth;
        public int gridHeight;
        public int cellSize;
        public Vector3 originPosition;
        public Vector2 gridOffset;
        public PositioningGridObjectData[] gridData;
        public void InitializeFromGrid(Grid<PositioningGridObject> grid) {
            gridWidth = grid.Width;
            gridHeight = grid.Height;
            cellSize = grid.CellSize;
            originPosition = grid.OriginPosition;

            gridData = new PositioningGridObjectData[gridWidth * gridHeight];
            for (var x = 0; x < gridWidth; x++) {
                for (var z = 0; z < gridHeight; z++) {
                    var gridObject = grid.GridArray[x, z];
                    gridData[x * gridHeight + z] = new PositioningGridObjectData(
                        gridObject.x,
                        gridObject.z,
                        gridObject.IsWalkable,
                        gridObject.NavMeshSamplePosition
                    );
                }
            }
        }

        public Grid<PositioningGridObject> ToGrid() {
            return new Grid<PositioningGridObject>(
                gridWidth,
                gridHeight,
                cellSize,
                originPosition + new Vector3(gridOffset.x, 0, gridOffset.y),
                (g, x, z) => {
                    var cellData = gridData[x * gridHeight + z];
                    return new PositioningGridObject(
                        cellData.x,
                        cellData.z,
                        cellData.isOnNavMesh,
                        cellData.highestReachableNavMeshPosition
                    );
                }
            );
        }
    }
}