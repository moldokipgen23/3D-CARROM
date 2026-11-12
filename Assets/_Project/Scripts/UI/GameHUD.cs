using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameHUD : MonoBehaviour
{
    [Header("Player References")]
    public GameObject player1UI;
    public GameObject player2UI;

    [Header("Score Display")]
    public Text player1ScoreText;
    public Text player2ScoreText;
    public Text player3ScoreText;

    [Header("Turn Display")]
    public Text currentTurnText;
    public Image turnIndicator;
    public Color player1TurnColor = new Color(0.96f, 0.77f, 0.34f);
    public Color player2TurnColor = new Color(0.62f, 0.60f, 0.56f);

    [Header("Coin Count Display")]
    public Text remainingCoinsText;

    [Header("References")]
    public TurnManager turnManager;
    public ScoreManager scoreManager;

    private Canvas canvas;
    private Text player1Label;
    private Text player2Label;
    private Text hintText;
    private Text toastText;
    private Image player1Card;
    private Image player2Card;
    private Image player1Accent;
    private Image player2Accent;
    private Image powerFill;
    private RectTransform powerRow;
    private GameObject powerRoot;
    private Text powerValueText;
    private StrikerController striker;
    private Coroutine toastRoutine;
    private int lastP1Score = -1;
    private int lastP2Score = -1;

    private void Start()
    {
        InitializeReferences();
        BuildUI();
        Subscribe();
        Refresh();
    }

    private void OnDestroy()
    {
        if (scoreManager != null)
            scoreManager.OnScoreUpdated -= HandleScoreUpdated;
        if (turnManager != null)
            turnManager.OnTurnChanged -= HandleTurnChanged;
    }

    private void InitializeReferences()
    {
        if (turnManager == null)
        {
            turnManager = TurnManager.Instance;
            if (turnManager == null)
                turnManager = FindObjectOfType<TurnManager>(true);
        }

        if (scoreManager == null)
        {
            scoreManager = ScoreManager.Instance;
            if (scoreManager == null)
                scoreManager = FindObjectOfType<ScoreManager>(true);
        }

        striker = FindObjectOfType<StrikerController>(true);
    }

    private void Subscribe()
    {
        if (scoreManager != null)
            scoreManager.OnScoreUpdated += HandleScoreUpdated;

        if (turnManager != null)
            turnManager.OnTurnChanged += HandleTurnChanged;
    }

    private void HandleScoreUpdated(int player, int score)
    {
        Refresh();
        ShowToast(player == 1 ? "YOU" : (turnManager != null && turnManager.aiControlsPlayer2 ? "AI" : "P2"),
            score, player == 1 ? lastP1Score : lastP2Score);
    }

    private void HandleTurnChanged(int player)
    {
        Refresh();
    }

    private void BuildUI()
    {
        if (player1ScoreText != null && player2ScoreText != null)
        {
            // UI was wired in the Inspector - leave it alone.
            return;
        }

        RuntimeUI.EnsureEventSystem();

        canvas = RuntimeUI.CreateCanvas("GameHUDCanvas", 100);
        RectTransform safe = UITheme.Rect(canvas.transform, "SafeArea");
        UITheme.ApplySafeArea(safe);

        Color shadeColor = new Color(0.05f, 0.035f, 0.025f);
        Image topShade = UITheme.Gradient(safe, "TopShade", shadeColor, shadeColor, 0.88f, 0f);
        UITheme.SetRect(topShade.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(0.5f, 1f), Vector2.zero, new Vector2(0f, 460f));

        Image bottomShade = UITheme.Gradient(safe, "BottomShade", shadeColor, shadeColor, 0f, 0.90f);
        UITheme.SetRect(bottomShade.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f),
            new Vector2(0.5f, 0f), Vector2.zero, new Vector2(0f, 400f));

        Image vignette = UITheme.Rect(safe, "Vignette").gameObject.AddComponent<Image>();
        vignette.sprite = UITheme.Vignette();
        vignette.color = new Color(1f, 1f, 1f, 0.45f);
        vignette.raycastTarget = false;
        UITheme.Stretch(vignette.rectTransform, -60f, -60f, -60f, -60f);

        BuildPlayerCard(safe, true);
        BuildPlayerCard(safe, false);
        BuildCenterPanel(safe);
        BuildBottomPanel(safe);

        toastText = UITheme.Label(safe, "Toast", "", 44, TextAnchor.MiddleCenter, UITheme.Gold,
            FontStyle.Bold);
        UITheme.SetRect(toastText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(0f, 240f), new Vector2(800f, 80f));
        toastText.gameObject.SetActive(false);
    }

    private void BuildPlayerCard(Transform parent, bool left)
    {
        Image card = UITheme.Panel(parent, left ? "Player1Card" : "Player2Card",
            UITheme.PanelBg, 34, true, 0.35f);
        Vector2 corner = left ? new Vector2(0f, 1f) : new Vector2(1f, 1f);
        Vector2 pivot = left ? new Vector2(0f, 1f) : new Vector2(1f, 1f);
        float x = left ? 18f : -18f;
        UITheme.SetRect(card.rectTransform, corner, corner, pivot, new Vector2(x, -18f),
            new Vector2(362f, 156f));

        if (left) player1Card = card; else player2Card = card;

        float side = left ? 0f : 1f;
        float dir = left ? 1f : -1f;

        // Active-turn accent strip along the inner edge.
        Image accent = UITheme.Panel(card.transform, "Accent", new Color(1f, 1f, 1f, 0f), 12);
        UITheme.SetRect(accent.rectTransform, new Vector2(side, 0f), new Vector2(side, 1f),
            new Vector2(side, 0.5f), new Vector2(dir * 6f, 0f), new Vector2(8f, -22f));
        if (left) player1Accent = accent; else player2Accent = accent;

        // Avatar with the player's coin face.
        Image ring = UITheme.Circle(card.transform, "AvatarRing",
            left ? UITheme.Cream : new Color(0.16f, 0.13f, 0.10f));
        UITheme.SetRect(ring.rectTransform, new Vector2(side, 0.5f), new Vector2(side, 0.5f),
            new Vector2(side, 0.5f), new Vector2(dir * 14f, 0f), new Vector2(84f, 84f));

        Image avatar = UITheme.Circle(card.transform, "Avatar",
            left ? new Color(0.78f, 0.70f, 0.55f) : new Color(0.10f, 0.09f, 0.08f));
        UITheme.SetRect(avatar.rectTransform, new Vector2(side, 0.5f), new Vector2(side, 0.5f),
            new Vector2(side, 0.5f), new Vector2(dir * 19f, 0f), new Vector2(74f, 74f));

        Image coinFace = UITheme.Icon(avatar.transform, "AvatarCoin", UITheme.CoinIcon(left), 52f);
        UITheme.SetRect(coinFace.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(52f, 52f));

        // Name + coin colour.
        TextAnchor align = left ? TextAnchor.MiddleLeft : TextAnchor.MiddleRight;
        float textX = dir * 108f;
        float textW = 236f;

        player1Label = left
            ? UITheme.Label(card.transform, "PlayerLabel", "YOU", 27, align, UITheme.Cream)
            : null;
        player2Label = left
            ? null
            : UITheme.Label(card.transform, "PlayerLabel", "AI", 27, align, UITheme.Cream);

        Text label = left ? player1Label : player2Label;
        UITheme.SetRect(label.rectTransform, new Vector2(side, 1f), new Vector2(side, 1f),
            new Vector2(side, 1f), new Vector2(textX, -18f), new Vector2(textW, 34f));

        Text subtitle = UITheme.Label(card.transform, "Subtitle", left ? "WHITE" : "BLACK", 18,
            align, new Color(0.74f, 0.68f, 0.58f, 0.95f), FontStyle.Normal);
        UITheme.SetRect(subtitle.rectTransform, new Vector2(side, 1f), new Vector2(side, 1f),
            new Vector2(side, 1f), new Vector2(textX, -48f), new Vector2(textW, 24f));

        // Score row.
        Image scoreCoin = UITheme.Icon(card.transform, "ScoreCoin", UITheme.CoinIcon(left), 34f);
        UITheme.SetRect(scoreCoin.rectTransform, new Vector2(side, 0f), new Vector2(side, 0f),
            new Vector2(side, 0f), new Vector2(textX + dir * 17f, 24f), new Vector2(34f, 34f));

        Text score = UITheme.Label(card.transform, "Score", "0", 40, align, UITheme.Gold);
        UITheme.SetRect(score.rectTransform, new Vector2(side, 0f), new Vector2(side, 0f),
            new Vector2(side, 0f), new Vector2(textX + dir * 44f, 20f), new Vector2(70f, 44f));

        Text of = UITheme.Label(card.transform, "Of", "/9", 20, align,
            new Color(0.74f, 0.68f, 0.58f, 0.9f), FontStyle.Normal);
        UITheme.SetRect(of.rectTransform, new Vector2(side, 0f), new Vector2(side, 0f),
            new Vector2(side, 0f), new Vector2(textX + dir * 104f, 22f), new Vector2(44f, 28f));

        if (left)
        {
            player1ScoreText = score;
            player1UI = card.gameObject;
        }
        else
        {
            player2ScoreText = score;
            player2UI = card.gameObject;
        }
    }

    private void BuildCenterPanel(Transform parent)
    {
        turnIndicator = UITheme.Panel(parent, "TurnPill", new Color(0.12f, 0.08f, 0.05f, 0.95f), 26, true, 0.8f);
        UITheme.SetRect(turnIndicator.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f), new Vector2(0f, -26f), new Vector2(286f, 60f));

        currentTurnText = UITheme.Label(turnIndicator.transform, "TurnText", "YOUR TURN", 24,
            TextAnchor.MiddleCenter, UITheme.Gold);
        UITheme.Stretch(currentTurnText.rectTransform, 0f, 0f, 0f, 0f);

        Image queenChip = UITheme.Panel(parent, "QueenChip", new Color(0.12f, 0.08f, 0.05f, 0.92f), 20);
        UITheme.SetRect(queenChip.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f), new Vector2(0f, -196f), new Vector2(328f, 46f));

        Image queenDot = UITheme.Circle(queenChip.transform, "QueenDot", new Color(0.72f, 0.12f, 0.11f));
        UITheme.SetRect(queenDot.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
            new Vector2(0f, 0.5f), new Vector2(14f, 0f), new Vector2(20f, 20f));

        player3ScoreText = UITheme.Label(queenChip.transform, "QueenText", "QUEEN · ON BOARD", 18,
            TextAnchor.MiddleLeft, new Color(0.93f, 0.86f, 0.72f), FontStyle.Bold);
        UITheme.SetRect(player3ScoreText.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f),
            new Vector2(0f, 0.5f), new Vector2(44f, 0f), new Vector2(-54f, 32f));
    }

    private void BuildBottomPanel(Transform parent)
    {
        powerRoot = UITheme.Rect(parent, "PowerRoot").gameObject;
        RectTransform powerRootRt = powerRoot.GetComponent<RectTransform>();
        UITheme.SetRect(powerRootRt, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f), new Vector2(0f, 176f), new Vector2(820f, 52f));

        Text powerLabel = UITheme.Label(powerRoot.transform, "PowerLabel", "POWER", 20,
            TextAnchor.MiddleLeft, new Color(0.86f, 0.78f, 0.62f), FontStyle.Bold);
        UITheme.SetRect(powerLabel.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 1f),
            new Vector2(0f, 0.5f), Vector2.zero, new Vector2(110f, 0f));

        Image track = UITheme.Panel(powerRoot.transform, "Track", new Color(0.08f, 0.055f, 0.035f, 0.95f), 10, true, 0.3f);
        UITheme.SetRect(track.rectTransform, new Vector2(0f, 0.5f), new Vector2(1f, 0.5f),
            new Vector2(0f, 0.5f), new Vector2(112f, 0f), new Vector2(-118f, 30f));

        powerFill = UITheme.Rect(track.transform, "Fill").gameObject.AddComponent<Image>();
        powerFill.sprite = UITheme.Rounded(10, Color.white, Color.white, 0);
        powerFill.type = Image.Type.Filled;
        powerFill.fillMethod = Image.FillMethod.Horizontal;
        powerFill.fillOrigin = 0;
        powerFill.fillAmount = 0f;
        powerFill.color = UITheme.Green;
        powerFill.raycastTarget = false;
        UITheme.Stretch(powerFill.rectTransform, 3f, 3f, 3f, 3f);

        powerValueText = UITheme.Label(powerRoot.transform, "PowerValue", "0%", 20,
            TextAnchor.MiddleRight, new Color(0.86f, 0.78f, 0.62f), FontStyle.Bold);
        UITheme.SetRect(powerValueText.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 1f),
            new Vector2(1f, 0.5f), Vector2.zero, new Vector2(90f, 0f));

        powerRow = powerRootRt;
        powerRoot.SetActive(false);

        Image hintPanel = UITheme.Panel(parent, "HintPanel", new Color(0.10f, 0.07f, 0.045f, 0.90f), 28);
        UITheme.SetRect(hintPanel.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f), new Vector2(0f, 92f), new Vector2(960f, 62f));

        hintText = UITheme.Label(hintPanel.transform, "Hint",
            "Drag from the striker to aim  ·  release to shoot", 24,
            TextAnchor.MiddleCenter, new Color(0.94f, 0.88f, 0.76f), FontStyle.Normal);
        UITheme.Stretch(hintText.rectTransform, 16f, 16f, 0f, 0f);

        remainingCoinsText = UITheme.Label(parent, "RemainingCoins", "18 COINS ON BOARD", 21,
            TextAnchor.MiddleCenter, new Color(0.72f, 0.66f, 0.56f, 0.85f), FontStyle.Normal);
        UITheme.SetRect(remainingCoinsText.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f), new Vector2(0f, 40f), new Vector2(700f, 32f));
    }

    private void Update()
    {
        if (striker == null)
            striker = FindObjectOfType<StrikerController>();

        if (powerRoot == null) return;

        bool aiming = striker != null && striker.IsAiming;
        powerRoot.SetActive(aiming);

        if (!aiming) return;

        float power = Mathf.Clamp01(striker.CurrentPowerNormalized);
        powerFill.fillAmount = power;
        powerFill.color = Color.Lerp(UITheme.Green, UITheme.Red, power);
        if (powerValueText != null)
            powerValueText.text = Mathf.RoundToInt(power * 100f) + "%";
    }

    public void Refresh()
    {
        if (player1ScoreText == null || player2ScoreText == null || currentTurnText == null)
            return;

        int p1Score = scoreManager != null ? scoreManager.GetPlayerScore(1) : 0;
        int p2Score = scoreManager != null ? scoreManager.GetPlayerScore(2) : 0;

        player1ScoreText.text = p1Score.ToString();
        player2ScoreText.text = p2Score.ToString();

        int current = turnManager != null ? turnManager.GetCurrentPlayer() : 1;

        if (scoreManager != null)
        {
            if (remainingCoinsText != null)
                remainingCoinsText.text = scoreManager.GetRemainingCoins() + " COINS ON BOARD";

            if (player3ScoreText != null)
                player3ScoreText.text = scoreManager.GetQueenStatusText().ToUpperInvariant();
        }

        bool aiTurn = turnManager != null && turnManager.aiControlsPlayer2 && current == 2;
        bool localMode = turnManager != null && !turnManager.aiControlsPlayer2;

        if (player2Label != null)
            player2Label.text = localMode ? "PLAYER 2" : "AI";

        if (scoreManager != null && scoreManager.IsGameEnded)
        {
            currentTurnText.text = "GAME OVER";
        }
        else if (aiTurn)
        {
            currentTurnText.text = "AI TURN";
        }
        else
        {
            currentTurnText.text = "YOUR TURN";
        }

        // Active player highlight.
        Color activeGold = player1TurnColor;
        Color inactiveGold = new Color(1f, 1f, 1f, 0f);
        if (player1Accent != null) player1Accent.color = current == 1 ? activeGold : inactiveGold;
        if (player2Accent != null)
            player2Accent.color = current == 2 ? new Color(0.80f, 0.78f, 0.72f) : inactiveGold;

        if (player1Card != null)
            player1Card.sprite = UITheme.Rounded(34, UITheme.PanelBg,
                new Color(UITheme.Gold.r, UITheme.Gold.g, UITheme.Gold.b, current == 1 ? 0.85f : 0.22f), 3);
        if (player2Card != null)
            player2Card.sprite = UITheme.Rounded(34, UITheme.PanelBg,
                new Color(0.85f, 0.82f, 0.75f, current == 2 ? 0.75f : 0.22f), 3);

        if (turnIndicator != null)
            turnIndicator.color = current == 1 ? Color.white : new Color(0.92f, 0.92f, 0.92f, 1f);

        if (hintText != null)
        {
            bool over = scoreManager != null && scoreManager.IsGameEnded;
            hintText.gameObject.SetActive(!over);
            if (!over)
                hintText.text = aiTurn
                    ? "Opponent is lining up a shot..."
                    : "Drag from the striker to aim  ·  release to shoot";
        }

        lastP1Score = p1Score;
        lastP2Score = p2Score;
    }

    private void ShowToast(string who, int score, int previousScore)
    {
        if (toastText == null) return;
        if (score <= previousScore) return;

        if (toastRoutine != null) StopCoroutine(toastRoutine);
        toastRoutine = StartCoroutine(ToastRoutine(who));
    }

    private IEnumerator ToastRoutine(string who)
    {
        toastText.text = who + "  +1";
        toastText.gameObject.SetActive(true);

        Vector2 start = toastText.rectTransform.anchoredPosition;
        float t = 0f;
        Color c = UITheme.Gold;

        while (t < 1.2f)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / 1.2f);
            toastText.rectTransform.anchoredPosition = start + new Vector2(0f, p * 40f);
            c.a = 1f - Mathf.SmoothStep(0.5f, 1f, p);
            toastText.color = c;
            yield return null;
        }

        toastText.gameObject.SetActive(false);
        toastText.rectTransform.anchoredPosition = start;
        toastText.color = UITheme.Gold;
        toastRoutine = null;
    }
}
