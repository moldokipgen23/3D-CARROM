using UnityEngine;
using System;
using System.Collections.Generic;

public class EventScheduler : MonoBehaviour
{
    [Header("Event Settings")]
    public float checkInterval = 60f;
    public GameEvent[] scheduledEvents;

    [Header("References")]
    public CurrencyService currencyService;

    private List<GameEvent> _activeEvents = new List<GameEvent>();
    private float _lastCheckTime;

    public event Action<GameEvent> OnEventStarted;
    public event Action<GameEvent> OnEventEnded;

    private void Start()
    {
        InitializeReferences();
        CheckEvents();
        InvokeRepeating(nameof(CheckEvents), checkInterval, checkInterval);
    }

    private void InitializeReferences()
    {
        if (currencyService == null)
        {
            currencyService = ServiceLocator.Get<CurrencyService>();
        }
    }

    private void CheckEvents()
    {
        if (scheduledEvents == null) return;

        DateTime now = DateTime.UtcNow;

        foreach (GameEvent gameEvent in scheduledEvents)
        {
            bool isActive = now >= gameEvent.startTime && now <= gameEvent.endTime;
            bool wasActive = _activeEvents.Contains(gameEvent);

            if (isActive && !wasActive)
            {
                ActivateEvent(gameEvent);
            }
            else if (!isActive && wasActive)
            {
                DeactivateEvent(gameEvent);
            }
        }
    }

    private void ActivateEvent(GameEvent gameEvent)
    {
        _activeEvents.Add(gameEvent);
        Debug.Log($"Event activated: {gameEvent.displayName}");
        OnEventStarted?.Invoke(gameEvent);
    }

    private void DeactivateEvent(GameEvent gameEvent)
    {
        _activeEvents.Remove(gameEvent);
        Debug.Log($"Event deactivated: {gameEvent.displayName}");
        OnEventEnded?.Invoke(gameEvent);
    }

    public bool IsEventActive(string eventId)
    {
        foreach (GameEvent evt in _activeEvents)
        {
            if (evt.id == eventId) return true;
        }
        return false;
    }

    public List<GameEvent> GetActiveEvents()
    {
        return new List<GameEvent>(_activeEvents);
    }

    public GameEvent GetEventById(string eventId)
    {
        if (scheduledEvents == null) return null;

        foreach (GameEvent evt in scheduledEvents)
        {
            if (evt.id == eventId) return evt;
        }
        return null;
    }
}

[Serializable]
public class GameEvent
{
    public string id;
    public string displayName;
    public string description;
    public EventType eventType;
    public DateTime startTime;
    public DateTime endTime;
    public EventReward[] rewards;
    public Sprite icon;
}

public enum EventType
{
    DoubleXP,
    CoinFrenzy,
    SpecialChallenge,
    Tournament,
    Seasonal
}

[Serializable]
public class EventReward
{
    public int requirement;
    public string rewardId;
    public int rewardAmount;
    public RewardType rewardType;
}

public enum RewardType
{
    Coins,
    Diamonds,
    Cosmetic,
    Title,
    Avatar
}
