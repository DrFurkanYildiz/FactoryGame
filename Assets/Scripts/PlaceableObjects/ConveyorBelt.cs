using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class ConveyorBelt : PlaceableObjectBase, IItemSlot
{
    private Vector2Int _previousPosition;
    private Vector2Int _gridPosition;
    public Vector2Int NextPosition { get; private set; }

    [ShowInInspector] private readonly List<Item> _items = new();
    private const int MaxItemCarryCount = 3;
    private ConveyorBeltVisualController.BeltVisualDirection _direction;

    [ShowInInspector] private List<Vector3> _itemCarryList;
    protected override void Setup()
    {
        _gridPosition = Origin;

        _previousPosition = Origin + PlaceableObjectBaseSo.GetDirForwardVector(Dir) * -1;
        NextPosition = Origin + PlaceableObjectBaseSo.GetDirForwardVector(Dir);
        
        
        _direction = transform.GetComponentInChildren<ConveyorBeltVisualController>().GVisualDirection;
        _itemCarryList = GetPo();
    }

    private void Update()
    {
        TakeAction();
    }
    public override void DestroySelf()
    {
        base.DestroySelf();
        /*
        if(_item != null)
            _item.DestroySelf();
            */
    }

    private void TakeAction()
    {
        if (_items.Count <= 0) return;

        var item = _items[0];
        if (!item.CanMove) return;
        
        var nextPlacedObject = Grid.GetGridObject(NextPosition).OwnedObjectBase;
        if (nextPlacedObject == null) return;
            
        if (nextPlacedObject is not IItemSlot nextPlacedSlot || !nextPlacedSlot.CanCarryItem(item.GetItemSo())) return;
        if (nextPlacedSlot.GetGridPosition().All(p => p != NextPosition)) return;

        if (nextPlacedSlot.TrySetWorldItem(item))
        {
            item.MoveToItemSlot(nextPlacedSlot.GetCarryItemWorldPosition(item));
            _items.RemoveAt(0);
            
            //Debug.Log(this.name+ ": " + _items.Count, gameObject);
        }

    }
    
    public bool CanCarryItem(ItemSo itemSo)
    {
        return itemSo.isSolidItem;
    }

    public bool TrySetWorldItem(Item item)
    {
        if (_items.Count >= MaxItemCarryCount) return false;
        _items.Add(item);
        return true;
    }

    public IEnumerable<Vector2Int> GetGridPosition()
    {
        return new[] { _gridPosition };
    }

    private List<Vector3> GetPo()
    {
        var list = new List<Vector3>();
        var a = Grid.GetCellSize() / MaxItemCarryCount;
        switch (_direction)
        {
            case ConveyorBeltVisualController.BeltVisualDirection.Flat:
                switch (Dir)
                {
                    case Dir.Down:
                        for (var i = -1; i < MaxItemCarryCount - 1; i++)
                            list.Add(Grid.GetWorldPosition(_gridPosition) + Grid.GetCellSizeOffset() + new Vector3(0, 0, a * i));
                        break;
                    case Dir.Left:
                        for (var i = -1; i < MaxItemCarryCount - 1; i++)
                            list.Add(Grid.GetWorldPosition(_gridPosition) + Grid.GetCellSizeOffset() + new Vector3(a * i, 0, 0));
                        break;
                    case Dir.Up:
                        for (var i = 1; i > 1 - MaxItemCarryCount; i--)
                            list.Add(Grid.GetWorldPosition(_gridPosition) + Grid.GetCellSizeOffset() + new Vector3(0, 0, a * i));
                        break;
                    case Dir.Right:
                        for (var i = 1; i > 1 - MaxItemCarryCount; i--)
                            list.Add(Grid.GetWorldPosition(_gridPosition) + Grid.GetCellSizeOffset() + new Vector3(a * i, 0, 0));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
                break;
            /*
            case ConveyorBeltVisualController.BeltVisualDirection.DownRight:
                
                for (var i = 1; i > 1 - MaxItemCarryCount; i--)
                    list.Add(Grid.GetWorldPosition(_gridPosition) + new Vector3(a * i, 0, 0));
                
                break;
            case ConveyorBeltVisualController.BeltVisualDirection.DownLeft:
                break;
            case ConveyorBeltVisualController.BeltVisualDirection.UpRight:
                break;
            case ConveyorBeltVisualController.BeltVisualDirection.UpLeft:
                break;
            case ConveyorBeltVisualController.BeltVisualDirection.RightDown:
                break;
            case ConveyorBeltVisualController.BeltVisualDirection.RightUp:
                break;
            case ConveyorBeltVisualController.BeltVisualDirection.LeftDown:
                break;
            case ConveyorBeltVisualController.BeltVisualDirection.LeftUp:
                break;
                */
            default:
                throw new ArgumentOutOfRangeException();
        }

        return list;
    }
    public Vector3 GetCarryItemWorldPosition(Item item)
    {
        //Buraya bak
        //var a = Grid.GetCellSize() / MaxItemCarryCount;
        
        //return Grid.GetWorldPosition(_gridPosition) + Grid.GetCellSizeOffset();
        return _itemCarryList[_items.IndexOf(item)];
    }
}
