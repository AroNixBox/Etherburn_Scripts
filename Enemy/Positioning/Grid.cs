using System;
using System.Text;
using Extensions;
using TMPro;
using UnityEngine;

namespace Enemy.Positioning {
    // Generic Base Grid Class to create a Grid with any type of Object.
    public class Grid<TGridObject> {
        // Invoked when building is placed, upgraded or deleted
        public event EventHandler<Vector2Int> OnPlacedObjectChanged;
    
        // Entire Grid width
        readonly int _width;
        // Entire Grid height
        readonly int _height;
        // Size of each cell
        readonly float _cellSize;
        // Middle of the Grid
        readonly Vector3 _originPosition;
    
        // 2D Array of the Grid
        readonly TGridObject[,] _gridArray;
        
        public Grid(int width, int height, float cellSize, Vector3 centerPosition, 
            Func<Grid<TGridObject>, int, int, TGridObject> createGridObject, bool showDebug) {
            // X
            _width = width;
            // Z
            _height = height;
            // Size of each cell
            _cellSize = cellSize;

            //Calculate the origin position
            _originPosition = centerPosition - new Vector3(width, 0, height) * cellSize * 0.5f;
        
            _gridArray = new TGridObject[width, height];
        
            // 2D Array of the Debug Text
            var debugTextArray = new TextMeshPro[width][];
            for (int index = 0; index < width; index++) {
                debugTextArray[index] = new TextMeshPro[height];
            }

            //Create the Grid Object with whatever type is passed in
            for(var x = 0; x < _gridArray.GetLength(0); x++) {
                for(var z = 0; z < _gridArray.GetLength(1); z++) {
                    _gridArray[x, z] = createGridObject(this, x, z);
                }
            }
        
            if(!showDebug) { return; }
        
            // Subscribe to the OnPlacedObjectChanged event
            OnPlacedObjectChanged += (_, eventArgs) => {
                var posX = eventArgs.x;
                var posZ = eventArgs.y;
                
                var gridObject = GetGridObject(posX, posZ);
                StringBuilder debugText = new StringBuilder();

                switch (gridObject) {
                    case null:
                        debugText.Append("null");
                        break;
                    case PositioningGridObject positioningGridObject:
                        // Change the debug text of a specific cell
                        // In 3D y represents z (because Unity is z forward)
                    
                        debugText.Append("Is Walkable: ");
                        debugText.Append(positioningGridObject.IsWalkable.ToString());
                    
                        debugText.Append("\n");
                    
                        debugText.Append("Is Occupied: ");
                        debugText.Append(positioningGridObject.IsOccupied.ToString());
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
            for(var x = 0; x < _gridArray.GetLength(0); x++) {
                //Cycle through the second dimension of the array
                for(int z = 0; z < _gridArray.GetLength(1); z++) {
                    debugTextArray[x][z] = TextExtensions.CreateWorldText(
                        "Not Initialized", parent, 
                        //Center the text in the middle of the cell
                        GetWorldPosition(x, z) + new Vector3(cellSize, 0,cellSize) * 0.5f, 
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
            return new Vector3(x, 0, z) * _cellSize + _originPosition;
        }
        
        // Converts world position to a XZ position
        public void GetXZ(Vector3 worldPosition, out int x, out int z) {
            x = Mathf.FloorToInt((worldPosition - _originPosition).x / _cellSize);
            z = Mathf.FloorToInt((worldPosition - _originPosition).z / _cellSize);
        }
    
        // Get the Grid Object based on the XZ-Grid Position => (So World position needs to be converted before calling this)
        public TGridObject GetGridObject(int x, int z) {
            return IsWithinBounds(x, z) ? _gridArray[x, z] :
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
            return x >= 0 && z >= 0 && x < _width && z < _height;
        }

        // Cell Size of the Cell
        public float GetCellSize() {
            return _cellSize;
        }

        public void TriggerGridObjectChanged(Vector2Int gridPosition) {
            OnPlacedObjectChanged?.Invoke(this, gridPosition);
        }
    }
}
