using UnityEngine;
using System;

[CreateAssetMenu(fileName = "CosmeticDatabase", menuName = "Game/CosmeticDatabase")]
public class CosmeticDatabase : ScriptableObject
{
    public CosmeticItem[] boards;
    public CosmeticItem[] strikers;

    public CosmeticItem GetItemById(string id)
    {
        foreach (var item in boards)
        {
            if (item.id == id) return item;
        }
        foreach (var item in strikers)
        {
            if (item.id == id) return item;
        }
        return null;
    }

    public CosmeticItem[] GetBoardSkins() => boards;
    public CosmeticItem[] GetStrikerSkins() => strikers;
}

[Serializable]
public class CosmeticItem
{
    public string id;
    public string displayName;
    public int price;
    public CurrencyType currencyType;
    public CosmeticRarity rarity;
    public Sprite icon;
    public Material material;
    public Mesh mesh;
    public UnlockRequirement unlockRequirement;
    public int unlockLevel;
}

public enum CurrencyType
{
    Coins,
    Diamonds,
    Tokens,
    Tickets
}

public enum CosmeticRarity
{
    Common,
    Rare,
    Epic,
    Legendary
}

public enum UnlockRequirement
{
    Purchase,
    Level,
    Event,
    Achievement
}
