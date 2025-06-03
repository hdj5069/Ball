// Assets/_Scripts/Player/UpgradeCard.cs 또는 Assets/Scripts/Player/UpgradeCard.cs
using UnityEngine; // Sprite, TextAreaAttribute 때문에 필요
using System.Collections.Generic; // List<> 때문에 필요

public class UpgradeCardData
{
    public string cardName;
    [TextArea] public string description; // 이제 TextAreaAttribute 인식 가능
    public Sprite icon;                   // 이제 Sprite 인식 가능
    public CardRarity rarity;
    public List<UpgradeEffectValue> effects; // 이제 List<> 인식 가능

    public UpgradeCardData(string name, string desc, CardRarity rar, List<UpgradeEffectValue> effs)
    {
        cardName = name;
        description = desc;
        rarity = rar;
        effects = effs;
    }
}

public enum CardRarity { Common, Rare, Epic }

[System.Serializable]
public struct UpgradeEffectValue
{
    public UpgradeEffect effectType;
    public float value;

    public UpgradeEffectValue(UpgradeEffect type, float val)
    {
        effectType = type;
        value = val;
    }
}