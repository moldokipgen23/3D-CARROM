using UnityEngine;
using UnityEngine.UI;
using System;

public class ShopItemUI : MonoBehaviour
{
    [Header("UI References")]
    public Text itemNameText;
    public Text priceText;
    public Text statusText;
    public Image itemIcon;
    public Button actionButton;
    public Image rarityBorder;

    [Header("Colors")]
    public Color ownedColor = Color.green;
    public Color equippedColor = Color.blue;
    public Color affordableColor = Color.white;
    public Color unaffordableColor = Color.gray;

    private string _itemId;
    private ShopItemType _itemType;
    private Action<string> _onBuyCallback;
    private Action<string> _onEquipCallback;
    private Action<string, int, CurrencyType> _onBuyCurrencyCallback;

    public void Setup(CosmeticItem item, bool isOwned, bool isEquipped,
        Action<string> onBuy, Action<string> onEquip)
    {
        _itemId = item.id;
        _itemType = ShopItemType.Board;
        _onBuyCallback = onBuy;
        _onEquipCallback = onEquip;

        if (itemNameText != null) itemNameText.text = item.displayName;
        if (priceText != null) priceText.text = isOwned ? "Owned" : $"{item.price} {item.currencyType}";
        if (itemIcon != null && item.icon != null) itemIcon.sprite = item.icon;

        if (statusText != null)
        {
            statusText.text = isEquipped ? "Equipped" : isOwned ? "Owned" : "";
        }

        if (actionButton != null)
        {
            actionButton.onClick.RemoveAllListeners();

            if (isEquipped)
            {
                actionButton.interactable = false;
                if (statusText != null) statusText.text = "Equipped";
            }
            else if (isOwned)
            {
                actionButton.interactable = true;
                actionButton.onClick.AddListener(() => _onEquipCallback?.Invoke(_itemId));
                if (priceText != null) priceText.text = "Equip";
            }
            else
            {
                actionButton.interactable = true;
                actionButton.onClick.AddListener(() => _onBuyCallback?.Invoke(_itemId));
                if (priceText != null) priceText.text = $"{item.price} {item.currencyType}";
            }
        }

        SetRarityColor(item.rarity);
    }

    public void SetupCurrencyPackage(string packageName, int amount, int price,
        CurrencyType type, Action<string, int, CurrencyType> onBuy)
    {
        _itemId = packageName;
        _itemType = ShopItemType.CurrencyPackage;
        _onBuyCurrencyCallback = onBuy;

        if (itemNameText != null) itemNameText.text = $"{packageName} Pack";
        if (priceText != null) priceText.text = $"{amount} {type}";
        if (statusText != null) statusText.text = $"${(price / 100f):F2}";

        if (actionButton != null)
        {
            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(() => _onBuyCurrencyCallback?.Invoke(_itemId, amount, type));
        }
    }

    private void SetRarityColor(CosmeticRarity rarity)
    {
        if (rarityBorder == null) return;

        switch (rarity)
        {
            case CosmeticRarity.Common:
                rarityBorder.color = Color.white;
                break;
            case CosmeticRarity.Rare:
                rarityBorder.color = Color.blue;
                break;
            case CosmeticRarity.Epic:
                rarityBorder.color = new Color(0.5f, 0f, 0.5f);
                break;
            case CosmeticRarity.Legendary:
                rarityBorder.color = Color.yellow;
                break;
        }
    }
}
