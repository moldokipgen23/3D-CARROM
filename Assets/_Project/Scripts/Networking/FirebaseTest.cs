using UnityEngine;
using System.Threading.Tasks;

public class FirebaseTest : MonoBehaviour
{
    private async void Start()
    {
        Debug.Log("Firebase test starting...");

        var firebaseService = ServiceLocator.Get<FirebaseService>();
        if (firebaseService == null)
        {
            Debug.LogError("FirebaseService not found in ServiceLocator");
            return;
        }

        Debug.Log("FirebaseService found in ServiceLocator");

        bool initialized = await firebaseService.InitializeAsync();
        if (initialized)
        {
            Debug.Log("FirebaseService initialized successfully");
        }
        else
        {
            Debug.LogError("FirebaseService initialization failed");
        }

        string playerId = await firebaseService.SignInAnonymouslyAsync();
        if (!string.IsNullOrEmpty(playerId))
        {
            Debug.Log($"Successfully signed in anonymously. Player ID: {playerId}");

            var testPlayerData = new PlayerData("TestPlayer")
            {
                Coins = 100,
                Diamonds = 50,
                XP = 250,
                Level = 5
            };

            await firebaseService.SavePlayerDataAsync(testPlayerData);

            var loadedData = await firebaseService.LoadPlayerDataAsync(playerId);
            if (loadedData != null)
            {
                Debug.Log($"Player data loaded successfully. Coins: {loadedData.Coins}, Diamonds: {loadedData.Diamonds}");
            }
        }

        firebaseService.LogTestEvent();
    }
}