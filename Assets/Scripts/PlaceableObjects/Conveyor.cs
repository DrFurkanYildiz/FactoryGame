using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Conveyor : PlaceableObjectBase, IItemCarrier
{
    private readonly Queue<Item> _items = new();
    protected const int MaxItemCarryCount = 3;
    
    protected List<Vector3> itemCarryList;
    private readonly Dictionary<int, Item> _indexItems = new();

    public List<Vector2Int> OutputCoordinates { get; set; } = new();
    public List<Vector2Int> InputCoordinates { get; set; } = new();

    protected override void Setup()
    {
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

    public virtual void ItemTransport()
    {
        if (_items.Count == 0) return;

        for (int i = 0; i < MaxItemCarryCount; i++)
        {
            var currentItem = _indexItems[i];
            if (currentItem == null) continue;

            // Öğeyi hedef konuma doğru hareket ettir
            var targetPosition = itemCarryList[i];
            currentItem.transform.position = 
                Vector3.MoveTowards(currentItem.transform.position, targetPosition, ItemCarrySpeed());

            // Hedef konuma ulaştığında
            if (currentItem.transform.position != targetPosition) continue;

            // Bir sonraki pozisyona taşı
            if (i < MaxItemCarryCount - 1 && _indexItems[i + 1] == null)
            {
                _indexItems[i + 1] = currentItem;
                _indexItems[i] = null;
                continue;
            }

            if(!IsItemSending(out var sendingCarrier)) return;
            
            // Son pozisyona ulaşıldığında sonraki taşıyıcıya ilet
            if (i == MaxItemCarryCount - 1)
            {
                // Öğeyi sonraki taşıyıcıya gönder
                if (sendingCarrier.TrySetWorldItem(currentItem))
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
        item.SetCarrier(this);
        return true;
    }

    public float ItemCarrySpeed()
    {
        return ((PlaceableObjectSo)placeableObjectSo).itemCarrySpeed;
    }

    public Dir GetDirectionAccordingOurCoordinate(Vector2Int coordinate)
    {
        return Dir;
    }

    protected virtual bool IsItemSending( out IItemCarrier sendingCarrier)
    {
        //Göndereceğimiz pozisyonda bir taşıyıcı yoksa veya olan taşıyıcının item alma pozisyonu biz değilsek durdur.
        if (Grid.GetGridObject(OutputCoordinates[0])?.OwnedObjectBase is IItemCarrier sCarrier &&
            sCarrier.InputCoordinates.Contains(Origin))
        {
            sendingCarrier = sCarrier;
            return true;
        }

        sendingCarrier = null;
        return false;
    }
    
    protected virtual List<Vector3> GetCarryPositions(ConveyorBeltVisualController.BeltVisualDirection vDirection)
    {
        var list = new List<Vector3>();
        var basePosition = Grid.GetWorldPosition(Origin) + Grid.GetCellSizeOffset();
        var a = Grid.GetCellSize() / MaxItemCarryCount;

        switch (vDirection)
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
