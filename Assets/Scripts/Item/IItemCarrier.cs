using System.Collections.Generic;
using UnityEngine;
public interface IItemCarrier
{
    bool TrySetWorldItem(Item item);
    IEnumerable<Vector2Int> GetGridPosition();
}