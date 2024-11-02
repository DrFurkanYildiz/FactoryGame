﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class ItemRecipeSo : ScriptableObject {

    public List<RecipeItem> outputItemList;
    public List<RecipeItem> inputItemList;
    public float craftingEffort;

    [System.Serializable]
    public struct RecipeItem {

        public ItemSo item;
        public int amount;
    }
}