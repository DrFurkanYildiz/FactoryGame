using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConveyorBelt : PlaceableObjectBase, IItemCarrier
{
    public Vector2Int NextPosition { get; private set; }
    public Vector2Int PreviousPosition { get; private set; }

    private readonly Queue<Item> _items = new();
    private const int MaxItemCarryCount = 3;

    public ConveyorBeltVisualController BeltVisual { get; private set; }

    private List<Vector3> _itemCarryList;
    private readonly Dictionary<int, Item> _indexItems = new();

    private ConveyorBelt _itemSendingBelt;

    // İtemi bir sonraki banda gönderirken hali hazırda o bandın item alıp almadığını kontrol için var.
    private ConveyorBelt _itemTakenBelt;

    protected override void Setup()
    {
        PreviousPosition = Origin + PlaceableObjectBaseSo.GetDirForwardVector(Dir) * -1;
        NextPosition = Origin + PlaceableObjectBaseSo.GetDirForwardVector(Dir);

        BeltVisual = transform.GetComponentInChildren<ConveyorBeltVisualController>();

        for (var i = 0; i < MaxItemCarryCount; i++)
            _indexItems.Add(i, null);

        BeltVisual.OnUpdateVisualDirection += UpdateItemCarryList;
        BeltVisual.OnUpdateVisualDirection += UpdateNextAndPreviousConveyorBelt;
    }

    private void Start()
    {
        UpdateItemCarryList();
        UpdateNextAndPreviousConveyorBelt();
    }

    private void UpdateNextAndPreviousConveyorBelt()
    {
        //Bant yönü değiştiğinde giriş çıkış bantlarını günceller.
        //TODO: Bant gibi tüm item taşıyıcılar için yapılmalı.

        if (GetNeighbourBelt().Count == 1)
        {
            GetNeighbourBelt()[0]._itemSendingBelt = this;
            _itemTakenBelt = GetNeighbourBelt()[0];
        }
        else
        {
            var nBelt = GetNextConveyorBeltToDirection();
            if (nBelt != null)
            {
                nBelt._itemSendingBelt = this;
                _itemTakenBelt = nBelt;
            }
        }

        if (Grid.GetGridObject(NextPosition)?.OwnedObjectBase is ConveyorBelt nextBelt &&
            nextBelt._itemTakenBelt == null &&
            !IsOppositeDirection(Dir, nextBelt.Dir))
        {
            _itemSendingBelt = nextBelt;
            nextBelt._itemTakenBelt = this;
        }
    }
    
    private void UpdateItemCarryList()
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
            var currentItem = _indexItems[i];
            if (currentItem == null) continue;

            // Öğeyi hedef konuma doğru hareket ettir
            var targetPosition = _itemCarryList[i];
            currentItem.transform.position = Vector3.MoveTowards(currentItem.transform.position, targetPosition, 0.01f);

            // Hedef konuma ulaştığında
            if (currentItem.transform.position != targetPosition) continue;

            // Bir sonraki pozisyona taşı
            if (i < MaxItemCarryCount - 1 && _indexItems[i + 1] == null)
            {
                _indexItems[i + 1] = currentItem;
                _indexItems[i] = null;
                continue;
            }

            // Son pozisyona ulaşıldığında sonraki taşıyıcıya ilet
            if (i == MaxItemCarryCount - 1 && _itemSendingBelt != null)
            {
                // Öğeyi sonraki taşıyıcıya gönder
                if (_itemSendingBelt.TrySetWorldItem(currentItem))
                {
                    _items.Dequeue();
                    _indexItems[i] = null;
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
        return new[] { Origin };
    }

    private bool IsOppositeDirection(Dir currentDir, Dir targetDir)
    {
        return currentDir switch
        {
            Dir.Down => targetDir is Dir.Up,
            Dir.Left => targetDir is Dir.Right,
            Dir.Up => targetDir is Dir.Down,
            Dir.Right => targetDir is Dir.Left,
            _ => throw new ArgumentOutOfRangeException(nameof(currentDir), currentDir, null)
        };
    }

    private ConveyorBelt GetNextConveyorBeltToDirection()
    {
        return BeltVisual.direction switch
        {
            ConveyorBeltVisualController.BeltVisualDirection.Flat => GetNeighbourBelt().
                Find(b => b.Dir == Dir),
            ConveyorBeltVisualController.BeltVisualDirection.DownRight => GetNeighbourBelt()
                .Find(b => b.Dir == Dir.Down),
            ConveyorBeltVisualController.BeltVisualDirection.DownLeft => GetNeighbourBelt()
                .Find(b => b.Dir == Dir.Down),
            ConveyorBeltVisualController.BeltVisualDirection.UpRight => GetNeighbourBelt()
                .Find(b => b.Dir == Dir.Up),
            ConveyorBeltVisualController.BeltVisualDirection.UpLeft => GetNeighbourBelt()
                .Find(b => b.Dir == Dir.Up),
            ConveyorBeltVisualController.BeltVisualDirection.RightDown => GetNeighbourBelt()
                .Find(b => b.Dir == Dir.Right),
            ConveyorBeltVisualController.BeltVisualDirection.RightUp => GetNeighbourBelt()
                .Find(b => b.Dir == Dir.Right),
            ConveyorBeltVisualController.BeltVisualDirection.LeftDown => GetNeighbourBelt()
                .Find(b => b.Dir == Dir.Left),
            ConveyorBeltVisualController.BeltVisualDirection.LeftUp => GetNeighbourBelt()
                .Find(b => b.Dir == Dir.Left),
            _ => null
        };
    }

    public List<ConveyorBelt> GetNeighbourBelt()
    {
        var pathfindingSystem = GridBuildingSystem.Instance.PathfindingSystem;
        var list = new List<ConveyorBelt>();

        foreach (var tile in pathfindingSystem.GetNeighbour(GetTile))
        {
            if (tile.OwnedObjectBase is ConveyorBelt neighbourBelt)
            {
                if (neighbourBelt.NextPosition == Origin && !IsOppositeDirection(Dir, neighbourBelt.Dir))
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
        var basePosition = Grid.GetWorldPosition(Origin) + Grid.GetCellSizeOffset();
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