using UnityEngine;

public class PocketTest : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("Pocket test starting...");

        GameObject pocket = new GameObject("TestPocket");
        SphereCollider sphereCollider = pocket.AddComponent<SphereCollider>();
        sphereCollider.isTrigger = true;
        sphereCollider.radius = 0.05f;

        PocketTrigger pocketTrigger = pocket.AddComponent<PocketTrigger>();
        pocketTrigger.pocketRadius = 0.05f;
        pocketTrigger.coinLayer = LayerMask.GetMask("Coin");

        pocketTrigger.OnCoinPocketed += (coinType, coin) =>
        {
            Debug.Log($"PocketTest: Coin {coinType} was pocketed in test pocket");
        };

        Debug.Log("Pocket test setup complete");
        Debug.Log("Test pocket created with radius 0.05f and Coin layer");
    }
}