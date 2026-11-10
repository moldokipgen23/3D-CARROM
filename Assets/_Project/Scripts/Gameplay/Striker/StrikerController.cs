using UnityEngine;
using System;

public class StrikerController : MonoBehaviour
{
    public static StrikerController Instance { get; private set; }

    [Header("Striker Settings")]
    public float strikerMass = 8f;
    public float maxDragDistance = 1.0f;
    public float minForce = 8f;
    public float maxForce = 55f;
    public float baselineWidth = 1.5f;
    public float strikerRadius = 0.045f;

    [Header("Table Layout")]
    public float baselineZ = 1.1f;
    public float strikerRestY = 0.0585f;

    [Header("Physics Settings")]
    public PhysicsMaterial strikerPhysicsMaterial;

    [Header("References")]
    public Rigidbody Rigidbody;
    public Collider StrikerCollider;

    public event Action<Vector2, float> OnShotFired;

    public bool IsAiming { get { return isAiming; } }
    public Vector3 ShotDirection { get { return shotDirection; } }
    public float CurrentPowerNormalized { get; private set; }

    private Camera mainCamera;
    private Vector3 shotDirection;
    private float shotPower;
    private bool isAiming = false;
    private GameObject strikerGlow;
    private GameObject aimLine;

    private void Awake()
    {
        Instance = this;

        mainCamera = Camera.main;
        SetupPhysics();
        BuildStrikerVisual();
        ResetToBaseline(1);
    }

    private void BuildStrikerVisual()
    {
        Material bodyMat = CreateStrikerMaterial();
        Material rimMat = CreateGoldRimMaterial();

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        body.name = "StrikerBody";
        body.transform.parent = transform;
        body.transform.localPosition = Vector3.zero;
        body.transform.localScale = new Vector3(strikerRadius * 2f, 0.015f, strikerRadius * 2f);
        body.GetComponent<MeshRenderer>().sharedMaterial = bodyMat;
        RemoveCollider(body);

        GameObject rim = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        rim.name = "StrikerRim";
        rim.transform.parent = transform;
        rim.transform.localPosition = new Vector3(0, 0, 0);
        rim.transform.localScale = new Vector3(strikerRadius * 2.3f, 0.013f, strikerRadius * 2.3f);
        rim.GetComponent<MeshRenderer>().sharedMaterial = rimMat;
        RemoveCollider(rim);

        GameObject top = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        top.name = "StrikerTop";
        top.transform.parent = transform;
        top.transform.localPosition = new Vector3(0, 0.016f, 0);
        top.transform.localScale = new Vector3(strikerRadius * 1.5f, 0.005f, strikerRadius * 1.5f);
        Material topMat = new Material(GetShader());
        topMat.color = new Color(0.95f, 0.92f, 0.88f);
        topMat.SetFloat("_Metallic", 0.85f);
        topMat.SetFloat("_Glossiness", 0.95f);
        top.GetComponent<MeshRenderer>().sharedMaterial = topMat;
        RemoveCollider(top);

        GameObject centerDot = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        centerDot.name = "CenterDot";
        centerDot.transform.parent = transform;
        centerDot.transform.localPosition = new Vector3(0, 0.022f, 0);
        centerDot.transform.localScale = new Vector3(strikerRadius * 0.5f, 0.003f, strikerRadius * 0.5f);
        Material dotMat = new Material(GetShader());
        dotMat.color = new Color(0.85f, 0.7f, 0.2f);
        dotMat.SetFloat("_Metallic", 0.9f);
        dotMat.SetFloat("_Glossiness", 0.95f);
        centerDot.GetComponent<MeshRenderer>().sharedMaterial = dotMat;
        RemoveCollider(centerDot);

        GameObject glowObj = new GameObject("StrikerGlow");
        glowObj.transform.parent = transform;
        glowObj.transform.localPosition = new Vector3(0, 0.01f, 0);
        Light glow = glowObj.AddComponent<Light>();
        glow.type = LightType.Point;
        glow.color = new Color(1f, 0.9f, 0.6f);
        glow.intensity = 0.15f;
        glow.range = 0.2f;
        strikerGlow = glowObj;
    }

