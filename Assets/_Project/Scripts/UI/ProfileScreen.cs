using UnityEngine;
using UnityEngine.UI;

public class ProfileScreen : MonoBehaviour
{
    [Header("UI References")]
    public GameObject profilePanel;
    public Text usernameText;
    public Text levelText;
    public Text xpText;
    public Image avatarImage;
    
    [Header("Stats References")]
    public Text matchesPlayedText;
    public Text winsText;
    public Text lossesText;
    public Text drawsText;
    public Text bestStreakText;
    public Text currentStreakText;
    public Text rankText;
    
    [Header("Buttons")]
    public Button closeButton;
    
    [Header("References")]
    public FirebaseService firebaseService;
    
    private void Start()
    {
        InitializeReferences();
        SetupButtonListeners();
    }
    
    private void InitializeReferences()
    {
        if (firebaseService == null)
        {
            firebaseService = ServiceLocator.Get<FirebaseService>();
        }
    }
    
    private void SetupButtonListeners()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnCloseClicked);
        }
    }
    
    public void ShowProfile(PlayerData playerData, PlayerStats playerStats)
    {
        if (profilePanel != null)
        {
            profilePanel.SetActive(true);
        }
        
        if (usernameText != null)
        {
            usernameText.text = playerData.Username;
        }
        
        if (levelText != null)
        {
            levelText.text = $"Level {playerData.Level}";
        }
        
        if (xpText != null)
        {
            xpText.text = $"XP: {playerData.XP}";
        }
        
        UpdateStatsDisplay(playerStats);
        
        Debug.Log($"Profile screen shown for player: {playerData.Username}");
    }
    
    private void UpdateStatsDisplay(PlayerStats stats)
    {
        if (matchesPlayedText != null)
        {
            matchesPlayedText.text = stats.matchesPlayed.ToString();
        }
        
        if (winsText != null)
        {
            winsText.text = stats.wins.ToString();
        }
        
        if (lossesText != null)
        {
            lossesText.text = stats.losses.ToString();
        }
        
        if (drawsText != null)
        {
            drawsText.text = stats.draws.ToString();
        }
        
        if (bestStreakText != null)
        {
            bestStreakText.text = stats.bestStreak.ToString();
        }
        
        if (currentStreakText != null)
        {
            currentStreakText.text = stats.currentStreak.ToString();
        }
        
        if (rankText != null)
        {
            rankText.text = stats.highestRank;
        }
    }
    
    private void OnCloseClicked()
    {
        Debug.Log("Close button clicked in profile screen");
        if (profilePanel != null)
        {
            profilePanel.SetActive(false);
        }
    }
}