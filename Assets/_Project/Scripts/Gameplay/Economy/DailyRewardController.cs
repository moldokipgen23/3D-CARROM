using UnityEngine;
using System.Threading.Tasks;
using System;

public class DailyRewardController : MonoBehaviour
{
    [Header("Daily Reward Settings")]
    public int dailyRewardDays = 7;
    public int[] dailyRewardAmounts = { 100, 200, 300, 400, 500, 600, 700 }; // coins per day
    public float rewardCooldownHours = 24f;
    
    [Header("References")]
    public CurrencyService currencyService;
    
    [Header("Test Mode")]
    public bool testMode = true; // For testing purposes
    
    private DateTime _lastRewardDate;
    private bool _rewardAvailable = false;
    
    private void Start()
    {
        InitializeReferences();
        CheckDailyRewardAvailability();
    }
    
    private void InitializeReferences()
    {
        if (currencyService == null)
        {
            currencyService = ServiceLocator.Get<CurrencyService>();
        }
    }
    
    public void CheckDailyRewardAvailability()
    {
        if (testMode)
        {
            // In test mode, always allow reward
            _rewardAvailable = true;
            Debug.Log("Test mode: Daily reward available");
            return;
        }
        
        DateTime today = DateTime.Today;
        
        if (_lastRewardDate == default)
        {
            _rewardAvailable = true;
            _lastRewardDate = today;
            Debug.Log("First login: Daily reward available");
        }
        else if (today > _lastRewardDate.AddHours(rewardCooldownHours))
        {
            _rewardAvailable = true;
            _lastRewardDate = today;
            Debug.Log("Daily reward available for new day");
        }
        else
        {
            _rewardAvailable = false;
            Debug.Log("Daily reward not yet available");
        }
    }
    
    public async Task ClaimDailyReward()
    {
        if (!_rewardAvailable)
        {
            Debug.LogWarning("Daily reward not available");
            return;
        }
        
        try
        {
            Debug.Log("Claiming daily reward...");
            
            await System.Threading.Tasks.Task.Delay((int)(rewardCooldownHours * 1000));
            
            // Calculate reward based on day of streak
            int dayOfWeek = (int)DateTime.Today.DayOfWeek;
            if (dayOfWeek == 0) dayOfWeek = 7; // Sunday is day 7
            
            int rewardAmount = dailyRewardAmounts[Math.Min(dayOfWeek - 1, dailyRewardDays - 1)];
            
            currencyService.AddCurrency("coins", rewardAmount);
            
            _rewardAvailable = false;
            
            Debug.Log($"Daily reward claimed: {rewardAmount} coins");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to claim daily reward: {ex.Message}");
        }
    }
    
    public int GetCurrentStreak()
    {
        if (testMode)
        {
            return 3; // Test with 3 days streak
        }
        
        DateTime today = DateTime.Today;
        if (_lastRewardDate == default)
        {
            return 0;
        }
        
        int streak = 0;
        DateTime checkDate = today;
        
        while (checkDate >= _lastRewardDate.AddHours(rewardCooldownHours * streak))
        {
            streak++;
            checkDate = checkDate.AddDays(-1);
        }
        
        return streak;
    }
    
    public bool IsRewardAvailable()
    {
        return _rewardAvailable;
    }
}