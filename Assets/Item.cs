using DG.Tweening;
using GridSystem;
using UnityEngine;

public class Item : MonoBehaviour
{
    public static Item CreateItem(Grid<Tile> grid, Vector2Int gridPosition, ItemSo itemSo)
    {
        var itemTransform = 
            Instantiate(itemSo.prefab, grid.GetWorldPosition(gridPosition) + new Vector3(0.5f, 0, 0.5f), Quaternion.identity);
        var item = itemTransform.GetComponent<Item>();
        item._grid = grid;
        item._gridPosition = gridPosition;
        item._itemSo = itemSo;
        return item;
    }

    private Grid<Tile> _grid;
    private Vector2Int _gridPosition;
    private bool _hasAlreadyMoved;
    private ItemSo _itemSo;
    public bool CanMove => !_hasAlreadyMoved;
    

    public void MoveToItemSlot(Vector2Int inputGate)
    {
        _hasAlreadyMoved = true;
        var position = _grid.GetWorldPosition(inputGate) + new Vector3(0.5f, 0, 0.5f);
        transform.DOMove(position, .1f).OnComplete(() => _hasAlreadyMoved = false);
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
