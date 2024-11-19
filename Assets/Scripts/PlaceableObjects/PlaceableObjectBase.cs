using System.Collections;
using System.Collections.Generic;
using GridSystem;
using UnityEngine;

public abstract class PlaceableObjectBase : MonoBehaviour
{
    public static PlaceableObjectBase Create(Grid<Tile> grid, Vector3 worldPosition, Vector2Int origin, Dir dir, PlaceableObjectBaseSo placeableObjectSo) {
        var placedObjectTransform = 
            Instantiate(placeableObjectSo.prefab, worldPosition, Quaternion.Euler(0, PlaceableObjectSo.GetRotationAngle(dir), 0));
        var placedObject = placedObjectTransform.GetComponent<PlaceableObjectBase>();

        placedObject.Grid = grid;
        placedObject.placeableObjectSo = placeableObjectSo;
        placedObject.Origin = origin;
        placedObject.Dir = dir;
        
        placedObject.Setup();
        return placedObject;
    }

    protected Grid<Tile> Grid;
    protected PlaceableObjectBaseSo placeableObjectSo;
    public Vector2Int Origin { get; private set; }
    public Dir Dir { get; private set; }
    
    public PlaceableObjectBase TunnelPlaceableObjectBase { get; set; }
    public ConveyorTunnelType ConveyorTunnelType { get; set; }
    public Tile CurrentTile => Grid.GetGridObject(Origin);

    protected virtual void Setup() {}
    public List<Vector2Int> GetGridPositionList() => placeableObjectSo.GetGridPositionList(Origin, Dir);

    public virtual void DestroySelf()
    {
        foreach (var gridPosition in GetGridPositionList())
            Grid.GetGridObject(gridPosition.x, gridPosition.y).ClearPlacedObject();
        Destroy(gameObject);
    }
}
