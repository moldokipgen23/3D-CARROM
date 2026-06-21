using UnityEngine;
using System;
using System.Threading.Tasks;

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

    private const string VIP_KEY = "VIP_Subscribed";
    private const string VIP_TIER_KEY = "VIP_Tier";
    private const string VIP_END_DATE_KEY = "VIP_EndDate";

    private void Start()
    {
        LoadSubscriptionData();
    }

    public bool IsSubscribed => _isSubscribed;
    public VIPTier CurrentTier => _currentTier;

    public async Task<bool> Subscribe(VIPTier tier)
    {
        try
        {
            Debug.Log($"Subscribing to VIP tier: {tier.name}");
            await Task.Delay(100);

            _isSubscribed = true;
            _currentTier = tier;
            _subscriptionEndDate = DateTime.UtcNow.AddDays(tier.durationDays);

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

    public async Task<bool> Unsubscribe()
    {
        try
        {
            Debug.Log("Unsubscribing from VIP");
            await Task.Delay(100);

            _isSubscribed = false;
            _currentTier = null;

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
            SaveSubscriptionData();
            OnSubscriptionChanged?.Invoke(false);
            return false;
        }

        return true;
    }

    public int GetAdjustedReward(int baseReward)
    {
        if (!_isSubscribed || _currentTier == null) return baseReward;
        return baseReward * _currentTier.rewardMultiplier;
    }

    public bool ShouldRemoveAds()
    {
        return _isSubscribed && removeAds;
    }

    public async Task ClaimDailyVIPReward(CurrencyService currencyService)
    {
        if (!_isSubscribed || _currentTier == null) return;

        await Task.Delay(100);

        int reward = _currentTier.dailyCoins;
        currencyService?.AddCurrency("coins", reward);

        Debug.Log($"VIP daily reward claimed: {reward} coins");
        OnRewardClaimed?.Invoke();
    }

    public TimeSpan GetTimeRemaining()
    {
        if (!_isSubscribed) return TimeSpan.Zero;
        return _subscriptionEndDate - DateTime.UtcNow;
    }

    private void SaveSubscriptionData()
    {
        PlayerPrefs.SetInt(VIP_KEY, _isSubscribed ? 1 : 0);
        PlayerPrefs.SetString(VIP_TIER_KEY, _currentTier?.id ?? "");
        PlayerPrefs.SetString(VIP_END_DATE_KEY, _subscriptionEndDate.ToBinary().ToString());
        PlayerPrefs.Save();
    }

    private void LoadSubscriptionData()
    {
        _isSubscribed = PlayerPrefs.GetInt(VIP_KEY, 0) == 1;
        string tierId = PlayerPrefs.GetString(VIP_TIER_KEY, "");
        string endDateStr = PlayerPrefs.GetString(VIP_END_DATE_KEY, "");

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
