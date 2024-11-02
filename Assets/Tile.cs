using UnityEngine;
public class Tile : IPathNode
{
    private Grid<Tile> Grid { get; }
    public int x { get; }
    public int y { get; }
    public int gCost { get; set; }
    public int hCost { get; set; }
    public int fCost { get; private set; }

    public bool canBuild => _ownedObject == null;
    public IPathNode cameFromNode { get; set; }
    
    private PlacedObject _ownedObject;
    public PlacedObject OwnedObject
    {
        get => _ownedObject;
        set
        {
            _ownedObject = value;
            TriggerGridObjectChanged();
        }
    }
    public Vector2Int GetGridPosition => new (x, y);

    public Tile(Grid<Tile> grid, int x, int y) {
        Grid = grid;
        this.x = x;
        this.y = y;
        _ownedObject = null;
    }

    public override string ToString() {
        return x + ", " + y + "\n" + _ownedObject;
    }

    private void TriggerGridObjectChanged() => Grid.TriggerGridObjectChanged(x, y);
    public void ClearPlacedObject() => OwnedObject = null;
    public void CalculateFCost()
    {
        fCost = gCost + hCost;
    }
}
