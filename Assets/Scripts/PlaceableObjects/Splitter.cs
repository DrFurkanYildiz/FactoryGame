using System;
using System.Collections.Generic;
using System.Linq;
using GridSystem;
using UnityEngine;

public class Splitter : PlaceableObjectBase, IItemCarrier
{
    //Ayırıcı..... 1 Giriş - 3 Çıkış
    private int _gateIndex;

    public List<Vector2Int> InputCoordinates { get; set; } = new();
    public List<Vector2Int> OutputCoordinates { get; set; } = new();


    public List<IItemCarrier> SendingItemCarriers { get; set; }
    public List<IItemCarrier> TakenItemCarriers { get; set; }
    public Dir GetDirectionAccordingOurCoordinate(Vector2Int coordinate)
    {
        return PlaceableObjectBaseSo.GetDir(Origin, coordinate);
    }


    private IItemCarrier _itemTakenCarrier;
    private Dictionary<Vector2Int, IItemCarrier> _itemSendingCarriers = new();

    protected override void Setup()
    {
        InputCoordinates.Add(Origin + PlaceableObjectBaseSo.GetDirForwardVector(Dir) * -1);

        OutputCoordinates = CurrentTile.GetNeighbourList(Grid).Select(t => t.GetGridPosition)
            .Where(c => !InputCoordinates.Contains(c)).ToList();
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