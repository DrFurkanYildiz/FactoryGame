using System;
using TMPro;
using UnityEngine;

namespace GridSystem
{
    public class Grid<TGridObject> {

        public event EventHandler<OnGridObjectChangedEventArgs> OnGridObjectChanged;
        public class OnGridObjectChangedEventArgs : EventArgs {
            public int x;
            public int z;
        }

        private readonly int _width;
        private readonly int _height;
        private readonly float _cellSize;
        private readonly Vector3 _originPosition;
        private readonly TGridObject[,] _gridArray;

        public Grid(int width, int height, float cellSize, Vector3 originPosition, Func<Grid<TGridObject>, int, int, TGridObject> createGridObject) {
            _width = width;
            _height = height;
            _cellSize = cellSize;
            _originPosition = originPosition;

            _gridArray = new TGridObject[width, height];

            for (int x = 0; x < _gridArray.GetLength(0); x++) {
                for (int z = 0; z < _gridArray.GetLength(1); z++) {
                    _gridArray[x, z] = createGridObject(this, x, z);
                }
            }
        
            var showDebug = true;
            if (showDebug)
            {
                TextMeshPro[,] debugTextArray = new TextMeshPro[width, height];

                for (int x = 0; x < _gridArray.GetLength(0); x++)
                {
                    for (int z = 0; z < _gridArray.GetLength(1); z++)
                    {
                        debugTextArray[x, z] = Helpers.CreateWorldTextPro(GameObject.Find("DebugText").transform, _gridArray[x, z]?.ToString(), GetWorldPosition(x, z) + new Vector3(cellSize, 0, cellSize) * .5f, .5f, Color.black);

                        Debug.DrawLine(GetWorldPosition(x, z), GetWorldPosition(x, z + 1), Color.white, 100f);
                        Debug.DrawLine(GetWorldPosition(x, z), GetWorldPosition(x + 1, z), Color.white, 100f);
                    }
                }
                Debug.DrawLine(GetWorldPosition(0, height), GetWorldPosition(width, height), Color.white, 100f);
                Debug.DrawLine(GetWorldPosition(width, 0), GetWorldPosition(width, height), Color.white, 100f);

                OnGridObjectChanged += (object sender, OnGridObjectChangedEventArgs eventArgs) => {
                    debugTextArray[eventArgs.x, eventArgs.z].text = _gridArray[eventArgs.x, eventArgs.z]?.ToString();
                };
            }
        }

        public int GetWidth() {
            return _width;
        }

        public int GetHeight() {
            return _height;
        }

        public float GetCellSize() {
            return _cellSize;
        }

        public Vector3 GetWorldPosition(int x, int z) 
        {
            return new Vector3(x, 0, z) * _cellSize + _originPosition;
        }

        public Vector3 GetWorldPosition(Vector2Int coordinate)
        {
            return GetWorldPosition(coordinate.x, coordinate.y);
        }
        public Vector3 GetWorldPosition(Vector3 position)
        {
            GetXZ(position, out var x, out var z);
            return GetWorldPosition(x, z);
        }
        public Vector2Int GetCoordinate(Vector3 worldPosition)
        {
            GetXZ(worldPosition, out int x, out int z);
            return new Vector2Int(x, z);
        }
        public void GetXZ(Vector3 worldPosition, out int x, out int z) {
            x = Mathf.FloorToInt((worldPosition - _originPosition).x / _cellSize);
            z = Mathf.FloorToInt((worldPosition - _originPosition).z / _cellSize);
        }

        public Vector3 GetOriginPosition()
        {
            return _originPosition;
        }
        public void SetGridObject(int x, int z, TGridObject value) {
            if (x >= 0 && z >= 0 && x < _width && z < _height) {
                _gridArray[x, z] = value;
                TriggerGridObjectChanged(x, z);
            }
        }

        public void TriggerGridObjectChanged(int x, int z) {
            OnGridObjectChanged?.Invoke(this, new OnGridObjectChangedEventArgs { x = x, z = z });
        }

        public void SetGridObject(Vector3 worldPosition, TGridObject value) {
            GetXZ(worldPosition, out int x, out int z);
            SetGridObject(x, z, value);
        }

        public TGridObject GetGridObject(int x, int z) {
            if (x >= 0 && z >= 0 && x < _width && z < _height) {
                return _gridArray[x, z];
            } else {
                return default(TGridObject);
            }
        }

        public TGridObject GetGridObject(Vector3 worldPosition) {
            int x, z;
            GetXZ(worldPosition, out x, out z);
            return GetGridObject(x, z);
        }
        public TGridObject GetGridObject(Vector2Int coordinate)
        {
            return GetGridObject(coordinate.x, coordinate.y);
        }

        public Vector2Int ValidateGridPosition(Vector2Int gridPosition) {
            return new Vector2Int(
                Mathf.Clamp(gridPosition.x, 0, _width - 1),
                Mathf.Clamp(gridPosition.y, 0, _height - 1)
            );
        }

        public bool IsValidGridPosition(Vector2Int gridPosition) {
            int x = gridPosition.x;
            int z = gridPosition.y;

            if (x >= 0 && z >= 0 && x < _width && z < _height) {
                return true;
            } else {
                return false;
            }
        }

        public bool IsValidGridPositionWithPadding(Vector2Int gridPosition) {
            Vector2Int padding = new Vector2Int(2, 2);
            int x = gridPosition.x;
            int z = gridPosition.y;

            if (x >= padding.x && z >= padding.y && x < _width - padding.x && z < _height - padding.y) {
                return true;
            } else {
                return false;
            }
        }
    }
}
