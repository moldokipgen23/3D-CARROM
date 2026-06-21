using UnityEngine;

public class CameraSetup : MonoBehaviour
{
    [Header("Camera Position")]
    public Vector3 cameraPosition = new Vector3(0f, 3.2f, -0.3f);
    public Vector3 lookAtOffset = new Vector3(0f, 0f, 0.15f);

    [Header("Camera Settings")]
    public float fieldOfView = 42f;
    public float nearClip = 0.1f;
    public float farClip = 50f;

    [Header("Background")]
    public Color backgroundColor = new Color(0.08f, 0.06f, 0.04f);

    private void Awake()
    {
        SetupCamera();
    }

    private void SetupCamera()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            cam = camObj.AddComponent<Camera>();
        }

        cam.transform.position = cameraPosition;
        cam.transform.LookAt(lookAtOffset);

        cam.fieldOfView = fieldOfView;
        cam.nearClipPlane = nearClip;
        cam.farClipPlane = farClip;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = backgroundColor;
        cam.allowHDR = true;
        cam.allowMSAA = true;
        cam.depth = -1;

        if (cam.GetComponent<AudioListener>() == null)
            cam.gameObject.AddComponent<AudioListener>();
    }
}
