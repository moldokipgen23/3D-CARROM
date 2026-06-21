using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

public class CurrencyService : IService
{
    private FirebaseService _firebaseService;
    
    private Dictionary<string, int> _currency = new Dictionary<string, int>();
    
    public event Action<string, int> OnCurrencyChanged;
    
    public void Initialize()
    {
        _firebaseService = ServiceLocator.Get<FirebaseService>();
        InitializeDefaultCurrency();
    }
    
    private void InitializeDefaultCurrency()
    {
        _currency["coins"] = 0;
        _currency["diamonds"] = 0;
        _currency["tokens"] = 0;
        _currency["tickets"] = 0;
        
        Debug.Log("Default currency initialized");
    }
    
    public int GetCurrency(string currencyType)
    {
        if (_currency.ContainsKey(currencyType))
        {
            return _currency[currencyType];
        }
        return 0;
    }
    
    public void AddCurrency(string currencyType, int amount)
    {
        if (amount <= 0) return;
        
        if (!_currency.ContainsKey(currencyType))
        {
            _currency[currencyType] = 0;
        }
        
        _currency[currencyType] += amount;
        
        Debug.Log($"Added {amount} {currencyType}, new total: {_currency[currencyType]}");
        
        OnCurrencyChanged?.Invoke(currencyType, _currency[currencyType]);
        
        _ = SaveCurrencyToFirebase(currencyType);
    }
    
    public bool SpendCurrency(string currencyType, int amount)
    {
        if (amount <= 0) return false;
        
        if (!_currency.ContainsKey(currencyType) || _currency[currencyType] < amount)
        {
            Debug.LogWarning($"Not enough {currencyType} to spend. Have: {_currency.GetValueOrDefault(currencyType, 0)}, Need: {amount}");
            return false;
        }
        
        _currency[currencyType] -= amount;
        
        Debug.Log($"Spent {amount} {currencyType}, new total: {_currency[currencyType]}");
        
        OnCurrencyChanged?.Invoke(currencyType, _currency[currencyType]);
        
        _ = SaveCurrencyToFirebase(currencyType);
        
        return true;
    }
    
    public async Task LoadCurrencyFromFirebase(string playerId)
    {
        try
        {
            Debug.Log($"Loading currency from Firebase for player: {playerId}");
            await Task.Delay(100);
            Debug.Log("Currency loaded from Firebase");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load currency from Firebase: {ex.Message}");
        }
    }
    
    public async Task SaveCurrencyToFirebase(string currencyType)
    {
        try
        {
            Debug.Log($"Saving {currencyType} to Firebase");
            await Task.Delay(100);
            Debug.Log($"Currency {currencyType} saved to Firebase");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save currency to Firebase: {ex.Message}");
        }
    }
}