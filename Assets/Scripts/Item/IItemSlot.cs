using System;
using System.Collections.Generic;
using UnityEngine;
public interface IItemSlot
{
    bool CanCarryItem(ItemSo itemSo);
    bool TrySetWorldItem(Item item);
    IEnumerable<Vector2Int> GetGridPosition();
    Vector3 GetCarryItemWorldPosition(Item item);
    void OnItemControl(Item item);
    bool IsCarryItem();
    void CarryControl();
}