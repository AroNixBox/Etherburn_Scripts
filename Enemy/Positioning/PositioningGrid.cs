using System;
using System.Linq;
using Drawing;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

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
        
        [BoxGroup("SaveLoadGroup")]
        [Tooltip("Position from where we check if a position is reachable on the NavMesh, should be an Enemy on the NavMesh")]
        [SerializeField] Transform navMeshReachabilityChecker;
        
        // Query
        [BoxGroup("Query Settings")]
        [HorizontalGroup("Query Settings/Split", LabelWidth = 100)]
        [PreviewField(Alignment = ObjectFieldAlignment.Left)]
        [SerializeField] GameObject player;

        [SerializeField] Transform testTarget;

        [VerticalGroup("Query Settings/Split/Range")]
        [SerializeField] float minQueryRange;

        [VerticalGroup("Query Settings/Split/Range")]
        [SerializeField] float maxQueryRange;
        
        
        // Grid, where each Cell is a PositioningGridObject
        Grid<PositioningGridObject> _grid;

        const string ResourcesPath = "Assets/Resources/";
        const string FileName = "PositioningData/GridPositioningWrapper";
        void Start() {
            LoadGridFromScriptableObject();
        }

        void LoadGridFromScriptableObject() {
            var savedGridData = Resources.Load<GridWrapper>(FileName);
            if (savedGridData != null) {
                _grid = savedGridData.ToGrid();
                Debug.Log("Grid successfully loaded from Resources!"); 
            }else {
                Debug.LogError("No saved grid data found in Resources.");
            }
        }

        void Update() {
            _ = GetClosestPositionInRange(testTarget);
        }


        [BoxGroup("SaveLoadGroup")]
        [GUIColor(0.4f, 0.8f, 1.0f)]
        [Button(ButtonSizes.Large)]
        void SaveGridToScriptableObject() {
            var grid = SaveGridPosition();

            // New ScriptableObject
            GridWrapper gridWrapper = ScriptableObject.CreateInstance<GridWrapper>();

            // Convert Grid to GridWrapper
            gridWrapper.InitializeFromGrid(grid);

            // Save the ScriptableObject with the GridWrapperData
#if UNITY_EDITOR
            const string fullAssetPath = ResourcesPath + FileName + ".asset";
            UnityEditor.AssetDatabase.CreateAsset(gridWrapper, fullAssetPath);
            UnityEditor.AssetDatabase.SaveAssets();
            Debug.Log($"Grid data saved to {ResourcesPath}");
#endif
        }

        
        
        public Vector3? GetClosestPositionInRange(Transform target) {
            if(target == null) {
                Debug.LogError("Test Target is null.");
                return null;
            }
            
            // Get Grid Coordinates of the Test Center
            _grid.GetXZ(player.transform.position, out var centerX, out var centerZ);

            // Calculate the Grid Radius based on the Max Range
            var gridRadius = Mathf.CeilToInt(maxQueryRange / _grid.CellSize);
    
            PositioningGridObject closestGridObject = null;
            var closestDistanceSqr = float.MaxValue;

            for (var dx = -gridRadius; dx <= gridRadius; dx++) {
                for (var dz = -gridRadius; dz <= gridRadius; dz++) {
                    var x = centerX + dx;
                    var z = centerZ + dz;

                    // Just to prevent IndexOutOfBounds
                    if (x < 0 || x >= _grid.Width || z < 0 || z >= _grid.Height) continue;
                    
                    var gridDistanceSqr = dx * _grid.CellSize * dx * _grid.CellSize + 
                                            dz * _grid.CellSize * dz * _grid.CellSize;
                    var minRangeSqr = minQueryRange * minQueryRange;
                    var maxRangeSqr = maxQueryRange * maxQueryRange;
                    
                    // Skip Cells smaller than Min Range or bigger than Max Range
                    if (gridDistanceSqr < minRangeSqr || gridDistanceSqr > maxRangeSqr) continue;
                    // Get the Grid Object of all Cells in Range
                    var gridObject = _grid.GetGridObject(x, z);

                    if (gridObject == null) {
                        Debug.LogWarning("Grid Object of Cell with X: " + x + " Z: " + z + " is null.");
                        continue;
                    }
                    
                    // Skip non-walkable cells
                    if (!gridObject.IsWalkable) { continue; }

                    // Skip Occupied Cells
                    if (gridObject.IsOccupied) { continue; }
                    
                    var distanceSqr = (target.position - gridObject.NavMeshSamplePosition).sqrMagnitude;
                    if (distanceSqr < closestDistanceSqr) {
                        closestDistanceSqr = distanceSqr;
                        closestGridObject = gridObject;
                    }
                    
                    // Draw a Box at the Cell Position
                    var cellPosition = gridObject.NavMeshSamplePosition;
                    Draw.SolidBox(
                        new Bounds(cellPosition, Vector3.one * (_grid.CellSize * 0.8f)),
                        Color.cyan
                    );
                }
            }
            // Draw the closest Cell red
            if (closestGridObject == null) {
                // TODO: Implement Fallback
                Debug.LogError("No reachable position found.");
                return null;
            }
            
            Draw.SolidBox(
                new Bounds(closestGridObject.NavMeshSamplePosition, Vector3.one * (_grid.CellSize * 0.8f)),
                Color.red
            );
                
            return closestGridObject.NavMeshSamplePosition;
        }
        
        Grid<PositioningGridObject> SaveGridPosition() {
            if (boundsTransforms is not {Length: 4}) {
                Debug.LogError("Bounds Transforms are not set correctly.");
                return null;
            }
            
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

            var grid = new Grid<PositioningGridObject>(
                cellCountWidth,
                cellCountHeight,
                cellSize,
                centerPosition + new Vector3(gridOffset.x, 0, gridOffset.y),
                (g, x, z) => {
                    // Get world position of the cell center
                    var worldPosition = g.GetCellCenterPositionInWorldSpace(x, z);

                    var highestReachableNavMeshPosition = GetHighestReachableNavMeshPosition(worldPosition);
                    var isOnNavMesh = highestReachableNavMeshPosition != Vector3.zero;

                    return new PositioningGridObject(x, z, isOnNavMesh, highestReachableNavMeshPosition);
                });
            
            return grid;
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

                if (!NavMesh.CalculatePath(navMeshReachabilityChecker.position, navMeshHit.position, NavMesh.AllAreas,
                        path) || path.status != NavMeshPathStatus.PathComplete) { continue; }
                if (!(navMeshHit.position.y > highestYPosition)) { continue; }
                
                highestYPosition = navMeshHit.position.y;
                highestPosition = navMeshHit.position;
            }

            return Mathf.Approximately(highestYPosition, float.MinValue) ? Vector3.zero : highestPosition;
        }
        

        public void OccupyCell(Vector3 worldPosition, bool isOccupied) {
            _grid.GetXZ(worldPosition, out var x, out var z);
            Vector2Int gridPosition = new Vector2Int(x, z);
            _grid.GetGridObject(worldPosition).SetOccupyState(isOccupied);
            _grid.TriggerGridObjectChanged(gridPosition);
        }
        
        void OnDrawGizmos() {
            if (!drawDebugGrid) { return; }
            if (boundsTransforms is not { Length: 4 }) {
                // Bounds are not set correctly
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

            for (var x = 0; x < cellCountWidth; x++) {
                for (var z = 0; z < cellCountHeight; z++) {
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