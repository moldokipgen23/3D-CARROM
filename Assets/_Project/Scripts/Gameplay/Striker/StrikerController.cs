using System.Collections.Generic;
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
    private GameObject baselineTrack;
    private bool isSlidingStriker;
    private List<GameObject> aimDots;
    private List<Material> aimDotMats;

    private void Awake()
    {
        Instance = this;

        mainCamera = Camera.main;
        SetupPhysics();
        BuildStrikerVisual();
        BuildBaselineTrack();
        ResetToBaseline(1);
    }

    private void BuildStrikerVisual()
    {
        Material ivory = CreateSimpleMaterial(new Color(0.97f, 0.95f, 0.90f), 0.15f, 0.90f);
        Material red = CreateSimpleMaterial(new Color(0.70f, 0.13f, 0.11f), 0.10f, 0.85f);
        Material gold = CreateSimpleMaterial(new Color(0.94f, 0.77f, 0.34f), 0.85f, 0.95f);

        Cylinder("StrikerBody", ivory,
            new Vector3(strikerRadius * 2f, 0.015f, strikerRadius * 2f), 0f);
        Cylinder("StrikerEdge", red,
            new Vector3(strikerRadius * 2.18f, 0.0136f, strikerRadius * 2.18f), 0f);
        Cylinder("StrikerFace", ivory,
            new Vector3(strikerRadius * 1.86f, 0.0026f, strikerRadius * 1.86f), 0.0164f);
        Cylinder("StrikerRing", red,
            new Vector3(strikerRadius * 1.30f, 0.0016f, strikerRadius * 1.30f), 0.0182f);
        Cylinder("StrikerInner", ivory,
            new Vector3(strikerRadius * 1.06f, 0.0014f, strikerRadius * 1.06f), 0.0190f);
        Cylinder("StrikerDot", gold,
            new Vector3(strikerRadius * 0.44f, 0.0012f, strikerRadius * 0.44f), 0.0198f);

        GameObject glowObj = new GameObject("StrikerGlow");
        glowObj.transform.parent = transform;
        glowObj.transform.localPosition = new Vector3(0, 0.01f, 0);
        Light glow = glowObj.AddComponent<Light>();
        glow.type = LightType.Point;
        glow.color = new Color(1f, 0.9f, 0.6f);
        glow.intensity = 0.12f;
        glow.range = 0.18f;
        strikerGlow = glowObj;
    }

    private GameObject Cylinder(string name, Material mat, Vector3 scale, float localY)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = name;
        go.transform.parent = transform;
        go.transform.localPosition = new Vector3(0f, localY, 0f);
        go.transform.localScale = scale;
        go.GetComponent<MeshRenderer>().sharedMaterial = mat;
        RemoveCollider(go);
        return go;
    }

    /// <summary>Thin inlaid rail so players discover they can slide the striker.</summary>
    private void BuildBaselineTrack()
    {
        if (baselineTrack != null) return;

        baselineTrack = new GameObject("BaselineTrack");
        baselineTrack.transform.position = Vector3.zero;

        Material railMat = CreateSimpleMaterial(new Color(0.40f, 0.25f, 0.11f), 0.25f, 0.65f);
        Material capMat = CreateSimpleMaterial(new Color(0.88f, 0.70f, 0.32f), 0.80f, 0.95f);

        GameObject rail = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rail.name = "Rail";
        rail.transform.SetParent(baselineTrack.transform, false);
        rail.transform.position = new Vector3(0f, 0.0436f, baselineZ);
        rail.transform.localScale = new Vector3(baselineWidth, 0.0008f, 0.012f);
        rail.GetComponent<MeshRenderer>().sharedMaterial = railMat;
        RemoveCollider(rail);

        for (int i = -1; i <= 1; i += 2)
        {
            GameObject cap = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cap.name = "RailCap";
            cap.transform.SetParent(baselineTrack.transform, false);
            cap.transform.position = new Vector3(i * (baselineWidth * 0.5f + 0.02f), 0.0437f, baselineZ);
            cap.transform.localScale = new Vector3(0.020f, 0.0006f, 0.020f);
            cap.GetComponent<MeshRenderer>().sharedMaterial = capMat;
            RemoveCollider(cap);
        }
    }

    private Material CreateSimpleMaterial(Color color, float metallic, float smoothness)
    {
        Shader shader = GetShader();
        Material mat = new Material(shader);
        mat.color = color;

        if (shader.name == "Standard")
        {
            mat.SetFloat("_Metallic", metallic);
            mat.SetFloat("_Glossiness", smoothness);
        }

        return mat;
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

            Vector3? pointer = GetPointerOnBoardPlane();
            if (!pointer.HasValue) return;

            Vector3 delta = pointer.Value - transform.position;
            delta.y = 0f;

            // Generous grab radius: the striker is small on a phone screen.
            if (delta.magnitude <= strikerRadius * 4.0f)
            {
                StartAiming();
            }
            else if (IsOnBaseline(pointer.Value) && TurnManager.Instance != null
                     && TurnManager.Instance.GetCurrentPlayer() == 1)
            {
                isSlidingStriker = true;
                SlideStrikerTo(pointer.Value.x);
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (isSlidingStriker)
            {
                isSlidingStriker = false;
                return;
            }

            if (isAiming)
                StopAimingAndShoot();
        }
        else if (isSlidingStriker)
        {
            Vector3? pointer = GetPointerOnBoardPlane();
            if (pointer.HasValue)
                SlideStrikerTo(pointer.Value.x);
        }
        else if (isAiming)
        {
            UpdateAim();
        }
    }

    private bool IsOnBaseline(Vector3 position)
    {
        return Mathf.Abs(position.z - baselineZ) < 0.46f
               && Mathf.Abs(position.x) < baselineWidth * 0.5f + 0.12f;
    }

    private void SlideStrikerTo(float x)
    {
        float half = baselineWidth * 0.5f;
        float clamped = Mathf.Clamp(x, -half, half);
        SetPosition(new Vector3(clamped, transform.position.y, baselineZ));
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

            aimLine.SetActive(true);

            float power = Mathf.Clamp01(CurrentPowerNormalized);
            float length = 0.45f + power * 1.55f;
            float startDistance = strikerRadius * 1.35f;

            Color low = new Color(0.42f, 0.92f, 0.55f);
            Color mid = new Color(1f, 0.85f, 0.36f);
            Color high = new Color(1f, 0.42f, 0.30f);
            Color color = power < 0.5f
                ? Color.Lerp(low, mid, power * 2f)
                : Color.Lerp(mid, high, (power - 0.5f) * 2f);

            Vector3 origin = transform.position + Vector3.up * 0.010f;

            for (int i = 0; i < aimDots.Count; i++)
            {
                float t = (i + 1) / (float)aimDots.Count;
                float distance = startDistance + t * length;
                GameObject dot = aimDots[i];
                dot.transform.position = origin + shotDirection * distance;

                // Dots taper but stay chunky enough to read on a phone screen.
                float size = 0.042f * (1f - 0.42f * t);
                dot.transform.localScale = new Vector3(size, 0.0012f, size);

                Material mat = aimDotMats[i];
                Color dotColor = Color.Lerp(Color.white, color, 0.55f);
                mat.color = dotColor;
                if (mat.HasProperty("_EmissionColor"))
                {
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", color * 1.5f);
                }
            }
        }
        else if (aimLine != null)
        {
            aimLine.SetActive(false);
        }
    }

    private GameObject CreateAimLine()
    {
        GameObject container = new GameObject("AimLine");
        aimDots = new List<GameObject>();
        aimDotMats = new List<Material>();

        for (int i = 0; i < 15; i++)
        {
            GameObject dot = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            dot.name = "AimDot_" + i;
            dot.transform.parent = container.transform;
            dot.GetComponent<MeshRenderer>().shadowCastingMode =
                UnityEngine.Rendering.ShadowCastingMode.Off;

            Material mat = new Material(GetShader());
            mat.color = Color.green;
            dot.GetComponent<MeshRenderer>().sharedMaterial = mat;

            RemoveCollider(dot);
            aimDots.Add(dot);
            aimDotMats.Add(mat);
        }

        return container;
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
