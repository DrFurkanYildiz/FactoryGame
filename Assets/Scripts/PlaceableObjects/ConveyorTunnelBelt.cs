using System;
using System.Collections.Generic;
using UnityEngine;

public class ConveyorTunnelBelt : Conveyor
{
    public ConveyorTunnelGate FirstGate { get; set; }
    protected override void Setup()
    {
        base.Setup();

        InputCoordinates.Add(Origin + PlaceableObjectBaseSo.GetDirForwardVector(Dir) * -1);
        OutputCoordinates.Add(Origin + PlaceableObjectBaseSo.GetDirForwardVector(Dir));

        itemCarryList = GetCarryPositions(ConveyorBeltVisualController.BeltVisualDirection.Flat);
    }

    private void Update()
    {
        ItemTransport();
    }
    
    protected override bool IsItemSending(out IItemCarrier sendingCarrier)
    {
        if (Grid.GetGridObject(OutputCoordinates[0])?.OwnedObjectBase is IItemCarrier sCarrier &&
            sCarrier.InputCoordinates.Contains(Origin))
        {
            sendingCarrier = sCarrier;
            return true;
        }
        
        //Göndereceğimiz pozisyonda bir TunnelBelt varsa gönder.
        foreach (var tunnelBelt in FirstGate.AllTunnelBelts)
        {
            if (tunnelBelt.Origin == OutputCoordinates[0])
            {
                sendingCarrier = tunnelBelt;
                return true;
            }
        }

        sendingCarrier = null;
        return false;
    }

    protected override List<Vector3> GetCarryPositions(ConveyorBeltVisualController.BeltVisualDirection direction)
    {
        var list = new List<Vector3>();
        var a = Grid.GetCellSize() / MaxItemCarryCount;
        var basePosition = Grid.GetWorldPosition(Origin) + Grid.GetCellSizeOffset();
        
        for (var i = 1; i > 1 - MaxItemCarryCount; i--)
        {
            var offset = Dir switch
            {
                Dir.Down => new Vector3(0, 0, a * i),
                Dir.Left => new Vector3(a * i, 0, 0),
                Dir.Up => new Vector3(0, 0, a * -i),
                Dir.Right => new Vector3(a * -i, 0, 0),
                _ => throw new ArgumentOutOfRangeException()
            };
            list.Add(basePosition + 2 * offset);
        }
        return list;
    }
}
