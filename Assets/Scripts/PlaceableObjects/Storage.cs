using System;
using System.Collections.Generic;
using System.Linq;
using GridSystem;
using Helpers;
using UnityEngine;

public class Storage : PlaceableObjectBase, IItemCarrier
{
    private Vector2Int _gatePosition;
    [SerializeField] private StorageType _storageType;
    private Tile _gateNeighbourTile;
    private Item _gateItem;
    private Item _item;
    private ItemSo _itemSo;
    
    
    public List<Vector2Int> OutputCoordinates { get; set; }
    public List<Vector2Int> InputCoordinates { get; set; }
    public List<IItemCarrier> SendingItemCarriers { get; set; }
    public List<IItemCarrier> TakenItemCarriers { get; set; }
    public Dir GetDirectionAccordingOurCoordinate(Vector2Int currentCoordinate)
    {
        throw new NotImplementedException();
    }


    [Serializable]
    public enum StorageType
    {
        Input, Output
    }

    private int _debugSpawnItemCount;
    private int _debugStoreItemCount;

    private Tile _itemSendingTile; 
    
    protected override void Setup()
    {
        var positionList = placeableObjectSo.GetGridPositionList(Origin, Dir);
        _gatePosition = positionList[(positionList.Count - 1) / 2] + 
               PlaceableObjectSo.GetDirForwardVector(Dir);

        _gateNeighbourTile = Grid.GetGridObject(_gatePosition + PlaceableObjectBaseSo.GetDirForwardVector(Dir));

        _itemSo = GameAssets.i.GetItemSo(ItemType.C);
        _debugSpawnItemCount = 60;

        var itemSendingCoordinate = _gatePosition + PlaceableObjectBaseSo.GetDirForwardVector(Dir);
        _itemSendingTile = Grid.GetGridObject(itemSendingCoordinate);

        //if (_storageType is StorageType.Output)
        //InitializedItemCreator();
    }
    

    public override void DestroySelf()
    {
        base.DestroySelf();

        if (_item != null)
            _item.DestroySelf();
    }

    private void Update()
    {
        if (_item == null && _debugSpawnItemCount > 0)
        {
            var position = Grid.GetWorldPosition(_gatePosition) + Grid.GetCellSizeOffset();
            _item = Item.CreateItem(position, _itemSo);
            _debugSpawnItemCount--;
        }
        
        
        if(_item != null)
        {
            if (_itemSendingTile?.OwnedObjectBase is not IItemCarrier nextCarrier) return;
            if (nextCarrier.GetGridPosition().All(p => p != _itemSendingTile.GetGridPosition)) return;
            if (nextCarrier.TrySetWorldItem(_item))
            {
                _item = null; 
            }
        }
        
    }

    private void InputStorageAction()
    {
        if (_gateItem != null)
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
}