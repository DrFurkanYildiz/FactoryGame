using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum Elements { Cross, Input, Output }
public enum IOType{ Input, Output}
public enum Dir { Down, Left, Up, Right, }

[CreateAssetMenu(fileName = "New PlacedObjectType", menuName = "PlacedObjectSO")]
public class PlacedObjectTypeSo : ScriptableObject
{
    public Wrapper<Elements>[] grid;
    public PlacedObjectType type;
    public bool isMachine;
    public ItemRecipeSo itemRecipeSo;
    
    public Transform prefab;
    public Transform visual;
    public Transform curvePrefab;
    public Transform curveVisual;
    [Range(1, 10)] public int height = 1;
    [Range(1, 10)] public int width = 1;
    
    
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
    

    #region Helper Func

    public static Dir GetNextDir(Dir dir) {
        switch (dir) {
            default:
            case Dir.Down:      return Dir.Left;
            case Dir.Left:      return Dir.Up;
            case Dir.Up:        return Dir.Right;
            case Dir.Right:     return Dir.Down;
        }
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

    #endregion
    
}

#if UNITY_EDITOR
[CustomEditor(typeof(PlacedObjectTypeSo))]
public class PlacedObjectTypeSoEditor : Editor
{
    private SerializedProperty grid;
    private SerializedProperty type;
    private SerializedProperty height;
    private SerializedProperty width;
    private SerializedProperty prefab;
    private SerializedProperty visual;
    int elementLength;

    private SerializedProperty isMachine;
    private SerializedProperty itemRecipeSo;

    private void OnEnable()
    {
        grid = serializedObject.FindProperty("grid");
        type = serializedObject.FindProperty("type");
        height = serializedObject.FindProperty("height");
        width = serializedObject.FindProperty("width");
        prefab = serializedObject.FindProperty("prefab");
        visual = serializedObject.FindProperty("visual");

        isMachine = serializedObject.FindProperty("isMachine");
        itemRecipeSo = serializedObject.FindProperty("itemRecipeSo");
        
        elementLength = Enum.GetValues(typeof(Elements)).Length;

        PlacedObjectTypeSo script = (PlacedObjectTypeSo)target;
        if (script.grid == null || script.grid.Length != script.height || script.grid[0]?.values.Length != script.width)
            script.ResetGrid();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        PlacedObjectTypeSo script = (PlacedObjectTypeSo)target;

        // Grid size inputs
        EditorGUILayout.PropertyField(type);
        EditorGUILayout.PropertyField(height);
        EditorGUILayout.PropertyField(width);
        EditorGUILayout.PropertyField(prefab);
        EditorGUILayout.PropertyField(visual);


        EditorGUILayout.PropertyField(isMachine);
        if (isMachine.boolValue) EditorGUILayout.PropertyField(itemRecipeSo);
        else script.itemRecipeSo = null;

        GUILayout.Space(10);
        
        if (GUILayout.Button("Reset Grid", GUILayout.Height(30), GUILayout.Width(200)))
        {
            script.ResetGrid();
        }
        
        GUILayout.Space(20);
        DrawGrid();



        serializedObject.ApplyModifiedProperties();
    }

    private void DrawGrid()
    {
        GUILayout.BeginVertical();
        PlacedObjectTypeSo script = (PlacedObjectTypeSo)target;

        if (script.grid == null || script.grid.Length != script.height || script.grid[0]?.values.Length != script.width)
        {
            script.ResetGrid();
            return;
        }

        for (int i = 0; i < script.height; i++)
        {
            GUILayout.BeginHorizontal();
            SerializedProperty row = grid.GetArrayElementAtIndex(i).FindPropertyRelative("values");

            for (int j = 0; j < script.width; j++)
            {
                var cell = row.GetArrayElementAtIndex(j);
                Elements element = (Elements)cell.intValue;
                Texture2D icon = GetIcon(element);
                string cord = j + "," + (script.height - 1 - i);
                
                // Draw button
                GUILayout.BeginVertical(GUILayout.Width(50));
                if (GUILayout.Button(icon, GUILayout.Width(50), GUILayout.Height(50)))
                {
                    cell.intValue = NextIndex(cell.intValue);
                }

                // Draw coordinates label below the button
                GUILayout.TextArea(cord, GUILayout.Width(25), GUILayout.Height(15));
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();
    }

    private int NextIndex(int index)
    {
        return (++index) % elementLength;
    }

    private Texture2D GetIcon(Elements element)
    {
        switch (element)
        {
            case Elements.Cross:
                return Resources.Load<Texture2D>("CrossIcon");
            case Elements.Input:
                return Resources.Load<Texture2D>("InputIcon");
            case Elements.Output:
                return Resources.Load<Texture2D>("OutputIcon");
            default:
                return Texture2D.grayTexture; // Default or empty
        }
    }
}
#endif

[Serializable]
public class Wrapper<T>
{
    public T[] values;
}
