using System;
using System.Collections.Generic;
using System.Linq;
using GridSystem;
using Helpers;
using UnityEngine;

public class Storage : PlaceableObjectBase, IItemSlot
{
    private Vector2Int _gatePosition;
    [SerializeField] private StorageType _storageType;
    private Tile _gateNeighbourTile;
    private Item _gateItem;
    private ItemSo _itemSo;
    
    [Serializable]
    public enum StorageType
    {
        Input, Output
    }

    private float _spawnTimer;
    private const float ItemSpawnTime = 1.5f;
    private int _debugSpawnItemCount;
    private int _debugStoreItemCount;
    protected override void Setup()
    {
        var positionList = placeableObjectSo.GetGridPositionList(Origin, Dir);
        _gatePosition = positionList[(positionList.Count - 1) / 2] + 
               PlaceableObjectSo.GetDirForwardVector(Dir);

        _gateNeighbourTile = Grid.GetGridObject(_gatePosition + PlaceableObjectBaseSo.GetDirForwardVector(Dir));

        _itemSo = GameAssets.i.GetItemSo(ItemType.C);
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
        if (_storageType is StorageType.Output)
        {
            if (_debugSpawnItemCount <= 0) return;
            _spawnTimer += Time.deltaTime;

            if (_spawnTimer >= ItemSpawnTime)
            {
                _spawnTimer = 0f;
                OutputStorageAction();
                //EventManager.TriggerEvent("OnItemAddedOrMoved");
            }

        }
        else
        {
            InputStorageAction();
        }
        
        //if(_storageType is StorageType.Input) InputStorageAction();
        //else OutputStorageAction();
        
        
        
    }

    private void OutputStorageAction()
    {
        Debug.Log("OutputStorageAction çağrıldı.");
        if (_gateNeighbourTile.OwnedObjectBase is not IItemSlot itemSlotObj) return;

        if (itemSlotObj.GetGridPosition().All(p => p != _gateNeighbourTile.GetGridPosition) ||
            !itemSlotObj.CanCarryItem(_itemSo)) return;

        if (_gateItem != null && _gateItem.CanMove)
        {
            //if (!itemSlotObj.TrySetWorldItem(_gateItem)) return;
            //_gateItem.MoveToItemSlot(itemSlotObj.GetCarryItemWorldPosition(_gateItem));
            
            if(!itemSlotObj.IsCarryItem()) return;
            _gateItem.MoveToItem(itemSlotObj);
            _gateItem = null;
            _debugSpawnItemCount--;
        }
        else
        {
            _gateItem = Item.CreateItem(Grid, _gatePosition, _itemSo);
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

    public Vector3 GetCarryItemWorldPosition(Item item)
    {
        return Grid.GetWorldPosition(_gatePosition) + Grid.GetCellSizeOffset();
    }

    public void OnItemControl(Item item)
    {
        OutputStorageAction();
    }

    public bool IsCarryItem()
    {
        throw new NotImplementedException();
    }

    public void CarryControl()
    {
        throw new NotImplementedException();
    }
}