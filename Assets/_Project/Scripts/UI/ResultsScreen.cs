using UnityEngine;
using UnityEngine.UI;

public class ResultsScreen : MonoBehaviour
{
    [Header("UI References")]
    public GameObject resultsPanel;
    public Text resultText;
    public Button playAgainButton;
    public Button mainMenuButton;

    [Header("Score Display")]
    public Text player1ScoreText;
    public Text player2ScoreText;
    public Text queenScoreText;

    [Header("References")]
    public ScoreManager scoreManager;

    private bool built = false;
    private Text titleText;
    private Text subtitleText;

    private void Start()
    {
        InitializeReferences();
        EnsureBuilt();
        HideResults();
    }

    private void InitializeReferences()
    {
        if (scoreManager == null)
        {
            scoreManager = ScoreManager.Instance;
            if (scoreManager == null)
                scoreManager = FindObjectOfType<ScoreManager>(true);
        }
    }

    private void EnsureBuilt()
    {
        if (built || resultsPanel != null) return;
        built = true;

        RuntimeUI.EnsureEventSystem();

        Canvas canvas = RuntimeUI.CreateCanvas("ResultsCanvas", 200);
        RectTransform safe = UITheme.Rect(canvas.transform, "SafeArea");
        UITheme.ApplySafeArea(safe);

        RectTransform root = UITheme.Rect(safe, "ResultsRoot");
        UITheme.Stretch(root, 0f, 0f, 0f, 0f);

        Image dimmer = UITheme.Panel(root, "Dimmer", new Color(0f, 0f, 0f, 0.78f), 0);
        UITheme.Stretch(dimmer.rectTransform, 0f, 0f, 0f, 0f);

        Image panel = UITheme.Panel(root, "ResultsPanel", new Color(0.10f, 0.07f, 0.045f, 0.99f), 48, true, 0.85f);
        UITheme.SetRect(panel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(900f, 1000f));

        // The whole overlay (dim + card) hides together.
        resultsPanel = root.gameObject;

        Image sparkleL = UITheme.Icon(panel.transform, "SparkleL", UITheme.Sparkle(UITheme.Gold), 56f);
        UITheme.SetRect(sparkleL.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f), new Vector2(-330f, -70f), new Vector2(56f, 56f));

