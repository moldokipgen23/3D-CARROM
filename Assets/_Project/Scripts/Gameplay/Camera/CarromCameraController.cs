using UnityEngine;

public class CarromCameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    public float cameraDistance = 5f;
    public float cameraHeight = 2f;
    public float cameraAngle = 55f;
    public float smoothTime = 0.3f;

    [Header("Target References")]
    public Transform strikerTarget;

    [Header("Camera Collision")]
    public LayerMask collisionLayer;
    public float collisionOffset = 0.1f;

    private Vector3 currentVelocity;
    private Vector3 targetPosition;
    private bool isInitialized = false;

    private void Start()
    {
        InitializeCamera();
    }

    private void LateUpdate()
    {
        if (!isInitialized) return;

        UpdateTargetPosition();
        UpdateCameraPosition();
    }

    private void InitializeCamera()
    {
        if (strikerTarget == null)
        {
            strikerTarget = GameObject.FindWithTag("Striker").transform;
        }

        if (strikerTarget != null)
        {
            isInitialized = true;
            Debug.Log("CarromCameraController initialized");
        }
        else
        {
            Debug.LogError("Striker target not found for camera initialization");
        }
    }

    private void UpdateTargetPosition()
    {
        if (strikerTarget != null)
        {
            targetPosition = strikerTarget.position + Vector3.up * cameraHeight;
        }
    }

    private void UpdateCameraPosition()
    {
        Vector3 desiredPosition = CalculateDesiredPosition();
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, smoothTime);
        transform.LookAt(strikerTarget.position + Vector3.up * 0.5f);
    }

    private Vector3 CalculateDesiredPosition()
    {
        Vector3 desiredPosition = strikerTarget.position - transform.forward * cameraDistance;
        desiredPosition.y = strikerTarget.position.y + cameraHeight;

        RaycastHit hit;
        if (Physics.Raycast(strikerTarget.position, transform.forward, out hit, cameraDistance, collisionLayer))
        {
            desiredPosition = hit.point + transform.forward * collisionOffset;
            desiredPosition.y = strikerTarget.position.y + cameraHeight;
        }

        return desiredPosition;
    }

    public void SetCameraAngle(float angle)
    {
        cameraAngle = angle;
        Debug.Log($"Camera angle set to: {angle} degrees");
    }
}