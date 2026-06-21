using System;
using System.Threading.Tasks;
using UnityEngine;

#if FIREBASE_SDK
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Crashlytics;
using Firebase.RemoteConfig;
#endif

public class FirebaseService : IService
{
#if FIREBASE_SDK
    private FirebaseAuth _auth;
    private FirestoreDb _firestoreDb;
#endif
    private bool _isInitialized;

    public bool IsInitialized => _isInitialized;

    public async Task<bool> InitializeAsync()
    {
        try
        {
            Debug.Log("Initializing Firebase...");

#if FIREBASE_SDK
            var firebaseApp = FirebaseApp.DefaultInstance;
            _auth = FirebaseAuth.DefaultInstance;
            _firestoreDb = FirestoreDb.DefaultInstance;
            await SetUpCrashlytics();
            await SetUpRemoteConfig();
#else
            Debug.LogWarning("Firebase SDK not imported. Running in stub mode.");
            await Task.Delay(100);
#endif

            _isInitialized = true;
            Debug.Log("Firebase initialized successfully");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Firebase initialization failed: {ex.Message}");
            _isInitialized = false;
            return false;
        }
    }

#if FIREBASE_SDK
    private async Task SetUpCrashlytics()
    {
        try
        {
            Crashlytics.Log("Application started");
            Crashlytics.SetCustomKey("game_version", "1.0.0");
            Crashlytics.SetCustomKey("platform", Application.platform.ToString());
            Debug.Log("Crashlytics configured");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to setup Crashlytics: {ex.Message}");
        }
    }

    private async Task SetUpRemoteConfig()
    {
        try
        {
            var remoteConfigSettings = new RemoteConfigSettings()
            {
                FetchTimeoutMilliseconds = 60000,
                CacheExpirationSeconds = 3600
            };
            RemoteConfig.DefaultInstance.SetDefaults(remoteConfigSettings);
            Debug.Log("Remote Config configured");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to setup Remote Config: {ex.Message}");
        }
    }
#endif

    public async Task<string> SignInAnonymouslyAsync()
    {
        try
        {
            if (!_isInitialized)
            {
                Debug.LogError("Firebase not initialized. Call InitializeAsync first.");
                return null;
            }

#if FIREBASE_SDK
            var authTask = _auth.SignInAnonymouslyAsync();
            var authResult = await authTask;
            Debug.Log($"Signed in anonymously. User ID: {authResult.User.UserId}");
            return authResult.User.UserId;
#else
            string stubId = Guid.NewGuid().ToString();
            Debug.Log($"[Stub] Signed in anonymously. User ID: {stubId}");
            await Task.Delay(100);
            return stubId;
#endif
        }
        catch (Exception ex)
        {
            Debug.LogError($"Anonymous sign-in failed: {ex.Message}");
            return null;
        }
    }

    public async Task SavePlayerDataAsync(PlayerData data)
    {
        try
        {
            if (!_isInitialized)
            {
                Debug.LogError("Firebase not initialized. Call InitializeAsync first.");
                return;
            }

#if FIREBASE_SDK
            var documentReference = _firestoreDb.Collection("players").Document(data.PlayerId);
            await documentReference.SetAsync(data);
#else
            Debug.Log($"[Stub] Player data saved to Firestore for player: {data.PlayerId}");
            await Task.Delay(100);
#endif

            Debug.Log($"Player data saved for player: {data.PlayerId}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save player data: {ex.Message}");
        }
    }

    public async Task<PlayerData> LoadPlayerDataAsync(string playerId)
    {
        try
        {
            if (!_isInitialized)
            {
                Debug.LogError("Firebase not initialized. Call InitializeAsync first.");
                return null;
            }

#if FIREBASE_SDK
            var documentReference = _firestoreDb.Collection("players").Document(playerId);
            var snapshot = await documentReference.GetSnapshotAsync();
            if (snapshot.Exists)
            {
                var playerData = snapshot.Deserialize<PlayerData>();
                Debug.Log($"Player data loaded from Firestore for player: {playerId}");
                return playerData;
            }
            else
            {
                Debug.LogWarning($"No player data found for player: {playerId}");
                return null;
            }
#else
            Debug.Log($"[Stub] Loading player data for: {playerId}");
            await Task.Delay(100);
            return new PlayerData(playerId);
#endif
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load player data: {ex.Message}");
            return null;
        }
    }

    public void LogTestEvent()
    {
        try
        {
#if FIREBASE_SDK
            Crashlytics.Log("Test non-fatal event logged");
#else
            Debug.Log("[Stub] Test non-fatal event logged to Crashlytics");
#endif
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to log test event: {ex.Message}");
        }
    }
}
