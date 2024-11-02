﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum ConveyorTunnelType{ Input, Output}
public class ConveyorTunnel : PlacedObject, IItemSlot
{
    private Item _item;
    
    private Vector2Int _nextPosition;
    
    protected override void Setup()
    {
        _nextPosition = Origin + PlacedObjectTypeSo.GetDirForwardVector(Dir);
    }

    public override void DestroySelf()
    {
        base.DestroySelf();
        
        if(_item != null)
            _item.DestroySelf();
    }

    private void Update()
    {
        if (_item == null || !_item.CanMove) return;
        if (ConveyorTunnelType is ConveyorTunnelType.Input)
        {
            if(TunnelPlacedObject is not IItemSlot targetConveyorTunnel) return;
            if (targetConveyorTunnel.TrySetWorldItem(_item))
            {
                _item.transform.position =
                    Grid.GetWorldPosition(targetConveyorTunnel.GetGridPosition().FirstOrDefault()) +
                    new Vector3(0.5f, 0, 0.5f);
                _item = null;
            
                Debug.Log("Take Action!");
            }
        }
        else
        {
            var nextPlacedObject = Grid.GetGridObject(_nextPosition).OwnedObject;
            if (nextPlacedObject == null) return;
            
            if (nextPlacedObject is not IItemSlot nextPlacedSlot || !nextPlacedSlot.CanCarryItem(_item.GetItemSo())) return;
            if (nextPlacedSlot.GetGridPosition().All(p => p != _nextPosition)) return;

            if (nextPlacedSlot.TrySetWorldItem(_item))
            {
                _item.MoveToItemSlot
                    (nextPlacedSlot.GetGridPosition().FirstOrDefault(p => p == _nextPosition));
                _item = null;
            
                Debug.Log("Take Action!");
            }
        }

    }

    

    public bool CanCarryItem(ItemSo itemSo)
    {
        return true;
    }

    public bool TrySetWorldItem(Item item)
    {
        if (_item != null) return false;
        _item = item;
        return true;
    }

    public IEnumerable<Vector2Int> GetGridPosition()
    {
        return new[] { Origin };
    }
}