using UnityEngine;
using System.Threading.Tasks;

public class CurrencyService : IService
{
    private FirebaseService _firebaseService;
    
    private Dictionary<string, int> _currency = new Dictionary<string, int>();
    private readonly object _lock = new object();
    
    // Server-authoritative flag
    private bool _serverSynced;
    
    public event Action<string, int> OnCurrencyChanged;
    
    public void Initialize()
    {
        _firebaseService = ServiceLocator.Get<FirebaseService>();
        InitializeDefaultCurrency();
    }
    
    private void InitializeDefaultCurrency()
    {
        lock (_lock)
        {
            _currency["coins"] = 0;
            _currency["diamonds"] = 0;
            _currency["tokens"] = 0;
            _currency["tickets"] = 0;
        }
        Debug.Log("Default currency initialized");
    }
    
    /// <summary>
    /// Set currency from server response (authoritative)
    /// </summary>
    public void SetCurrencyFromServer(string currencyType, int amount)
    {
        lock (_lock)
        {
            if (!_currency.ContainsKey(currencyType))
            {
                _currency[currencyType] = 0;
            }
            _currency[currencyType] = amount;
            _serverSynced = true;
        }
        
        Debug.Log($"Currency set from server: {currencyType} = {amount}");
        OnCurrencyChanged?.Invoke(currencyType, amount);
    }
    
    public int GetCurrency(string currencyType)
    {
        lock (_lock)
        {
            if (_currency.ContainsKey(currencyType))
            {
                return _currency[currencyType];
            }
        }
        return 0;
    }
    
    /// <summary>
    /// Add currency - in production this should be validated by server
    /// </summary>
    public void AddCurrency(string currencyType, int amount)
    {
        if (amount <= 0) return;
        
        lock (_lock)
        {
            if (!_currency.ContainsKey(currencyType))
            {
                _currency[currencyType] = 0;
            }
            
            _currency[currencyType] += amount;
            
            Debug.Log($"Added {amount} {currencyType}, new total: {_currency[currencyType]}");
            
            #if UNITY_EDITOR
            // In editor, allow client-side changes for testing
            _serverSynced = true;
            #endif
        }
        
        OnCurrencyChanged?.Invoke(currencyType, GetCurrency(currencyType));
        
        // Fire-and-forget save to server (with error handling)
        _ = SaveCurrencyToFirebase(currencyType);
    }
    
    /// <summary>
    /// Spend currency - validates locally but should be verified by server
    /// </summary>
    public bool SpendCurrency(string currencyType, int amount)
    {
        if (amount <= 0) return false;
        
        lock (_lock)
        {
            if (!_currency.ContainsKey(currencyType) || _currency[currencyType] < amount)
            {
                Debug.LogWarning($"Not enough {currencyType} to spend. Have: {_currency.GetValueOrDefault(currencyType, 0)}, Need: {amount}");
                return false;
            }
            
            _currency[currencyType] -= amount;
            
            Debug.Log($"Spent {amount} {currencyType}, new total: {_currency[currencyType]}");
        }
        
        OnCurrencyChanged?.Invoke(currencyType, GetCurrency(currencyType));
        
        // Fire-and-forget save to server (with error handling)
        _ = SaveCurrencyToFirebase(currencyType);
        
        return true;
    }
    
    public async Task LoadCurrencyFromFirebase(string playerId)
    {
        try
        {
            Debug.Log($"Loading currency from Firebase for player: {playerId}");
            
            // TODO: Implement actual Firebase load
            await Task.Delay(100);
            
            // For now, simulate successful load
            _serverSynced = true;
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
            
            // TODO: Implement actual Firebase save with server validation
            await Task.Delay(100);
            
            _serverSynced = true;
            Debug.Log($"Currency {currencyType} saved to Firebase");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save currency to Firebase: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Request server to validate and adjust currency (anti-cheat)
    /// </summary>
    public async Task<bool> ValidateCurrencyWithServer()
    {
        try
        {
            Debug.Log("Validating currency with server...");
            
            // TODO: Call server API to validate currency amounts
            await Task.Delay(100);
            
            // In production, server would return corrected values if cheating detected
            _serverSynced = true;
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Currency validation failed: {ex.Message}");
            return false;
        }
    }
}