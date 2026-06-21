using UnityEngine;

public class AimIndicatorTest : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("AimIndicator test starting...");

        AimIndicator aimIndicator = GetComponent<AimIndicator>();
        if (aimIndicator == null)
        {
            Debug.LogError("AimIndicator not found on this GameObject");
            return;
        }

        Debug.Log("AimIndicator found");
        Debug.Log($"Line renderer enabled: {aimIndicator.lineRenderer.enabled}");
        Debug.Log($"Line width: {aimIndicator.lineWidth}");
        Debug.Log($"Aim line color: {aimIndicator.aimLineColor}");
        Debug.Log($"Power line color: {aimIndicator.powerLineColor}");

        if (aimIndicator.strikerController != null)
        {
            Debug.Log("StrikerController reference set");
        }
        else
        {
            Debug.LogWarning("No StrikerController reference set");
        }

        Debug.Log("AimIndicator test setup complete");
    }
}