    private void SetupPhysics()
    {
        if (Rigidbody == null)
            Rigidbody = GetComponent<Rigidbody>();

        if (Rigidbody == null)
            Rigidbody = gameObject.AddComponent<Rigidbody>();

        Rigidbody.mass = strikerMass;
        Rigidbody.useGravity = true;
        Rigidbody.constraints = RigidbodyConstraints.FreezePositionY |
                                RigidbodyConstraints.FreezeRotationX |
                                RigidbodyConstraints.FreezeRotationZ;
        Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        Rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        Rigidbody.linearDamping = 0.1f;
        Rigidbody.angularDamping = 0.8f;

        if (strikerPhysicsMaterial == null)
        {
            strikerPhysicsMaterial = new PhysicsMaterial();
            strikerPhysicsMaterial.dynamicFriction = 0.08f;
            strikerPhysicsMaterial.staticFriction = 0.08f;
            strikerPhysicsMaterial.bounciness = 0.45f;
            strikerPhysicsMaterial.frictionCombine = PhysicsMaterialCombine.Minimum;
            strikerPhysicsMaterial.bounceCombine = PhysicsMaterialCombine.Maximum;
        }

        StrikerCollider = GetComponent<Collider>();
        if (StrikerCollider == null)
        {
            BoxCollider box = gameObject.AddComponent<BoxCollider>();
            box.size = new Vector3(strikerRadius * 2f, 0.03f, strikerRadius * 2f);
            box.center = Vector3.zero;
            StrikerCollider = box;
        }

        StrikerCollider.sharedMaterial = strikerPhysicsMaterial;
    }

    private void Update()
    {
        if (!CanAcceptInput())
        {
            if (isAiming)
                CancelAim();
            return;
        }

        HandleInput();
        UpdateAimVisual();
    }

    private bool CanAcceptInput()
    {
        if (TurnManager.Instance == null) return true;
        if (TurnManager.Instance.IsShotInProgress()) return false;
        if (ScoreManager.Instance != null && ScoreManager.Instance.IsGameEnded) return false;

        int player = TurnManager.Instance.GetCurrentPlayer();
        return player == 1 || !TurnManager.Instance.aiControlsPlayer2;
    }

    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (mainCamera == null)
                mainCamera = Camera.main;

