using UnityEngine;

public enum GameState
{
    Boot,
    MainMenu,
    InGame,
    Results
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public GameState CurrentState { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log($"GameManager InstanceID: {GetInstanceID()}");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetState(GameState newState)
    {
        CurrentState = newState;
        Debug.Log($"GameState changed to: {newState}");
    }
}