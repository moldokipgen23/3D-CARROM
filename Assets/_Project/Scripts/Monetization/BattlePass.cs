using UnityEngine;
using System;
using System.Collections.Generic;

public class BattlePass : MonoBehaviour
{
    [Header("Battle Pass Settings")]
    public int currentSeason = 1;
    public int maxLevel = 100;
    public int xpPerLevel = 1000;
    public float seasonDurationDays = 30f;

    [Header("Tracks")]
    public BattlePassLevel[] freeTrack;
    public BattlePassLevel[] premiumTrack;

    [Header("References")]
    public CurrencyService currencyService;
    public VIPSubscription vipSubscription;

    private int _currentLevel;
    private int _currentXP;
    private bool _isPremiumUnlocked;
    private DateTime _seasonEndDate;

    public event Action<int> OnLevelUp;
    public event Action<int> OnXPGained;
    public event Action<BattlePassReward> OnRewardClaimed;

    private const string BP_LEVEL_KEY = "BP_Level";
    private const string BP_XP_KEY = "BP_XP";
    private const string BP_PREMIUM_KEY = "BP_Premium";
    private const string BP_SEASON_KEY = "BP_Season";

    private void Start()
    {
        InitializeReferences();
        LoadBattlePassData();
        CheckSeasonReset();
    }

    private void InitializeReferences()
    {
        if (currencyService == null)
        {
            currencyService = ServiceLocator.Get<CurrencyService>();
        }
    }

    public int CurrentLevel => _currentLevel;
    public int CurrentXP => _currentXP;
    public bool IsPremiumUnlocked => _isPremiumUnlocked;
    public int XPToNextLevel => xpPerLevel;

    public void AddXP(int amount)
    {
        if (IsSeasonExpired()) return;

        int adjustedAmount = vipSubscription != null && vipSubscription.IsSubscribed ? amount * 2 : amount;
        _currentXP += adjustedAmount;

        OnXPGained?.Invoke(adjustedAmount);

        while (_currentXP >= xpPerLevel && _currentLevel < maxLevel)
        {
            _currentXP -= xpPerLevel;
            _currentLevel++;
            OnLevelUp?.Invoke(_currentLevel);
            Debug.Log($"Battle Pass level up! Level {_currentLevel}");
        }

        SaveBattlePassData();
    }

    public bool UnlockPremium()
    {
        if (_isPremiumUnlocked) return false;

        _isPremiumUnlocked = true;
        SaveBattlePassData();
        Debug.Log("Premium Battle Pass unlocked!");
        return true;
    }

    public BattlePassReward ClaimFreeReward(int level)
    {
        if (level > _currentLevel || freeTrack == null || level > freeTrack.Length)
        {
            Debug.LogWarning("Cannot claim free reward: level not reached or invalid");
            return null;
        }

        BattlePassLevel bpLevel = freeTrack[level - 1];
        if (bpLevel.freeReward.claimed)
        {
            Debug.LogWarning("Free reward already claimed");
            return null;
        }

        bpLevel.freeReward.claimed = true;
        GrantReward(bpLevel.freeReward);
        return bpLevel.freeReward;
    }

    public BattlePassReward ClaimPremiumReward(int level)
    {
        if (!_isPremiumUnlocked)
        {
            Debug.LogWarning("Cannot claim premium reward: premium not unlocked");
            return null;
        }

        if (level > _currentLevel || premiumTrack == null || level > premiumTrack.Length)
        {
            Debug.LogWarning("Cannot claim premium reward: level not reached or invalid");
            return null;
        }

        BattlePassLevel bpLevel = premiumTrack[level - 1];
        if (bpLevel.premiumReward.claimed)
        {
            Debug.LogWarning("Premium reward already claimed");
            return null;
        }

        bpLevel.premiumReward.claimed = true;
        GrantReward(bpLevel.premiumReward);
        return bpLevel.premiumReward;
    }

    private void GrantReward(BattlePassReward reward)
    {
        switch (reward.rewardType)
        {
            case BattlePassRewardType.Coins:
                currencyService?.AddCurrency("coins", reward.amount);
                break;
            case BattlePassRewardType.Diamonds:
                currencyService?.AddCurrency("diamonds", reward.amount);
                break;
            case BattlePassRewardType.Cosmetic:
                Debug.Log($"Granted cosmetic: {reward.rewardId}");
                break;
            case BattlePassRewardType.Title:
                Debug.Log($"Granted title: {reward.rewardId}");
                break;
        }

        OnRewardClaimed?.Invoke(reward);
        Debug.Log($"Battle pass reward claimed: {reward.rewardType} x{reward.amount}");
    }

    public float GetSeasonProgress()
    {
        TimeSpan remaining = GetTimeRemaining();
        TimeSpan total = TimeSpan.FromDays(seasonDurationDays);
        return 1f - (float)(remaining.TotalSeconds / total.TotalSeconds);
    }

    public TimeSpan GetTimeRemaining()
    {
        if (_seasonEndDate == default) return TimeSpan.Zero;
        return _seasonEndDate - DateTime.UtcNow;
    }

    private bool IsSeasonExpired()
    {
        return DateTime.UtcNow >= _seasonEndDate;
    }

    private void CheckSeasonReset()
    {
        if (_seasonEndDate == default)
        {
            _seasonEndDate = DateTime.UtcNow.AddDays(seasonDurationDays);
            SaveBattlePassData();
            return;
        }

        if (IsSeasonExpired())
        {
            currentSeason++;
            _currentLevel = 0;
            _currentXP = 0;
            _isPremiumUnlocked = false;
            _seasonEndDate = DateTime.UtcNow.AddDays(seasonDurationDays);
            SaveBattlePassData();
            Debug.Log($"New season started: {currentSeason}");
        }
    }

    private void SaveBattlePassData()
    {
        PlayerPrefs.SetInt(BP_LEVEL_KEY, _currentLevel);
        PlayerPrefs.SetInt(BP_XP_KEY, _currentXP);
        PlayerPrefs.SetInt(BP_PREMIUM_KEY, _isPremiumUnlocked ? 1 : 0);
        PlayerPrefs.SetInt(BP_SEASON_KEY, currentSeason);
        PlayerPrefs.SetString("BP_SeasonEnd", _seasonEndDate.ToBinary().ToString());
        PlayerPrefs.Save();
    }

    private void LoadBattlePassData()
    {
        _currentLevel = PlayerPrefs.GetInt(BP_LEVEL_KEY, 0);
        _currentXP = PlayerPrefs.GetInt(BP_XP_KEY, 0);
        _isPremiumUnlocked = PlayerPrefs.GetInt(BP_PREMIUM_KEY, 0) == 1;
        currentSeason = PlayerPrefs.GetInt(BP_SEASON_KEY, 1);

        string endDateStr = PlayerPrefs.GetString("BP_SeasonEnd", "");
        if (!string.IsNullOrEmpty(endDateStr))
        {
            _seasonEndDate = DateTime.FromBinary(long.Parse(endDateStr));
        }
        else
        {
            _seasonEndDate = DateTime.UtcNow.AddDays(seasonDurationDays);
        }
    }
}

[Serializable]
public class BattlePassLevel
{
    public int level;
    public BattlePassReward freeReward;
    public BattlePassReward premiumReward;
}

[Serializable]
public class BattlePassReward
{
    public BattlePassRewardType rewardType;
    public string rewardId;
    public int amount;
    public Sprite icon;
    public bool claimed;
}

public enum BattlePassRewardType
{
    Coins,
    Diamonds,
    Cosmetic,
    Title,
    Avatar,
    Emote
}
