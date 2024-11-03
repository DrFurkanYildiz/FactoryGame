using System.Collections.Generic;
using UnityEngine;

public class BeltTest: PlaceableObjectBase, IItemSlot
{
    public bool CanCarryItem(ItemSo itemSo)
    {
        return itemSo.isSolidItem;
    }

    public bool TrySetWorldItem(Item item)
    {
        return true;
    }

    public IEnumerable<Vector2Int> GetGridPosition()
    {
        return new[] { Origin };
    }

    public Vector3 GetCarryItemWorldPosition(Item item)
    {
        throw new System.NotImplementedException();
    }
}
