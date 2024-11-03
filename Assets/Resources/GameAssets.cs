using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameAssets : MonoBehaviour
{
    #region Singleton
    private static GameAssets _i;
    public static GameAssets i
    {
        get
        {
            if (_i == null) _i = Instantiate(Resources.Load<GameAssets>("GameAssets"));
            return _i;
        }
    }

    private void Awake()
    {
        _i = this;
        LoadResource();
    }
    #endregion


    private List<PlaceableObjectBaseSo> _placedObjectTypeSoList;
    private List<ItemSo> _itemSoList;

    private void LoadResource()
    {
        _placedObjectTypeSoList = Resources.LoadAll<PlaceableObjectBaseSo>("PlaceableObjectsSo").ToList();
        _itemSoList = Resources.LoadAll<ItemSo>("ItemsSo").ToList();
    }
    
    public PlaceableObjectBaseSo GetPlacedSo(PlaceableType type)
    {
        return _placedObjectTypeSoList.Find(t => t.type == type);
    }

    public ItemSo GetItemSo(ItemType type)
    {
        return _itemSoList.Find(i => i.type == type);
    }
}

