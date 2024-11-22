using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Machine : PlaceableObjectBase, IItemCarrier
{
    private const int InputItemMaxStackAmount = 10;
    private PlaceableBlueprintSo _blueprintSo;
    private float _craftingProgress;
    
    public List<Vector2Int> OutputCoordinates { get; set; } = new();
    public List<Vector2Int> InputCoordinates { get; set; } = new();
 
    private List<ItemRecipeSo.RecipeItem> _recipeInputs;
    private List<ItemRecipeSo.RecipeItem> _recipeOutputs;
    
    private readonly List<ItemStack> _inputItemStacks = new();
    private readonly List<ItemStack> _outputItemStacks = new();

    [Serializable]
    private class ItemStack
    {
        public ItemSo itemSo;
        public Item item;
        public int count;
        public Vector2Int coordinate;
    }

    protected override void Setup()
    {
        _blueprintSo = (PlaceableBlueprintSo)placeableObjectSo;
        _recipeInputs = _blueprintSo.itemRecipeSo.inputItemList;
        _recipeOutputs = _blueprintSo.itemRecipeSo.outputItemList;

        var input = _blueprintSo.GetIOTypePositionList(IOType.Input, Origin, Dir);
        var output = _blueprintSo.GetIOTypePositionList(IOType.Output, Origin, Dir);
        
        input.ForEach(c => InputCoordinates.Add(c + PlaceableObjectBaseSo.GetDirForwardVector(Dir) * -1));
        output.ForEach(c => OutputCoordinates.Add(c + PlaceableObjectBaseSo.GetDirForwardVector(Dir)));
        
        for (var i = 0; i < _recipeInputs.Count; i++)
            _inputItemStacks.Add(new ItemStack { itemSo = _recipeInputs[i].item });

        for (var i = 0; i < output.Count; i++)
            if (i < _recipeOutputs.Count)
                _outputItemStacks.Add(new ItemStack
                    { itemSo = _recipeOutputs[i].item, item = null, count = 0, coordinate = output[i] });

    }

    private void Update()
    {

        if (HasEnoughItemsToCraft())
        {
            _craftingProgress += Time.deltaTime;

            if (_craftingProgress >= _blueprintSo.itemRecipeSo.craftingEffort)
            {
                _craftingProgress = 0f;
                ItemCraft();

                Debug.Log("Craft Machine!");
            }
        }

        OutputAction();

    }

    private void ItemCraft()
    {
        //Girdileri tüket
        foreach (var recipeInput in _recipeInputs)
        {
            foreach (var itemStack in _inputItemStacks)
                if (itemStack.itemSo == recipeInput.item)
                    itemStack.count -= recipeInput.amount;
        }

        //Çıktıları ekle
        foreach (var recipeOutput in _recipeOutputs)
        {
            foreach (var itemStack in _outputItemStacks)
            {
                if (itemStack.itemSo == recipeOutput.item)
                    itemStack.count += recipeOutput.amount;
            }
        }
    }

    public bool TrySetWorldItem(Item item)
    {
        if (_inputItemStacks.All(s => s.itemSo != item.ItemSo || s.count >= InputItemMaxStackAmount))
            return false;

        foreach (var itemStack in _inputItemStacks)
            if (itemStack.itemSo == item.ItemSo)
            {
                itemStack.count++;
                item.DestroySelf();
            }

        return true;
    }

    public Dir GetDirectionAccordingOurCoordinate(Vector2Int coordinate)
    {
        return Dir;
    }

    private bool HasEnoughItemsToCraft()
    {
        return _recipeInputs.TrueForAll(r => _inputItemStacks.Find(s => s.itemSo == r.item).count >= r.amount);
    }

    private void OutputAction()
    {
        foreach (var nextCoordinate in OutputCoordinates)
        {
            var outputGate = nextCoordinate + PlaceableObjectBaseSo.GetDirForwardVector(Dir) * -1;
            if (Grid.GetGridObject(nextCoordinate).OwnedObjectBase is not IItemCarrier sendingCarrier ||
                !sendingCarrier.InputCoordinates.Contains(outputGate))
                continue;

            foreach (var itemStack in _outputItemStacks)
            {
                if (itemStack.coordinate == outputGate)
                {
                    if (itemStack.item == null && itemStack.count > 0)
                    {
                        var position = Grid.GetWorldPosition(outputGate) + Grid.GetCellSizeOffset();
                        itemStack.item = Item.CreateItem(position, itemStack.itemSo);
                        itemStack.count--;
                    }
                    
                    if (itemStack.item != null && sendingCarrier.TrySetWorldItem(itemStack.item))
                    {
                        itemStack.item = null;
                    }
                }
            }
        }
    }
}
