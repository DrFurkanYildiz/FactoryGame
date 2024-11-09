using System;
using DG.Tweening;
using GridSystem;
using UnityEngine;

public class Item : MonoBehaviour
{
    public static Item CreateItem(Vector3 position, ItemSo itemSo)
    {
        var itemTransform = Instantiate(itemSo.prefab, position, Quaternion.identity);
        var item = itemTransform.GetComponent<Item>();
        item.ItemSo = itemSo;
        return item;
    }

    private Vector2Int _gridPosition;
    public ItemSo ItemSo { get; private set; }

    public void DestroySelf()
    {
        Destroy(gameObject);
    }
}
