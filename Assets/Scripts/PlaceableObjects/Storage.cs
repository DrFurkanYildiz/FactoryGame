using System.Collections.Generic;
using UnityEngine;

public class Storage : PlaceableObjectBase, IItemCarrier
{
    private ItemSo _itemSo;

    private Item _inputItem;
    private Item _outputItem;
    private Vector2Int _inputCoordinate;
    private Vector2Int _outputCoordinate;
    
    private Vector3 _outputWorldPosition;
    private Vector3 _inputWorldPosition;

    private int _inputItemCount;

    [field:SerializeField] public List<Vector2Int> OutputCoordinates { get; set; }
    [field:SerializeField]public List<Vector2Int> InputCoordinates { get; set; }
    
    
    
    protected override void Setup()
    {
        _itemSo = GameAssets.i.GetItemSo((ItemType)Random.Range(1,3));
        
        _inputCoordinate = ((PlaceableBlueprintSo)placeableObjectSo).GetIOTypePositionList(IOType.Input, Origin, Dir)[0];
        _outputCoordinate = ((PlaceableBlueprintSo)placeableObjectSo).GetIOTypePositionList(IOType.Output, Origin, Dir)[0];

        InputCoordinates.Add(_inputCoordinate + PlaceableObjectBaseSo.GetDirForwardVector(Dir) * -1);
        OutputCoordinates.Add(_outputCoordinate + PlaceableObjectBaseSo.GetDirForwardVector(Dir));
        
        _inputWorldPosition = Grid.GetWorldPosition(_inputCoordinate) + Grid.GetCellSizeOffset();
        _outputWorldPosition = Grid.GetWorldPosition(_outputCoordinate) + Grid.GetCellSizeOffset();
    }
    

    public override void DestroySelf()
    {
        base.DestroySelf();

        if (_inputItem != null)
            _inputItem.DestroySelf();
        if(_outputItem != null)
            _outputItem.DestroySelf();
    }

    private void Update()
    {
        if (_inputItem != null)
        {
            // İtemi hedef konuma doğru hareket ettir
            _inputItem.transform.position = Vector3.MoveTowards(_inputItem.transform.position, _inputWorldPosition, 0.01f);
            if (_inputItem.transform.position != _inputWorldPosition) return;
            
            _inputItem.DestroySelf();
            _inputItemCount++;
            _inputItem = null;
        }

        if (_outputItem == null)
        {
            _outputItem = Item.CreateItem(_outputWorldPosition, _itemSo);
        }
        else
        {
            if (Grid.GetGridObject(OutputCoordinates[0]).OwnedObjectBase is not IItemCarrier sendingCarrier ||
                !sendingCarrier.InputCoordinates.Contains(_outputCoordinate))
                return;
            
            if (sendingCarrier.TrySetWorldItem(_outputItem))
            {
                _outputItem = null;
            }
        }
        
    }

    public bool TrySetWorldItem(Item item)
    {
        if (_inputItem != null) return false;
        _inputItem = item;
        return true;
    }

    public Dir GetDirectionAccordingOurCoordinate(Vector2Int currentCoordinate)
    {
        return Dir;
    }
}