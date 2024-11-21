using System.Collections.Generic;
using System.Linq;
using GridSystem;
using UnityEngine;

public class Splitter : PlaceableObjectBase, IItemCarrier
{
    //Ayırıcı..... 1 Giriş - 3 Çıkış

    private Item _currentItem;
    private int _sendingIndex;
    public List<Vector2Int> InputCoordinates { get; set; } = new();
    public List<Vector2Int> OutputCoordinates { get; set; } = new();


    protected override void Setup()
    {
        InputCoordinates.Add(Origin + PlaceableObjectBaseSo.GetDirForwardVector(Dir) * -1);
        OutputCoordinates = CurrentTile.GetNeighbourList(Grid).Select(t => t.GetGridPosition)
            .Where(c => !InputCoordinates.Contains(c)).ToList();
    }

    public override void DestroySelf()
    {
        base.DestroySelf();
        if(_currentItem != null)
            _currentItem.DestroySelf();
    }

    private void Update()
    {
        if (_currentItem == null) return;

        var targetPosition = Grid.GetWorldPosition(Origin) + Grid.GetCellSizeOffset();

        // İtemi hedef konuma doğru hareket ettir
        _currentItem.transform.position = Vector3.MoveTowards(_currentItem.transform.position, targetPosition, 0.01f);
        if (_currentItem.transform.position != targetPosition) return;

        // İtem gönderimi için döngü başlat
        for (int i = 0; i < OutputCoordinates.Count; i++)
        {
            var coordinate = OutputCoordinates[_sendingIndex];
            _sendingIndex = (_sendingIndex + 1) % OutputCoordinates.Count; // Sıradaki çıkışa geç
        
            // Hedef çıkış koordinatına sahip nesneyi bul
            if (Grid.GetGridObject(coordinate).OwnedObjectBase is not IItemCarrier sendingCarrier ||
                !sendingCarrier.InputCoordinates.Contains(Origin)) continue;

            // Eğer item gönderimi başarılıysa gönderimi tamamla
            if (sendingCarrier.TrySetWorldItem(_currentItem))
            {
                _currentItem = null;
                break;
            }
        }
    }


    public bool TrySetWorldItem(Item item)
    {
        if (_currentItem != null) 
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
}