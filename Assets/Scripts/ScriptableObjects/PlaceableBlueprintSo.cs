using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Placeable Blueprint So")]
public class PlaceableBlueprintSo : PlaceableObjectSo
{
    public Wrapper<Elements>[] grid;
    public bool isMachine;
    public ItemRecipeSo itemRecipeSo;
    
    
    private void OnEnable()
    {
        if (grid == null || grid.Length != height || grid[0]?.values.Length != width)
            ResetGrid();

    }
    public void ResetGrid()
    {
        grid = new Wrapper<Elements>[height];
        for (int i = 0; i < height; i++)
        {
            grid[i] = new Wrapper<Elements>();
            grid[i].values = new Elements[width];
        }
    }
    
    public List<Vector2Int> GetIOTypePositionList(IOType iType ,Vector2Int origin, Dir dir)
    {
        var inputList = new List<Vector2Int>();
        var type = iType == IOType.Input ? Elements.Input : Elements.Output;
        for (var i = 0; i < height; i++)
        {
            for (var j = 0; j < width; j++)
            {
                if (grid[i].values[j] == type)
                {
                    var inputPos = CalculateRotatedPosition(new Vector2Int(j, height - 1 - i), origin, dir);
                    inputList.Add(inputPos);
                }
            }
        }
        return inputList;
    }
    
    
    private Vector2Int CalculateRotatedPosition(Vector2Int localPos, Vector2Int origin, Dir dir)
    {
        var rotatedPos = dir switch
        {
            Dir.Down => localPos,
            Dir.Left => new Vector2Int(localPos.y, -localPos.x),
            Dir.Up => new Vector2Int(-localPos.x, -localPos.y),
            Dir.Right => new Vector2Int(-localPos.y, localPos.x),
            _ => localPos
        };
    
        var offset = dir switch
        {
            Dir.Down => origin,
            Dir.Left => origin + new Vector2Int(0, width - 1),
            Dir.Up => origin + new Vector2Int(width - 1, height - 1),
            Dir.Right => origin + new Vector2Int(height - 1, 0),
            _ => origin
        };

        return offset + rotatedPos;
    }

}