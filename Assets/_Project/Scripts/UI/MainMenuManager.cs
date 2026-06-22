using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private Button _playVsAIButton;
    [SerializeField] private Button _playLocalButton;

    private void Start()
    {
        if (_playVsAIButton != null)
            _playVsAIButton.onClick.AddListener(() => SceneFlow.LoadScene(SceneFlow.GAME));

        if (_playLocalButton != null)
            _playLocalButton.onClick.AddListener(() => SceneFlow.LoadScene(SceneFlow.GAME));

        // Fallback: if no buttons wired in Inspector, auto-load after 2s so scene is not stuck
        if (_playVsAIButton == null && _playLocalButton == null)
        {
            Debug.LogWarning("MainMenuManager: No buttons assigned in Inspector. Add a Play button and assign it. Auto-loading in 2s as fallback.");
            Invoke(nameof(AutoLoad), 2f);
        }
    }

    private void AutoLoad()
    {
        SceneFlow.LoadScene(SceneFlow.GAME);
    }
}