            if (mainCamera == null) return;

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit) && hit.collider == StrikerCollider)
                StartAiming();
        }
        else if (Input.GetMouseButtonUp(0) && isAiming)
        {
            StopAimingAndShoot();
        }
        else if (isAiming)
        {
            UpdateAim();
        }
    }

    private void StartAiming()
    {
        isAiming = true;
        shotDirection = Vector3.zero;
        shotPower = 0f;
        CurrentPowerNormalized = 0f;

        if (strikerGlow != null)
            strikerGlow.GetComponent<Light>().intensity = 0.4f;
    }

    private void UpdateAim()
    {
        Vector3? pointer = GetPointerOnBoardPlane();
        if (!pointer.HasValue) return;

        Vector3 toPointer = pointer.Value - transform.position;
        toPointer.y = 0f;

        float distance = toPointer.magnitude;
        if (distance < 0.001f) return;

        shotDirection = toPointer.normalized;
        shotPower = Mathf.Clamp01(distance / maxDragDistance) * (maxForce - minForce) + minForce;
        CurrentPowerNormalized = Mathf.Clamp01((shotPower - minForce) / Mathf.Max(0.0001f, maxForce - minForce));
    }

    private Vector3? GetPointerOnBoardPlane()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
        if (mainCamera == null) return null;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, new Vector3(0f, transform.position.y, 0f));

        float enter;
        if (plane.Raycast(ray, out enter))
            return ray.GetPoint(enter);

        return null;
    }

    private void UpdateAimVisual()
    {
        if (isAiming && shotDirection != Vector3.zero)
        {
            if (aimLine == null)
                aimLine = CreateAimLine();

            if (aimLine == null) return;

            Color lineColor = Color.Lerp(new Color(0.2f, 0.8f, 0.2f), new Color(1f, 0.2f, 0.2f), CurrentPowerNormalized);
            aimLine.GetComponent<MeshRenderer>().material.color = lineColor;

            float lineLength = 0.15f + CurrentPowerNormalized * 0.9f;
            Vector3 linePos = transform.position + shotDirection * (lineLength / 2f + strikerRadius);
            linePos.y = transform.position.y + 0.006f;

            aimLine.transform.position = linePos;
            aimLine.transform.localScale = new Vector3(0.012f, 0.004f, lineLength);
            aimLine.transform.rotation = Quaternion.LookRotation(shotDirection);
            aimLine.SetActive(true);
        }
        else if (aimLine != null)
        {
            aimLine.SetActive(false);
        }
    }

    private GameObject CreateAimLine()
    {
        GameObject line = GameObject.CreatePrimitive(PrimitiveType.Cube);
        line.name = "AimLine";
        line.transform.parent = null;

        Material lineMat = new Material(GetShader());
        lineMat.color = Color.green;
        line.GetComponent<MeshRenderer>().sharedMaterial = lineMat;

        RemoveCollider(line);
        return line;
    }

    private void CancelAim()
    {
        isAiming = false;
        shotDirection = Vector3.zero;
        shotPower = 0f;
        CurrentPowerNormalized = 0f;

        if (strikerGlow != null)
            strikerGlow.GetComponent<Light>().intensity = 0.15f;
        if (aimLine != null)
            aimLine.SetActive(false);
    }

    private void StopAimingAndShoot()
    {
        bool hasShot = shotDirection != Vector3.zero && CurrentPowerNormalized > 0.01f;
        Vector3 direction = shotDirection;
        float power = shotPower;

        CancelAim();

        if (hasShot)
            ShootTowards(direction, power);
    }

    /// <summary>
    /// Shared shot entry point for both the human striker and the AI.
    /// Applies the impulse and tells TurnManager that a shot has begun and ended
    /// (TurnManager waits for physics before switching turns).
    /// </summary>
    public bool ShootTowards(Vector3 worldDirection, float power)
    {
        if (worldDirection.sqrMagnitude < 0.0001f || power <= 0f) return false;
        if (TurnManager.Instance != null && TurnManager.Instance.IsShotInProgress()) return false;

        Vector3 direction = new Vector3(worldDirection.x, 0f, worldDirection.z).normalized;

        if (Rigidbody == null)
            Rigidbody = GetComponent<Rigidbody>();
        if (Rigidbody == null) return false;

        Rigidbody.isKinematic = false;
        Rigidbody.linearVelocity = Vector3.zero;
        Rigidbody.angularVelocity = Vector3.zero;
        Rigidbody.AddForce(direction * power, ForceMode.Impulse);

        CurrentPowerNormalized = Mathf.Clamp01((power - minForce) / Mathf.Max(0.0001f, maxForce - minForce));

        Debug.Log($"Shot fired - direction {direction}, power {power:F1}");
        OnShotFired?.Invoke(new Vector2(direction.x, direction.z), power);

        TurnManager.Instance?.BeginShot();
        TurnManager.Instance?.EndShot();
        return true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider == null) return;

        if (collision.collider.GetComponent<Coin>() != null || collision.collider.CompareTag("Coin"))
            FoulDetector.Instance?.RegisterCoinTouched();
    }

    public bool IsWithinBaseline(Vector3 position)
    {
        float halfBaselineWidth = baselineWidth / 2f;
        return Mathf.Abs(position.x) <= halfBaselineWidth;
    }

    public void ResetToBaseline(int player)
    {
        float z = player == 2 ? baselineZ : -baselineZ;
        SetPosition(new Vector3(0f, strikerRestY, z));
    }

    public void PlaceOnBaseline(float x)
    {
        float half = Mathf.Max(0f, baselineWidth * 0.5f - strikerRadius);
        x = Mathf.Clamp(x, -half, half);

        Vector3 pos = transform.position;
        pos.x = x;
        pos.y = strikerRestY;
        SetPosition(pos);
    }

    public void SetPosition(Vector3 pos)
    {
        // Coins scattered onto the baseline would overlap the striker. Nudge
        // them clear so the next shot starts from a clean, stable position.
        ClearCoinsAt(pos);

        transform.position = pos;

        if (Rigidbody != null)
        {
            Rigidbody.isKinematic = false;
            Rigidbody.linearVelocity = Vector3.zero;
            Rigidbody.angularVelocity = Vector3.zero;
        }
    }

    private void ClearCoinsAt(Vector3 pos)
    {
        float radius = strikerRadius + 0.03f;
        Collider[] overlaps = Physics.OverlapSphere(pos, radius);
        for (int i = 0; i < overlaps.Length; i++)
        {
            Collider other = overlaps[i];
            if (other == null) continue;

            Coin coin = other.GetComponent<Coin>();
            if (coin == null)
                coin = other.GetComponentInParent<Coin>();
            if (coin == null) continue;

            Rigidbody coinBody = coin.GetComponent<Rigidbody>();

            Vector3 away = coin.transform.position - pos;
            away.y = 0f;
            if (away.sqrMagnitude < 0.0001f)
                away = Vector3.right;

            Vector3 newPos = pos + away.normalized * (radius + 0.05f);
            newPos.y = coin.transform.position.y;
            coin.transform.position = newPos;

            if (coinBody != null)
            {
                coinBody.linearVelocity = Vector3.zero;
                coinBody.angularVelocity = Vector3.zero;
            }
        }
    }

    private void RemoveCollider(GameObject go)
    {
        if (go == null) return;
        Collider collider = go.GetComponent<Collider>();
        if (collider == null) return;
        collider.enabled = false;
        Destroy(collider);
    }

    private Shader GetShader()
    {
        Shader shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Diffuse");
        return shader;
    }

    private Material CreateStrikerMaterial()
    {
        Material mat = new Material(GetShader());
        mat.color = new Color(0.92f, 0.88f, 0.82f);
        mat.SetFloat("_Metallic", 0.8f);
        mat.SetFloat("_Glossiness", 0.92f);
        return mat;
    }

    private Material CreateGoldRimMaterial()
    {
        Material mat = new Material(GetShader());
        mat.color = new Color(0.85f, 0.7f, 0.2f);
        mat.SetFloat("_Metallic", 0.85f);
        mat.SetFloat("_Glossiness", 0.95f);
        return mat;
    }
}
