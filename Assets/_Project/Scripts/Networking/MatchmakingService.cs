using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

public class MatchmakingService : IService
{
    private NakamaService _nakamaService;

    public event Action<IMatchResult> OnMatchFound;

    public void Initialize()
    {
        _nakamaService = ServiceLocator.Get<NakamaService>();
    }

    public async Task FindCasualMatch()
    {
        try
        {
            Debug.Log("Looking for casual match...");
            var match = await _nakamaService.FindMatchAsync(MatchType.Casual);
            OnMatchFound?.Invoke(match);
            Debug.Log("Casual match found");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to find casual match: {ex.Message}");
            throw;
        }
    }

    public async Task FindRankedMatch()
    {
        try
        {
            Debug.Log("Looking for ranked match...");
            await Task.Delay(1000);
            var match = await _nakamaService.FindMatchAsync(MatchType.Ranked);
            OnMatchFound?.Invoke(match);
            Debug.Log("Ranked match found");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to find ranked match: {ex.Message}");
            throw;
        }
    }

    public async Task FindPrivateMatch(string roomCode)
    {
        try
        {
            Debug.Log($"Looking for private match with code: {roomCode}");
            var match = await _nakamaService.FindMatchAsync(MatchType.Private);
            OnMatchFound?.Invoke(match);
            Debug.Log("Private match found");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to find private match: {ex.Message}");
            throw;
        }
    }
}
