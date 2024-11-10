using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class Splitter : PlaceableObjectBase, IItemCarrier
{
    
    //İtem ayırıcı yerine birleştirici gibi oldu kuyruk kaldırılabilir
    [ShowInInspector] private readonly Queue<Item> _items = new();
    private const int MaxItemCarryCount = 3;

    private void Update()
    {
        if (_items.Count == 0) return;

        foreach (var item in _items)
        {
            var targetPosition = Grid.GetWorldPosition(Origin) + Grid.GetCellSizeOffset();
            if (item.transform.position != targetPosition)
            {
                item.transform.position = Vector3.MoveTowards(item.transform.position, targetPosition, .01f);
            }
        }
    }

    public bool TrySetWorldItem(Item item)
    {
        if (!item.ItemSo.isSolidItem) return false;
        if (_items.Count >= MaxItemCarryCount) return false;

        _items.Enqueue(item);
        return true;
    }

    public IEnumerable<Vector2Int> GetGridPosition()
    {
        return new[] { Origin };
    }
}