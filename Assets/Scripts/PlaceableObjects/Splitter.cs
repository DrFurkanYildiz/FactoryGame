using System;
using System.Collections.Generic;
using System.Linq;
using GridSystem;
using UnityEngine;

public class Splitter : PlaceableObjectBase, IItemCarrier
{
    //Ayırıcı..... 1 Giriş - 3 Çıkış
    private int _gateIndex;

    private Vector2Int _inputCoordinates;
    private List<Vector2Int> _outputCoordinates = new();

    
    
    private IItemCarrier _itemTakenCarrier;
    private Dictionary<Vector2Int, IItemCarrier> _itemSendingCarriers = new();

    protected override void Setup()
    {
        _inputCoordinates = Origin + PlaceableObjectBaseSo.GetDirForwardVector(Dir) * -1;
        _outputCoordinates = CurrentTile.GetNeighbourList(Grid).Select(t => t.GetGridPosition)
            .Where(c => c != _inputCoordinates).ToList();
    }

    private void Update()
    {
        
    }

    public bool TrySetWorldItem(Item item)
    {
        return true;
    }

    public IEnumerable<Vector2Int> GetGridPosition()
    {
        return new[] { Origin };
    }

}