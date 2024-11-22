using System;
using System.Collections.Generic;
using UnityEngine;

public enum Elements { Cross, Input, Output }
public enum IOType{ Input, Output}
public enum Dir { Down, Left, Up, Right, }
public enum PlaceableType
{
    ConveyorBelt,
    Storage,
    Machine,
    Splitter,
    Merger
}
public class PlaceableObjectBaseSo : ScriptableObject
{
    public PlaceableType type;
    public Transform prefab;
    public Transform visual;
    [Range(1, 10)] public int height = 1;
    [Range(1, 10)] public int width = 1;
    
    
    public static Dir GetNextDir(Dir dir) {
        switch (dir) {
            default:
            case Dir.Down:      return Dir.Left;
            case Dir.Left:      return Dir.Up;
            case Dir.Up:        return Dir.Right;
            case Dir.Right:     return Dir.Down;
        }
    }

    public static Dir GetOppositeDirection(Dir dir)
    {
        return dir switch
        {
            Dir.Down => Dir.Up,
            Dir.Left => Dir.Right,
            Dir.Up => Dir.Down,
            Dir.Right => Dir.Left,
            _ => throw new ArgumentOutOfRangeException(nameof(dir), dir, null)
        };
    }
    public static Vector2Int GetDirForwardVector(Dir dir) {
        switch (dir) {
            default:
            case Dir.Down:  return new Vector2Int( 0, -1);
            case Dir.Left:  return new Vector2Int(-1,  0);
            case Dir.Up:    return new Vector2Int( 0, +1);
            case Dir.Right: return new Vector2Int(+1,  0);
        }
    }
    public static List<Vector2Int> GetNeighbours(Vector2Int origin)
    {
        return new List<Vector2Int>
        {
            origin + new Vector2Int(0, -1),
            origin + new Vector2Int(-1, 0),
            origin + new Vector2Int(0, +1),
            origin + new Vector2Int(+1, 0)
        };
    }

    public static Dir GetDir(Vector2Int from, Vector2Int to) {
        if (from.x < to.x) {
            return Dir.Right;
        } else {
            if (from.x > to.x) {
                return Dir.Left;
            } else {
                if (from.y < to.y) {
                    return Dir.Up;
                } else {
                    return Dir.Down;
                }
            }
        }
    }
    public static int GetRotationAngle(Dir dir) {
        switch (dir) {
            default:
            case Dir.Down:  return 0;
            case Dir.Left:  return 90;
            case Dir.Up:    return 180;
            case Dir.Right: return 270;
        }
    }
    public Vector2Int GetRotationOffset(Dir dir) {
        switch (dir) {
            default:
            case Dir.Down:  return new Vector2Int(0, 0);
            case Dir.Left:  return new Vector2Int(0, width);
            case Dir.Up:    return new Vector2Int(width, height);
            case Dir.Right: return new Vector2Int(height, 0);
        }
    }
    public List<Vector2Int> GetGridPositionList(Vector2Int offset, Dir dir) {
        List<Vector2Int> gridPositionList = new List<Vector2Int>();
        switch (dir) {
            default:
            case Dir.Down:
            case Dir.Up:
                for (int x = 0; x < width; x++) {
                    for (int y = 0; y < height; y++) {
                        gridPositionList.Add(offset + new Vector2Int(x, y));
                    }
                }
                break;
            case Dir.Left:
            case Dir.Right:
                for (int x = 0; x < height; x++) {
                    for (int y = 0; y < width; y++) {
                        gridPositionList.Add(offset + new Vector2Int(x, y));
                    }
                }
                break;
        }
        return gridPositionList;
    }
}

[Serializable]
public class Wrapper<T>
{
    public T[] values;
}
