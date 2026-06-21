using UnityEngine;

public class CarromCameraControllerTest : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("CarromCameraController test starting...");

        CarromCameraController cameraController = GetComponent<CarromCameraController>();
        if (cameraController == null)
        {
            Debug.LogError("CarromCameraController not found on this GameObject");
            return;
        }

        Debug.Log("CarromCameraController found");
        Debug.Log($"Camera distance: {cameraController.cameraDistance}");
        Debug.Log($"Camera height: {cameraController.cameraHeight}");
        Debug.Log($"Camera angle: {cameraController.cameraAngle} degrees");
        Debug.Log($"Smooth time: {cameraController.smoothTime}");

        if (cameraController.strikerTarget != null)
        {
            Debug.Log("Striker target reference set");
        }
        else
        {
            Debug.LogWarning("No striker target reference set");
        }

        Debug.Log("CarromCameraController test setup complete");
    }
}