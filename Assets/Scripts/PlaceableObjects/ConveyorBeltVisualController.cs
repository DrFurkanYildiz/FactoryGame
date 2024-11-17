using System;
using System.Collections.Generic;
using UnityEngine;
public class ConveyorBeltVisualController : MonoBehaviour
{
    public enum BeltVisualDirection
    {
        Flat,
        DownRight,
        DownLeft,
        UpRight,
        UpLeft,
        RightDown,
        RightUp,
        LeftDown,
        LeftUp
    }

    private readonly List<Transform> _visualList = new();
    public BeltVisualDirection direction { get; private set; }

    private void Awake()
    {
        for (var i = 0; i < transform.childCount; i++)
            _visualList.Add(transform.GetChild(i).transform);
    }
    
    public void SetVisualDirection(BeltVisualDirection dir)
    {
        direction = dir;
        ChangeVisual(direction);
        GetComponentInParent<ConveyorBelt>()?.UpdateItemCarryList();
        //Debug.Log(dir);
    }

    private void ChangeVisual(BeltVisualDirection dir)
    {
        var i = dir switch
        {
            BeltVisualDirection.Flat => 0,
            BeltVisualDirection.DownRight => 2,
            BeltVisualDirection.DownLeft => 1,
            BeltVisualDirection.UpRight => 1,
            BeltVisualDirection.UpLeft => 2,
            BeltVisualDirection.RightDown => 1,
            BeltVisualDirection.RightUp => 2,
            BeltVisualDirection.LeftDown => 2,
            BeltVisualDirection.LeftUp => 1,
            _ => throw new ArgumentOutOfRangeException()
        };

        HideAllVisual();
        ActivatedVisual(i);
    }
    private void HideAllVisual()
    {
        _visualList.ForEach(v => v.gameObject.SetActive(false));
    }

    private void ActivatedVisual(int index)
    {
        _visualList[index].gameObject.SetActive(true);
    }
    public BeltVisualDirection GetVisualDirection(Dir myDirection, Dir preDirection)
    {
        return (myDirection, preDirection) switch
        {
            (Dir.Down, Dir.Right) => BeltVisualDirection.RightDown,
            (Dir.Up, Dir.Right) => BeltVisualDirection.RightUp,
            (Dir.Down, Dir.Left) => BeltVisualDirection.LeftDown,
            (Dir.Up, Dir.Left) => BeltVisualDirection.LeftUp,
            (Dir.Right, Dir.Down) => BeltVisualDirection.DownRight,
            (Dir.Left, Dir.Down) => BeltVisualDirection.DownLeft,
            (Dir.Right, Dir.Up) => BeltVisualDirection.UpRight,
            (Dir.Left, Dir.Up) => BeltVisualDirection.UpLeft,
            (Dir.Left, Dir.Left or Dir.Right) => BeltVisualDirection.Flat,
            (Dir.Right, Dir.Left or Dir.Right) => BeltVisualDirection.Flat,
            (Dir.Up, Dir.Up or Dir.Down) => BeltVisualDirection.Flat,
            (Dir.Down, Dir.Up or Dir.Down) => BeltVisualDirection.Flat,
            _ => direction
        };
    }
}
