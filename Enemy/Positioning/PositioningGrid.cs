using System.Linq;
using Drawing;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;

namespace Enemy.Positioning {
    public class PositioningGrid : MonoBehaviour {
        [BoxGroup("Debug Grid")]
        [HorizontalGroup("Debug Grid/Split", LabelWidth = 100)] 
        [SerializeField] bool drawDebugGrid;
    
        [BoxGroup("Debug Grid")]
        [ShowIf("drawDebugGrid")]
        [SerializeField] Color debugGridColor = Color.red;
    
        [Header("Values")]
        [SerializeField, Range(1,15)] int cellSize = 2;
        // Y = Z, because Y is up and we use z for the grid-"height"
        [SerializeField] Vector2 gridOffset;
    
        [Tooltip("Four Corners of the Grid")]
        [SerializeField] Transform[] boundsTransforms;
        
        
        // Grid, where each Cell is a PositioningGridObject
        Grid<PositioningGridObject> _grid;
        
        void Start() {
            CreateGrid();
        }

        void CreateGrid() {
            // Create min and max values for X and Z based on the 4 Corners
            var minX = boundsTransforms.Min(t => t.position.x);
            var maxX = boundsTransforms.Max(t => t.position.x);
            var minZ = boundsTransforms.Min(t => t.position.z);
            var maxZ = boundsTransforms.Max(t => t.position.z);

            // Calculate the width and height of the grid
            var gridWidth = maxX - minX;
            var gridHeight = maxZ - minZ;

            // Center position of the grid
            var centerPosition = new Vector3((minX + maxX) / 2, 0, (minZ + maxZ) / 2);

            // Calculate the number of cells that can fit in the width and height of the grid
            var cellCountWidth = Mathf.FloorToInt(gridWidth / cellSize);
            var cellCountHeight = Mathf.FloorToInt(gridHeight / cellSize);

            
            _grid = new Grid<PositioningGridObject>(
                cellCountWidth,
                cellCountHeight,
                cellSize,
                centerPosition + new Vector3(gridOffset.x, 0, gridOffset.y),
                (g, x, z) => {
                    // Weltposition der Zelle ohne angepasste Y-Höhe
                    Vector3 worldPosition = g.GetWorldPosition(x, z);

                    // Anpassen der Y-Position auf die Höhe des NavMeshes
                    Vector3 positionOnNavMesh = AdjustPositionToNavMesh(worldPosition);

                    // NavMesh-Check (falls du die Existenz prüfen möchtest)
                    bool isOnNavMesh = positionOnNavMesh != Vector3.zero;

                    // Erstellen des PositioningGridObject
                    return new PositioningGridObject(g, x, z, isOnNavMesh);
                },
                drawDebugGrid
            );


        }
        Vector3 AdjustPositionToNavMesh(Vector3 position) {
            // TODO: Shoot a ray from that position up,
            // TODO: check all hits and use the first hit position where the hit is on the navmesh
            return new Vector3();
        }



        bool CheckIfPositionOnNavMesh(Vector3 position) {
            // TODO: Sample Position on NavMesh
            return default;
        }

        
        public void OccupyCell(Vector3 worldPosition, bool isOccupied) {
            _grid.GetXZ(worldPosition, out var x, out var z);
            Vector2Int gridPosition = new Vector2Int(x, z);
            _grid.GetGridObject(worldPosition).SetOccupyState(isOccupied);
            _grid.TriggerGridObjectChanged(gridPosition);
        }
        
        void OnDrawGizmos() {
            if (!drawDebugGrid || boundsTransforms == null || boundsTransforms.Length != 4) {
                return;
            }

            var minX = boundsTransforms.Min(t => t.position.x);
            var maxX = boundsTransforms.Max(t => t.position.x);
            var minZ = boundsTransforms.Min(t => t.position.z);
            var maxZ = boundsTransforms.Max(t => t.position.z);

            var gridWidth = maxX - minX;
            var gridHeight = maxZ - minZ;

            var centerPosition = new Vector3((minX + maxX) / 2, 0, (minZ + maxZ) / 2);

            var cellCountWidth = Mathf.FloorToInt(gridWidth / cellSize);
            var cellCountHeight = Mathf.FloorToInt(gridHeight / cellSize);

            Gizmos.color = debugGridColor;

            for (int x = 0; x < cellCountWidth; x++) {
                for (int z = 0; z < cellCountHeight; z++) {
                    var cellCenter = centerPosition + new Vector3(
                        (x - cellCountWidth * 0.5f) * cellSize + cellSize * 0.5f,
                        0,
                        (z - cellCountHeight * 0.5f) * cellSize + cellSize * 0.5f
                    ) + new Vector3(gridOffset.x, 0, gridOffset.y);

                    Gizmos.DrawWireCube(cellCenter, new Vector3(cellSize, 0, cellSize));
                }
            }
        }

    }
}