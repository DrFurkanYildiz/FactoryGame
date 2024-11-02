using System;
using System.Collections.Generic;
using System.Linq;
using GridSystem;
using UnityEngine;

public class Storage : PlacedObject, IItemSlot
{
    private Vector2Int _gatePosition;
    [SerializeField] private StorageType _storageType;
    private Tile _gateNeighbourTile;
    private Item _gateItem;
    [SerializeField] private ItemSo _testItemSo;
    
    [Serializable]
    public enum StorageType
    {
        Input, Output
    }
    
    
    private int _debugSpawnItemCount;
    private int _debugStoreItemCount;
    protected override void Setup()
    {
        var positionList = PlacedObjectTypeSo.GetGridPositionList(Origin, Dir);
        _gatePosition = positionList[(positionList.Count - 1) / 2] + 
               PlacedObjectTypeSo.GetDirForwardVector(Dir);

        _gateNeighbourTile = Grid.GetGridObject(_gatePosition + PlacedObjectTypeSo.GetDirForwardVector(Dir));

        _debugSpawnItemCount = 60;
    }

    public override void DestroySelf()
    {
        base.DestroySelf();
        
        if(_gateItem != null)
            _gateItem.DestroySelf();
    }

    private void Update()
    {
        if(_storageType is StorageType.Input) InputStorageAction();
        else OutputStorageAction();
    }

    private void OutputStorageAction()
    {
        if (_gateNeighbourTile.OwnedObject is not IItemSlot itemSlotObj) return;

        if (_debugSpawnItemCount <= 0) return;
        if (itemSlotObj.GetGridPosition().All(p => p != _gateNeighbourTile.GetGridPosition) ||
            !itemSlotObj.CanCarryItem(_testItemSo)) return;
        
        if (_gateItem != null && _gateItem.CanMove)
        {
            if (!itemSlotObj.TrySetWorldItem(_gateItem)) return;
            _gateItem.MoveToItemSlot(
                itemSlotObj.GetGridPosition().FirstOrDefault(p => p == _gateNeighbourTile.GetGridPosition));
            _gateItem = null;
            _debugSpawnItemCount--;
        }
        else
        {
            _gateItem = Item.CreateItem(Grid, _gatePosition, _testItemSo);
        }
    }

    private void InputStorageAction()
    {
        if (_gateItem != null && _gateItem.CanMove)
        {
            _gateItem.DestroySelf();
            _gateItem = null;
            _debugStoreItemCount++;
        }
    }

    public bool CanCarryItem(ItemSo itemSo)
    {
        return itemSo.isSolidItem;
    }

    public bool TrySetWorldItem(Item item)
    {
        if (_gateItem != null) return false;
        _gateItem = item;
        return true;
    }

    public IEnumerable<Vector2Int> GetGridPosition()
    {
        return new[] { _gatePosition };
    }
}