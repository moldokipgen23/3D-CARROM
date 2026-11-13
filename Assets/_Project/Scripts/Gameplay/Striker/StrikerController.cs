using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class StrikerController : MonoBehaviour
{
    public static StrikerController Instance { get; private set; }

    [Header("Striker Settings")]
    public float strikerMass = 5f;
    public float maxDragDistance = 1.0f;
    public float minForce = 7f;
    // A lively but controllable flick: full power crosses the board in about
    // half a second, then the rails and the coin pack bleed the speed off.
    public float maxForce = 30f;
    public float baselineWidth = 1.842f;
    // 1.5x the ICF 4.13 cm striker. Commercial mobile carrom titles
    // deliberately oversize pieces so they stay readable on a phone screen.
    public float strikerRadius = 0.131f;

    [Header("Table Layout")]
    public float baselineZ = 0.99f;
    // Two base-circle radii: the striker may sit between the base circles but
    // never cover them (ICF rule).
    public float baselineEndInset = 0.1246f;
    public float strikerRestY = 0.080f;
    public float strikerThickness = 0.070f;
    public float strikerBevel = 0.014f;

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
    private bool isSlidingStriker;
    private List<GameObject> aimDots;
    private List<Material> aimDotMats;
    private Transform haloTransform;
    private Material haloMaterial;

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
        // Warm turned brass for the rim, satin ivory for the printed face.
        // Heavy metalness turns almost black in Built-in without reflection
        // probes, so keep it low and let the warm diffuse carry the brass.
        Material rimMat = CreateSimpleMaterial(new Color(0.93f, 0.72f, 0.34f), 0.35f, 0.92f);
        Material faceMat = CreateSimpleMaterial(Color.white, 0.04f, 0.62f);
        faceMat.mainTexture = ProceduralTextures.CreateStrikerFaceTexture();

        GameObject body = new GameObject("StrikerBody");
        body.transform.SetParent(transform, false);

        MeshFilter filter = body.AddComponent<MeshFilter>();
        filter.sharedMesh = PieceMeshFactory.GetDisc(
            "Striker", strikerRadius, strikerThickness, strikerBevel, 72);

        MeshRenderer renderer = body.AddComponent<MeshRenderer>();
        renderer.sharedMaterials = new Material[] { rimMat, faceMat };
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        renderer.receiveShadows = true;

        // Grounds the striker on the board and gives it visible thickness.
        PieceMeshFactory.CreateContactShadow(
            transform, strikerRadius, -strikerThickness * 0.5f + 0.0008f, 2.5f);

        CreateHalo();

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

    /// <summary>
    /// Soft gold halo under the striker, like the glowing ring around the
    /// striker in the reference game. It makes the piece read instantly even
    /// on a pale board and reinforces the "premium piece" look.
    /// </summary>
    private void CreateHalo()
    {
        GameObject halo = GameObject.CreatePrimitive(PrimitiveType.Quad);
        halo.name = "StrikerHalo";
        halo.transform.SetParent(transform, false);
        halo.transform.localPosition = new Vector3(
            0f, -strikerThickness * 0.5f + 0.0016f, 0f);
        halo.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        halo.transform.localScale = new Vector3(strikerRadius * 2.3f, strikerRadius * 2.3f, 1f);
        haloTransform = halo.transform;

        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null) shader = Shader.Find("Unlit/Transparent");
        if (shader == null) shader = Shader.Find("Standard");

        haloMaterial = new Material(shader);
        haloMaterial.mainTexture = ProceduralTextures.CreateGlowTexture();
        haloMaterial.color = new Color(1f, 0.80f, 0.45f, 0.26f);
        haloMaterial.renderQueue = 2990;

        MeshRenderer renderer = halo.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = haloMaterial;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        RemoveCollider(halo);
    }

    private void UpdateHalo()
    {
        if (haloTransform == null || haloMaterial == null) return;

        float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * 2.1f);
        float size = strikerRadius * (2.30f + pulse * 0.18f);
        haloTransform.localScale = new Vector3(size, size, 1f);

        Color c = haloMaterial.color;
        c.a = 0.18f + pulse * 0.07f + (isAiming ? 0.10f : 0f);
        haloMaterial.color = c;
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
        // The board is slick, but a little drag keeps the striker from
        // ricocheting around the rails forever.
        Rigidbody.linearDamping = 0.35f;
        Rigidbody.angularDamping = 0.90f;

        if (strikerPhysicsMaterial == null)
        {
            strikerPhysicsMaterial = new PhysicsMaterial();
            strikerPhysicsMaterial.dynamicFriction = 0.10f;
            strikerPhysicsMaterial.staticFriction = 0.10f;
            strikerPhysicsMaterial.bounciness = 0.50f;
            strikerPhysicsMaterial.frictionCombine = PhysicsMaterialCombine.Minimum;
            strikerPhysicsMaterial.bounceCombine = PhysicsMaterialCombine.Maximum;
        }

        StrikerCollider = GetComponent<Collider>();
        if (StrikerCollider == null)
        {
            // Convex mesh collider gives the striker a round footprint, so it
            // deflects off coins like a disc instead of snagging on box corners.
            MeshCollider mesh = gameObject.AddComponent<MeshCollider>();
            mesh.convex = true;
            mesh.sharedMesh = PieceMeshFactory.GetColliderCylinder(
                "StrikerCollider", strikerRadius * 0.98f, strikerThickness * 0.94f);
            StrikerCollider = mesh;
        }

        StrikerCollider.sharedMaterial = strikerPhysicsMaterial;
    }

    private void Update()
    {
        UpdateHalo();

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
        // Touches that begin on HUD widgets (zoom buttons, the baseline slider)
        // must not also start a board drag underneath them.
        if (Input.GetMouseButtonDown(0) && IsPointerOverUI())
            return;

        if (Input.GetMouseButtonDown(0))
        {
            if (mainCamera == null)
                mainCamera = Camera.main;

            if (mainCamera == null) return;

            Vector3? pointer = GetPointerOnBoardPlane();
            if (!pointer.HasValue) return;

            Vector3 delta = pointer.Value - transform.position;
            delta.y = 0f;

            // Generous grab radius, but still leaves the rest of the baseline
            // free for tap-to-slide placement.
            if (delta.magnitude <= strikerRadius * 3.0f)
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
        // Player 1 uses the near line (-baselineZ), the AI the far line
        // (+baselineZ), so accept whichever line is closer.
        float nearest = Mathf.Min(Mathf.Abs(position.z - baselineZ),
            Mathf.Abs(position.z + baselineZ));
        return nearest < 0.30f
               && Mathf.Abs(position.x) < baselineWidth * 0.5f + 0.12f;
    }

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;

        if (Input.touchCount > 0)
            return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);

        return EventSystem.current.IsPointerOverGameObject();
    }

    /// <summary>Largest legal distance from the centre of the baseline.</summary>
    public float BaselineLimit
    {
        get { return Mathf.Max(0f, baselineWidth * 0.5f - baselineEndInset - strikerRadius); }
    }

    private void SlideStrikerTo(float x)
    {
        float half = BaselineLimit;
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

        // Slingshot control: pull back from the striker and the shot fires the
        // opposite way, exactly like a real carrom flick. The guideline is
        // therefore drawn ahead of the striker, not under the finger.
        shotDirection = -toPointer.normalized;
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
            UpdateAimDots();
        }
        else if (aimLine != null)
        {
            aimLine.SetActive(false);
        }
    }

    private void UpdateAimDots()
    {
        float power = Mathf.Clamp01(CurrentPowerNormalized);
        float length = 0.45f + power * 1.55f;
        float startDistance = strikerRadius * 1.35f;

        // Clean ivory guide dots like the reference game. Force is shown by the
        // power bar and by how far the dots reach, not by rainbow colours.
        Color color = new Color(0.995f, 0.975f, 0.915f);

        Vector3 origin = transform.position + Vector3.up * 0.010f;

        for (int i = 0; i < aimDots.Count; i++)
        {
            float t = (i + 1) / (float)aimDots.Count;
            float distance = startDistance + t * length;
            GameObject dot = aimDots[i];
            dot.transform.position = origin + shotDirection * distance;

            // Fine, tapering dotted guideline.
            float size = 0.034f * (1f - 0.38f * t);
            dot.transform.localScale = new Vector3(size, 0.0012f, size);

            Material mat = aimDotMats[i];
            mat.color = color;
            if (mat.HasProperty("_EmissionColor"))
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", color * 0.55f);
            }
        }
    }

    /// <summary>
    /// Shows the aiming dots without letting the player shoot. The AI uses this so
    /// its shot is clearly telegraphed before the strike.
    /// </summary>
    public void ShowAimPreview(Vector3 worldDirection, float normalizedPower)
    {
        Vector3 flat = new Vector3(worldDirection.x, 0f, worldDirection.z);
        if (flat.sqrMagnitude < 0.0001f) return;

        shotDirection = flat.normalized;
        CurrentPowerNormalized = Mathf.Clamp01(normalizedPower);

        if (aimLine == null)
            aimLine = CreateAimLine();
        if (aimLine == null) return;

        aimLine.SetActive(true);
        UpdateAimDots();
    }

    public void HideAimPreview()
    {
        if (aimLine != null)
            aimLine.SetActive(false);
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

        power = Mathf.Clamp(power, minForce, maxForce);
        Vector3 direction = new Vector3(worldDirection.x, 0f, worldDirection.z).normalized;

        HideAimPreview();

        if (Rigidbody == null)
            Rigidbody = GetComponent<Rigidbody>();
        if (Rigidbody == null) return false;

        Rigidbody.isKinematic = false;
        Rigidbody.linearVelocity = Vector3.zero;
        Rigidbody.angularVelocity = Vector3.zero;
        Rigidbody.AddForce(direction * power, ForceMode.Impulse);

        CurrentPowerNormalized = Mathf.Clamp01((power - minForce) / Mathf.Max(0.0001f, maxForce - minForce));

        Debug.Log($"Shot fired - direction {direction}, power {power:F1}");

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayStrikerShoot();

        OnShotFired?.Invoke(new Vector2(direction.x, direction.z), power);

        TurnManager.Instance?.BeginShot();
        TurnManager.Instance?.EndShot();
        return true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider == null) return;

        float impact = collision.relativeVelocity.magnitude;
        bool hitCoin = collision.collider.GetComponent<Coin>() != null ||
                       collision.collider.CompareTag("Coin");

        if (hitCoin)
        {
            FoulDetector.Instance?.RegisterCoinTouched();
            AbsorbCoinImpact();

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayCoinHit(impact);
        }
        else if (AudioManager.Instance != null && impact > 0.35f)
        {
            AudioManager.Instance.PlayBoardHit(impact);
        }
    }

    /// <summary>
    /// A real carrom striker keeps most of its momentum when it clips a coin.
    /// Only a light speed trim here: the physics solver already transfers the
    /// impulse, and a heavy scripted loss made the striker die on the pack's
    /// front row, so shots looked like they did nothing.
    /// </summary>
    private void AbsorbCoinImpact()
    {
        if (Rigidbody == null || Rigidbody.isKinematic) return;

        Vector3 velocity = Rigidbody.linearVelocity;
        if (velocity.sqrMagnitude < 0.01f) return;

        Rigidbody.linearVelocity = velocity * 0.86f;
        Rigidbody.angularVelocity *= 0.9f;
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
        float half = BaselineLimit;
        x = Mathf.Clamp(x, -half, half);

        Vector3 pos = transform.position;
        pos.x = x;
        pos.y = strikerRestY;
        SetPosition(pos);
    }

    /// <summary>
    /// Smoothly slides the striker along the baseline. Used by the AI so the
    /// player can see it line up before the shot instead of teleporting.
    /// </summary>
    public IEnumerator SlideAlongBaseline(float targetX, float duration = 0.45f)
    {
        float half = BaselineLimit;
        targetX = Mathf.Clamp(targetX, -half, half);

        Vector3 start = new Vector3(transform.position.x, strikerRestY, baselineZ);

        if (Rigidbody != null)
        {
            Rigidbody.isKinematic = true;
            Rigidbody.linearVelocity = Vector3.zero;
            Rigidbody.angularVelocity = Vector3.zero;
        }

        transform.position = start;

        float elapsed = 0f;
        duration = Mathf.Max(0.05f, duration);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            transform.position = new Vector3(
                Mathf.Lerp(start.x, targetX, t), strikerRestY, baselineZ);
            yield return null;
        }

        transform.position = new Vector3(targetX, strikerRestY, baselineZ);

        if (Rigidbody != null)
        {
            Rigidbody.isKinematic = false;
            Rigidbody.linearVelocity = Vector3.zero;
            Rigidbody.angularVelocity = Vector3.zero;
        }
    }

    public void SetPosition(Vector3 pos)
    {
        // Coins scattered onto the baseline would overlap the striker. Nudge
        // them clear so the next shot starts from a clean, stable position.
        ClearCoinsAt(pos);

        if (Rigidbody != null)
        {
            // Hold the striker still at the baseline. A non-kinematic body with
            // interpolation enabled overwrites transform.position with its old
            // physics position on the next frame, which made slider placement
            // snap back to the centre. The shot re-enables dynamics.
            Rigidbody.isKinematic = true;
            Rigidbody.linearVelocity = Vector3.zero;
            Rigidbody.angularVelocity = Vector3.zero;
            Rigidbody.position = pos;
            Rigidbody.rotation = Quaternion.identity;
        }

        transform.position = pos;
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
