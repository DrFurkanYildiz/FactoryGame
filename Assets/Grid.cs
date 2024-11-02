using System;
using UnityEngine;
using TMPro;
public class Grid<TGridObject> {

    public event EventHandler<OnGridObjectChangedEventArgs> OnGridObjectChanged;
    public class OnGridObjectChangedEventArgs : EventArgs {
        public int x;
        public int z;
    }

    private int width;
    private int height;
    private float cellSize;
    private Vector3 originPosition;
    private TGridObject[,] gridArray;

    public Grid(int width, int height, float cellSize, Vector3 originPosition, Func<Grid<TGridObject>, int, int, TGridObject> createGridObject) {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.originPosition = originPosition;

        gridArray = new TGridObject[width, height];

        for (int x = 0; x < gridArray.GetLength(0); x++) {
            for (int z = 0; z < gridArray.GetLength(1); z++) {
                gridArray[x, z] = createGridObject(this, x, z);
            }
        }
        
        bool showDebug = true;
        if (showDebug)
        {
            TextMeshPro[,] debugTextArray = new TextMeshPro[width, height];

            for (int x = 0; x < gridArray.GetLength(0); x++)
            {
                for (int z = 0; z < gridArray.GetLength(1); z++)
                {
                    debugTextArray[x, z] = Helpers.CreateWorldTextPro(GameObject.Find("DebugText").transform, gridArray[x, z]?.ToString(), GetWorldPosition(x, z) + new Vector3(cellSize, 0, cellSize) * .5f, .5f, Color.black);

                    Debug.DrawLine(GetWorldPosition(x, z), GetWorldPosition(x, z + 1), Color.white, 100f);
                    Debug.DrawLine(GetWorldPosition(x, z), GetWorldPosition(x + 1, z), Color.white, 100f);
                }
            }
            Debug.DrawLine(GetWorldPosition(0, height), GetWorldPosition(width, height), Color.white, 100f);
            Debug.DrawLine(GetWorldPosition(width, 0), GetWorldPosition(width, height), Color.white, 100f);

            OnGridObjectChanged += (object sender, OnGridObjectChangedEventArgs eventArgs) => {
                debugTextArray[eventArgs.x, eventArgs.z].text = gridArray[eventArgs.x, eventArgs.z]?.ToString();
            };
        }
    }

    public int GetWidth() {
        return width;
    }

    public int GetHeight() {
        return height;
    }

    public float GetCellSize() {
        return cellSize;
    }

    public Vector3 GetWorldPosition(int x, int z) 
    {
        return new Vector3(x, 0, z) * cellSize + originPosition;
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
        x = Mathf.FloorToInt((worldPosition - originPosition).x / cellSize);
        z = Mathf.FloorToInt((worldPosition - originPosition).z / cellSize);
    }

    public Vector3 GetOriginPosition()
    {
        return originPosition;
    }
    public void SetGridObject(int x, int z, TGridObject value) {
        if (x >= 0 && z >= 0 && x < width && z < height) {
            gridArray[x, z] = value;
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
        if (x >= 0 && z >= 0 && x < width && z < height) {
            return gridArray[x, z];
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
            Mathf.Clamp(gridPosition.x, 0, width - 1),
            Mathf.Clamp(gridPosition.y, 0, height - 1)
        );
    }

    public bool IsValidGridPosition(Vector2Int gridPosition) {
        int x = gridPosition.x;
        int z = gridPosition.y;

        if (x >= 0 && z >= 0 && x < width && z < height) {
            return true;
        } else {
            return false;
        }
    }

    public bool IsValidGridPositionWithPadding(Vector2Int gridPosition) {
        Vector2Int padding = new Vector2Int(2, 2);
        int x = gridPosition.x;
        int z = gridPosition.y;

        if (x >= padding.x && z >= padding.y && x < width - padding.x && z < height - padding.y) {
            return true;
        } else {
            return false;
        }
    }
}
