using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

public class MissionSystem : MonoBehaviour
{
    [Header("Mission Settings")]
    public float missionUpdateInterval = 60f;
    public int dailyMissionCount = 3;
    public int weeklyMissionCount = 3;
    
    [Header("References")]
    public CurrencyService currencyService;
    public ScoreManager scoreManager;
    
    private List<Mission> _dailyMissions = new List<Mission>();
    private List<Mission> _weeklyMissions = new List<Mission>();
    private List<Mission> _activeMissions = new List<Mission>();
    
    private void Start()
    {
        InitializeReferences();
        GenerateMissions();
        StartCoroutine(MissionUpdateCoroutine());
    }
    
    private void InitializeReferences()
    {
        if (currencyService == null)
        {
            currencyService = ServiceLocator.Get<CurrencyService>();
        }
        if (scoreManager == null)
        {
            scoreManager = FindObjectOfType<ScoreManager>();
        }
    }
    
    private void GenerateMissions()
    {
        _dailyMissions.Clear();
        _weeklyMissions.Clear();
        
        string[] dailyMissionTypes = { "play_games", "win_matches", "pocket_coins" };
        foreach (string missionType in dailyMissionTypes)
        {
            Mission mission = CreateMission(missionType, MissionType.Daily);
            _dailyMissions.Add(mission);
        }
        
        string[] weeklyMissionTypes = { "play_10_games", "win_5_matches", "complete_daily_missions" };
        foreach (string missionType in weeklyMissionTypes)
        {
            Mission mission = CreateMission(missionType, MissionType.Weekly);
            _weeklyMissions.Add(mission);
        }
        
        RefreshActiveMissions();
    }
    
    private Mission CreateMission(string missionTypeStr, MissionType missionType)
    {
        Mission mission = new Mission
        {
            id = Guid.NewGuid().ToString(),
            type = missionTypeStr,
            missionType = missionType,
            description = GetMissionDescription(missionTypeStr),
            target = GetMissionTarget(missionTypeStr),
            reward = GetMissionReward(missionTypeStr),
            completed = false,
            progress = 0
        };
        
        return mission;
    }
    
    private string GetMissionDescription(string missionType)
    {
        switch (missionType)
        {
            case "play_games": return "Play 5 games";
            case "win_matches": return "Win 3 matches";
            case "pocket_coins": return "Pocket 10 coins";
            case "play_10_games": return "Play 10 games";
            case "win_5_matches": return "Win 5 matches";
            case "complete_daily_missions": return "Complete all daily missions";
            default: return "Complete mission";
        }
    }
    
    private int GetMissionTarget(string missionType)
    {
        switch (missionType)
        {
            case "play_games": return 5;
            case "win_matches": return 3;
            case "pocket_coins": return 10;
            case "play_10_games": return 10;
            case "win_5_matches": return 5;
            case "complete_daily_missions": return dailyMissionCount;
            default: return 1;
        }
    }
    
    private int GetMissionReward(string missionType)
    {
        switch (missionType)
        {
            case "play_games": return 50;
            case "win_matches": return 100;
            case "pocket_coins": return 25;
            case "play_10_games": return 200;
            case "win_5_matches": return 500;
            case "complete_daily_missions": return 150;
            default: return 50;
        }
    }
    
    private void RefreshActiveMissions()
    {
        _activeMissions.Clear();
        _activeMissions.AddRange(_dailyMissions);
        
        if (DateTime.Today.DayOfWeek == DayOfWeek.Monday)
        {
            _activeMissions.AddRange(_weeklyMissions);
        }
    }
    
    private System.Collections.IEnumerator MissionUpdateCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(missionUpdateInterval);
            UpdateAllMissionProgress();
        }
    }
    
    private void UpdateAllMissionProgress()
    {
        for (int i = 0; i < _activeMissions.Count; i++)
        {
            Mission mission = _activeMissions[i];
            if (mission.completed) continue;
            
            mission.progress = GetMissionCurrentProgress(mission);
            _activeMissions[i] = mission;
            
            if (mission.progress >= mission.target)
            {
                CompleteMission(mission);
            }
        }
    }
    
    private int GetMissionCurrentProgress(Mission mission)
    {
        switch (mission.type)
        {
            case "play_games":
            case "play_10_games":
                TurnManager tm = FindObjectOfType<TurnManager>();
                return tm != null ? tm.GetTurnCount() : 0;
            case "win_matches":
            case "win_5_matches":
                return scoreManager != null ? scoreManager.GetPlayerScore(1) + scoreManager.GetPlayerScore(2) : 0;
            case "pocket_coins":
                return GetTotalCoinsPocketed();
            case "complete_daily_missions":
                return GetCompletedDailyMissions();
            default:
                return 0;
        }
    }
    
    private void CompleteMission(Mission mission)
    {
        mission.completed = true;
        currencyService?.AddCurrency("coins", mission.reward);
        Debug.Log($"Mission completed: {mission.description}, reward: {mission.reward} coins");
    }
    
    private int GetTotalCoinsPocketed()
    {
        return 0;
    }
    
    private int GetCompletedDailyMissions()
    {
        int completedCount = 0;
        foreach (Mission mission in _dailyMissions)
        {
            if (mission.completed) completedCount++;
        }
        return completedCount;
    }
    
    public List<Mission> GetActiveMissions()
    {
        return new List<Mission>(_activeMissions);
    }
    
    public List<Mission> GetCompletedMissions()
    {
        List<Mission> completed = new List<Mission>();
        foreach (Mission mission in _activeMissions)
        {
            if (mission.completed) completed.Add(mission);
        }
        return completed;
    }
}