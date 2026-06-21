using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI References")]
    public Button playButton;
    public Button settingsButton;
    public Button quitButton;
    
    [Header("Panel References")]
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;
    
    private void Start()
    {
        InitializeUI();
        playButton.onClick.AddListener(OnPlayClicked);
        settingsButton.onClick.AddListener(OnSettingsClicked);
        quitButton.onClick.AddListener(OnQuitClicked);
    }
    
    private void InitializeUI()
    {
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
    }
    
    private void OnPlayClicked()
    {
        Debug.Log("Play button clicked");
        SceneFlow.LoadScene(SceneFlow.GAME);
    }
    
    private void OnSettingsClicked()
    {
        Debug.Log("Settings button clicked");
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }
    
    private void OnQuitClicked()
    {
        Debug.Log("Quit button clicked");
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}