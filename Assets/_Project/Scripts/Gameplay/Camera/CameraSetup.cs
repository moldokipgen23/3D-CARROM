using UnityEngine;

public class CameraSetup : MonoBehaviour
{
    [Header("Camera Position")]
    // Portrait phone: camera must be high and pulled back so the full board fits on screen.
    // Y=4.5 height, Z=-1.2 pull-back gives ~55 degrees tilt and shows the full 2.9x2.9 board.
    public Vector3 cameraPosition = new Vector3(0f, 4.5f, -1.2f);
    public Vector3 lookAtOffset = new Vector3(0f, 0f, 0.3f);

    [Header("Camera Settings")]
    public float fieldOfView = 60f;
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
