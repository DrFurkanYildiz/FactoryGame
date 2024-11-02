using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ConveyorBelt : PlacedObject, IItemSlot
{
    
    private Vector2Int _previousPosition;
    private Vector2Int _gridPosition;
    private Vector2Int _nextPosition;

    private Item _item;
    
    protected override void Setup() 
    {
        _gridPosition = Origin;

        _previousPosition = Origin + PlacedObjectTypeSo.GetDirForwardVector(Dir) * -1;
        _nextPosition = Origin + PlacedObjectTypeSo.GetDirForwardVector(Dir);
    }

    private void Update()
    {
        TakeAction();
    }
    public override void DestroySelf()
    {
        base.DestroySelf();
        
        if(_item != null)
            _item.DestroySelf();
    }

    private void TakeAction() 
    {
        if (_item == null || !_item.CanMove) return;
        var nextPlacedObject = Grid.GetGridObject(_nextPosition).OwnedObject;
        if (nextPlacedObject == null) return;
            
        if (nextPlacedObject is not IItemSlot nextPlacedSlot || !nextPlacedSlot.CanCarryItem(_item.GetItemSo())) return;
        if (nextPlacedSlot.GetGridPosition().All(p => p != _nextPosition)) return;

        if (nextPlacedSlot.TrySetWorldItem(_item))
        {
            _item.MoveToItemSlot
                (nextPlacedSlot.GetGridPosition().FirstOrDefault(p => p == _nextPosition));
            _item = null;
            
            Debug.Log("Take Action!");
        }

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
        return new[] { _gridPosition };
    }
}
