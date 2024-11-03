using UnityEngine;
[CreateAssetMenu()]
public class ItemSo : ScriptableObject
{
    public ItemType type;
    public GameObject prefab;
    public bool isSolidItem;
}
public enum ItemType
{
    A, B, C, D
}