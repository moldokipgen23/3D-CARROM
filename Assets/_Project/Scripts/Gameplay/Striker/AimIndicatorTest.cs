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

        Debug.Log("AimIndicator found and configured");
    }
}
