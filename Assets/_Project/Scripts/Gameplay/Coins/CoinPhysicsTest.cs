using UnityEngine;

public class CoinPhysicsTest : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("Coin physics test starting...");

        GameObject[] coins = GameObject.FindGameObjectsWithTag("Coin");
        if (coins.Length == 0)
        {
            Debug.LogError("No coins found for physics test");
            return;
        }

        GameObject testCoin = coins[0];
        Rigidbody rb = testCoin.GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogError("Test coin has no Rigidbody");
            return;
        }

        Debug.Log($"Test coin physics properties:");
        Debug.Log($"  Mass: {rb.mass}");
        Debug.Log($"  UseGravity: {rb.useGravity}");
        Debug.Log($"  Constraints: {rb.constraints}");
        Debug.Log($"  Velocity: {rb.velocity}");

        PhysicsMaterial material = testCoin.GetComponent<Collider>()?.sharedMaterial;
        if (material != null)
        {
            Debug.Log($"  Physics material properties:");
            Debug.Log($"    Dynamic Friction: {material.dynamicFriction}");
            Debug.Log($"    Static Friction: {material.staticFriction}");
            Debug.Log($"    Bounciness: {material.bounciness}");
            Debug.Log($"    Friction Combine: {material.frictionCombine}");
            Debug.Log($"    Bounce Combine: {material.bounceCombine}");
        }

        ApplyTestForce(testCoin);
    }

    private void ApplyTestForce(GameObject coin)
    {
        Rigidbody rb = coin.GetComponent<Rigidbody>();
        Vector3 forceDirection = new Vector3(1, 0, 0);
        float forceMagnitude = 2f;

        rb.AddForce(forceDirection * forceMagnitude, ForceMode.Impulse);
        Debug.Log($"Applied test force: {forceDirection} with magnitude {forceMagnitude}");

        Debug.Log("Physics test setup complete. Watch for collision propagation.");
    }
}