using UnityEngine;
using System;
using System.Collections.Generic;

public class CosmeticEquipper : MonoBehaviour
{
    [Header("References")]
    public CosmeticDatabase cosmeticDatabase;
    public MeshRenderer boardRenderer;
    public MeshFilter boardFilter;
    public MeshRenderer strikerRenderer;
    public MeshFilter strikerFilter;

    [Header("Currently Equipped")]
    public string equippedBoardId = "board_classic";
    public string equippedStrikerId = "striker_wood";

    private Dictionary<string, Material> _materialCache = new Dictionary<string, Material>();
    private Dictionary<string, Mesh> _meshCache = new Dictionary<string, Mesh>();

    public event Action<string> OnBoardCosmeticChanged;
    public event Action<string> OnStrikerCosmeticChanged;

    private void Start()
    {
        LoadEquippedCosmetics();
        ApplyBoardCosmetic(equippedBoardId);
        ApplyStrikerCosmetic(equippedStrikerId);
    }

    public void EquipBoard(string cosmeticId)
    {
        CosmeticItem item = cosmeticDatabase.GetItemById(cosmeticId);
        if (item == null)
        {
            Debug.LogWarning($"Board cosmetic not found: {cosmeticId}");
            return;
        }

        equippedBoardId = cosmeticId;
        ApplyBoardCosmetic(cosmeticId);
        SaveEquippedCosmetics();

        Debug.Log($"Board cosmetic equipped: {item.displayName}");
        OnBoardCosmeticChanged?.Invoke(cosmeticId);
    }

    public void EquipStriker(string cosmeticId)
    {
        CosmeticItem item = cosmeticDatabase.GetItemById(cosmeticId);
        if (item == null)
        {
            Debug.LogWarning($"Striker cosmetic not found: {cosmeticId}");
            return;
        }

        equippedStrikerId = cosmeticId;
        ApplyStrikerCosmetic(cosmeticId);
        SaveEquippedCosmetics();

        Debug.Log($"Striker cosmetic equipped: {item.displayName}");
        OnStrikerCosmeticChanged?.Invoke(cosmeticId);
    }

    private void ApplyBoardCosmetic(string cosmeticId)
    {
        CosmeticItem item = cosmeticDatabase.GetItemById(cosmeticId);
        if (item == null) return;

        if (boardRenderer != null && item.material != null)
        {
            boardRenderer.material = item.material;
        }

        if (boardFilter != null && item.mesh != null)
        {
            boardFilter.mesh = item.mesh;
        }
    }

    private void ApplyStrikerCosmetic(string cosmeticId)
    {
        CosmeticItem item = cosmeticDatabase.GetItemById(cosmeticId);
        if (item == null) return;

        if (strikerRenderer != null && item.material != null)
        {
            strikerRenderer.material = item.material;
        }

        if (strikerFilter != null && item.mesh != null)
        {
            strikerFilter.mesh = item.mesh;
        }
    }

    public CosmeticItem GetEquippedBoard()
    {
        return cosmeticDatabase.GetItemById(equippedBoardId);
    }

    public CosmeticItem GetEquippedStriker()
    {
        return cosmeticDatabase.GetItemById(equippedStrikerId);
    }

    private void SaveEquippedCosmetics()
    {
        PlayerPrefs.SetString("EquippedBoard", equippedBoardId);
        PlayerPrefs.SetString("EquippedStriker", equippedStrikerId);
        PlayerPrefs.Save();
    }

    private void LoadEquippedCosmetics()
    {
        equippedBoardId = PlayerPrefs.GetString("EquippedBoard", "board_classic");
        equippedStrikerId = PlayerPrefs.GetString("EquippedStriker", "striker_wood");
    }
}
