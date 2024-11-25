using System;
using System.Linq;
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

        [SerializeField] Transform exampleAgentOnNavMesh;
        
        
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
            var centerPosition = new Vector3((minX + maxX) / 2, transform.position.y, (minZ + maxZ) / 2);

            // Calculate the number of cells that can fit in the width and height of the grid
            var cellCountWidth = Mathf.FloorToInt(gridWidth / cellSize);
            var cellCountHeight = Mathf.FloorToInt(gridHeight / cellSize);

            
            _grid = new Grid<PositioningGridObject>(
                cellCountWidth,
                cellCountHeight,
                cellSize,
                centerPosition + new Vector3(gridOffset.x, 0, gridOffset.y),
                (g, x, z) => {
                    // Get world position of the cell center
                    Vector3 worldPosition = g.GetCellCenterPositionInWorldSpace(x, z);
                    
                    Vector3 highestReachableNavMeshPosition = GetHighestReachableNavMeshPosition(worldPosition);
                    bool isOnNavMesh = highestReachableNavMeshPosition != Vector3.zero;
                    if (isOnNavMesh) {
                        Debug.DrawLine(highestReachableNavMeshPosition, highestReachableNavMeshPosition + Vector3.up * .5f, Color.red, Mathf.Infinity);
                    }
                    return new PositioningGridObject(g, x, z, isOnNavMesh, highestReachableNavMeshPosition);
                },
                drawDebugGrid
            );


        }
        Vector3 GetHighestReachableNavMeshPosition(Vector3 position) {
            var ray = new Ray(position, Vector3.down);
            var hits = new RaycastHit[100];
            var hitCount = Physics.RaycastNonAlloc(ray, hits, 500f);
            var highestYPosition = float.MinValue;
            Vector3 highestPosition = default;

            var path = new NavMeshPath();
            for (var i = 0; i < hitCount; i++) {
                var samplePosition = hits[i].point;
                if (!NavMesh.SamplePosition(samplePosition, out var navMeshHit, .25f, NavMesh.AllAreas)) { continue; }

                if (!NavMesh.CalculatePath(exampleAgentOnNavMesh.position, navMeshHit.position, NavMesh.AllAreas,
                        path) || path.status != NavMeshPathStatus.PathComplete) { continue; }
                if (!(navMeshHit.position.y > highestYPosition)) { continue; }
                
                highestYPosition = navMeshHit.position.y;
                highestPosition = navMeshHit.position;
            }

            return Mathf.Approximately(highestYPosition, float.MinValue) ? Vector3.zero : highestPosition;
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

            var centerPosition = new Vector3((minX + maxX) / 2, transform.position.y, (minZ + maxZ) / 2);

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