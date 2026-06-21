using UnityEngine;
using System;

public class AdMobService : MonoBehaviour, IService
{
    [Header("Ad Settings")]
    public bool testMode = true;
    public float interstitialInterval = 60f;
    public float bannerRefreshRate = 60f;

    [Header("Test Ad Unit IDs")]
    public string testBannerAdUnitId = "ca-app-pub-3940256099942544/6300978111";
    public string testInterstitialAdUnitId = "ca-app-pub-3940256099942544/1033173712";
    public string testRewardedAdUnitId = "ca-app-pub-3940256099942544/5224354917";

    [Header("Production Ad Unit IDs")]
    public string bannerAdUnitId = "";
    public string interstitialAdUnitId = "";
    public string rewardedAdUnitId = "";

    private float _lastInterstitialTime;
    private bool _isInitialized;

    public event Action OnRewardedAdCompleted;
    public event Action OnRewardedAdFailed;
    public event Action OnInterstitialAdShown;
    public event Action OnBannerAdLoaded;

    public void Initialize()
    {
        Debug.Log("AdMob service initialized (stub - requires AdMob SDK)");
        _isInitialized = true;
        _lastInterstitialTime = Time.time;
    }

    public string GetBannerAdUnitId()
    {
        return testMode ? testBannerAdUnitId : bannerAdUnitId;
    }

    public string GetInterstitialAdUnitId()
    {
        return testMode ? testInterstitialAdUnitId : interstitialAdUnitId;
    }

    public string GetRewardedAdUnitId()
    {
        return testMode ? testRewardedAdUnitId : rewardedAdUnitId;
    }

    public bool CanShowInterstitial()
    {
        return _isInitialized && (Time.time - _lastInterstitialTime >= interstitialInterval);
    }

    public void ShowBanner()
    {
        Debug.Log($"Showing banner ad: {GetBannerAdUnitId()}");
        OnBannerAdLoaded?.Invoke();
    }

    public void HideBanner()
    {
        Debug.Log("Hiding banner ad");
    }

    public void ShowInterstitial()
    {
        if (!CanShowInterstitial())
        {
            Debug.Log("Interstitial ad not ready yet");
            return;
        }

        Debug.Log($"Showing interstitial ad: {GetInterstitialAdUnitId()}");
        _lastInterstitialTime = Time.time;
        OnInterstitialAdShown?.Invoke();
    }

    public void ShowRewarded()
    {
        Debug.Log($"Showing rewarded ad: {GetRewardedAdUnitId()}");
    }

    public void OnRewardedCallback()
    {
        Debug.Log("Rewarded ad completed - granting reward");
        OnRewardedAdCompleted?.Invoke();
    }

    public void OnRewardedFailedCallback()
    {
        Debug.Log("Rewarded ad failed");
        OnRewardedAdFailed?.Invoke();
    }

    public void ShowAppOpenAd()
    {
        Debug.Log("Showing app open ad");
    }

    public void ShowRewardedInterstitial()
    {
        Debug.Log("Showing rewarded interstitial ad");
    }

    public bool IsInitialized => _isInitialized;
}
