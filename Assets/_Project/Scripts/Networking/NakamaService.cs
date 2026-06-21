using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

#if NAKAMA_SDK
using Nakama;
#endif

public class NakamaService : IService
{
#if NAKAMA_SDK
    private ISession _session;
    private IClient _client;
    private IMatch _currentMatch;
#else
    private bool _connected;
    private string _userId;
#endif

    public event Action<Vector2, float> OpponentShotReceived;

    public void Initialize()
    {
#if NAKAMA_SDK
        _client = new Client("127.0.0.1", 7350, "", false);
        Debug.Log("Nakama client initialized");
#else
        Debug.Log("[Stub] Nakama client initialized");
#endif
    }

    public async Task ConnectAsync(string firebaseToken)
    {
        try
        {
            Debug.Log("Connecting to Nakama server...");

#if NAKAMA_SDK
            var session = await _client.AuthenticateCustomAsync(firebaseToken);
            _session = session;
            Debug.Log($"Connected to Nakama - User ID: {session.UserId}");
#else
            _userId = Guid.NewGuid().ToString();
            _connected = true;
            Debug.Log($"[Stub] Connected to Nakama - User ID: {_userId}");
            await Task.Delay(100);
#endif
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to connect to Nakama: {ex.Message}");
            throw;
        }
    }

    public async Task<IMatchResult> FindMatchAsync(MatchType type)
    {
        try
        {
            Debug.Log($"Looking for {type} match...");

#if NAKAMA_SDK
            var match = await _client.CreateMatchAsync(_session, type.ToString());
            _currentMatch = match;
            Debug.Log($"Match found - Match ID: {match.MatchId}");
            return match;
#else
            Debug.Log($"[Stub] Looking for {type} match...");
            await Task.Delay(500);
            var stubResult = new StubMatchResult { MatchId = Guid.NewGuid().ToString() };
            Debug.Log($"[Stub] Match found - Match ID: {stubResult.MatchId}");
            return stubResult;
#endif
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to find match: {ex.Message}");
            throw;
        }
    }

    public async Task SendShotAsync(Vector2 direction, float power)
    {
        try
        {
            Debug.Log($"Shot sent - Direction: {direction}, Power: {power:F1}");
            await Task.Delay(50);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to send shot: {ex.Message}");
            throw;
        }
    }

    public void HandleIncomingMatchData(byte[] data, string senderId)
    {
        try
        {
            var payload = ParsePayloadFromBytes(data);

            if (payload.ContainsKey("direction_x") && payload.ContainsKey("direction_y") && payload.ContainsKey("power"))
            {
                float directionX = float.Parse(payload["direction_x"]);
                float directionY = float.Parse(payload["direction_y"]);
                float power = float.Parse(payload["power"]);

                Vector2 direction = new Vector2(directionX, directionY);
                OpponentShotReceived?.Invoke(direction, power);
                Debug.Log($"Received opponent shot - Direction: {direction}, Power: {power:F1}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to handle incoming match data: {ex.Message}");
        }
    }

    private Dictionary<string, string> ParsePayloadFromBytes(byte[] data)
    {
        var payload = new Dictionary<string, string>();
        string text = System.Text.Encoding.UTF8.GetString(data);
        string[] pairs = text.Split('&');

        foreach (string pair in pairs)
        {
            string[] keyValue = pair.Split('=');
            if (keyValue.Length == 2)
            {
                payload[keyValue[0]] = keyValue[1];
            }
        }

        return payload;
    }

    public void Disconnect()
    {
        try
        {
#if NAKAMA_SDK
            if (_session != null)
            {
                _client.LogoutAsync(_session).ContinueWith(t => { });
                _session = null;
            }
#else
            _connected = false;
            _userId = null;
#endif
            Debug.Log("Disconnected from Nakama server");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error during disconnection: {ex.Message}");
        }
    }
}

public interface IMatchResult
{
    string MatchId { get; }
}

public class StubMatchResult : IMatchResult
{
    public string MatchId { get; set; }
}
