using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class ConveyorSplitter : PlacedObject, IItemSlot
{
    private Vector2Int _previousPosition;
    private Tile _previousTile;
    private readonly List<Tile> _gatesTiles = new();
    private Item _item;
    private int _currentGateIndex;
    
    protected override void Setup()
    {
        var inputs = PlacedObjectTypeSo.GetIOTypePositionList(IOType.Input, Origin, Dir);
        var outputs = PlacedObjectTypeSo.GetIOTypePositionList(IOType.Output, Origin, Dir);
        
        _previousPosition = inputs[0];
        _previousTile = Grid.GetGridObject(_previousPosition);
        
        foreach (var gate in outputs)
        {
            var gateNeighbours = PlacedObjectTypeSo.GetNeighbours(gate);
            foreach (var gateNeighbour in gateNeighbours)
            {
                if (!GetGridPositionList().Contains(gateNeighbour))
                    _gatesTiles.Add(Grid.GetGridObject(gateNeighbour));
            }
        }
    }
    public override void DestroySelf()
    {
        base.DestroySelf();
        
        if(_item != null)
            _item.DestroySelf();
    }
    private void Update()
    {
        ItemSplitting();
    }

    private void ItemSplitting()
    {
        if (_previousTile.OwnedObject is not IItemSlot previousSlot || _item == null || !_item.CanMove) return;
        if (previousSlot.GetGridPosition().All(p => p != _previousPosition)) return;
        var startIndex = _currentGateIndex;
        
        do
        {
            var tile = _gatesTiles[_currentGateIndex];
        
            if (tile.OwnedObject is IItemSlot nextSlot && 
                nextSlot.GetGridPosition().Any(p => p == tile.GetGridPosition) && 
                nextSlot.TrySetWorldItem(_item) &&
                nextSlot.CanCarryItem(_item.GetItemSo()))
            {
                _item.MoveToItemSlot
                    (nextSlot.GetGridPosition().FirstOrDefault(p=> p == tile.GetGridPosition));
                _item = null;
                _currentGateIndex = (_currentGateIndex + 1) % _gatesTiles.Count;
                break;
            }
            
            _currentGateIndex = (_currentGateIndex + 1) % _gatesTiles.Count;
        } while (_currentGateIndex != startIndex);
    }
    
    public bool CanCarryItem(ItemSo itemSo)
    {
        return itemSo.isSolidItem;
    }
    public bool TrySetWorldItem(Item item) 
    {
        if (_item != null) return false;
        _item = item;
        return true;
    }

    public IEnumerable<Vector2Int> GetGridPosition()
    {
        return new[] { _previousPosition };
    }
}