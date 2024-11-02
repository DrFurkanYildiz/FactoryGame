using System.Collections.Generic;
using UnityEngine;
public interface IItemSlot
{
    bool CanCarryItem(ItemSo itemSo);
    bool TrySetWorldItem(Item item);
    IEnumerable<Vector2Int> GetGridPosition();
}