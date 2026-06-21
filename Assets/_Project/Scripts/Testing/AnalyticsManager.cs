using UnityEngine;

public class AnalyticsManager : MonoBehaviour
{
    [Header("Analytics Settings")]
    public bool enableAnalytics = true;
    public bool debugMode = true;

    public static AnalyticsManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LogEvent(string eventName)
    {
        if (!enableAnalytics) return;

        if (debugMode)
        {
            Debug.Log($"[Analytics] Event: {eventName}");
        }
    }

    public void LogEvent(string eventName, string parameterName, string parameterValue)
    {
        if (!enableAnalytics) return;

        if (debugMode)
        {
            Debug.Log($"[Analytics] Event: {eventName}, {parameterName}: {parameterValue}");
        }
    }

    public void LogEvent(string eventName, string parameterName, float parameterValue)
    {
        if (!enableAnalytics) return;

        if (debugMode)
        {
            Debug.Log($"[Analytics] Event: {eventName}, {parameterName}: {parameterValue}");
        }
    }

    public void LogEvent(string eventName, string parameterName, int parameterValue)
    {
        if (!enableAnalytics) return;

        if (debugMode)
        {
            Debug.Log($"[Analytics] Event: {eventName}, {parameterName}: {parameterValue}");
        }
    }

    public void LogSessionStart()
    {
        LogEvent("session_start");
    }

    public void LogSessionEnd(float duration)
    {
        LogEvent("session_end", "duration_seconds", duration);
    }

    public void LogMatchComplete(string matchType, bool won, float duration)
    {
        LogEvent("match_complete", "match_type", matchType);
        LogEvent("match_result", "won", won ? 1 : 0);
        LogEvent("match_duration", "seconds", duration);
    }

    public void LogPurchase(string itemId, string currency, int amount)
    {
        LogEvent("purchase", "item_id", itemId);
        LogEvent("purchase_detail", "currency", currency);
        LogEvent("purchase_detail", "amount", amount);
    }

    public void LogAdImpression(string adType, bool completed)
    {
        LogEvent("ad_impression", "ad_type", adType);
        LogEvent("ad_completed", "completed", completed ? 1 : 0);
    }

    public void LogLevelUp(int newLevel)
    {
        LogEvent("level_up", "new_level", newLevel);
    }

    public void LogMissionComplete(string missionId, int reward)
    {
        LogEvent("mission_complete", "mission_id", missionId);
        LogEvent("mission_reward", "reward", reward);
    }

    public void LogCustomEvent(string eventName, params object[] args)
    {
        if (!enableAnalytics) return;

        if (debugMode && args.Length > 0)
        {
            string argsStr = string.Join(", ", args);
            Debug.Log($"[Analytics] Custom Event: {eventName} ({argsStr})");
        }
    }
}
