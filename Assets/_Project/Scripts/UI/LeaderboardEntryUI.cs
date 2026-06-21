using UnityEngine;
using UnityEngine.UI;

public class LeaderboardEntryUI : MonoBehaviour
{
    [Header("UI References")]
    public Text rankText;
    public Text usernameText;
    public Text scoreText;
    public Text tierText;
    public Image background;
    public Image avatarImage;

    [Header("Colors")]
    public Color topThreeColor = new Color(1f, 0.84f, 0f);
    public Color playerColor = new Color(0f, 0.8f, 1f);
    public Color defaultColor = Color.white;

    public void Setup(LeaderboardEntry entry)
    {
        if (rankText != null) rankText.text = $"#{entry.rank}";
        if (usernameText != null) usernameText.text = entry.username;
        if (scoreText != null) scoreText.text = entry.score.ToString("N0");
        if (tierText != null) tierText.text = entry.rankTier;

        if (background != null)
        {
            if (entry.rank <= 3)
            {
                background.color = topThreeColor;
            }
            else
            {
                background.color = defaultColor;
            }
        }
    }

    public void SetAsCurrentPlayer(bool isCurrentPlayer)
    {
        if (isCurrentPlayer && background != null)
        {
            background.color = playerColor;
        }
    }
}
