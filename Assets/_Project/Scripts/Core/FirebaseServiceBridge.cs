using UnityEngine;

public class FirebaseServiceBridge : MonoBehaviour
{
    public FirebaseService Service { get; private set; }

    private void Awake()
    {
        Service = new FirebaseService();
    }
}
