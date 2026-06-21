using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class LeaderboardScreen : MonoBehaviour
{
    [Header("UI References")]
    public GameObject leaderboardPanel;
    public Button closeButton;

    [Header("Tab Buttons")]
    public Button globalTab;
    public Button countryTab;
    public Button friendsTab;
    public Button seasonTab;

    [Header("Content")]
    public Transform entriesParent;
    public GameObject entryPrefab;

    [Header("Player Rank")]
    public Text playerRankText;
    public Text playerScoreText;
    public Text playerTierText;

    [Header("References")]
    public LeaderboardService leaderboardService;
    public FirebaseService firebaseService;

    private LeaderboardTab _currentTab = LeaderboardTab.Global;
    private List<GameObject> _spawnedEntries = new List<GameObject>();

    private void Start()
    {
        InitializeReferences();
        SetupButtons();
    }

    private void InitializeReferences()
    {
        if (leaderboardService == null)
        {
            leaderboardService = ServiceLocator.Get<LeaderboardService>();
        }
    }

    private void SetupButtons()
    {
        if (closeButton != null) closeButton.onClick.AddListener(HideLeaderboard);
        if (globalTab != null) globalTab.onClick.AddListener(() => SwitchTab(LeaderboardTab.Global));
        if (countryTab != null) countryTab.onClick.AddListener(() => SwitchTab(LeaderboardTab.Country));
        if (friendsTab != null) friendsTab.onClick.AddListener(() => SwitchTab(LeaderboardTab.Friends));
        if (seasonTab != null) seasonTab.onClick.AddListener(() => SwitchTab(LeaderboardTab.Season));
    }

    public void ShowLeaderboard()
    {
        if (leaderboardPanel != null)
        {
            leaderboardPanel.SetActive(true);
        }
        SwitchTab(_currentTab);
        UpdatePlayerRank();
        Debug.Log("Leaderboard opened");
    }

    public void HideLeaderboard()
    {
        if (leaderboardPanel != null)
        {
            leaderboardPanel.SetActive(false);
        }
    }

    private async void SwitchTab(LeaderboardTab tab)
    {
        _currentTab = tab;
        ClearEntries();

        List<LeaderboardEntry> entries = new List<LeaderboardEntry>();

        switch (tab)
        {
            case LeaderboardTab.Global:
                entries = await leaderboardService.GetLeaderboard(100);
                break;
            case LeaderboardTab.Country:
                entries = await leaderboardService.GetLeaderboard(50);
                break;
            case LeaderboardTab.Friends:
                entries = await leaderboardService.GetFriendsLeaderboard(new List<string>());
                break;
            case LeaderboardTab.Season:
                entries = await leaderboardService.GetLeaderboard(50);
                break;
        }

        PopulateEntries(entries);
    }

    private void PopulateEntries(List<LeaderboardEntry> entries)
    {
        foreach (LeaderboardEntry entry in entries)
        {
            CreateEntryUI(entry);
        }
    }

    private void CreateEntryUI(LeaderboardEntry entry)
    {
        if (entryPrefab == null || entriesParent == null) return;

        GameObject entryObj = Instantiate(entryPrefab, entriesParent);
        _spawnedEntries.Add(entryObj);

        LeaderboardEntryUI entryUI = entryObj.GetComponent<LeaderboardEntryUI>();
        if (entryUI != null)
        {
            entryUI.Setup(entry);
        }
    }

    private async void UpdatePlayerRank()
    {
        string playerId = firebaseService.IsInitialized ? "current_player" : "test_player";
        LeaderboardEntry playerEntry = await leaderboardService.GetPlayerRank(playerId);

        if (playerEntry != null)
        {
            if (playerRankText != null) playerRankText.text = $"#{playerEntry.rank}";
            if (playerScoreText != null) playerScoreText.text = playerEntry.score.ToString();
            if (playerTierText != null) playerTierText.text = playerEntry.rankTier;
        }
    }

    private void ClearEntries()
    {
        foreach (GameObject entry in _spawnedEntries)
        {
            if (entry != null) Destroy(entry);
        }
        _spawnedEntries.Clear();
    }
}

public enum LeaderboardTab
{
    Global,
    Country,
    Friends,
    Season
}
