using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private Button _playVsAIButton;
    [SerializeField] private Button _playLocalButton;

    private void Start()
    {
        SetupCamera();
        BuildUI();

        if (_playVsAIButton != null)
            _playVsAIButton.onClick.AddListener(OnPlayVsAI);

        if (_playLocalButton != null)
            _playLocalButton.onClick.AddListener(OnPlayLocal);
    }

    private void SetupCamera()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.06f, 0.045f, 0.03f, 1f);
    }

    private void BuildUI()
    {
        if (_playVsAIButton != null && _playLocalButton != null)
            return;

        RuntimeUI.EnsureEventSystem();
        Canvas canvas = RuntimeUI.CreateCanvas("MainMenuCanvas", 0);

        Image backdrop = RuntimeUI.CreatePanel(canvas.transform, "Backdrop",
            new Color(0.06f, 0.045f, 0.03f, 1f));
        RuntimeUI.Stretch(backdrop.rectTransform, 0f, 0f, 0f, 0f);

        Image glow = RuntimeUI.CreatePanel(canvas.transform, "Glow",
            new Color(0.20f, 0.13f, 0.05f, 0.65f));
        RuntimeUI.SetRect(glow.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(0f, 120f), new Vector2(900f, 900f));

        Text title = RuntimeUI.CreateText(canvas.transform, "Title", "CARROM 3D", 96,
            TextAnchor.MiddleCenter, RuntimeUI.AccentGold, FontStyle.Bold);
        RuntimeUI.SetRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f), new Vector2(0f, -190f), new Vector2(1400f, 130f));

        Text subtitle = RuntimeUI.CreateText(canvas.transform, "Subtitle",
            "Classic carrom, one pocket at a time", 30,
            TextAnchor.MiddleCenter, new Color(0.9f, 0.86f, 0.78f));
        RuntimeUI.SetRect(subtitle.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f), new Vector2(0f, -300f), new Vector2(1200f, 50f));

        _playVsAIButton = RuntimeUI.CreateButton(canvas.transform, "PlayVsAIButton",
            "PLAY vs AI", RuntimeUI.ButtonGreen, RuntimeUI.TextLight);
        RuntimeUI.SetRect(_playVsAIButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -20f),
            new Vector2(620f, 110f));

        _playLocalButton = RuntimeUI.CreateButton(canvas.transform, "PlayLocalButton",
            "PLAY LOCAL", RuntimeUI.ButtonBrown, RuntimeUI.TextLight);
        RuntimeUI.SetRect(_playLocalButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -160f),
            new Vector2(620f, 110f));

        Button quitButton = RuntimeUI.CreateButton(canvas.transform, "QuitButton",
            "QUIT", RuntimeUI.ButtonRed, RuntimeUI.TextLight);
        RuntimeUI.SetRect(quitButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -300f),
            new Vector2(620f, 110f));
        quitButton.onClick.AddListener(OnQuit);

        Text footer = RuntimeUI.CreateText(canvas.transform, "Footer",
            "Pocket all 9 of your coins and cover the queen to win", 22,
            TextAnchor.MiddleCenter, new Color(0.7f, 0.66f, 0.58f));
        RuntimeUI.SetRect(footer.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f), new Vector2(0f, 70f), new Vector2(1200f, 40f));
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
