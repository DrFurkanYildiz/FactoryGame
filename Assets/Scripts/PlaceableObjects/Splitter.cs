using System;
using System.Collections.Generic;
using System.Linq;
using GridSystem;
using UnityEngine;

public class Splitter : PlaceableObjectBase, IItemCarrier
{
    //Ayırıcı..... 1 Giriş - 3 Çıkış
    public List<Vector2Int> InputCoordinates { get; set; } = new();
    public List<Vector2Int> OutputCoordinates { get; set; } = new();
    

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

    public List<Vector2Int> GetGridPosition()
    {
        return new List<Vector2Int> { Origin };
    }
    
    public Dir GetDirectionAccordingOurCoordinate(Vector2Int coordinate)
    {
        return PlaceableObjectBaseSo.GetDir(Origin, coordinate);
    }
}