using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GridSystem;
using UnityEngine;

public class ConveyorBelt : PlaceableObjectBase, IItemCarrier
{
    private Tile _itemSendingTile;
    private Vector2Int _previousPosition;
    private Vector2Int _gridPosition;
    public Vector2Int NextPosition { get; private set; }

    private readonly Queue<Item> _items = new();
    private const int MaxItemCarryCount = 3;
    private ConveyorBeltVisualController.BeltVisualDirection _direction;
    private List<Vector3> _itemCarryList;
    private readonly Dictionary<int, Item> _indexItems = new();
    
    protected override void Setup()
    {
        _gridPosition = Origin;

        _previousPosition = Origin + PlaceableObjectBaseSo.GetDirForwardVector(Dir) * -1;
        NextPosition = Origin + PlaceableObjectBaseSo.GetDirForwardVector(Dir);


        _direction = transform.GetComponentInChildren<ConveyorBeltVisualController>().GVisualDirection;
        _itemCarryList = GetCarryPositions();


        _itemSendingTile = Grid.GetGridObject(NextPosition);

        for (var i = 0; i < MaxItemCarryCount; i++)
            _indexItems.Add(i, null);
    }
    
    public override void DestroySelf()
    {
        base.DestroySelf();

        while (_items.Count > 0)
        {
            var item = _items.Dequeue();
            item.DestroySelf();
        }
    }

    private void Update()
    {
        if (_items.Count == 0) return;

        for (int i = 0; i < MaxItemCarryCount; i++)
        {
            if (_indexItems[i] == null) continue;

            var item = _indexItems[i];
            var targetPosition = _itemCarryList[i];

            item.transform.position = Vector3.MoveTowards(item.transform.position, targetPosition, .01f);

            if (item.transform.position == targetPosition)
            {
                if (i < MaxItemCarryCount - 1 && _indexItems[i + 1] == null)
                {
                    _indexItems[i + 1] = item;
                    _indexItems[i] = null;
                }
                else if (i == MaxItemCarryCount - 1)
                {
                    if (_itemSendingTile?.OwnedObjectBase is not IItemCarrier nextCarrier) return;
                    if (nextCarrier.GetGridPosition().All(p => p != _itemSendingTile.GetGridPosition)) return;

                    if (nextCarrier.TrySetWorldItem(item))
                    {
                        _items.Dequeue();
                        _indexItems[i] = null;
                    }
                }
            }
        }
    }

    public bool TrySetWorldItem(Item item)
    {
        if (!item.ItemSo.isSolidItem) return false;
        if (_items.Count >= MaxItemCarryCount || _indexItems[0] != null) return false;
        
        _items.Enqueue(item);
        _indexItems[0] = item;
        return true;
    }

    public IEnumerable<Vector2Int> GetGridPosition()
    {
        return new[] { _gridPosition };
    }
    
    private List<Vector3> GetCarryPositions()
    {
        var list = new List<Vector3>();
        var a = Grid.GetCellSize() / MaxItemCarryCount;
        switch (_direction)
        {
            case ConveyorBeltVisualController.BeltVisualDirection.Flat:
                switch (Dir)
                {
                    case Dir.Down:
                        for (var i = 1; i > 1 - MaxItemCarryCount; i--)
                            list.Add(Grid.GetWorldPosition(_gridPosition) + Grid.GetCellSizeOffset() +
                                     new Vector3(0, 0, a * i));
                        break;
                    case Dir.Left:
                        for (var i = 1; i > 1 - MaxItemCarryCount; i--)
                            list.Add(Grid.GetWorldPosition(_gridPosition) + Grid.GetCellSizeOffset() +
                                     new Vector3(a * i, 0, 0));
                        break;
                    case Dir.Up:
                        for (var i = -1; i < MaxItemCarryCount - 1; i++)
                            list.Add(Grid.GetWorldPosition(_gridPosition) + Grid.GetCellSizeOffset() +
                                     new Vector3(0, 0, a * i));
                        break;
                    case Dir.Right:
                        for (var i = -1; i < MaxItemCarryCount - 1; i++)
                            list.Add(Grid.GetWorldPosition(_gridPosition) + Grid.GetCellSizeOffset() +
                                     new Vector3(a * i, 0, 0));
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

}