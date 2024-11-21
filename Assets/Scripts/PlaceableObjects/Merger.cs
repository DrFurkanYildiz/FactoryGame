using System.Collections.Generic;
using System.Linq;
using GridSystem;
using UnityEngine;

public class Merger : PlaceableObjectBase, IItemCarrier
{
    //Birleştirici..... 3 Giriş - 1 Çıkış

    private Item _currentItem;
    private int _takenIndex;
    public List<Vector2Int> InputCoordinates { get; set; } = new();
    public List<Vector2Int> OutputCoordinates { get; set; } = new();

    private List<Vector2Int> _inputItemCarrierCoordinatesCache = new();
    private Vector2Int _nextTakenCoordinate;

    protected override void Setup()
    {
        OutputCoordinates.Add(Origin + PlaceableObjectBaseSo.GetDirForwardVector(Dir));
        InputCoordinates = CurrentTile.GetNeighbourList(Grid).Select(t => t.GetGridPosition)
            .Where(c => !OutputCoordinates.Contains(c)).ToList();
        UpdateInputCarrierCoordinatesCache();
    }

    public override void DestroySelf()
    {
        base.DestroySelf();
        if(_currentItem != null)
            _currentItem.DestroySelf();
    }

    private void Update()
    {
        if (_currentItem == null)
        {
            if (_inputItemCarrierCoordinatesCache.Count <= 0) return;

            _takenIndex = (_takenIndex + 1) % _inputItemCarrierCoordinatesCache.Count;
            _nextTakenCoordinate = _inputItemCarrierCoordinatesCache[_takenIndex];
        }
        else
        {
            var targetPosition = Grid.GetWorldPosition(Origin) + Grid.GetCellSizeOffset();

            _currentItem.transform.position = Vector3.MoveTowards(_currentItem.transform.position, targetPosition, 0.01f);
            if (_currentItem.transform.position != targetPosition) return;
        
            if (Grid.GetGridObject(OutputCoordinates[0]).OwnedObjectBase is not IItemCarrier sendingCarrier ||
                !sendingCarrier.InputCoordinates.Contains(Origin)) return;
            
            
            if (sendingCarrier.TrySetWorldItem(_currentItem))
            {
                _currentItem = null;
                if (_inputItemCarrierCoordinatesCache.Count > 0)
                {
                    _takenIndex = (_takenIndex + 1) % _inputItemCarrierCoordinatesCache.Count;
                    _nextTakenCoordinate = _inputItemCarrierCoordinatesCache[_takenIndex];
                }
            }
        }
        
    }

    public bool TrySetWorldItem(Item item)
    {
        if (_currentItem != null) 
            return false;

        if (Grid.GetCoordinate(item.transform.position) != _nextTakenCoordinate)
            return false;
        
        _currentItem = item;
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

    public void UpdateInputCarrierCoordinatesCache()
    {
        _inputItemCarrierCoordinatesCache = InputCoordinates
            .Where(c =>
                Grid.GetGridObject(c).OwnedObjectBase is IItemCarrier carrier &&
                carrier.OutputCoordinates.Contains(Origin))
            .ToList();
    }
}