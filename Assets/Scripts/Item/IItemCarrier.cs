using System.Collections.Generic;
using UnityEngine;
public interface IItemCarrier
{
    bool TrySetWorldItem(Item item);
    IEnumerable<Vector2Int> GetGridPosition();
    
    List<Vector2Int> OutputCoordinates { get; set; }
    List<Vector2Int> InputCoordinates { get; set; }
    
    List<IItemCarrier> SendingItemCarriers { get; set; }
    List<IItemCarrier> TakenItemCarriers { get; set; }

    Dir GetDirectionAccordingOurCoordinate(Vector2Int coordinate);

}