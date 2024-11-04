using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using Sirenix.OdinInspector;
using UnityEngine;

public class ConveyorBelt : PlaceableObjectBase, IItemSlot
{
    private Vector2Int _previousPosition;
    private Vector2Int _gridPosition;
    public Vector2Int NextPosition { get; private set; }

    [SerializeField] private List<Item> _items = new();
    private const int MaxItemCarryCount = 1;
    private ConveyorBeltVisualController.BeltVisualDirection _direction;

    [ShowInInspector] private List<Vector3> _itemCarryList;
    private List<ConveyorBelt> _neighbourSlots;
    protected override void Setup()
    {
        _neighbourSlots = new List<ConveyorBelt>();
        _gridPosition = Origin;

        _previousPosition = Origin + PlaceableObjectBaseSo.GetDirForwardVector(Dir) * -1;
        NextPosition = Origin + PlaceableObjectBaseSo.GetDirForwardVector(Dir);
        
        
        _direction = transform.GetComponentInChildren<ConveyorBeltVisualController>().GVisualDirection;
        _itemCarryList = GetPo();
    }

    public void AddNeighbour(ConveyorBelt slot)
    {
        if (!_neighbourSlots.Contains(slot))
            _neighbourSlots.Add(slot);
        
        if (_neighbourSlots.Count > 0)
            _neighbourSlots.ForEach(n => n.CarryControl());
            
    }

    public void RemoveNeighbour(ConveyorBelt slot)
    {
        if (_neighbourSlots.Contains(slot))
            _neighbourSlots.Remove(slot);
    }
    private void Update()
    {
        //TakeAction();
    }
    public override void DestroySelf()
    {
        base.DestroySelf();
        /*
        if(_item != null)
            _item.DestroySelf();
            */
    }

    public void TakeAction()
    {
        Debug.Log("TakeAction çağrıldı.");
        if (_items.Count <= 0) return;

        var item = _items[0];
        if (!item.CanMove) return;
        
        var nextPlacedObject = Grid.GetGridObject(NextPosition).OwnedObjectBase;
        if (nextPlacedObject == null) return;
            
        if (nextPlacedObject is not IItemSlot nextPlacedSlot || !nextPlacedSlot.CanCarryItem(item.GetItemSo())) return;
        if (nextPlacedSlot.GetGridPosition().All(p => p != NextPosition)) return;

        if (nextPlacedSlot.IsCarryItem())
        {
            item.MoveToItem(nextPlacedSlot);
            _items.RemoveAt(0);
        }
        
        if (nextPlacedSlot.TrySetWorldItem(item))
        {
            item.MoveToItemSlot(nextPlacedSlot.GetCarryItemWorldPosition(item));
            _items.RemoveAt(0);
            
            //Debug.Log(this.name+ ": " + _items.Count, gameObject);
        }
        else
        {
            Debug.Log("Dolu", gameObject);
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

    #region Of

    
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
        
        return Grid.GetWorldPosition(_gridPosition) + Grid.GetCellSizeOffset();
        //return _itemCarryList[_items.IndexOf(item)];
    }

    #endregion

    public void OnItemControl(Item item)
    {
        _items.Add(item);
        
        var nextPlacedObject = Grid.GetGridObject(NextPosition).OwnedObjectBase;
        if (nextPlacedObject == null) return;
        
        if (nextPlacedObject is not IItemSlot nextPlacedSlot || !nextPlacedSlot.CanCarryItem(item.GetItemSo())) return;
        if (nextPlacedSlot.GetGridPosition().All(p => p != NextPosition)) return;
        
        if(!nextPlacedSlot.IsCarryItem()) return;
        item.MoveToItem(nextPlacedSlot);
        _items.RemoveAt(0);
        
        if (_neighbourSlots.Count > 0)
            _neighbourSlots.ForEach(n => n.CarryControl());
    }

    public bool IsCarryItem()
    {
        return _items.Count < MaxItemCarryCount;
    }

    public void CarryControl()
    {
        if (_items.Count > 0)
            OnItemControl(_items[0]);
    }
}
