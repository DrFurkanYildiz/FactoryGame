using System.Collections.Generic;
using GridSystem;
using UnityEngine;

public class ConveyorBelt : Conveyor
{
    public ConveyorBeltVisualController BeltVisual { get; private set; }

    protected override void Setup()
    {
        base.Setup();
        
        BeltVisual = transform.GetComponentInChildren<ConveyorBeltVisualController>();
        BeltVisual.OnUpdateVisualDirection += UpdateItemCarryList;
        BeltVisual.OnUpdateVisualDirection += UpdateNextAndPreviousConveyorBelt;
    }

    private void Start()
    {
        UpdateItemCarryList();
        UpdateNextAndPreviousConveyorBelt();
    }
    
    
    private void UpdateNextAndPreviousConveyorBelt()
    {
        //Bant yönü değiştiğinde giriş çıkış kordinatlarını günceller.
        
        OutputCoordinates.Clear();
        InputCoordinates.Clear();
        
        if (!OutputCoordinates.Contains(GetOutputCoordinate()))
            OutputCoordinates.Add(GetOutputCoordinate());
        if (!InputCoordinates.Contains(GetInputCoordinate()))
            InputCoordinates.Add(GetInputCoordinate());
    }
    
    private void UpdateItemCarryList()
    {
        itemCarryList = GetCarryPositions(BeltVisual.direction);
    }



    private void Update()
    {
        ItemTransport();
    }
    

    private Vector2Int GetInputCoordinate()
    {
        return BeltVisual.direction switch
        {
            ConveyorBeltVisualController.BeltVisualDirection.Flat => Origin + PlaceableObjectBaseSo.GetDirForwardVector(Dir) * -1,
            ConveyorBeltVisualController.BeltVisualDirection.RightDown => Origin - Vector2Int.right,
            ConveyorBeltVisualController.BeltVisualDirection.LeftDown => Origin - Vector2Int.left,
            ConveyorBeltVisualController.BeltVisualDirection.DownRight => Origin - Vector2Int.down,
            ConveyorBeltVisualController.BeltVisualDirection.DownLeft => Origin - Vector2Int.down,
            ConveyorBeltVisualController.BeltVisualDirection.LeftUp => Origin - Vector2Int.left,
            ConveyorBeltVisualController.BeltVisualDirection.RightUp => Origin - Vector2Int.right,
            ConveyorBeltVisualController.BeltVisualDirection.UpRight => Origin - Vector2Int.up,
            ConveyorBeltVisualController.BeltVisualDirection.UpLeft => Origin - Vector2Int.up,
            _ => Vector2Int.zero
        };
    }

    private Vector2Int GetOutputCoordinate()
    {
        return Origin + PlaceableObjectBaseSo.GetDirForwardVector(Dir);
    }

    /// <summary>
    /// Bize item gönderebilecek item taşıyıcıların listesi. Yani bize bakan taşıyıcılar.
    /// </summary>
    /// <returns></returns>
    public List<IItemCarrier> GetNeighborCarriersThatCanSend()
    {
        var list = new List<IItemCarrier>();

        foreach (var tile in CurrentTile.GetNeighbourList(Grid))
        {
            switch (tile.OwnedObjectBase)
            {
                case ConveyorBelt neighbourBelt:
                {
                    if (neighbourBelt.OutputCoordinates.Contains(Origin) && Dir != PlaceableObjectBaseSo.GetOppositeDirection(neighbourBelt.Dir))
                    {
                        if (!list.Contains(neighbourBelt))
                            list.Add(neighbourBelt);
                    }

                    break;
                }
                case Splitter neighbourSplitter:
                    var nDir = PlaceableObjectBaseSo.GetDir(neighbourSplitter.Origin, Origin);
                    if (neighbourSplitter.OutputCoordinates.Contains(Origin) && Dir != PlaceableObjectBaseSo.GetOppositeDirection(nDir))
                    {
                        if (!list.Contains(neighbourSplitter))
                            list.Add(neighbourSplitter);
                    }
                    break;
            }
        }

        return list;
    }


    
}