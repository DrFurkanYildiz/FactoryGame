using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class Machine : PlaceableObjectBase, IItemSlot
{
    private PlaceableBlueprintSo _blueprintSo;
    private float _craftingProgress;
    private List<ItemRecipeSo.RecipeItem> _recipeInputs;
    private List<ItemRecipeSo.RecipeItem> _recipeOutputs;

    private List<Vector2Int> _inputGates;
    private List<Vector2Int> _outputGates;

    private const int InputItemMaxStackAmount = 10;

    [ShowInInspector] private Dictionary<ItemSo, int> _inputStacks = new Dictionary<ItemSo, int>();
    [ShowInInspector] private Dictionary<ItemSo, int> _outputStacks = new Dictionary<ItemSo, int>();

    [ShowInInspector] private List<Item> _stackItem = new();

    private Dictionary<Vector2Int, ItemSo> _outputGateItemMap;

    protected override void Setup()
    {
        if (placeableObjectSo is not PlaceableBlueprintSo blueprintSo) return;
        _blueprintSo = blueprintSo;
        _recipeInputs = blueprintSo.itemRecipeSo.inputItemList;
        _recipeOutputs = blueprintSo.itemRecipeSo.outputItemList;

        _inputGates = blueprintSo.GetIOTypePositionList(IOType.Input, Origin, Dir);
        _outputGates = blueprintSo.GetIOTypePositionList(IOType.Output, Origin, Dir);

        _recipeInputs.ForEach(r => _inputStacks.Add(r.item, 0));
        _recipeOutputs.ForEach(r => _outputStacks.Add(r.item, 0));


        _outputGateItemMap = new Dictionary<Vector2Int, ItemSo>();

        for (int i = 0; i < _outputGates.Count; i++)
        {
            if (i < _recipeOutputs.Count) // Eğer bir ürün varsa kapıya eşle
            {
                _outputGateItemMap[_outputGates[i]] = _recipeOutputs[i].item;
                Debug.Log($"{_outputGates[i]} kapısına {_recipeOutputs[i].item.name} eklendi.");
            }
        }
    }

    private void Update()
    {
        if (HasEnoughItemsToCraft())
        {
            _craftingProgress += Time.deltaTime;

            if (_craftingProgress >= _blueprintSo.itemRecipeSo.craftingEffort)
            {
                _craftingProgress = 0f;

                foreach (var recipeItem in _recipeInputs)
                    if (_inputStacks.ContainsKey(recipeItem.item))
                        _inputStacks[recipeItem.item] -= recipeItem.amount;

                foreach (var recipeItem in _recipeOutputs)
                {
                    if (_outputStacks.ContainsKey(recipeItem.item))
                    {
                        _outputStacks[recipeItem.item] += recipeItem.amount;
                    }
                }


                Debug.Log("Craft Machine!");
            }
        }

        if (_stackItem.Count > 0 && _stackItem[0]?.CanMove == true)
        {
            _stackItem[0].DestroySelf();
            _stackItem.RemoveAt(0);
        }


        OutputStorageAction();
    }

    public bool CanCarryItem(ItemSo itemSo)
    {
        //Makinalar ürün tipine bakarak aldığı için taşıma solid kontolü yapmasına gerek yok.
        return true;
    }

    public bool TrySetWorldItem(Item item)
    {
        var itemSo = item.GetItemSo();
        if (!_inputStacks.ContainsKey(itemSo) || _inputStacks[itemSo] >= InputItemMaxStackAmount)
            return false;

        _stackItem.Add(item);
        _inputStacks[itemSo]++;
        return true;
    }

    public IEnumerable<Vector2Int> GetGridPosition()
    {
        return _inputGates.ToArray();
    }

    private bool HasEnoughItemsToCraft()
    {
        return _recipeInputs.TrueForAll(recipeInput => _inputStacks[recipeInput.item] >= recipeInput.amount);
    }

    private void OutputStorageAction()
    {
        Debug.Log("OutputStorageAction başlatıldı.");

        foreach (var outputGate in _outputGates)
        {
            Debug.Log($"Çıkış kapısı kontrol ediliyor: {outputGate}");

            // Çıkış kapısındaki komşu nesneyi al
            var outputPosition = outputGate + PlaceableObjectSo.GetDirForwardVector(Dir);
            var outputTile = Grid.GetGridObject(outputPosition);
            if (outputTile.OwnedObjectBase is not IItemSlot itemSlotObj)
            {
                Debug.LogWarning($"Komşu nesne IItemSlot değil: {outputTile.OwnedObjectBase}");
                continue;
            }

            if (itemSlotObj.GetGridPosition().All(p => p != outputPosition))
            {
                Debug.LogWarning($"Komşu nesne giriş kapısı değil: {outputPosition}");
                continue;
            }

            // Bu kapıya eşlenen öğeyi al
            if (!_outputGateItemMap.TryGetValue(outputGate, out var outputItem))
            {
                Debug.LogWarning($"Kapıya öğe eşlemesi yapılmamış: {outputGate}");
                continue;
            }

            Debug.Log($"Kontrol edilen öğe: {outputItem.name}, Mevcut stok: {_outputStacks[outputItem]}");

            if (_outputStacks[outputItem] <= 0)
            {
                Debug.Log($"Stokta yeterli {outputItem.name} yok, diğer öğeye geçiliyor.");
                continue;
            }

            // Öğeyi çıkış noktasına aktarmayı dene
            var itemInstance = Item.CreateItem(Grid, outputGate, outputItem);
            Debug.Log($"{outputItem.name} öğesi {outputGate} noktasında oluşturuldu.");

            if (itemSlotObj.TrySetWorldItem(itemInstance) && itemSlotObj.CanCarryItem(itemInstance.GetItemSo()))
            {
                // Başarıyla aktarılırsa öğeyi taşı ve stoktan düş
                Debug.Log($"{outputItem.name} öğesi başarıyla çıkış kapısına aktarıldı.");
                itemInstance.MoveToItemSlot(outputPosition);
                _outputStacks[outputItem]--;
                Debug.Log($"{outputItem.name} öğesi stoktan düşüldü. Kalan miktar: {_outputStacks[outputItem]}");
            }
            else
            {
                Debug.LogWarning($"{outputItem.name} öğesi aktarılamadı, çıkış kapısına yerleştirilemedi.");
                itemInstance.DestroySelf();
            }
        }

        Debug.Log("OutputStorageAction tamamlandı.");
    }
}