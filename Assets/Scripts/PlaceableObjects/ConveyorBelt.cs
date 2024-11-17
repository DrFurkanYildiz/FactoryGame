using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GridSystem;
using UnityEngine;

public class ConveyorBelt : PlaceableObjectBase, IItemCarrier
{
    private Tile _itemSendingTile;
    private Vector2Int _gridPosition;
    public Vector2Int NextPosition { get; private set; }
    public Vector2Int PreviousPosition { get; private set; }

    private readonly Queue<Item> _items = new();
    private const int MaxItemCarryCount = 3;

    public ConveyorBeltVisualController BeltVisual { get; private set; }

    private List<Vector3> _itemCarryList;
    private readonly Dictionary<int, Item> _indexItems = new();

    protected override void Setup()
    {
        _gridPosition = Origin;

        PreviousPosition = Origin + PlaceableObjectBaseSo.GetDirForwardVector(Dir) * -1;
        NextPosition = Origin + PlaceableObjectBaseSo.GetDirForwardVector(Dir);

        BeltVisual = transform.GetComponentInChildren<ConveyorBeltVisualController>();
        _itemSendingTile = Grid.GetGridObject(NextPosition);

        for (var i = 0; i < MaxItemCarryCount; i++)
            _indexItems.Add(i, null);
    }

    private void Start()
    {
        UpdateItemCarryList();
    }

    public void UpdateItemCarryList()
    {
        _itemCarryList = GetCarryPositions();
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

                    if (nextCarrier is ConveyorBelt conveyorBelt && conveyorBelt.Dir != Dir &&
                        !CanTransferItem(Dir, conveyorBelt.BeltVisual.direction))
                        return;
                    
                    if(nextCarrier is Splitter splitter && splitter.Dir != Dir)
                        return;
                    
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

    private bool CanTransferItem(Dir currentDir, ConveyorBeltVisualController.BeltVisualDirection targetDirection)
    {
        switch (currentDir)
        {
            case Dir.Down:
                if (targetDirection is ConveyorBeltVisualController.BeltVisualDirection.DownLeft
                    or ConveyorBeltVisualController.BeltVisualDirection.DownRight)
                    return true;
                break;
            case Dir.Left:
                if (targetDirection is ConveyorBeltVisualController.BeltVisualDirection.LeftDown
                    or ConveyorBeltVisualController.BeltVisualDirection.LeftUp)
                    return true;
                break;
            case Dir.Up:
                if (targetDirection is ConveyorBeltVisualController.BeltVisualDirection.UpLeft
                    or ConveyorBeltVisualController.BeltVisualDirection.UpRight)
                    return true;
                break;
            case Dir.Right:
                if (targetDirection is ConveyorBeltVisualController.BeltVisualDirection.RightDown
                    or ConveyorBeltVisualController.BeltVisualDirection.RightUp)
                    return true;
                break;
        }

        return false;
    }

    public int GetNeighbourBeltCount()
    {
        return GetNeighbourBelt().Count;
    }

    public List<ConveyorBelt> GetNeighbourBelt()
    {
        var pathfindingSystem = GridBuildingSystem.Instance.PathfindingSystem;
        var list = new List<ConveyorBelt>();
        
        foreach (var tile in pathfindingSystem.GetNeighbour(GetTile))
        {
            if (tile.OwnedObjectBase is ConveyorBelt neighbourBelt)
            {
                if (neighbourBelt.NextPosition == Origin)
                {
                    if (!list.Contains(neighbourBelt))
                        list.Add(neighbourBelt);
                }
            }
        }

        return list;
    }
    private List<Vector3> GetCarryPositions()
    {
        var list = new List<Vector3>();
        var basePosition = Grid.GetWorldPosition(_gridPosition) + Grid.GetCellSizeOffset();
        var a = Grid.GetCellSize() / MaxItemCarryCount;

        switch (BeltVisual.direction)
        {
            case ConveyorBeltVisualController.BeltVisualDirection.Flat:
                for (var i = 1; i > 1 - MaxItemCarryCount; i--)
                {
                    var offset = Dir switch
                    {
                        Dir.Down => new Vector3(0, 0, a * i),
                        Dir.Left => new Vector3(a * i, 0, 0),
                        Dir.Up => new Vector3(0, 0, a * -i),
                        Dir.Right => new Vector3(a * -i, 0, 0),
                        _ => throw new ArgumentOutOfRangeException()
                    };
                    list.Add(basePosition + offset);
                }

                break;
            case ConveyorBeltVisualController.BeltVisualDirection.DownRight:
                for (var i = 1; i > 1 - MaxItemCarryCount; i--)
                    list.Add(basePosition + new Vector3(i > 0 ? 0 : -a * i, 0, i > 0 ? a * i : 0));
                break;
            case ConveyorBeltVisualController.BeltVisualDirection.DownLeft:
                for (var i = 1; i > 1 - MaxItemCarryCount; i--)
                    list.Add(basePosition + new Vector3(i > 0 ? 0 : a * i, 0, i > 0 ? a * i : 0));
                break;
            case ConveyorBeltVisualController.BeltVisualDirection.UpRight:
                for (var i = 1; i > 1 - MaxItemCarryCount; i--)
                    list.Add(basePosition + new Vector3(i > 0 ? 0 : -a * i, 0, i > 0 ? -a * i : 0));
                break;
            case ConveyorBeltVisualController.BeltVisualDirection.UpLeft:
                for (var i = 1; i > 1 - MaxItemCarryCount; i--)
                    list.Add(basePosition + new Vector3(i > 0 ? 0 : a * i, 0, i > 0 ? -a * i : 0));
                break;
            case ConveyorBeltVisualController.BeltVisualDirection.RightDown:
                for (var i = 1; i > 1 - MaxItemCarryCount; i--)
                    list.Add(basePosition + new Vector3(i > 0 ? -a * i : 0, 0, i > 0 ? 0 : a * i));
                break;
            case ConveyorBeltVisualController.BeltVisualDirection.RightUp:
                for (var i = 1; i > 1 - MaxItemCarryCount; i--)
                    list.Add(basePosition + new Vector3(i > 0 ? -a * i : 0, 0, i > 0 ? 0 : -a * i));
                break;
            case ConveyorBeltVisualController.BeltVisualDirection.LeftDown:
                for (var i = 1; i > 1 - MaxItemCarryCount; i--)
                    list.Add(basePosition + new Vector3(i > 0 ? a * i : 0, 0, i > 0 ? 0 : a * i));
                break;
            case ConveyorBeltVisualController.BeltVisualDirection.LeftUp:
                for (var i = 1; i > 1 - MaxItemCarryCount; i--)
                    list.Add(basePosition + new Vector3(i > 0 ? a * i : 0, 0, i > 0 ? 0 : -a * i));
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return list;
    }
}