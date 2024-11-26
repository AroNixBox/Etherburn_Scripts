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
        
        [BoxGroup("SaveLoadGroup")]
        [Tooltip("Position from where we check if a position is reachable on the NavMesh, should be an Enemy on the NavMesh")]
        [SerializeField] Transform navMeshReachabilityChecker;
        
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