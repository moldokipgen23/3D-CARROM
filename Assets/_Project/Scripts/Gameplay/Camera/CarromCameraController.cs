using UnityEngine;

/// <summary>
/// Frames the whole carrom board on a portrait phone. The controller solves the
/// camera distance from the real board extents instead of using a hand-tuned
/// number, so the complete board (frame included) is always visible no matter
/// the screen aspect. Pinch, mouse wheel and the HUD buttons zoom in for detail.
/// </summary>
public class CarromCameraController : MonoBehaviour
{
    public static CarromCameraController Instance { get; private set; }

    [Header("Camera Settings")]
    public float cameraDistance = 8f;
    // Kept for the existing camera test / inspector compatibility.
    public float cameraHeight = 2f;
    // 90 = straight down. A top-down camera keeps the square board perfectly
    // square on screen; the pieces still read as 3D because of the lighting,
    // soft contact shadows and the bevelled meshes.
    public float cameraAngle = 90f;
    public float smoothTime = 0.3f;

    [Header("Framing")]
    public Vector3 boardCenter = new Vector3(0f, 0f, 0f);
    public float fieldOfView = 52f;
    // When enabled the camera distance is calculated from the board size.
    public bool autoFit = true;
    // Fraction of the viewport the full board (frame included) should span.
    public float fitMargin = 0.92f;
    public float lookAtHeight = 0.35f;
    // Lifts the board above the viewport centre so the HUD and the placement
    // slider get breathing room without covering the frame. Fraction of span.
    public float framingBias = 0.12f;

    private const float TopDownAngle = 89.9f;

    [Header("Zoom")]
    public float zoom = 1f;
    public float minZoom = 0.90f;
    public float maxZoom = 1.60f;
    public float zoomStep = 0.12f;
    public float zoomLerpSpeed = 10f;

    [Header("Target References")]
    public Transform strikerTarget;

    [Header("Camera Collision")]
    public LayerMask collisionLayer;
    public float collisionOffset = 0.1f;

    private Camera cam;
    private bool initialized;
    private float targetZoom = 1f;
    private float fittedDistance = 8f;
    private float pinchStartDistance;
    private float pinchStartZoom;
    private bool pinching;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        targetZoom = Mathf.Clamp(zoom, minZoom, maxZoom);
        zoom = targetZoom;
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

        if (strikerTarget == null)
        {
            GameObject striker = GameObject.FindWithTag("Striker");
            if (striker != null)
                strikerTarget = striker.transform;
        }

