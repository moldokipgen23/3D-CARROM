using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

public class ChatService : MonoBehaviour
{
    [Header("Chat Settings")]
    public int maxMessageHistory = 100;
    public float messageCooldown = 1f;

    private Dictionary<string, List<ChatMessage>> _channels = new Dictionary<string, List<ChatMessage>>();
    private float _lastMessageTime;

    public event Action<ChatMessage> OnMessageReceived;
    public event Action<string> OnChannelJoined;
    public event Action<string> OnChannelLeft;

    public void Initialize()
    {
        Debug.Log("Chat service initialized");
    }

    public async Task JoinChannel(ChatChannelType channelType, string channelId = "")
    {
        try
        {
            string channelName = GetChannelName(channelType, channelId);
            Debug.Log($"Joining chat channel: {channelName}");

            await Task.Delay(100);

            if (!_channels.ContainsKey(channelName))
            {
                _channels[channelName] = new List<ChatMessage>();
            }

            OnChannelJoined?.Invoke(channelName);
            Debug.Log($"Joined channel: {channelName}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to join channel: {ex.Message}");
        }
    }

    public async Task LeaveChannel(string channelId)
    {
        try
        {
            Debug.Log($"Leaving channel: {channelId}");
            await Task.Delay(100);

            _channels.Remove(channelId);
            OnChannelLeft?.Invoke(channelId);
            Debug.Log($"Left channel: {channelId}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to leave channel: {ex.Message}");
        }
    }

    public async Task SendMessage(string channelId, string message)
    {
        try
        {
            if (Time.time - _lastMessageTime < messageCooldown)
            {
                Debug.LogWarning("Message cooldown active");
                return;
            }

            Debug.Log($"Sending message to {channelId}: {message}");
            await Task.Delay(100);

            ChatMessage chatMessage = new ChatMessage
            {
                senderId = "local",
                senderName = "Player",
                content = message,
                timestamp = DateTime.UtcNow,
                channel = channelId
            };

            if (_channels.ContainsKey(channelId))
            {
                _channels[channelId].Add(chatMessage);
                if (_channels[channelId].Count > maxMessageHistory)
                {
                    _channels[channelId].RemoveAt(0);
                }
            }

            OnMessageReceived?.Invoke(chatMessage);
            _lastMessageTime = Time.time;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to send message: {ex.Message}");
        }
    }

    public List<ChatMessage> GetMessageHistory(string channelId)
    {
        if (_channels.ContainsKey(channelId))
        {
            return new List<ChatMessage>(_channels[channelId]);
        }
        return new List<ChatMessage>();
    }

    public List<string> GetJoinedChannels()
    {
        return new List<string>(_channels.Keys);
    }

    private string GetChannelName(ChatChannelType channelType, string channelId)
    {
        switch (channelType)
        {
            case ChatChannelType.Global:
                return "global";
            case ChatChannelType.Friends:
                return "friends";
            case ChatChannelType.Clan:
                return $"clan_{channelId}";
            case ChatChannelType.Match:
                return $"match_{channelId}";
            default:
                return "global";
        }
    }
}

[Serializable]
public class ChatMessage
{
    public string senderId;
    public string senderName;
    public string content;
    public DateTime timestamp;
    public string channel;
}

public enum ChatChannelType
{
    Global,
    Friends,
    Clan,
    Match
}
