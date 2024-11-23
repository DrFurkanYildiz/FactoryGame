using System.Linq;
using System.Collections.Generic;
using GridSystem;

public class ConveyorTunnelGate : Conveyor
{
    public ConveyorTunnelGate ConnectTunnelGate { get; private set; }
    private List<ConveyorTunnelBelt> _myTunnelBelt;
    public List<ConveyorTunnelBelt> AllTunnelBelts => _myTunnelBelt;
    private TunnelType _tunnelType;
    public enum TunnelType
    {
        Input, Output
    }
    public void SetTunnelBelt(List<ConveyorTunnelBelt> belts, ConveyorTunnelGate connectTunnelGate, TunnelType type)
    {
        _myTunnelBelt = belts;
        ConnectTunnelGate = connectTunnelGate;
        _tunnelType = type;
        UpdateCoordinate();
    }

    public override void DestroySelf()
    {
        base.DestroySelf();
        
        if (_myTunnelBelt is { Count: > 0 })
        {
            for (var i = _myTunnelBelt.Count - 1; i >= 0; i--)
            {
                if (_myTunnelBelt[i] != null)
                    _myTunnelBelt[i].DestroySelf();
            }
        }
        
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
            OutputCoordinates.Add(Origin + PlaceableObjectBaseSo.GetDirForwardVector(Dir));
            
            foreach (var tunnelBelt in _myTunnelBelt)
            {
                foreach (var nTile in CurrentTile.GetNeighbourList(Grid))
                {
                    if (nTile.GetGridPosition == tunnelBelt.Origin && tunnelBelt.OutputCoordinates.Contains(Origin))
                    {
                        InputCoordinates.Add(nTile.GetGridPosition);
                        var dir = PlaceableObjectBaseSo.GetDir(Origin, nTile.GetGridPosition);
                        vDirection = ConveyorBeltVisualController.GetVisualDirection(Dir, PlaceableObjectBaseSo.GetOppositeDirection(dir));
                    }
                }
            }
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
        if (_myTunnelBelt.Any(b=>b.Origin == OutputCoordinates[0]))
        {
            sendingCarrier = _myTunnelBelt.Find(t=>t.Origin == OutputCoordinates[0]);
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