using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using GridSystem;
using Sirenix.OdinInspector;
using UnityEngine;

public class ConveyorBelt : PlaceableObjectBase, IItemCarrier
{
    private Tile _itemSendingTile;
    private Vector2Int _previousPosition;
    private Vector2Int _gridPosition;
    public Vector2Int NextPosition { get; private set; }

    [ShowInInspector] private Queue<Item> _items = new();
    private const int MaxItemCarryCount = 3;
    private ConveyorBeltVisualController.BeltVisualDirection _direction;

    [ShowInInspector] private List<Vector3> _itemCarryList;

    protected override void Setup()
    {
        _gridPosition = Origin;

        _previousPosition = Origin + PlaceableObjectBaseSo.GetDirForwardVector(Dir) * -1;
        NextPosition = Origin + PlaceableObjectBaseSo.GetDirForwardVector(Dir);


        _direction = transform.GetComponentInChildren<ConveyorBeltVisualController>().GVisualDirection;
        GetPo().Reverse();
        _itemCarryList = GetPo();


        _itemSendingTile = Grid.GetGridObject(NextPosition);
    }
/*
    private void Update()
    {
        // Eğer bant boşsa veya taşıyacak item yoksa çık
        if (_items.Count <= 0) return;

        if (_items.Count < MaxItemCarryCount)
        {
            // Her item için sırayla pozisyonlarını güncelle
            for (int i = 0; i < _items.Count; i++)
            {
                var item = _items[i];

                // Eğer item hareket edebiliyorsa pozisyonunu güncelle
                if (item.CanMove)
                {
                    var targetPosition = _itemCarryList[i]; // Hedef pozisyonu sıraya göre al
                    item.CanMove = false;
                    item.transform.DOMove(targetPosition, .5f).OnComplete(() => item.CanMove = true);
                }
            }
        }

        // Son item banda ulaştığında bir sonraki banda aktar
        if (_items.Count == MaxItemCarryCount)
        {
            var lastItem = _items[0];
            Debug.Log(lastItem, lastItem.gameObject);

            // Eğer son item bir sonraki banda aktarılabilir durumdaysa aktar
            if (lastItem.CanMove && _itemSendingTile?.OwnedObjectBase is IItemCarrier nextCarrier)
            {
                if (nextCarrier.GetGridPosition().All(p => p != _itemSendingTile.GetGridPosition)) return;
                if (nextCarrier.TrySetWorldItem(lastItem))
                {
                    _items.RemoveAt(0); // Son itemi listeden kaldır
                }
            }
        }


        if (true) return;
        switch (_items.Count)
        {
            case <= 0:
                return;
            case > 0 and < MaxItemCarryCount:
                for (int i = 0; i < _items.Count; i++)
                {
                    var item = _items[i];
                    if (item.CanMove)
                    {
                        var position = _itemCarryList[i];
                        item.CanMove = false;
                        item.transform.DOMove(position, .5f).OnComplete(() => item.CanMove = true);
                    }
                }


                break;
            case > MaxItemCarryCount:
                var lastItem = _items[2];
                if (!lastItem.CanMove) return;
                if (_itemSendingTile?.OwnedObjectBase is not IItemCarrier nextCarrier) return;
                if (nextCarrier.GetGridPosition().All(p => p != _itemSendingTile.GetGridPosition)) return;

                if (nextCarrier.TrySetWorldItem(lastItem))
                {
                    //item.MoveToItem(nextCarrier);
                    _items.RemoveAt(0);
                }

                break;
        }



        if(_items.Count <= 0) return;
        var item = _items[0];
        if (!item.CanMove) return;
        if (_itemSendingTile?.OwnedObjectBase is not IItemCarrier nextCarrier) return;
        if (nextCarrier.GetGridPosition().All(p => p != _itemSendingTile.GetGridPosition)) return;

        if (nextCarrier.TrySetWorldItem(item))
        {
            //item.MoveToItem(nextCarrier);
            _items.RemoveAt(0);
        }

    }
*/
    public override void DestroySelf()
    {
        base.DestroySelf();

        if (_items.Count <= 0) return;
        foreach (var item in _items)
            item.DestroySelf();
    }
    private void Update()
    {
        // Eğer kuyruk boşsa veya taşıyacak item yoksa çık
        if (_items.Count <= 0) return;

        // Bant dolu değilse her item için sırayla pozisyonları güncelle
        if (_items.Count < MaxItemCarryCount)
        {
            UpdateItemPositions();
        }

        // Kuyruktaki ilk itemi kontrol et (bant dolu olsun veya olmasın)
        var firstItem = _items.Peek();
        if (firstItem.CanMove && _itemSendingTile?.OwnedObjectBase is IItemCarrier nextCarrier)
        {
            // İlk itemin pozisyon çakışmasını önlemek için bir sonraki banda taşınmasını kontrol et
            if (nextCarrier.TrySetWorldItem(firstItem))
            {
                _items.Dequeue(); // Başarıyla aktarıldıysa ilk item’i kuyruktan çıkar
                UpdateItemPositions(); // Geri kalan itemlerin pozisyonlarını güncelle
            }
        }
    }
    private void UpdateItemPositions()
    {
        int index = 0;
        foreach (var item in _items)
        {
            if (item.CanMove)
            {
                var targetPosition = _itemCarryList[index];
                item.CanMove = false;

                item.transform.DOMove(targetPosition, .5f).OnComplete(() =>
                {
                    item.CanMove = true;
                });
                index++;
            }
        }
    }
/*
    public bool TrySetWorldItem(Item item)
    {
        if (_items.Count >= MaxItemCarryCount || !item.GetItemSo().isSolidItem) return false;
        _items.Add(item);

        return true;
        /*

        //if (_items.Count >= MaxItemCarryCount || !item.GetItemSo().isSolidItem) return false;
        if (_items.Count > 0 && _items[0] != null || !item.GetItemSo().isSolidItem) return false;
        _items.Add(item);
        //var position = Grid.GetWorldPosition(Origin) + new Vector3(0.5f, 0, 0.5f);
        item.index = _items.IndexOf(item);
        var position = _itemCarryList[item.index];
        item.CanMove = false;
        item.transform.DOMove(position, .5f).OnComplete(() => item.CanMove = true);
        return true;
        
    }
*/
    public bool TrySetWorldItem(Item item)
    {
        if (_items.Count >= MaxItemCarryCount || !item.GetItemSo().isSolidItem) return false;

        // Kuyruğa yeni item ekliyoruz ve ilk pozisyona taşıyoruz
        _items.Enqueue(item);
        
        /*
        item.CanMove = false;

        item.transform.DOMove(_itemCarryList[_items.Count - 1], .5f).OnComplete(() =>
        {
            item.CanMove = true;
        });
        */
        //item.transform.position = _itemCarryList[_items.Count - 1]; // En son pozisyona doğrudan yerleştiriyoruz

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
                            list.Add(Grid.GetWorldPosition(_gridPosition) + Grid.GetCellSizeOffset() +
                                     new Vector3(0, 0, a * i));
                        break;
                    case Dir.Left:
                        for (var i = -1; i < MaxItemCarryCount - 1; i++)
                            list.Add(Grid.GetWorldPosition(_gridPosition) + Grid.GetCellSizeOffset() +
                                     new Vector3(a * i, 0, 0));
                        break;
                    case Dir.Up:
                        for (var i = 1; i > 1 - MaxItemCarryCount; i--)
                            list.Add(Grid.GetWorldPosition(_gridPosition) + Grid.GetCellSizeOffset() +
                                     new Vector3(0, 0, a * i));
                        break;
                    case Dir.Right:
                        for (var i = 1; i > 1 - MaxItemCarryCount; i--)
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


    #endregion
}