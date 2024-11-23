using System;
using System.Collections.Generic;
using GridSystem;
using UnityEngine;

public class ConveyorTunnelGate : Conveyor
{
    private List<ConveyorTunnelBelt> _myTunnelBelt;
    private TunnelType _tunnelType;
    public enum TunnelType
    {
        Input, Output
    }
    public void SetTunnelBelt(List<ConveyorTunnelBelt> belts, TunnelType type)
    {
        _myTunnelBelt = belts;
        _tunnelType = type;
        UpdateCoordinate();
    }

    private void UpdateCoordinate()
    {
        var vDirection = ConveyorBeltVisualController.BeltVisualDirection.Flat;
        if (_tunnelType is TunnelType.Input)
        {
            InputCoordinates.Add(Origin + PlaceableObjectBaseSo.GetDirForwardVector(Dir) * -1);
            OutputCoordinates.Add(Origin + PlaceableObjectBaseSo.GetDirForwardVector(Dir));
        }
        else
        {
            foreach (var nTile in CurrentTile.GetNeighbourList(Grid))
            {
                if (_myTunnelBelt.Contains(nTile.OwnedTunnelBelt) &&
                    nTile.OwnedTunnelBelt.OutputCoordinates.Contains(Origin))
                {
                    InputCoordinates.Add(nTile.GetGridPosition);

                    var dir = PlaceableObjectBaseSo.GetDir(Origin, nTile.GetGridPosition);
                    vDirection = ConveyorBeltVisualController.GetVisualDirection(Dir, PlaceableObjectBaseSo.GetOppositeDirection(dir));
                }
            }
            OutputCoordinates.Add(Origin + PlaceableObjectBaseSo.GetDirForwardVector(Dir));
            
            
        }
        
        itemCarryList = GetCarryPositions(vDirection);
    }

    private void Update()
    {
        ItemTransport();
    }

    protected override bool IsItemSending(out IItemCarrier sendingCarrier)
    {
        //Göndereceğimiz pozisyonda bir TunnelBelt varsa gönder.
        if (Grid.GetGridObject(OutputCoordinates[0])?.OwnedTunnelBelt != null)
        {
            sendingCarrier = Grid.GetGridObject(OutputCoordinates[0])?.OwnedTunnelBelt;
            return true;
        }
        
        //Göndereceğimiz pozisyonda bir TunnelBelt yoksa veya olan taşıyıcının item alma pozisyonu ise gönder.
        if (Grid.GetGridObject(OutputCoordinates[0])?.OwnedObjectBase is IItemCarrier sCarrier &&
            sCarrier.InputCoordinates.Contains(Origin))
        {
            sendingCarrier = sCarrier;
            return true;
        }

        sendingCarrier = null;
        return false;
    }
}