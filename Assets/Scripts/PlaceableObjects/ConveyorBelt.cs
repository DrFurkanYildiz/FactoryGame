using System;
using System.Collections;
using System.Collections.Generic;
using GridSystem;
using UnityEngine;

public class ConveyorBelt : PlaceableObjectBase, IItemCarrier
{
    private readonly Queue<Item> _items = new();
    private const int MaxItemCarryCount = 3;

    public ConveyorBeltVisualController BeltVisual { get; private set; }

    private List<Vector3> _itemCarryList;
    private readonly Dictionary<int, Item> _indexItems = new();

    public List<Vector2Int> InputCoordinates { get; set; } = new();
    public List<Vector2Int> OutputCoordinates { get; set; } = new();
    


    protected override void Setup()
    {
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
        //Bant yönü değiştiğinde giriş çıkış kordinatlarını günceller.
        
        OutputCoordinates.Clear();
        InputCoordinates.Clear();
        
        if (!OutputCoordinates.Contains(GetOutputCoordinate()))
            OutputCoordinates.Add(GetOutputCoordinate());
        if (!InputCoordinates.Contains(GetInputCoordinate()))
            InputCoordinates.Add(GetInputCoordinate());
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

            //Göndereceğimiz pozisyonda bir taşıyıcı yoksa veya olan taşıyıcının item alma pozisyonu biz değilsek durdur.
            if (Grid.GetGridObject(OutputCoordinates[0]).OwnedObjectBase is not IItemCarrier sendingCarrier ||
                !sendingCarrier.InputCoordinates.Contains(Origin))
                return;
            
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
        return true;
    }
    
    public Dir GetDirectionAccordingOurCoordinate(Vector2Int coordinate)
    {
        return Dir;
    }

    private Vector2Int GetInputCoordinate()
    {
        return BeltVisual.direction switch
        {
            ConveyorBeltVisualController.BeltVisualDirection.Flat => Origin + PlaceableObjectBaseSo.GetDirForwardVector(Dir) * -1,
            ConveyorBeltVisualController.BeltVisualDirection.RightDown => Origin - Vector2Int.right,
            ConveyorBeltVisualController.BeltVisualDirection.LeftDown => Origin - Vector2Int.left,
            ConveyorBeltVisualController.BeltVisualDirection.DownRight => Origin - Vector2Int.down,
            ConveyorBeltVisualController.BeltVisualDirection.DownLeft => Origin - Vector2Int.down,
            ConveyorBeltVisualController.BeltVisualDirection.LeftUp => Origin - Vector2Int.left,
            ConveyorBeltVisualController.BeltVisualDirection.RightUp => Origin - Vector2Int.right,
            ConveyorBeltVisualController.BeltVisualDirection.UpRight => Origin - Vector2Int.up,
            ConveyorBeltVisualController.BeltVisualDirection.UpLeft => Origin - Vector2Int.up,
            _ => Vector2Int.zero
        };
    }

    private Vector2Int GetOutputCoordinate()
    {
        return Origin + PlaceableObjectBaseSo.GetDirForwardVector(Dir);
    }

    /// <summary>
    /// Bize item gönderebilecek item taşıyıcıların listesi. Yani bize bakan taşıyıcılar.
    /// </summary>
    /// <returns></returns>
    public List<IItemCarrier> GetNeighborCarriersThatCanSend()
    {
        var list = new List<IItemCarrier>();

        foreach (var tile in CurrentTile.GetNeighbourList(Grid))
        {
            switch (tile.OwnedObjectBase)
            {
                case ConveyorBelt neighbourBelt:
                {
                    if (neighbourBelt.OutputCoordinates.Contains(Origin) && Dir != PlaceableObjectBaseSo.GetOppositeDirection(neighbourBelt.Dir))
                    {
                        if (!list.Contains(neighbourBelt))
                            list.Add(neighbourBelt);
                    }

                    break;
                }
                case Splitter neighbourSplitter:
                    var nDir = PlaceableObjectBaseSo.GetDir(neighbourSplitter.Origin, Origin);
                    if (neighbourSplitter.OutputCoordinates.Contains(Origin) && Dir != PlaceableObjectBaseSo.GetOppositeDirection(nDir))
                    {
                        if (!list.Contains(neighbourSplitter))
                            list.Add(neighbourSplitter);
                    }
                    break;
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