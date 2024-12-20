using System.Collections.Generic;
using UnityEngine;
public interface IItemCarrier
{
    void ItemTransport();
    bool TrySetWorldItem(Item item);
    float ItemCarrySpeed();
    List<Vector2Int> OutputCoordinates { get; set; }
    List<Vector2Int> InputCoordinates { get; set; }

    Dir GetDirectionAccordingOurCoordinate(Vector2Int coordinate);
    

}