using UnityEngine;
using UnityEngine.UI;

public class BootSceneInitializer : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("Boot scene initializing...");
        
        // Initialize Firebase
        var firebaseServiceObj = new GameObject("FirebaseService");
        var firebaseService = firebaseServiceObj.AddComponent<FirebaseServiceBridge>();
        ServiceLocator.Register<FirebaseService>(firebaseService.Service);
        
        // Start Firebase initialization
        StartCoroutine(InitializeFirebaseAsync());
        
        // Load MainMenu after Firebase is ready
        StartCoroutine(LoadMainMenuAfterDelay());
    }
    
    private System.Collections.IEnumerator InitializeFirebaseAsync()
    {
        var firebaseService = ServiceLocator.Get<FirebaseService>();
        if (firebaseService != null)
        {
            yield return firebaseService.InitializeAsync();
            if (firebaseService.IsInitialized)
            {
                Debug.Log("Firebase initialized successfully");
            }
            else
            {
                Debug.LogError("Failed to initialize Firebase");
            }
        }
    }
    
    private System.Collections.IEnumerator LoadMainMenuAfterDelay()
    {
        yield return new WaitForSeconds(2f);
        SceneFlow.LoadScene(SceneFlow.MAIN_MENU);
    }
}