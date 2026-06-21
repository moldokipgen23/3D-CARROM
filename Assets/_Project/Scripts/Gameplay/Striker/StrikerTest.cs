using UnityEngine;

public class StrikerTest : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("Striker test starting...");

        GameObject striker = GameObject.FindWithTag("Striker");
        if (striker == null)
        {
            Debug.LogError("Striker not found in scene");
            return;
        }

        StrikerController strikerController = striker.GetComponent<StrikerController>();
        if (strikerController == null)
        {
            Debug.LogError("StrikerController not found on striker");
            return;
        }

        Debug.Log("StrikerController found");
        Debug.Log($"Striker mass: {strikerController.strikerMass}");
        Debug.Log($"Max drag distance: {strikerController.maxDragDistance}");
        Debug.Log($"Force range: {strikerController.minForce} - {strikerController.maxForce}");
        Debug.Log($"Baseline width: {strikerController.baselineWidth}");

        Rigidbody rb = strikerController.Rigidbody;
        if (rb != null)
        {
            Debug.Log($"Rigidbody mass: {rb.mass}");
            Debug.Log($"Rigidbody useGravity: {rb.useGravity}");
            Debug.Log($"Rigidbody constraints: {rb.constraints}");
        }

        strikerController.OnShotFired += OnShotFired;

        Debug.Log("Striker test setup complete");
    }

    private void OnShotFired(Vector2 direction, float power)
    {
        Debug.Log($"StrikerTest: Shot fired - Direction: {direction}, Power: {power:F1}");
    }
}