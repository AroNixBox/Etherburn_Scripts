﻿using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Enemy.Positioning {
    /// <summary> Wrapper Class to serialize the grid - So it can be created off runtime </summary>
    [PreferBinarySerialization] // Binary Serialization is faster than the default JSON
    public class GridWrapper : ScriptableObject {
        [ReadOnly] public int gridWidth;
        [ReadOnly] public int gridHeight;
        [ReadOnly] public int cellSize;
        [ReadOnly] public Vector3 originPosition;
        [ReadOnly] public Vector2 gridOffset;
        [ReadOnly] public PositioningGridObjectData[] gridData;
        // Setter
        public void InitializeFromGrid(Grid<PositioningGridObject> grid) {
            // Initialize the grid data
            gridWidth = grid.Width;
            gridHeight = grid.Height;
            cellSize = grid.CellSize;
            originPosition = grid.OriginPosition;

            // If the grid has filtered cells, the GridArray will be smaller than the [width * height]
            var filteredCells = new List<PositioningGridObjectData>();

            // Iterate over all possible cells
            for (var x = 0; x < gridWidth; x++) {
                for (var z = 0; z < gridHeight; z++) {
                    // try get the grid object
                    var gridObject = grid.GridArray[x, z];
                    if (gridObject == null) { continue; }  // Null if the cell was removed beforehand

                    // Add the grid object to the filtered cells
                    filteredCells.Add(new PositioningGridObjectData(
                        gridObject.X,
                        gridObject.Z,
                        gridObject.IsWalkable,
                        gridObject.NavMeshSamplePosition
                    ));
                }
            }

            // Save the filtered cells
            gridData = filteredCells.ToArray();
        }


        public Grid<PositioningGridObject> ToGrid() {
            var includedCells = new List<Vector2Int>();

            foreach (var cellData in gridData) {
                if (cellData == null) {
                    Debug.LogWarning("GridWrapper: GridData is null - <b>Skipping</b>");
                    continue;
                }

                // Add the cell position to the included cells
                includedCells.Add(new Vector2Int(cellData.x, cellData.z));
            }

            // Create the grid and return it
            return new Grid<PositioningGridObject>(
                gridWidth,
                gridHeight,
                cellSize,
                originPosition + new Vector3(gridOffset.x, 0, gridOffset.y),
                (g, x, z) => {
                    var cellData = gridData.FirstOrDefault(data => data.x == x && data.z == z);
                    if (cellData == null) return null;
                    return new PositioningGridObject(
                        cellData.x,
                        cellData.z,
                        cellData.isOnNavMesh,
                        cellData.highestReachableNavMeshPosition
                    );
                },
                includedCells
            );
        }
    }
}