        Image sparkleR = UITheme.Icon(panel.transform, "SparkleR", UITheme.Sparkle(UITheme.Gold), 34f);
        UITheme.SetRect(sparkleR.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f), new Vector2(340f, -48f), new Vector2(34f, 34f));

        Text ribbon = UITheme.Label(panel.transform, "Ribbon", "M A T C H   C O M P L E T E", 22,
            TextAnchor.MiddleCenter, new Color(0.80f, 0.72f, 0.58f), FontStyle.Normal);
        UITheme.SetRect(ribbon.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f), new Vector2(0f, -56f), new Vector2(700f, 36f));

        titleText = UITheme.Label(panel.transform, "Title", "VICTORY", 86,
            TextAnchor.MiddleCenter, UITheme.Gold, FontStyle.Bold);
        UITheme.SetRect(titleText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f), new Vector2(0f, -104f), new Vector2(820f, 110f));

        Outline titleOutline = titleText.gameObject.AddComponent<Outline>();
        titleOutline.effectColor = new Color(0.26f, 0.12f, 0.03f, 0.9f);
        titleOutline.effectDistance = new Vector2(2.5f, -2.5f);

        subtitleText = UITheme.Label(panel.transform, "ResultText", "You win the match", 30,
            TextAnchor.MiddleCenter, UITheme.Cream, FontStyle.Normal);
        UITheme.SetRect(subtitleText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f), new Vector2(0f, -206f), new Vector2(800f, 44f));
        resultText = subtitleText;

        BuildScoreRow(panel.transform, true, 300f);
        BuildScoreRow(panel.transform, false, 400f);

        Image queenChip = UITheme.Panel(panel.transform, "QueenChip",
            new Color(0.95f, 0.76f, 0.34f, 0.10f), 20);
        UITheme.SetRect(queenChip.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f), new Vector2(0f, -492f), new Vector2(520f, 54f));

        queenScoreText = UITheme.Label(queenChip.transform, "QueenScore", "QUEEN · ON BOARD", 22,
            TextAnchor.MiddleCenter, UITheme.Gold, FontStyle.Bold);
        UITheme.Stretch(queenScoreText.rectTransform, 0f, 0f, 0f, 0f);

        playAgainButton = UITheme.Button(panel.transform, "PlayAgainButton", "PLAY AGAIN",
            UITheme.Gold, UITheme.Ink, 40, 40);
        UITheme.SetRect(playAgainButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 190f),
            new Vector2(620f, 120f));

        mainMenuButton = UITheme.Button(panel.transform, "MainMenuButton", "MAIN MENU",
            UITheme.PanelSoft, UITheme.Cream, 34, 40);
        UITheme.SetRect(mainMenuButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 62f),
            new Vector2(620f, 104f));

        playAgainButton.onClick.AddListener(OnPlayAgainClicked);
        mainMenuButton.onClick.AddListener(OnMainMenuClicked);
    }

    private void BuildScoreRow(Transform parent, bool playerOne, float topOffset)
    {
        Image row = UITheme.Panel(parent, playerOne ? "P1Row" : "P2Row",
            new Color(0.16f, 0.11f, 0.07f, 0.9f), 24);
        UITheme.SetRect(row.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f), new Vector2(0f, -topOffset), new Vector2(740f, 82f));

        Image coin = UITheme.Icon(row.transform, "Coin", UITheme.CoinIcon(playerOne), 54f);
        UITheme.SetRect(coin.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
            new Vector2(0f, 0.5f), new Vector2(22f, 0f), new Vector2(54f, 54f));

        Text name = UITheme.Label(row.transform, "Name",
            playerOne ? "YOU  ·  WHITE" : "AI  ·  BLACK", 24,
            TextAnchor.MiddleLeft, new Color(0.86f, 0.80f, 0.68f), FontStyle.Bold);
        UITheme.SetRect(name.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f),
            new Vector2(0f, 0.5f), new Vector2(94f, 0f), new Vector2(-300f, 40f));

        Text score = UITheme.Label(row.transform, "Score", "0", 46,
            TextAnchor.MiddleRight, UITheme.Gold, FontStyle.Bold);
        UITheme.SetRect(score.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 1f),
            new Vector2(1f, 0.5f), new Vector2(-34f, 0f), new Vector2(200f, 60f));

        if (playerOne) player1ScoreText = score;
        else player2ScoreText = score;
    }

    /// <summary>Called by ScoreManager when the match ends.</summary>
    public void ShowResults(int winner, string message)
    {
        EnsureBuilt();
        InitializeReferences();

        if (resultsPanel != null)
            resultsPanel.SetActive(true);

        bool playerWon = winner == 1;

        if (titleText != null)
            titleText.text = playerWon ? "VICTORY" : "DEFEAT";

        if (titleText != null)
            titleText.color = playerWon ? UITheme.Gold : new Color(0.82f, 0.40f, 0.34f);

        if (subtitleText != null)
            subtitleText.text = string.IsNullOrEmpty(message)
                ? (playerWon ? "You win the match" : "Better luck next time")
                : message;

        if (scoreManager != null)
        {
            if (player1ScoreText != null)
                player1ScoreText.text = scoreManager.GetPlayerScore(1).ToString();
            if (player2ScoreText != null)
                player2ScoreText.text = scoreManager.GetPlayerScore(2).ToString();
            if (queenScoreText != null)
                queenScoreText.text = scoreManager.GetQueenStatusText().ToUpperInvariant();
        }

        Debug.Log("Results screen shown: " + (resultText != null ? resultText.text : message));
    }

    public void HideResults()
    {
        if (resultsPanel != null)
            resultsPanel.SetActive(false);
    }

    private void OnPlayAgainClicked()
    {
        Time.timeScale = 1f;
        SceneFlow.LoadScene(SceneFlow.GAME);
    }

    private void OnMainMenuClicked()
    {
        Time.timeScale = 1f;
        SceneFlow.LoadScene(SceneFlow.MAIN_MENU);
    }
}
