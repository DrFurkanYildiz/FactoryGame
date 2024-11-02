using System.Collections;
using System.Collections.Generic;
using GridSystem;
using UnityEngine;

public abstract class PlacedObject : MonoBehaviour
{
    public static PlacedObject Create(Grid<Tile> grid, Vector3 worldPosition, Vector2Int origin, Dir dir, PlacedObjectTypeSo placedObjectTypeSo) {
        var placedObjectTransform = 
            Instantiate(placedObjectTypeSo.prefab, worldPosition, Quaternion.Euler(0, PlacedObjectTypeSo.GetRotationAngle(dir), 0));
        var placedObject = placedObjectTransform.GetComponent<PlacedObject>();

        placedObject.Grid = grid;
        placedObject.PlacedObjectTypeSo = placedObjectTypeSo;
        placedObject.Origin = origin;
        placedObject.Dir = dir;
        
        placedObject.Setup();
        return placedObject;
    }

    protected Grid<Tile> Grid;
    protected PlacedObjectTypeSo PlacedObjectTypeSo;
    public Vector2Int Origin { get; private set; }
    public Dir Dir { get; private set; }
    
    public PlacedObject TunnelPlacedObject { get; set; }
    public ConveyorTunnelType ConveyorTunnelType { get; set; }

    protected virtual void Setup() {}
    public List<Vector2Int> GetGridPositionList() => PlacedObjectTypeSo.GetGridPositionList(Origin, Dir);

    public virtual void DestroySelf()
    {
        foreach (var gridPosition in GetGridPositionList())
            Grid.GetGridObject(gridPosition.x, gridPosition.y).ClearPlacedObject();
        Destroy(gameObject);
    }
}
