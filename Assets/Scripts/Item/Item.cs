using System;
using DG.Tweening;
using GridSystem;
using UnityEngine;
using UnityEngine.Serialization;

public class Item : MonoBehaviour
{
    public static Item CreateItem(Vector3 position, ItemSo itemSo)
    {
        var itemTransform = Instantiate(itemSo.prefab, position, Quaternion.identity);
        var item = itemTransform.GetComponent<Item>();
        item.ItemSo = itemSo;
        return item;
    }

    public ItemSo ItemSo { get; private set; }
    [SerializeField] private GameObject visual;

    public void DestroySelf()
    {
        Destroy(gameObject);
    }

    public void SetCarrier(IItemCarrier carrier)
    {
        visual.SetActive(carrier is not ConveyorTunnelBelt);
    }
}