        cam.fieldOfView = fieldOfView;
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 120f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.210f, 0.045f, 0.040f, 1f);
        cam.allowHDR = true;
        cam.allowMSAA = true;

        if (GetComponent<AudioListener>() == null &&
            FindObjectOfType<AudioListener>() == null)
        {
            gameObject.AddComponent<AudioListener>();
        }

        fittedDistance = autoFit ? ComputeFitDistance() : cameraDistance;
        ApplyTransform();

        initialized = true;
    }

    private void ApplyTransform()
    {
        float baseDistance = autoFit ? fittedDistance : cameraDistance;
        float distance = Mathf.Max(2f, baseDistance) / Mathf.Max(0.25f, zoom);

        Vector3 target;
        Vector3 offsetDir;
        GetFraming(out target, out offsetDir);

        cam.transform.position = target + offsetDir * distance;

        if (IsTopDown)
        {
            // LookAt with the world-up vector is degenerate when the camera
            // points straight down, so set the rotation explicitly. Screen up
            // becomes world +z, putting the far baseline at the top.
            cam.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }
        else
        {
            cam.transform.LookAt(target);
        }

        cam.fieldOfView = fieldOfView;
    }

    private bool IsTopDown
    {
        get { return cameraAngle >= TopDownAngle; }
    }

    /// <summary>
    /// Position direction and the point the camera looks at. The target is
    /// nudged down-screen so the board sits a little above the middle of the
    /// phone, clear of the HUD and the placement slider.
    /// </summary>
    private void GetFraming(out Vector3 target, out Vector3 offsetDir)
    {
        float span = GetBoardSpan();
        float angleRad = cameraAngle * Mathf.Deg2Rad;
        offsetDir = new Vector3(0f, Mathf.Sin(angleRad), -Mathf.Cos(angleRad)).normalized;

        Vector3 forward = -offsetDir;
        Vector3 right = Vector3.Cross(Vector3.up, forward);
        if (right.sqrMagnitude < 0.000001f)
            right = Vector3.right;
        else
            right.Normalize();

        // Screen-up direction flattened onto the board plane, so the bias is a
        // pure on-screen vertical shift for any camera pitch.
        Vector3 screenUp = Vector3.Cross(forward, right);
        screenUp.y = 0f;
        if (screenUp.sqrMagnitude < 0.000001f)
            screenUp = Vector3.forward;
        else
            screenUp.Normalize();

        target = boardCenter + Vector3.up * lookAtHeight - screenUp * (span * framingBias);
    }

    /// <summary>
    /// Distance at which the four corners of the full board (including the
    /// wooden frame) sit inside the viewport. The solve is iterative because the
    /// projection is perspective, not orthographic.
    /// </summary>
    private float ComputeFitDistance()
    {
        float span = GetBoardSpan();
        float half = span * 0.5f;

        float aspect = cam != null && cam.aspect > 0.01f
            ? cam.aspect
            : (float)Screen.width / Mathf.Max(1, Screen.height);

        float tanV = Mathf.Tan(fieldOfView * 0.5f * Mathf.Deg2Rad);
        float tanH = Mathf.Max(0.0001f, tanV * aspect);
        float target = Mathf.Clamp(fitMargin, 0.5f, 0.995f);

        Vector3 lookTarget;
        Vector3 offsetDir;
        GetFraming(out lookTarget, out offsetDir);

        Vector3 forward = -offsetDir;
        Vector3 right = Vector3.Cross(Vector3.up, forward);
        if (right.sqrMagnitude < 0.000001f)
            right = Vector3.right;
        else
            right.Normalize();
        Vector3 up = Vector3.Cross(forward, right).normalized;

        float distance = Mathf.Max(2f, cameraDistance);
        for (int iteration = 0; iteration < 16; iteration++)
        {
            Vector3 camPos = lookTarget + offsetDir * distance;

            float worst = 0f;
            for (int sx = -1; sx <= 1; sx += 2)
            {
                for (int sz = -1; sz <= 1; sz += 2)
                {
                    Vector3 corner = new Vector3(
                        boardCenter.x + sx * half,
                        boardCenter.y,
                        boardCenter.z + sz * half) - camPos;
                    float depth = Vector3.Dot(corner, forward);
                    if (depth < 0.05f) depth = 0.05f;

                    float x = Vector3.Dot(corner, right) / (depth * tanH);
                    float y = Vector3.Dot(corner, up) / (depth * tanV);
                    worst = Mathf.Max(worst, Mathf.Abs(x), Mathf.Abs(y));
                }
            }

            if (worst < 0.0001f) break;
            distance *= worst / target;
        }

        return distance;
    }

    private float GetBoardSpan()
    {
        BoardSetup board = FindObjectOfType<BoardSetup>(true);
        if (board != null && board.boardSize > 0.01f)
        {
            // The whole board, frame included, must stay on screen: the outer
            // rail is part of the silhouette and cropping it makes the board
            // look tilted. The small extra allowance covers the proud frame cap.
            return board.boardSize + board.borderWidth * 2f + 0.04f;
        }

        // Fallback matches the current scene board (74 cm surface + frame).
        return 3.18f;
    }

    private void Update()
    {
        HandleZoomInput();
    }

    private void HandleZoomInput()
    {
        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) > 0.01f)
            StepZoom(scroll > 0f ? zoomStep : -zoomStep);

        if (Input.touchCount == 2)
        {
            Touch a = Input.GetTouch(0);
            Touch b = Input.GetTouch(1);
            float distance = Vector2.Distance(a.position, b.position);

            if (!pinching)
            {
                pinching = true;
                pinchStartDistance = distance;
                pinchStartZoom = targetZoom;
            }
            else if (Mathf.Abs(pinchStartDistance) > 1f)
            {
                targetZoom = Mathf.Clamp(
                    pinchStartZoom * (distance / pinchStartDistance), minZoom, maxZoom);
            }
        }
        else
        {
            pinching = false;
        }
    }

    private void LateUpdate()
    {
        if (!initialized)
        {
            SetupCamera();
            return;
        }

        if (!Mathf.Approximately(zoom, targetZoom))
        {
            zoom = Mathf.Lerp(zoom, targetZoom, 1f - Mathf.Exp(-zoomLerpSpeed * Time.deltaTime));
            if (Mathf.Abs(zoom - targetZoom) < 0.001f)
                zoom = targetZoom;

            ApplyTransform();
        }
    }

    public void StepZoom(float delta)
    {
        targetZoom = Mathf.Clamp(targetZoom + delta, minZoom, maxZoom);
    }

    public void ZoomIn()
    {
        StepZoom(zoomStep);
    }

    public void ZoomOut()
    {
        StepZoom(-zoomStep);
    }

    public void ResetZoom()
    {
        targetZoom = 1f;
    }

    /// <summary>1 = whole board fitted, up to maxZoom = close-up.</summary>
    public float ZoomLevel
    {
        get { return pinching ? zoom : targetZoom; }
    }

    public bool IsFitted
    {
        get { return Mathf.Abs(targetZoom - 1f) < 0.01f; }
    }

    /// <summary>Re-solve the framing. Call after changing the board size.</summary>
    public void RecalculateFraming()
    {
        SetupCamera();
    }
}
