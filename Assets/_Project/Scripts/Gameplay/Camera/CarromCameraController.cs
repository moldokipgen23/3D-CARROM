using UnityEngine;

public class CarromCameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    public float cameraDistance = 7.2f;
    public float cameraHeight = 2f;
    public float cameraAngle = 55f;
    public float smoothTime = 0.3f;

    [Header("Target References")]
    public Transform strikerTarget;

    [Header("Camera Collision")]
    public LayerMask collisionLayer;
    public float collisionOffset = 0.1f;

    [Header("Framing")]
    public Vector3 boardCenter = Vector3.zero;
    public float fieldOfView = 58f;

    private Camera cam;
    private bool initialized = false;

    private void Start()
    {
        SetupCamera();
    }

    private void SetupCamera()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
            cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("CarromCameraController: no camera found");
            return;
        }

        // Null-safe lookup - the old code called .transform on a null result.
        if (strikerTarget == null)
        {
            GameObject striker = GameObject.FindWithTag("Striker");
            if (striker != null)
                strikerTarget = striker.transform;
        }

        float angleRad = cameraAngle * Mathf.Deg2Rad;
        float distance = Mathf.Max(4f, cameraDistance);

        // Orbit direction: straight behind the board, tilted down by cameraAngle.
        Vector3 direction = new Vector3(0f, Mathf.Sin(angleRad), -Mathf.Cos(angleRad));
        Vector3 target = boardCenter + Vector3.up * 0.15f;

        cam.transform.position = boardCenter + direction * distance;
        cam.transform.LookAt(target);
        cam.fieldOfView = fieldOfView;
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 100f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.07f, 0.05f, 0.035f, 1f);
        cam.allowHDR = true;
        cam.allowMSAA = true;

        if (GetComponent<AudioListener>() == null)
        {
            if (FindObjectOfType<AudioListener>() == null)
                gameObject.AddComponent<AudioListener>();
        }

        initialized = true;
    }

    public void Recenter()
    {
        SetupCamera();
    }

    private void LateUpdate()
    {
        // The camera is intentionally fixed. Following the striker caused the
        // board to drift and made aiming impossible on a phone screen.
        if (!initialized)
            SetupCamera();
    }
}
