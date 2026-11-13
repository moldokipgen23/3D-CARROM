using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private Button _playVsAIButton;
    [SerializeField] private Button _playLocalButton;

    private Text _coinBalance;
    private Button _soundButton;
    private Text _soundLabel;
    private Canvas canvas;

    private void Start()
    {
        EnsureBackdrop();
        SetupCamera();
        BuildUI();

        if (_playVsAIButton != null)
            _playVsAIButton.onClick.AddListener(OnPlayVsAI);

        if (_playLocalButton != null)
            _playLocalButton.onClick.AddListener(OnPlayLocal);
    }

    private void EnsureBackdrop()
    {
        if (GetComponent<MenuBackdrop>() == null && FindObjectOfType<MenuBackdrop>() == null)
            gameObject.AddComponent<MenuBackdrop>();
    }

    private void SetupCamera()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.07f, 0.05f, 0.035f, 1f);
    }

    private void BuildUI()
    {
        if (_playVsAIButton != null && _playLocalButton != null)
            return;

        RuntimeUI.EnsureEventSystem();
        canvas = RuntimeUI.CreateCanvas("MainMenuCanvas", 10);

        RectTransform safe = UITheme.Rect(canvas.transform, "SafeArea");
        UITheme.ApplySafeArea(safe);

        // --- readability overlays over the 3D hero board -------------------
        Image topFade = UITheme.Gradient(safe, "TopFade", new Color(0.05f, 0.035f, 0.025f),
            new Color(0.05f, 0.035f, 0.025f), 0.92f, 0f);
        UITheme.SetRect(topFade.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(0.5f, 1f), Vector2.zero, new Vector2(0f, 760f));

        Image bottomFade = UITheme.Gradient(safe, "BottomFade", new Color(0.06f, 0.042f, 0.030f),
            new Color(0.06f, 0.042f, 0.030f), 0f, 0.80f);
        UITheme.SetRect(bottomFade.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f),
            new Vector2(0.5f, 0f), Vector2.zero, new Vector2(0f, 760f));

        Image vignette = UITheme.Rect(safe, "Vignette").gameObject.AddComponent<Image>();
        vignette.sprite = UITheme.Vignette();
        vignette.color = Color.white;
        vignette.raycastTarget = false;
        UITheme.Stretch(vignette.rectTransform, -80f, -80f, -80f, -80f);

        // --- top bar -------------------------------------------------------
        BuildProfileChip(safe);
        BuildWalletChip(safe);
        BuildSoundToggle(safe);

        // --- logo ----------------------------------------------------------
        RectTransform logoGroup = UITheme.Rect(safe, "LogoGroup");
        UITheme.SetRect(logoGroup, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f), new Vector2(0f, -250f), new Vector2(1000f, 320f));

        Image sparkleL = UITheme.Icon(logoGroup, "SparkleL", UITheme.Sparkle(UITheme.Gold), 54f);
        UITheme.SetRect(sparkleL.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(-330f, 66f), new Vector2(54f, 54f));

        Image sparkleR = UITheme.Icon(logoGroup, "SparkleR", UITheme.Sparkle(UITheme.Gold), 34f);
        UITheme.SetRect(sparkleR.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(322f, 0f), new Vector2(34f, 34f));

        Text titleShadow = UITheme.Label(logoGroup, "TitleShadow", "CARROM <color=#F7EDD6>3D</color>", 132,
            TextAnchor.MiddleCenter, new Color(0.10f, 0.05f, 0.02f, 0.85f), FontStyle.Bold, false);
        UITheme.SetRect(titleShadow.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(6f, 2f), new Vector2(1000f, 180f));

        Text title = UITheme.Label(logoGroup, "Title", "CARROM <color=#F7EDD6>3D</color>", 132,
            TextAnchor.MiddleCenter, UITheme.Gold, FontStyle.Bold, false);
        UITheme.SetRect(title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1000f, 180f));

        Outline titleOutline = title.gameObject.AddComponent<Outline>();
        titleOutline.effectColor = new Color(0.26f, 0.12f, 0.03f, 0.9f);
        titleOutline.effectDistance = new Vector2(2.5f, -2.5f);

        Image ruleL = UITheme.Panel(logoGroup, "RuleL", new Color(0.95f, 0.76f, 0.34f, 0.45f), 2);
        UITheme.SetRect(ruleL.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f), new Vector2(-150f, 86f), new Vector2(210f, 3f));

        Image ruleR = UITheme.Panel(logoGroup, "RuleR", new Color(0.95f, 0.76f, 0.34f, 0.45f), 2);
        UITheme.SetRect(ruleR.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f), new Vector2(150f, 86f), new Vector2(210f, 3f));

        Image diamond = UITheme.Icon(logoGroup, "Diamond", UITheme.Sparkle(UITheme.Gold), 28f);
        UITheme.SetRect(diamond.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f), new Vector2(0f, 86f), new Vector2(28f, 28f));

        Text subtitle = UITheme.Label(logoGroup, "Subtitle", "C L A S S I C   B O A R D   G A M E", 26,
            TextAnchor.MiddleCenter, new Color(0.93f, 0.86f, 0.72f, 0.9f), FontStyle.Normal);
        UITheme.SetRect(subtitle.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f), new Vector2(0f, 36f), new Vector2(900f, 40f));

        // --- actions -------------------------------------------------------
        Text modeLabel = UITheme.Label(safe, "ModeLabel", "QUICK MATCH  ·  VS AI", 25,
            TextAnchor.MiddleCenter, new Color(0.88f, 0.80f, 0.66f, 0.85f), FontStyle.Normal);
        UITheme.SetRect(modeLabel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(0f, -262f), new Vector2(800f, 40f));

        _playVsAIButton = UITheme.Button(safe, "PlayButton", "PLAY", UITheme.Gold, UITheme.Ink, 62, 48);
        UITheme.SetRect(_playVsAIButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -360f),
            new Vector2(720f, 152f));

        _playLocalButton = UITheme.Button(safe, "LocalButton", "2 PLAYERS", UITheme.PanelSoft,
            UITheme.Cream, 34, 40);
        UITheme.SetRect(_playLocalButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-190f, -520f),
            new Vector2(340f, 112f));

        Button quitButton = UITheme.Button(safe, "QuitButton", "QUIT", UITheme.PanelSoft,
            new Color(0.95f, 0.82f, 0.72f), 32, 40, true);
        UITheme.SetRect(quitButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(190f, -520f),
            new Vector2(340f, 112f));
        quitButton.onClick.AddListener(OnQuit);

        Text footer = UITheme.Label(safe, "Footer",
            "Pocket all 9 of your coins · Cover the queen", 22,
            TextAnchor.MiddleCenter, new Color(0.72f, 0.66f, 0.56f, 0.8f), FontStyle.Normal);
        UITheme.SetRect(footer.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f), new Vector2(0f, 46f), new Vector2(1000f, 36f));
    }

    private void BuildProfileChip(Transform parent)
    {
        Image chip = UITheme.Panel(parent, "ProfileChip", UITheme.PanelBg, 34, true);
        UITheme.SetRect(chip.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(0f, 1f), new Vector2(28f, -28f), new Vector2(430f, 118f));

        Image avatarRing = UITheme.Circle(chip.transform, "AvatarRing", UITheme.Gold);
        UITheme.SetRect(avatarRing.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
            new Vector2(0f, 0.5f), new Vector2(16f, 0f), new Vector2(88f, 88f));

        Image avatar = UITheme.Circle(chip.transform, "Avatar", new Color(0.20f, 0.33f, 0.52f));
        UITheme.SetRect(avatar.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
            new Vector2(0f, 0.5f), new Vector2(22f, 0f), new Vector2(76f, 76f));

        Text initial = UITheme.Label(avatar.transform, "Initial", "P", 40,
            TextAnchor.MiddleCenter, UITheme.Cream, FontStyle.Bold);
        UITheme.Stretch(initial.rectTransform, 0f, 0f, 0f, 0f);

        Text name = UITheme.Label(chip.transform, "Name", "PLAYER ONE", 28,
            TextAnchor.MiddleLeft, UITheme.Cream, FontStyle.Bold);
        UITheme.SetRect(name.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(0f, 1f), new Vector2(120f, -20f), new Vector2(-10f, 36f));

        Image levelChip = UITheme.Panel(chip.transform, "LevelChip",
            new Color(0.95f, 0.76f, 0.34f, 0.16f), 16);
        UITheme.SetRect(levelChip.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f),
            new Vector2(0f, 0f), new Vector2(120f, 19f), new Vector2(150f, 40f));

        Text level = UITheme.Label(levelChip.transform, "LevelText", "LEVEL 12", 21,
            TextAnchor.MiddleCenter, UITheme.Gold, FontStyle.Bold);
        UITheme.Stretch(level.rectTransform, 0f, 0f, 0f, 0f);
    }

    private void BuildWalletChip(Transform parent)
    {
        Image chip = UITheme.Panel(parent, "WalletChip", UITheme.PanelBg, 34, true);
        UITheme.SetRect(chip.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(1f, 1f), new Vector2(-28f, -28f), new Vector2(250f, 78f));

        Image coin = UITheme.Icon(chip.transform, "CoinIcon", UITheme.CoinIcon(true), 48f);
        UITheme.SetRect(coin.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
            new Vector2(0f, 0.5f), new Vector2(18f, 0f), new Vector2(48f, 48f));

        _coinBalance = UITheme.Label(chip.transform, "Balance", "2,450", 30,
            TextAnchor.MiddleLeft, UITheme.Cream, FontStyle.Bold);
        UITheme.SetRect(_coinBalance.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f),
            new Vector2(0f, 0.5f), new Vector2(78f, 0f), new Vector2(-90f, 48f));
    }

    private void BuildSoundToggle(Transform parent)
    {
        _soundButton = UITheme.Button(parent, "SoundButton", "SOUND ON",
            new Color(0.16f, 0.11f, 0.07f, 0.92f), UITheme.Cream, 22, 28, true);
        UITheme.SetRect(_soundButton.GetComponent<RectTransform>(), new Vector2(1f, 1f),
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-28f, -120f),
            new Vector2(250f, 62f));

        _soundLabel = _soundButton.GetComponentInChildren<Text>();
        _soundButton.onClick.AddListener(OnSoundToggle);
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
        if (_soundLabel == null) return;

        bool muted = AudioManager.Instance != null && AudioManager.Instance.IsMuted;
        _soundLabel.text = muted ? "SOUND OFF" : "SOUND ON";
        _soundLabel.color = muted
            ? new Color(0.74f, 0.67f, 0.57f)
            : UITheme.Cream;
    }

    private void OnPlayVsAI()
    {
        Debug.Log("Starting game vs AI");
        PlayerPrefs.SetInt("carrom_local_multiplayer", 0);
        PlayerPrefs.Save();
        SceneFlow.LoadScene(SceneFlow.GAME);
    }

    private void OnPlayLocal()
    {
        Debug.Log("Starting local 2-player game");
        PlayerPrefs.SetInt("carrom_local_multiplayer", 1);
        PlayerPrefs.Save();
        SceneFlow.LoadScene(SceneFlow.GAME);
    }

    private void OnQuit()
    {
#if UNITY_EDITOR
        Debug.Log("Quit requested (ignored in Editor)");
#else
        Application.Quit();
#endif
    }
}
