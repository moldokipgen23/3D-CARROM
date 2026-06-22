using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

public class VIPSubscription : MonoBehaviour, IService
{
    [Header("VIP Settings")]
    public float monthlyPrice = 9.99f;
    public float yearlyPrice = 79.99f;
    public int dailyCoins = 500;
    public int rewardMultiplier = 2;
    public bool removeAds = true;

    [Header("VIP Tiers")]
    public VIPTier[] tiers;

    private bool _isSubscribed;
    private VIPTier _currentTier;
    private DateTime _subscriptionEndDate;

    public event Action<bool> OnSubscriptionChanged;
    public event Action OnRewardClaimed;

    // Server-side validation flag (set by server response)
    private bool _serverValidated;

    private const string VIP_KEY = "VIP_Subscribed";
    private const string VIP_TIER_KEY = "VIP_Tier";
    private const string VIP_END_DATE_KEY = "VIP_EndDate";

    private void Start()
    {
        LoadSubscriptionData();
    }

    public bool IsSubscribed => _isSubscribed && (_serverValidated || Application.isEditor);
    public VIPTier CurrentTier => _currentTier;

    public async Task<bool> Subscribe(VIPTier tier)
    {
        try
        {
            Debug.Log($"Subscribing to VIP tier: {tier.name}");
            
            // In production, this should call server-side purchase validation
            #if UNITY_EDITOR
            await Task.Delay(100);
            _isSubscribed = true;
            _currentTier = tier;
            _subscriptionEndDate = DateTime.UtcNow.AddDays(tier.durationDays);
            _serverValidated = true;
            #else
            // TODO: Call server-side purchase validation here
            // For now, simulate delay and mark as not validated until server confirms
            await Task.Delay(100);
            _isSubscribed = true;
            _currentTier = tier;
            _subscriptionEndDate = DateTime.UtcNow.AddDays(tier.durationDays);
            _serverValidated = false; // Will be set to true when server validates
            #endif

            SaveSubscriptionData();
            OnSubscriptionChanged?.Invoke(true);

            Debug.Log($"VIP subscription active until: {_subscriptionEndDate}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to subscribe to VIP: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Called by server to validate subscription after purchase verification
    /// </summary>
    public void ValidateSubscriptionFromServer(bool isValid, VIPTier tier, DateTime endDate)
    {
        _serverValidated = isValid;
        if (isValid)
        {
            _currentTier = tier;
            _subscriptionEndDate = endDate;
            _isSubscribed = true;
            SaveSubscriptionData();
            OnSubscriptionChanged?.Invoke(true);
            Debug.Log("VIP subscription validated by server");
        }
        else
        {
            _isSubscribed = false;
            _currentTier = null;
            SaveSubscriptionData();
            OnSubscriptionChanged?.Invoke(false);
            Debug.LogWarning("VIP subscription invalid per server");
        }
    }

    public async Task<bool> Unsubscribe()
    {
        try
        {
            Debug.Log("Unsubscribing from VIP");
            await Task.Delay(100);

            _isSubscribed = false;
            _currentTier = null;
            _serverValidated = false;

            SaveSubscriptionData();
            OnSubscriptionChanged?.Invoke(false);

            Debug.Log("VIP subscription cancelled");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to unsubscribe from VIP: {ex.Message}");
            return false;
        }
    }

    public bool CheckSubscriptionStatus()
    {
        if (!_isSubscribed) return false;

        if (DateTime.UtcNow >= _subscriptionEndDate)
        {
            _isSubscribed = false;
            _currentTier = null;
            _serverValidated = false;
            SaveSubscriptionData();
            OnSubscriptionChanged?.Invoke(false);
            return false;
        }

        return _serverValidated || Application.isEditor;
    }

    public int GetAdjustedReward(int baseReward)
    {
        if (!IsSubscribed || _currentTier == null) return baseReward;
        return baseReward * _currentTier.rewardMultiplier;
    }

    public bool ShouldRemoveAds()
    {
        return IsSubscribed && removeAds;
    }

    public async Task ClaimDailyVIPReward(CurrencyService currencyService)
    {
        if (!IsSubscribed || _currentTier == null) return;

        await Task.Delay(100);

        int reward = _currentTier.dailyCoins;
        currencyService?.AddCurrency("coins", reward);

        Debug.Log($"VIP daily reward claimed: {reward} coins");
        OnRewardClaimed?.Invoke();
    }

    public TimeSpan GetTimeRemaining()
    {
        if (!IsSubscribed) return TimeSpan.Zero;
        return _subscriptionEndDate - DateTime.UtcNow;
    }

    private void SaveSubscriptionData()
    {
        PlayerPrefs.SetInt(VIP_KEY, _isSubscribed ? 1 : 0);
        PlayerPrefs.SetString(VIP_TIER_KEY, _currentTier?.id ?? "");
        PlayerPrefs.SetString(VIP_END_DATE_KEY, _subscriptionEndDate.ToBinary().ToString());
        PlayerPrefs.SetInt("VIP_ServerValidated", _serverValidated ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void LoadSubscriptionData()
    {
        _isSubscribed = PlayerPrefs.GetInt(VIP_KEY, 0) == 1;
        string tierId = PlayerPrefs.GetString(VIP_TIER_KEY, "");
        string endDateStr = PlayerPrefs.GetString(VIP_END_DATE_KEY, "");
        _serverValidated = PlayerPrefs.GetInt("VIP_ServerValidated", 0) == 1;

        if (!string.IsNullOrEmpty(tierId) && tiers != null)
        {
            foreach (VIPTier tier in tiers)
            {
                if (tier.id == tierId)
                {
                    _currentTier = tier;
                    break;
                }
            }
        }

        if (!string.IsNullOrEmpty(endDateStr))
        {
            _subscriptionEndDate = DateTime.FromBinary(long.Parse(endDateStr));
            CheckSubscriptionStatus();
        }
    }
}

[Serializable]
public class VIPTier
{
    public string id;
    public string name;
    public float price;
    public int durationDays;
    public int dailyCoins;
    public int rewardMultiplier;
    public Sprite icon;
    public string[] benefits;
}
