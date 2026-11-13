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
    private GameObject toastRoot;
    private CanvasGroup toastGroup;
    private GameObject foulBanner;
    private CanvasGroup foulCanvas;
    private Text foulTitleText;
    private Text foulDetailText;
    private Image foulFlash;
    private Coroutine foulRoutine;
    private bool foulVisible;
    private int lastTurnNotified = 1;
    private Image player1Card;
    private Image player2Card;
    private Image player1Accent;
    private Image player2Accent;
    private Image player1Progress;
    private Image player2Progress;
    private Image player1AvatarRing;
    private Image player2AvatarRing;
    private Image powerFill;
    private RectTransform powerRow;
    private GameObject powerRoot;
    private Text powerValueText;
    private StrikerController striker;
    private CarromCameraController cameraController;
    private Slider placementSlider;
    private bool syncingPlacement;
    private Coroutine toastRoutine;
    private Button soundButton;
    private Text soundLabel;
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
        cameraController = FindObjectOfType<CarromCameraController>(true);
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

        // A short "YOUR TURN" call-out makes the hand-off obvious without
        // repeating on every extra turn the same player earns.
        if (player != lastTurnNotified)
        {
            lastTurnNotified = player;

            // The foul card already explains the hand-off; stacking the turn
            // toast behind it looked cluttered and duplicated the message.
            if (!foulVisible)
                ShowTurnToast(player);
        }
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

        Color shadeColor = new Color(0.155f, 0.048f, 0.038f);
        Image topShade = UITheme.Gradient(safe, "TopShade", shadeColor, shadeColor, 0.90f, 0f);
        UITheme.SetRect(topShade.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(0.5f, 1f), Vector2.zero, new Vector2(0f, 500f));

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
        BuildZoomControls(safe);
        BuildSoundToggle(safe);
        BuildBottomPanel(safe);
        BuildMenuButton(safe);

        // Turn call-out: dark pill behind bold text so it stays readable on
        // both the pale board and the dark background.
        toastRoot = new GameObject("ToastRoot", typeof(RectTransform));
        toastRoot.transform.SetParent(safe, false);
        toastGroup = toastRoot.AddComponent<CanvasGroup>();
        toastGroup.alpha = 0f;
        toastGroup.blocksRaycasts = false;
        toastGroup.interactable = false;

        RectTransform toastRect = toastRoot.GetComponent<RectTransform>();
        UITheme.SetRect(toastRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(0f, 250f), new Vector2(640f, 104f));

        Image toastBg = UITheme.Panel(toastRoot.transform, "ToastBg",
            new Color(0.075f, 0.045f, 0.030f), 26, true, 0.55f);
        toastBg.raycastTarget = false;
        UITheme.Stretch(toastBg.rectTransform, 0f, 0f, 0f, 0f);

        toastText = UITheme.Label(toastRoot.transform, "Toast", "", 44,
            TextAnchor.MiddleCenter, UITheme.Gold, FontStyle.Bold);
        UITheme.Stretch(toastText.rectTransform, 12f, 12f, 0f, 0f);
        toastRoot.SetActive(false);

        BuildFoulBanner(safe);
    }

    /// <summary>
    /// Big, unmistakable foul call: a centre-screen card plus a brief red edge
    /// flash. Fouls used to be silent (only a log line), so players could not
    /// tell why the turn changed.
    /// </summary>
    private void BuildFoulBanner(Transform parent)
    {
        foulFlash = UITheme.Rect(parent, "FoulFlash").gameObject.AddComponent<Image>();
        foulFlash.sprite = null;
        foulFlash.color = new Color(0.85f, 0.06f, 0.03f, 0f);
        foulFlash.raycastTarget = false;
        UITheme.Stretch(foulFlash.rectTransform, 0f, 0f, 0f, 0f);

        Image card = UITheme.Panel(parent, "FoulBanner",
            new Color(0.235f, 0.038f, 0.030f, 0.97f), 34, true, 0.60f);
        UITheme.SetRect(card.rectTransform, new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, 30f), new Vector2(880f, 206f));

        foulBanner = card.gameObject;
        foulCanvas = foulBanner.AddComponent<CanvasGroup>();
        foulCanvas.alpha = 0f;
        foulCanvas.blocksRaycasts = false;
        foulCanvas.interactable = false;

        // Thin gold rule above the title.
        Image rule = UITheme.Rect(card.transform, "Rule").gameObject.AddComponent<Image>();
        rule.color = new Color(UITheme.Gold.r, UITheme.Gold.g, UITheme.Gold.b, 0.85f);
        rule.raycastTarget = false;
        UITheme.SetRect(rule.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f), new Vector2(0f, -22f), new Vector2(120f, 3f));

        foulTitleText = UITheme.Label(card.transform, "FoulTitle", "FOUL!", 50,
            TextAnchor.MiddleCenter, new Color(1f, 0.58f, 0.26f));
        foulTitleText.horizontalOverflow = HorizontalWrapMode.Overflow;
        foulTitleText.verticalOverflow = VerticalWrapMode.Overflow;
        foulTitleText.resizeTextForBestFit = true;
        foulTitleText.resizeTextMinSize = 28;
        foulTitleText.resizeTextMaxSize = 50;
        UITheme.SetRect(foulTitleText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(0.5f, 1f), new Vector2(0f, -24f), new Vector2(-40f, 74f));

        foulDetailText = UITheme.Label(card.transform, "FoulDetail", "", 26,
            TextAnchor.MiddleCenter, new Color(0.97f, 0.92f, 0.84f), FontStyle.Normal);
        foulDetailText.horizontalOverflow = HorizontalWrapMode.Wrap;
        foulDetailText.verticalOverflow = VerticalWrapMode.Overflow;
        UITheme.SetRect(foulDetailText.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f),
            new Vector2(0.5f, 0f), new Vector2(0f, 30f), new Vector2(-48f, 84f));

        foulBanner.SetActive(false);
    }

    /// <summary>
    /// Shows the foul card for a couple of seconds. If the card is already up
    /// (striker pocketed, then the penalty resolves a moment later) the text is
    /// updated in place instead of replaying the pop, which would look glitchy.
    /// </summary>
    public void ShowFoul(string title, string detail)
    {
        if (foulBanner == null) return;

        bool wasVisible = foulVisible;

        if (foulRoutine != null)
            StopCoroutine(foulRoutine);

        foulTitleText.text = title;
        foulDetailText.text = detail;
        foulBanner.SetActive(true);
        foulVisible = true;

        if (wasVisible)
        {
            foulCanvas.alpha = 1f;
            foulCanvas.transform.localScale = Vector3.one;
        }

        foulRoutine = StartCoroutine(FoulRoutine(wasVisible));
    }

    private IEnumerator FoulRoutine(bool skipIntro)
    {
        if (foulCanvas == null) yield break;

        const float inTime = 0.16f;
        const float holdTime = 1.6f;
        const float outTime = 0.30f;

        if (!skipIntro)
        {
            float t = 0f;
            while (t < inTime)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / inTime);
                foulCanvas.alpha = p;
                float pop = 1f - (1f - p) * (1f - p);
                foulCanvas.transform.localScale = Vector3.one * Mathf.Lerp(0.84f, 1f, pop);
                foulFlash.color = new Color(0.85f, 0.06f, 0.03f, 0.10f * (1f - p));
                yield return null;
            }
        }

        foulCanvas.alpha = 1f;
        foulCanvas.transform.localScale = Vector3.one;
        foulFlash.color = new Color(0.85f, 0.06f, 0.03f, 0f);

        yield return new WaitForSeconds(holdTime);

        float elapsed = 0f;
        while (elapsed < outTime)
        {
            elapsed += Time.deltaTime;
            foulCanvas.alpha = 1f - Mathf.Clamp01(elapsed / outTime);
            yield return null;
        }

        foulBanner.SetActive(false);
        foulCanvas.alpha = 0f;
        foulFlash.color = new Color(0.85f, 0.06f, 0.03f, 0f);
        foulRoutine = null;
        foulVisible = false;
    }

    private void BuildPlayerCard(Transform parent, bool left)
    {
        // Compact reference-style player strip: bright orange avatar tile,
        // small name, coin icon and score. Much lighter than the old full-width
        // dark cards, so the board stays the hero of the screen.
        Image card = UITheme.Panel(parent, left ? "Player1Card" : "Player2Card",
            new Color(0.105f, 0.058f, 0.038f, 0.92f), 28, true, 0.40f);
        Vector2 corner = left ? new Vector2(0f, 1f) : new Vector2(1f, 1f);
        float x = left ? 12f : -12f;
        UITheme.SetRect(card.rectTransform, corner, corner, corner, new Vector2(x, -12f),
            new Vector2(350f, 160f));

        if (left) player1Card = card; else player2Card = card;

        float side = left ? 0f : 1f;
        float dir = left ? 1f : -1f;
        Vector2 sideTop = new Vector2(side, 1f);
        Vector2 sideBottom = new Vector2(side, 0f);

        // Active-turn accent strip along the inner edge. The sprite is plain
        // white so Refresh() can tint it gold or fade it out.
        Image accent = UITheme.Rect(card.transform, "Accent").gameObject.AddComponent<Image>();
        accent.sprite = UITheme.Rounded(6, Color.white, Color.white, 0);
        accent.type = Image.Type.Sliced;
        accent.color = new Color(1f, 1f, 1f, 0f);
        accent.raycastTarget = false;
        UITheme.SetRect(accent.rectTransform, new Vector2(side, 0f), new Vector2(side, 1f),
            new Vector2(side, 0.5f), new Vector2(dir * 5f, 0f), new Vector2(7f, -22f));
        if (left) player1Accent = accent; else player2Accent = accent;

        // Square orange avatar tile, like the reference profile cards.
        Image ring = UITheme.Panel(card.transform, "AvatarRing",
            new Color(0.960f, 0.560f, 0.165f), 18, true, 0.75f);
        UITheme.SetRect(ring.rectTransform, sideTop, sideTop, sideTop,
            new Vector2(dir * 14f, -18f), new Vector2(104f, 104f));
        if (left) player1AvatarRing = ring; else player2AvatarRing = ring;

        Image portrait = UITheme.Rect(card.transform, "Portrait").gameObject.AddComponent<Image>();
        portrait.sprite = UITheme.PlayerPortrait(left);
        portrait.preserveAspect = true;
        portrait.raycastTarget = false;
        UITheme.SetRect(portrait.rectTransform, sideTop, sideTop, sideTop,
            new Vector2(dir * 22f, -26f), new Vector2(88f, 88f));

        // Text block, mirrored for the right-hand card.
        TextAnchor align = left ? TextAnchor.MiddleLeft : TextAnchor.MiddleRight;
        float textX = dir * 132f;
        float textW = 196f;

        if (left)
            player1Label = UITheme.Label(card.transform, "PlayerLabel", "YOU", 27, align, UITheme.Cream);
        else
            player2Label = UITheme.Label(card.transform, "PlayerLabel", "ROOKIE", 27, align, UITheme.Cream);

        Text label = left ? player1Label : player2Label;
        UITheme.SetRect(label.rectTransform, sideTop, sideTop, sideTop,
            new Vector2(textX, -18f), new Vector2(textW, 32f));

        Text disc = UITheme.Label(card.transform, "Disc", left ? "WHITE" : "BLACK", 15,
            align, new Color(0.92f, 0.80f, 0.58f, 0.95f), FontStyle.Bold, false);
        UITheme.SetRect(disc.rectTransform, sideTop, sideTop, sideTop,
            new Vector2(textX, -50f), new Vector2(textW, 22f));

        // Coin tally: coin icon + score. The thin bar underneath carries the
        // out-of-nine progress, so the card stays as clean as the reference.
        float scoreStart = dir * 132f;
        float scoreOffset = left ? 172f : -176f;

        Image scoreCoin = UITheme.Icon(card.transform, "ScoreCoin", UITheme.CoinIcon(left), 30f);
        UITheme.SetRect(scoreCoin.rectTransform, sideTop, sideTop, sideTop,
            new Vector2(scoreStart, -80f), new Vector2(30f, 30f));

        Text score = UITheme.Label(card.transform, "Score", "0", 38,
            left ? TextAnchor.MiddleLeft : TextAnchor.MiddleRight, UITheme.Gold);
        UITheme.SetRect(score.rectTransform, sideTop, sideTop, sideTop,
            new Vector2(scoreOffset, -74f), new Vector2(58f, 40f));

        // Thin progress bar showing how many of the nine coins are home.
        Image track = UITheme.Panel(card.transform, "ProgressTrack",
            new Color(0f, 0f, 0f, 0.38f), 4);
        UITheme.SetRect(track.rectTransform, sideBottom, sideBottom, sideBottom,
            new Vector2(textX, 12f), new Vector2(textW, 7f));

        Image fill = UITheme.Rect(track.transform, "ProgressFill").gameObject.AddComponent<Image>();
        fill.sprite = UITheme.Rounded(4, Color.white, Color.white, 0);
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillOrigin = left ? 0 : 1;
        fill.fillAmount = 0f;
        fill.color = left ? UITheme.Gold : new Color(0.78f, 0.76f, 0.70f);
        fill.raycastTarget = false;
        UITheme.Stretch(fill.rectTransform, 0f, 0f, 0f, 0f);

        if (left)
        {
            player1ScoreText = score;
            player1Progress = fill;
            player1UI = card.gameObject;
        }
        else
        {
            player2ScoreText = score;
            player2Progress = fill;
            player2UI = card.gameObject;
        }
    }

    private void BuildCenterPanel(Transform parent)
    {
        turnIndicator = UITheme.Panel(parent, "TurnPill", new Color(0.10f, 0.07f, 0.045f, 0.96f),
            24, true, 0.85f);
        UITheme.SetRect(turnIndicator.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f), new Vector2(0f, -20f), new Vector2(238f, 58f));

        currentTurnText = UITheme.Label(turnIndicator.transform, "TurnText", "YOUR TURN", 23,
            TextAnchor.MiddleCenter, UITheme.Gold);
        UITheme.Stretch(currentTurnText.rectTransform, 8f, 8f, 0f, 0f);

        // Coin wallet, echoing the reference's centre coin stack. Cosmetic in
        // quick match, but it anchors the top bar and balances the two cards.
        Image balancePill = UITheme.Panel(parent, "BalancePill",
            new Color(0.10f, 0.065f, 0.042f, 0.88f), 22, true, 0.50f);
        UITheme.SetRect(balancePill.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f), new Vector2(0f, -84f), new Vector2(208f, 58f));

        Image coinBack = UITheme.Icon(balancePill.transform, "CoinBack", UITheme.CoinIcon(true), 38f);
        UITheme.SetRect(coinBack.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
            new Vector2(0f, 0.5f), new Vector2(10f, 4f), new Vector2(38f, 38f));

        Image coinFront = UITheme.Icon(balancePill.transform, "CoinFront", UITheme.CoinIcon(true), 42f);
        UITheme.SetRect(coinFront.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
            new Vector2(0f, 0.5f), new Vector2(24f, -3f), new Vector2(42f, 42f));

        Text balance = UITheme.Label(balancePill.transform, "Balance", "2,450", 28,
            TextAnchor.MiddleLeft, UITheme.Cream, FontStyle.Bold);
        UITheme.SetRect(balance.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f),
            new Vector2(0f, 0.5f), new Vector2(84f, 0f), new Vector2(-94f, 40f));

        Image queenChip = UITheme.Panel(parent, "QueenChip", new Color(0.10f, 0.07f, 0.045f, 0.92f), 20);
        UITheme.SetRect(queenChip.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f), new Vector2(0f, -196f), new Vector2(300f, 44f));

        Image queenDot = UITheme.Circle(queenChip.transform, "QueenDot", new Color(0.72f, 0.12f, 0.11f));
        UITheme.SetRect(queenDot.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
            new Vector2(0f, 0.5f), new Vector2(16f, 0f), new Vector2(18f, 18f));

        player3ScoreText = UITheme.Label(queenChip.transform, "QueenText", "QUEEN · ON BOARD", 18,
            TextAnchor.MiddleLeft, new Color(0.93f, 0.86f, 0.72f), FontStyle.Bold);
        UITheme.SetRect(player3ScoreText.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f),
            new Vector2(0f, 0.5f), new Vector2(42f, 0f), new Vector2(-52f, 30f));
    }

    private void BuildZoomControls(Transform parent)
    {
        // Compact zoom pill tucked under the opponent card, over the empty wood
        // above the board so it never covers coins or the striker.
        Image zoomPanel = UITheme.Panel(parent, "ZoomPanel", new Color(0.08f, 0.055f, 0.035f, 0.80f),
            28, true, 0.35f);
        UITheme.SetRect(zoomPanel.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(1f, 1f), new Vector2(-14f, -198f), new Vector2(154f, 60f));

        BuildZoomButton(zoomPanel.transform, "ZoomOut", "–", -38f, -1);
        BuildZoomButton(zoomPanel.transform, "ZoomIn", "+", 38f, 1);
    }

    private void BuildZoomButton(Transform parent, string name, string label, float x, int direction)
    {
        Button button = UITheme.Button(parent, name, label, new Color(0.20f, 0.14f, 0.09f, 0.95f),
            UITheme.Cream, 34, 24, true);
        UITheme.SetRect(button.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(x, 0f),
            new Vector2(54f, 48f));

        button.onClick.AddListener(() =>
        {
            if (cameraController == null)
                cameraController = FindObjectOfType<CarromCameraController>(true);
            if (cameraController == null) return;

            if (direction > 0) cameraController.ZoomIn();
            else cameraController.ZoomOut();
        });
    }

    private void BuildSoundToggle(Transform parent)
    {
        // Sits beside the zoom pill, under the opponent card, so it never
        // covers the board or the striker.
        soundButton = UITheme.Button(parent, "SoundButton", "SOUND",
            new Color(0.08f, 0.055f, 0.035f, 0.80f), UITheme.Cream, 18, 28, true);
        UITheme.SetRect(soundButton.GetComponent<RectTransform>(), new Vector2(1f, 1f),
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-182f, -198f),
            new Vector2(130f, 60f));

        soundLabel = soundButton.GetComponentInChildren<Text>();
        soundButton.onClick.AddListener(OnSoundToggle);
        UpdateSoundLabel();
    }

    private void OnSoundToggle()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.ToggleMuted();

        UpdateSoundLabel();
    }

    private void UpdateSoundLabel()
    {
        if (soundLabel == null) return;

        bool muted = AudioManager.Instance != null && AudioManager.Instance.IsMuted;
        soundLabel.text = muted ? "MUTED" : "SOUND";
        soundLabel.color = muted
            ? new Color(0.74f, 0.67f, 0.57f)
            : UITheme.Cream;
    }

    /// <summary>Round menu button in the bottom corner, matching the reference layout.</summary>
    private void BuildMenuButton(Transform parent)
    {
        Button menu = UITheme.Button(parent, "MenuButton", "",
            new Color(0.045f, 0.028f, 0.018f, 0.94f), UITheme.Gold, 1, 44, true);
        UITheme.SetRect(menu.GetComponent<RectTransform>(), new Vector2(1f, 0f),
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-22f, 24f),
            new Vector2(84f, 84f));

        for (int i = 0; i < 3; i++)
        {
            Image bar = UITheme.Rect(menu.transform, "Bar" + i).gameObject.AddComponent<Image>();
            bar.sprite = UITheme.Rounded(2, Color.white, Color.white, 0);
            bar.color = UITheme.Gold;
            bar.raycastTarget = false;
            UITheme.SetRect(bar.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(0f, 14f - i * 14f), new Vector2(34f, 4f));
        }

        menu.onClick.AddListener(() => SceneFlow.LoadScene(SceneFlow.MAIN_MENU));
    }

    private void BuildBottomPanel(Transform parent)
    {
        // --- Striker placement slider (Carrom King style wooden rail) ---
        Image placementPanel = UITheme.Panel(parent, "PlacementPanel",
            new Color(0.085f, 0.058f, 0.038f, 0.92f), 30, true, 0.28f);
        UITheme.SetRect(placementPanel.rectTransform, new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 322f),
            new Vector2(1000f, 158f));

        Text caption = UITheme.Label(placementPanel.transform, "Caption", "STRIKER POSITION", 14,
            TextAnchor.MiddleCenter, new Color(0.82f, 0.74f, 0.58f), FontStyle.Bold, false);
        UITheme.SetRect(caption.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f), new Vector2(0f, -12f), new Vector2(420f, 20f));

        RectTransform sliderRoot = UITheme.Rect(placementPanel.transform, "PlacementSlider");
        UITheme.SetRect(sliderRoot, new Vector2(0f, 0f), new Vector2(1f, 0f),
            new Vector2(0.5f, 0f), new Vector2(0f, 8f), new Vector2(-92f, 120f));

        Slider slider = sliderRoot.gameObject.AddComponent<Slider>();
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = -1f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
        slider.value = 0f;
        slider.transition = Selectable.Transition.None;

        // Warm wooden rail with a recessed groove; the control now belongs to
        // the board instead of looking like a thin stock Unity slider.
        Image track = UITheme.Panel(sliderRoot, "Track",
            new Color(0.345f, 0.210f, 0.118f, 1f), 22, true, 0.45f);
        UITheme.SetRect(track.rectTransform, new Vector2(0f, 0.5f), new Vector2(1f, 0.5f),
            new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(-40f, 50f));
        track.raycastTarget = true;

        Image groove = UITheme.Panel(track.transform, "Groove",
            new Color(0.048f, 0.032f, 0.021f, 0.92f), 9);
        UITheme.SetRect(groove.rectTransform, new Vector2(0f, 0.5f), new Vector2(1f, 0.5f),
            new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(-28f, 16f));

        RectTransform fillArea = UITheme.Rect(sliderRoot, "FillArea");
        UITheme.Stretch(fillArea, 34f, 34f, 52f, 52f);
        Image fill = UITheme.Rect(fillArea, "Fill").gameObject.AddComponent<Image>();
        fill.sprite = UITheme.Rounded(8, Color.white, Color.white, 0);
        fill.type = Image.Type.Sliced;
        fill.color = new Color(0.98f, 0.79f, 0.36f, 0.34f);
        fill.raycastTarget = false;
        UITheme.Stretch(fill.rectTransform, 0f, 0f, 0f, 0f);

        RectTransform handleArea = UITheme.Rect(sliderRoot, "HandleSlideArea");
        UITheme.Stretch(handleArea, 32f, 32f, 0f, 0f);

        // Large striker knob: the same turned gold/ivory/red piece as the board.
        Image handle = UITheme.Icon(handleArea, "Handle", UITheme.StrikerIcon(), 104f);
        UITheme.SetRect(handle.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 1f),
            new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(104f, 0f));
        handle.raycastTarget = true;

        // Small brass screws that finish the rail ends.
        Image leftCap = UITheme.Circle(track.transform, "LeftCap",
            new Color(0.90f, 0.72f, 0.34f, 0.90f), 3, new Color(0.36f, 0.24f, 0.10f));
        UITheme.SetRect(leftCap.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(24f, 0f), new Vector2(12f, 12f));

        Image rightCap = UITheme.Circle(track.transform, "RightCap",
            new Color(0.90f, 0.72f, 0.34f, 0.90f), 3, new Color(0.36f, 0.24f, 0.10f));
        UITheme.SetRect(rightCap.rectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(-24f, 0f), new Vector2(12f, 12f));

        slider.fillRect = fill.rectTransform;
        slider.handleRect = handle.rectTransform;
        slider.targetGraphic = handle;
        slider.onValueChanged.AddListener(HandlePlacementChanged);
        placementSlider = slider;

        // --- Power meter, shown only while pulling a shot ---
        powerRoot = UITheme.Rect(parent, "PowerRoot").gameObject;
        RectTransform powerRootRt = powerRoot.GetComponent<RectTransform>();
        UITheme.SetRect(powerRootRt, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f), new Vector2(0f, 226f), new Vector2(900f, 52f));

        Text powerLabel = UITheme.Label(powerRoot.transform, "PowerLabel", "POWER", 20,
            TextAnchor.MiddleLeft, new Color(0.86f, 0.78f, 0.62f), FontStyle.Bold);
        UITheme.SetRect(powerLabel.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 1f),
            new Vector2(0f, 0.5f), Vector2.zero, new Vector2(110f, 0f));

        Image powerTrack = UITheme.Panel(powerRoot.transform, "Track",
            new Color(0.08f, 0.055f, 0.035f, 0.95f), 10, true, 0.3f);
        UITheme.SetRect(powerTrack.rectTransform, new Vector2(0f, 0.5f), new Vector2(1f, 0.5f),
            new Vector2(0f, 0.5f), new Vector2(112f, 0f), new Vector2(-118f, 30f));

        powerFill = UITheme.Rect(powerTrack.transform, "Fill").gameObject.AddComponent<Image>();
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

        Image hintPanel = UITheme.Panel(parent, "HintPanel",
            new Color(0.10f, 0.07f, 0.045f, 0.90f), 28);
        UITheme.SetRect(hintPanel.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f), new Vector2(0f, 112f), new Vector2(990f, 58f));

        hintText = UITheme.Label(hintPanel.transform, "Hint",
            "Pull back to aim  ·  pinch or tap +/− to zoom", 22,
            TextAnchor.MiddleCenter, new Color(0.94f, 0.88f, 0.76f), FontStyle.Normal);
        UITheme.Stretch(hintText.rectTransform, 16f, 16f, 0f, 0f);

        remainingCoinsText = UITheme.Label(parent, "RemainingCoins", "18 COINS ON BOARD", 20,
            TextAnchor.MiddleCenter, new Color(0.72f, 0.66f, 0.56f, 0.85f), FontStyle.Normal);
        UITheme.SetRect(remainingCoinsText.rectTransform, new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 62f),
            new Vector2(700f, 30f));
    }

    private void HandlePlacementChanged(float value)
    {
        if (syncingPlacement) return;

        if (striker == null)
            striker = FindObjectOfType<StrikerController>();
        if (striker == null) return;

        if (turnManager != null)
        {
            if (turnManager.IsShotInProgress()) return;
            if (turnManager.aiControlsPlayer2 && turnManager.GetCurrentPlayer() == 2) return;
        }

        striker.PlaceOnBaseline(value * striker.BaselineLimit);
    }

    private void UpdatePlacementSlider()
    {
        if (placementSlider == null || striker == null) return;

        bool canPlace = true;
        if (turnManager != null)
        {
            if (turnManager.IsShotInProgress()) canPlace = false;
            if (turnManager.aiControlsPlayer2 && turnManager.GetCurrentPlayer() == 2)
                canPlace = false;
        }

        placementSlider.interactable = canPlace;

        if (!canPlace || striker.IsAiming) return;

        float limit = Mathf.Max(0.0001f, striker.BaselineLimit);
        float value = Mathf.Clamp(striker.transform.position.x / limit, -1f, 1f);
        syncingPlacement = true;
        placementSlider.SetValueWithoutNotify(value);
        syncingPlacement = false;
    }

    private void Update()
    {
        if (striker == null)
            striker = FindObjectOfType<StrikerController>();

        UpdatePlacementSlider();

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

        if (player1Label != null)
            player1Label.text = "YOU";
        if (player2Label != null)
            player2Label.text = localMode ? "PLAYER 2" : "ROOKIE";

        if (player1Label != null && scoreManager != null)
            player1Label.text = "YOU";

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
            player1Card.sprite = UITheme.Rounded(28, new Color(0.105f, 0.058f, 0.038f, 0.92f),
                new Color(UITheme.Gold.r, UITheme.Gold.g, UITheme.Gold.b, current == 1 ? 0.85f : 0.22f), 3);
        if (player2Card != null)
            player2Card.sprite = UITheme.Rounded(28, new Color(0.105f, 0.058f, 0.038f, 0.92f),
                new Color(0.85f, 0.82f, 0.75f, current == 2 ? 0.75f : 0.22f), 3);

        if (player1AvatarRing != null)
            player1AvatarRing.canvasRenderer.SetAlpha(current == 1 ? 1f : 0.60f);
        if (player2AvatarRing != null)
            player2AvatarRing.canvasRenderer.SetAlpha(current == 2 ? 1f : 0.60f);

        float p1Fill = Mathf.Clamp01(p1Score / 9f);
        float p2Fill = Mathf.Clamp01(p2Score / 9f);
        if (player1Progress != null) player1Progress.fillAmount = p1Fill;
        if (player2Progress != null) player2Progress.fillAmount = p2Fill;

        if (turnIndicator != null)
            turnIndicator.color = current == 1 ? Color.white : new Color(0.92f, 0.92f, 0.92f, 1f);

        if (hintText != null)
        {
            bool over = scoreManager != null && scoreManager.IsGameEnded;
            hintText.gameObject.SetActive(!over);
            if (!over)
                hintText.text = aiTurn
                    ? "Opponent is lining up a shot..."
                    : "Pull back to aim  ·  pinch or tap +/− to zoom";
        }

        lastP1Score = p1Score;
        lastP2Score = p2Score;
    }

    private void ShowToast(string who, int score, int previousScore)
    {
        if (toastText == null) return;
        if (score <= previousScore) return;

        ShowToastText(who + "  +1", UITheme.Gold);
    }

    private void ShowTurnToast(int player)
    {
        string label = player == 1
            ? "YOUR TURN"
            : (turnManager != null && turnManager.aiControlsPlayer2
                ? "ROOKIE'S TURN"
                : "PLAYER 2'S TURN");

        Color color = player == 1 ? UITheme.Gold : new Color(0.94f, 0.88f, 0.76f);
        ShowToastText(label, color);
    }

    private void ShowToastText(string message, Color color)
    {
        if (toastText == null) return;

        if (toastRoutine != null) StopCoroutine(toastRoutine);
        toastRoutine = StartCoroutine(ToastRoutine(message, color));
    }

    private IEnumerator ToastRoutine(string message, Color baseColor)
    {
        if (toastRoot == null) yield break;

        toastText.text = message;
        toastText.color = baseColor;
        toastRoot.SetActive(true);

        RectTransform rect = toastRoot.GetComponent<RectTransform>();
        Vector2 start = rect.anchoredPosition;

        // Quick fade in, a readable hold, then a short drift-up fade out.
        float t = 0f;
        const float inTime = 0.14f;
        while (t < inTime)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / inTime);
            toastGroup.alpha = p;
            rect.anchoredPosition = start + new Vector2(0f, Mathf.Lerp(-14f, 0f, p));
            yield return null;
        }

        toastGroup.alpha = 1f;
        rect.anchoredPosition = start;

        yield return new WaitForSeconds(1.05f);

        float elapsed = 0f;
        const float outTime = 0.42f;
        while (elapsed < outTime)
        {
            elapsed += Time.deltaTime;
            float p = Mathf.Clamp01(elapsed / outTime);
            toastGroup.alpha = 1f - p;
            rect.anchoredPosition = start + new Vector2(0f, p * 26f);
            yield return null;
        }

        toastRoot.SetActive(false);
        toastGroup.alpha = 0f;
        rect.anchoredPosition = start;
        toastRoutine = null;
    }
}
