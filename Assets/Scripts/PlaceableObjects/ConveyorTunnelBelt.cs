using System.Linq;

public class ConveyorTunnelBelt : Conveyor
{
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
        //Göndereceğimiz pozisyonda bir TunnelBelt varsa gönder.
        if (Grid.GetGridObject(OutputCoordinates.First())?.OwnedTunnelBelt != null)
        {
            sendingCarrier = Grid.GetGridObject(OutputCoordinates.First())?.OwnedTunnelBelt;
            return true;
        }
        
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
