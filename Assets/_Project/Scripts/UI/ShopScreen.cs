using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class ShopScreen : MonoBehaviour
{
    [Header("UI References")]
    public GameObject shopPanel;
    public Text titleText;
    public Button closeButton;

    [Header("Tab Buttons")]
    public Button coinsTab;
    public Button diamondsTab;
    public Button boardsTab;
    public Button strikersTab;
    public Button bundlesTab;
    public Button vipTab;

    [Header("Content Area")]
    public Transform contentParent;
    public GameObject shopItemPrefab;

    [Header("Currency Display")]
    public Text coinsText;
    public Text diamondsText;

    [Header("References")]
    public CosmeticDatabase cosmeticDatabase;
    public CurrencyService currencyService;
    public CosmeticEquipper cosmeticEquipper;

    private ShopCategory _currentCategory = ShopCategory.Boards;
    private List<GameObject> _spawnedItems = new List<GameObject>();

    public event Action<string, CurrencyType> OnItemPurchased;
    public event Action<string> OnItemEquipped;

    private void Start()
    {
        InitializeReferences();
        SetupButtons();
        RefreshCurrencyDisplay();
    }

    private void InitializeReferences()
    {
        if (currencyService == null)
        {
            currencyService = ServiceLocator.Get<CurrencyService>();
        }
        if (cosmeticEquipper == null)
        {
            cosmeticEquipper = FindObjectOfType<CosmeticEquipper>();
        }
    }

    private void SetupButtons()
    {
        if (closeButton != null) closeButton.onClick.AddListener(HideShop);
        if (coinsTab != null) coinsTab.onClick.AddListener(() => SwitchCategory(ShopCategory.Coins));
        if (diamondsTab != null) diamondsTab.onClick.AddListener(() => SwitchCategory(ShopCategory.Diamonds));
        if (boardsTab != null) boardsTab.onClick.AddListener(() => SwitchCategory(ShopCategory.Boards));
        if (strikersTab != null) strikersTab.onClick.AddListener(() => SwitchCategory(ShopCategory.Strikers));
        if (bundlesTab != null) bundlesTab.onClick.AddListener(() => SwitchCategory(ShopCategory.Bundles));
        if (vipTab != null) vipTab.onClick.AddListener(() => SwitchCategory(ShopCategory.VIP));
    }

    public void ShowShop()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(true);
        }
        RefreshCurrencyDisplay();
        SwitchCategory(_currentCategory);
        Debug.Log("Shop opened");
    }

    public void HideShop()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }
        Debug.Log("Shop closed");
    }

    private void SwitchCategory(ShopCategory category)
    {
        _currentCategory = category;
        ClearItems();

        switch (category)
        {
            case ShopCategory.Boards:
                PopulateBoardItems();
                break;
            case ShopCategory.Strikers:
                PopulateStrikerItems();
                break;
            case ShopCategory.Coins:
                PopulateCurrencyPackages(CurrencyType.Coins);
                break;
            case ShopCategory.Diamonds:
                PopulateCurrencyPackages(CurrencyType.Diamonds);
                break;
            case ShopCategory.Bundles:
                PopulateBundles();
                break;
            case ShopCategory.VIP:
                PopulateVIP();
                break;
        }
    }

    private void PopulateBoardItems()
    {
        if (cosmeticDatabase == null) return;

        CosmeticItem[] boards = cosmeticDatabase.GetBoardSkins();
        foreach (CosmeticItem item in boards)
        {
            CreateShopItem(item, ShopItemType.Board);
        }
    }

    private void PopulateStrikerItems()
    {
        if (cosmeticDatabase == null) return;

        CosmeticItem[] strikers = cosmeticDatabase.GetStrikerSkins();
        foreach (CosmeticItem item in strikers)
        {
            CreateShopItem(item, ShopItemType.Striker);
        }
    }

    private void PopulateCurrencyPackages(CurrencyType type)
    {
        string[] packageNames = { "Small", "Medium", "Large", "Mega" };
        int[] amounts = { 100, 500, 1200, 3000 };
        int[] prices = { 99, 399, 799, 1499 };

        for (int i = 0; i < packageNames.Length; i++)
        {
            CreateCurrencyPackageItem(packageNames[i], amounts[i], prices[i], type);
        }
    }

    private void PopulateBundles()
    {
        Debug.Log("Bundles category - coming soon");
    }

    private void PopulateVIP()
    {
        Debug.Log("VIP category - coming soon");
    }

    private void CreateShopItem(CosmeticItem item, ShopItemType type)
    {
        if (shopItemPrefab == null || contentParent == null) return;

        GameObject itemObj = Instantiate(shopItemPrefab, contentParent);
        _spawnedItems.Add(itemObj);

        ShopItemUI itemUI = itemObj.GetComponent<ShopItemUI>();
        if (itemUI != null)
        {
            bool isOwned = IsItemOwned(item.id);
            bool isEquipped = IsItemEquipped(item.id);
            itemUI.Setup(item, isOwned, isEquipped, OnBuyClicked, OnEquipClicked);
        }
    }

    private void CreateCurrencyPackageItem(string packageName, int amount, int price, CurrencyType type)
    {
        if (shopItemPrefab == null || contentParent == null) return;

        GameObject itemObj = Instantiate(shopItemPrefab, contentParent);
        _spawnedItems.Add(itemObj);

        ShopItemUI itemUI = itemObj.GetComponent<ShopItemUI>();
        if (itemUI != null)
        {
            itemUI.SetupCurrencyPackage(packageName, amount, price, type, OnBuyCurrencyClicked);
        }
    }

    private void OnBuyClicked(string itemId)
    {
        CosmeticItem item = cosmeticDatabase.GetItemById(itemId);
        if (item == null) return;

        string currency = item.currencyType == CurrencyType.Coins ? "coins" : "diamonds";
        if (currencyService.SpendCurrency(currency, item.price))
        {
            MarkItemOwned(itemId);
            OnItemPurchased?.Invoke(itemId, item.currencyType);
            Debug.Log($"Purchased: {item.displayName}");
            RefreshCurrencyDisplay();
            SwitchCategory(_currentCategory);
        }
        else
        {
            Debug.LogWarning($"Not enough currency to buy: {item.displayName}");
        }
    }

    private void OnEquipClicked(string itemId)
    {
        if (cosmeticEquipper == null) return;

        CosmeticItem item = cosmeticDatabase.GetItemById(itemId);
        if (item == null) return;

        if (itemId.StartsWith("board_"))
        {
            cosmeticEquipper.EquipBoard(itemId);
        }
        else if (itemId.StartsWith("striker_"))
        {
            cosmeticEquipper.EquipStriker(itemId);
        }

        OnItemEquipped?.Invoke(itemId);
        SwitchCategory(_currentCategory);
    }

    private void OnBuyCurrencyClicked(string packageName, int amount, CurrencyType type)
    {
        Debug.Log($"IAP purchase initiated for: {packageName} ({amount} {type})");
    }

    private bool IsItemOwned(string itemId)
    {
        return PlayerPrefs.GetInt($"Owned_{itemId}", 0) == 1;
    }

    private bool IsItemEquipped(string itemId)
    {
        if (cosmeticEquipper == null) return false;
        return cosmeticEquipper.equippedBoardId == itemId || cosmeticEquipper.equippedStrikerId == itemId;
    }

    private void MarkItemOwned(string itemId)
    {
        PlayerPrefs.SetInt($"Owned_{itemId}", 1);
        PlayerPrefs.Save();
    }

    private void RefreshCurrencyDisplay()
    {
        if (currencyService == null) return;

        if (coinsText != null)
        {
            coinsText.text = currencyService.GetCurrency("coins").ToString("N0");
        }
        if (diamondsText != null)
        {
            diamondsText.text = currencyService.GetCurrency("diamonds").ToString("N0");
        }
    }

    private void ClearItems()
    {
        foreach (GameObject item in _spawnedItems)
        {
            if (item != null) Destroy(item);
        }
        _spawnedItems.Clear();
    }
}

public enum ShopCategory
{
    Coins,
    Diamonds,
    Boards,
    Strikers,
    Bundles,
    VIP
}

public enum ShopItemType
{
    Board,
    Striker,
    CurrencyPackage,
    Bundle,
    VIP
}
