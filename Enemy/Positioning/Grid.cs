using System;
using System.Text;
using Extensions;
using TMPro;
using UnityEngine;

namespace Enemy.Positioning {
    // Generic Base Grid Class to create a Grid with any type of Object.
    public class Grid<TGridObject> {
        // Invoked when building is placed, upgraded or deleted
        public event EventHandler<Vector2Int> OnGridCellValueChanged;
    
        // Entire Grid width
        public readonly int Width;
        // Entire Grid height
        public readonly int Height;
        // Size of each cell
        public readonly int CellSize;
        // Middle of the Grid
        readonly Vector3 _calculatedCenterPosition;
        public readonly Vector3 OriginPosition;
    
        // 2D Array of the Grid
        public readonly TGridObject[,] GridArray;
        
        public Grid(int width, int height, int cellSize, Vector3 originPosition, 
            Func<Grid<TGridObject>, int, int, TGridObject> createGridObject) {
            // X
            Width = width;
            // Z
            Height = height;
            // Size of each cell
            CellSize = cellSize;

            OriginPosition = originPosition;
            //Calculate the origin position
            _calculatedCenterPosition = originPosition - new Vector3(width, 0, height) * cellSize * 0.5f;
        
            GridArray = new TGridObject[width, height];
        
            //Create the Grid Object with whatever type is passed in
            for(var x = 0; x < GridArray.GetLength(0); x++) {
                for(var z = 0; z < GridArray.GetLength(1); z++) {
                    GridArray[x, z] = createGridObject(this, x, z);
                }
            }
            
            if(!Application.isPlaying) { return; }
            
            // 2D Array of the Debug Text
            var debugTextArray = new TextMeshPro[width][];
            for (int index = 0; index < width; index++) {
                debugTextArray[index] = new TextMeshPro[height];
            }
            
            // Subscribe to the OnPlacedObjectChanged event
            OnGridCellValueChanged += (_, eventArgs) => {
                var posX = eventArgs.x;
                var posZ = eventArgs.y;
                
                var gridObject = GetGridObject(posX, posZ);
                var debugText = new StringBuilder();

                switch (gridObject) {
                    case null:
                        debugText.Append("null");
                        break;
                    case PositioningGridObject positioningGridObject:
                        // Change the debug text of a specific cell
                        // In 3D y represents z (because Unity is z forward)

                        debugText.Append("<b>");
                        debugText.Append(!positioningGridObject.IsOccupied ? "<color=green>Unoccupied</color>" : "<color=red>Occupied</color>");
                        debugText.Append("</b>");
                        break;
                    default:
                        debugText.Append("Debug Texttype not implemented");
                        break;
                }
                
                var debugPosition = $"{posX}, {posZ}";
                debugText.Append("\n" + debugPosition);
                
                // Change the debug text of a specific cell
                // In 3D y represents z (because Unity is z forward)
                debugTextArray[eventArgs.x][eventArgs.y].text = debugText.ToString();
            };
            
            var parent = new GameObject("Visual Grid").transform;
        
            //Cycle through the first dimension of the array
            for(var x = 0; x < GridArray.GetLength(0); x++) {
                //Cycle through the second dimension of the array
                for(var z = 0; z < GridArray.GetLength(1); z++) {
                    float individualHeight = 0;
                    if(GetGridObject(x, z) is PositioningGridObject positioningGridObject) {
                        individualHeight = positioningGridObject.NavMeshSamplePosition.y;
                    }
                    var worldPosition = GetWorldPosition(x, z);
                    var worldPositionWithSampleHeight = new Vector3(worldPosition.x, individualHeight, worldPosition.z);
                    debugTextArray[x][z] = TextExtensions.CreateWorldText(
                        "Not Initialized", parent,
                        //Center the text in the middle of the cell
                        worldPositionWithSampleHeight + new Vector3(cellSize, 0 ,cellSize) * 0.5f, 
                        cellSize, 3, Color.white, TextAlignmentOptions.Center, 10);
                    
                    TriggerGridObjectChanged(new Vector2Int(x, z));
                
                
                    //Vertical lines
                    Debug.DrawLine(GetWorldPosition(x, z), GetWorldPosition(x, z +1), Color.white, Mathf.Infinity);
                
                    //Horizontal lines
                    Debug.DrawLine(GetWorldPosition(x, z), GetWorldPosition(x +1, z), Color.white, Mathf.Infinity);
                }
            }
            //Horizontal outside line
            Debug.DrawLine(GetWorldPosition(0, height), GetWorldPosition(width, height), Color.white, Mathf.Infinity);
        
            //Vertical outside line
            Debug.DrawLine(GetWorldPosition(width, 0), GetWorldPosition(width, height), Color.white, Mathf.Infinity);
        }
        
        // Converts XZ position to World Position
        public Vector3 GetWorldPosition(int x, int z) {
            return new Vector3(x, 0, z) * CellSize + _calculatedCenterPosition;
        }
        
        // Converts world position to a XZ position
        public void GetXZ(Vector3 worldPosition, out int x, out int z) {
            x = Mathf.FloorToInt((worldPosition - _calculatedCenterPosition).x / CellSize);
            z = Mathf.FloorToInt((worldPosition - _calculatedCenterPosition).z / CellSize);
        }
    
        // Get the Grid Object based on the XZ-Grid Position => (So World position needs to be converted before calling this)
        public TGridObject GetGridObject(int x, int z) {
            return IsWithinBounds(x, z) ? GridArray[x, z] :
                //Value is Invalid, but throw no exception
                default;
        }
    
        //Get the value based on the world position (e.g. mouse hit ground position), no convert needed
        public TGridObject GetGridObject(Vector3 worldPosition) {
            GetXZ(worldPosition, out var x, out var z);
        
            return GetGridObject(x, z);
        }
    
        // Did we click on the Grid?
        public bool IsWithinBounds(int x, int z) {
            return x >= 0 && z >= 0 && x < Width && z < Height;
        }

        public Vector3 GetCellCenterPositionInWorldSpace(int x, int z) {
            return GetWorldPosition(x, z) + new Vector3(CellSize, 0, CellSize) * 0.5f;
        }
        public void TriggerGridObjectChanged(Vector2Int gridPosition) {
            OnGridCellValueChanged?.Invoke(this, gridPosition);
        }
        public void TriggerGridObjectChanged(int x, int z) {
            TriggerGridObjectChanged(new Vector2Int(x, z));
        }
    }
    
    [Serializable]
    public class PositioningGridObjectData {
        public int x;
        public int z;
        public bool isOnNavMesh;
        public Vector3 highestReachableNavMeshPosition;

        public PositioningGridObjectData(int x, int z, bool isOnNavMesh, Vector3 position) {
            this.x = x;
            this.z = z;
            this.isOnNavMesh = isOnNavMesh;
            highestReachableNavMeshPosition = position;
        }
    }
}
