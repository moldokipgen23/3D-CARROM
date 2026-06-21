using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

public class EmoteSystem : MonoBehaviour
{
    [Header("Emote Settings")]
    public float emoteCooldown = 5f;
    public float emoteDisplayDuration = 3f;
    public int maxEmotesPerMatch = 10;

    [Header("Emote Definitions")]
    public EmoteData[] availableEmotes;

    private int _emotesUsedThisMatch;
    private float _lastEmoteTime;
    private Dictionary<string, EmoteData> _emoteLookup = new Dictionary<string, EmoteData>();

    public event Action<EmoteData> OnEmoteTriggered;
    public event Action<EmoteData> OnEmoteReceived;
    public event Action OnEmoteLimitReached;

    private void Start()
    {
        BuildEmoteLookup();
    }

    private void BuildEmoteLookup()
    {
        _emoteLookup.Clear();
        if (availableEmotes == null) return;

        foreach (EmoteData emote in availableEmotes)
        {
            if (!string.IsNullOrEmpty(emote.id))
            {
                _emoteLookup[emote.id] = emote;
            }
        }
    }

    public bool TriggerEmote(string emoteId)
    {
        if (_emotesUsedThisMatch >= maxEmotesPerMatch)
        {
            Debug.LogWarning("Emote limit reached for this match");
            OnEmoteLimitReached?.Invoke();
            return false;
        }

        if (Time.time - _lastEmoteTime < emoteCooldown)
        {
            Debug.LogWarning("Emote cooldown active");
            return false;
        }

        if (!_emoteLookup.TryGetValue(emoteId, out EmoteData emote))
        {
            Debug.LogWarning($"Emote not found: {emoteId}");
            return false;
        }

        _emotesUsedThisMatch++;
        _lastEmoteTime = Time.time;

        OnEmoteTriggered?.Invoke(emote);
        Debug.Log($"Emote triggered: {emote.displayName}");

        _ = SendEmoteToOpponent(emoteId);

        return true;
    }

    public void ReceiveEmote(string emoteId)
    {
        if (!_emoteLookup.TryGetValue(emoteId, out EmoteData emote))
        {
            Debug.LogWarning($"Received unknown emote: {emoteId}");
            return;
        }

        OnEmoteReceived?.Invoke(emote);
        Debug.Log($"Received emote: {emote.displayName}");
    }

    private async Task SendEmoteToOpponent(string emoteId)
    {
        try
        {
            await Task.Delay(50);
            Debug.Log($"Emote sent to opponent: {emoteId}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to send emote: {ex.Message}");
        }
    }

    public EmoteData GetEmoteById(string id)
    {
        _emoteLookup.TryGetValue(id, out EmoteData emote);
        return emote;
    }

    public EmoteData[] GetAvailableEmotes()
    {
        return availableEmotes;
    }

    public void ResetMatchEmotes()
    {
        _emotesUsedThisMatch = 0;
        _lastEmoteTime = 0f;
    }

    public int GetRemainingEmotes()
    {
        return maxEmotesPerMatch - _emotesUsedThisMatch;
    }
}

[Serializable]
public class EmoteData
{
    public string id;
    public string displayName;
    public Sprite icon;
    public AnimationClip animation;
    public AudioClip sound;
    public float duration;
    public EmoteRarity rarity;
}

public enum EmoteRarity
{
    Common,
    Rare,
    Epic,
    Legendary
}
