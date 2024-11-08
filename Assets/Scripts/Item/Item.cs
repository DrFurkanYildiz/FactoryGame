using System;
using DG.Tweening;
using GridSystem;
using UnityEngine;

public class Item : MonoBehaviour
{
    public static Item CreateItem(Grid<Tile> grid, Vector3 position, ItemSo itemSo)
    {
        var itemTransform = Instantiate(itemSo.prefab, position, Quaternion.identity);
        var item = itemTransform.GetComponent<Item>();
        item._grid = grid;
        //item._gridPosition = gridPosition;
        item._itemSo = itemSo;
        return item;
    }

    private Grid<Tile> _grid;
    private Vector2Int _gridPosition;
    private bool _hasAlreadyMoved;
    private ItemSo _itemSo;
    //public bool CanMove => !_hasAlreadyMoved;
    public bool CanMove { get; set; } = true;
    public Vector3 target;
    public int index;
    public bool debugMove;
    private void Update()
    {
        debugMove = CanMove;
    }

    public void MoveToItemSlot(Vector3 position)
    {
        _hasAlreadyMoved = true;
        target = position;
        //var position = _grid.GetWorldPosition(inputGate) + new Vector3(0.5f, 0, 0.5f);
        //var position = _grid.GetWorldPosition(inputGate);
        transform.DOLocalMove(position, .5f).OnComplete(() => _hasAlreadyMoved = false);
    }

    public ItemSo GetItemSo()
    {
        return _itemSo;
    }

    public void DestroySelf()
    {
        Destroy(gameObject);
    }
}
