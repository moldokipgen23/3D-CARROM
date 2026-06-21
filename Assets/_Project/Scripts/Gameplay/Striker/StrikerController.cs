using UnityEngine;
using System;

public class StrikerController : MonoBehaviour
{
    [Header("Striker Settings")]
    public float strikerMass = 8f;
    public float maxDragDistance = 1.0f;
    public float minForce = 2f;
    public float maxForce = 15f;
    public float baselineWidth = 1.5f;
    public float strikerRadius = 0.045f;

    [Header("Physics Settings")]
    public PhysicsMaterial strikerPhysicsMaterial;

    [Header("References")]
    public Rigidbody Rigidbody;
    public Collider StrikerCollider;

    public event Action<Vector2, float> OnShotFired;

    public bool IsAiming => isAiming;
    public Vector3 ShotDirection => shotDirection;

    private Camera mainCamera;
    private Vector3 shotDirection;
    private float shotPower;
    private bool isAiming = false;
    private Vector3 aimStartPosition;
    private Vector3 aimEndPosition;
    private GameObject strikerGlow;
    private GameObject aimLine;

    private void Awake()
    {
        mainCamera = Camera.main;
        SetupPhysics();
        BuildStrikerVisual();
    }

    private void BuildStrikerVisual()
    {
        Material bodyMat = CreateStrikerMaterial();
        Material rimMat = CreateGoldRimMaterial();
        Material topMat = new Material(GetShader());
        topMat.color = new Color(0.95f, 0.92f, 0.88f);
        topMat.SetFloat("_Metallic", 0.85f);
        topMat.SetFloat("_Smoothness", 0.95f);

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        body.name = "StrikerBody";
        body.transform.parent = transform;
        body.transform.localPosition = Vector3.zero;
        body.transform.localScale = new Vector3(strikerRadius * 2f, 0.015f, strikerRadius * 2f);
        body.GetComponent<MeshRenderer>().sharedMaterial = bodyMat;

        GameObject rim = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        rim.name = "StrikerRim";
        rim.transform.parent = transform;
        rim.transform.localPosition = new Vector3(0, 0, 0);
        rim.transform.localScale = new Vector3(strikerRadius * 2.3f, 0.013f, strikerRadius * 2.3f);
        rim.GetComponent<MeshRenderer>().sharedMaterial = rimMat;

        GameObject top = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        top.name = "StrikerTop";
        top.transform.parent = transform;
        top.transform.localPosition = new Vector3(0, 0.016f, 0);
        top.transform.localScale = new Vector3(strikerRadius * 1.5f, 0.005f, strikerRadius * 1.5f);
        top.GetComponent<MeshRenderer>().sharedMaterial = topMat;

        GameObject centerDot = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        centerDot.name = "CenterDot";
        centerDot.transform.parent = transform;
        centerDot.transform.localPosition = new Vector3(0, 0.022f, 0);
        centerDot.transform.localScale = new Vector3(strikerRadius * 0.5f, 0.003f, strikerRadius * 0.5f);
        Material dotMat = new Material(GetShader());
        dotMat.color = new Color(0.85f, 0.7f, 0.2f);
        dotMat.SetFloat("_Metallic", 0.9f);
        dotMat.SetFloat("_Smoothness", 0.95f);
        centerDot.GetComponent<MeshRenderer>().sharedMaterial = dotMat;

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
            Rigidbody = gameObject.AddComponent<Rigidbody>();

        Rigidbody.mass = strikerMass;
        Rigidbody.useGravity = true;
        Rigidbody.constraints = RigidbodyConstraints.FreezePositionY;
        Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        Rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        Rigidbody.drag = 0.3f;
        Rigidbody.angularDrag = 0.8f;

        if (strikerPhysicsMaterial == null)
        {
            strikerPhysicsMaterial = new PhysicsMaterial();
            strikerPhysicsMaterial.dynamicFriction = 0.5f;
            strikerPhysicsMaterial.staticFriction = 0.5f;
            strikerPhysicsMaterial.bounciness = 0.1f;
            strikerPhysicsMaterial.frictionCombine = PhysicsMaterialCombine.Minimum;
            strikerPhysicsMaterial.bounceCombine = PhysicsMaterialCombine.Minimum;
        }

        Collider col = GetComponent<Collider>();
        if (col != null)
            col.sharedMaterial = strikerPhysicsMaterial;

        if (StrikerCollider == null)
            StrikerCollider = GetComponent<Collider>();
    }

    private void Update()
    {
        HandleInput();
        UpdateAimVisual();
    }

    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit) && hit.collider == StrikerCollider)
            {
                StartAiming();
            }
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
        aimStartPosition = Input.mousePosition;
        shotDirection = Vector3.zero;
        shotPower = 0f;

        if (strikerGlow != null)
            strikerGlow.GetComponent<Light>().intensity = 0.4f;
    }

    private void UpdateAim()
    {
        aimEndPosition = Input.mousePosition;

        Vector3 screenDirection = aimEndPosition - aimStartPosition;
        float screenDistance = screenDirection.magnitude;

        shotPower = Mathf.Clamp01(screenDistance / maxDragDistance) * (maxForce - minForce) + minForce;

        shotDirection = mainCamera.ScreenToWorldPoint(new Vector3(aimEndPosition.x, 0, aimEndPosition.y)) -
                       mainCamera.ScreenToWorldPoint(new Vector3(aimStartPosition.x, 0, aimStartPosition.y));

        if (shotDirection != Vector3.zero)
            shotDirection = shotDirection.normalized;
    }

    private void UpdateAimVisual()
    {
        if (isAiming && shotDirection != Vector3.zero)
        {
            if (aimLine == null)
                aimLine = CreateAimLine();

            float powerPercent = shotPower / maxForce;
            Color lineColor = Color.Lerp(new Color(0.2f, 0.8f, 0.2f), new Color(1f, 0.2f, 0.2f), powerPercent);
            aimLine.GetComponent<MeshRenderer>().sharedMaterial.color = lineColor;

            float lineLength = powerPercent * 0.8f;
            Vector3 linePos = transform.position + new Vector3(shotDirection.x, 0.01f, shotDirection.z) * (lineLength / 2f + strikerRadius);
            aimLine.transform.position = linePos;
            aimLine.transform.localScale = new Vector3(0.008f, 0.003f, lineLength);
            aimLine.transform.rotation = Quaternion.LookRotation(new Vector3(shotDirection.x, 0, shotDirection.z));
            aimLine.SetActive(true);
        }
        else
        {
            if (aimLine != null)
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
        lineMat.SetFloat("_Metallic", 0f);
        lineMat.SetFloat("_Smoothness", 0.3f);
        line.GetComponent<MeshRenderer>().sharedMaterial = lineMat;
        Destroy(line.GetComponent<BoxCollider>());
        return line;
    }

    private void StopAimingAndShoot()
    {
        isAiming = false;

        if (strikerGlow != null)
            strikerGlow.GetComponent<Light>().intensity = 0.15f;

        if (aimLine != null)
            aimLine.SetActive(false);

        if (shotDirection != Vector3.zero && shotPower > 0)
        {
            Vector3 worldDirection = mainCamera.transform.TransformDirection(shotDirection);
            Vector3 flatDirection = new Vector3(worldDirection.x, 0, worldDirection.z).normalized;
            Vector3 worldForce = flatDirection * shotPower;

            Rigidbody.AddForce(worldForce, ForceMode.Impulse);
            OnShotFired?.Invoke(new Vector2(flatDirection.x, flatDirection.z), worldForce.magnitude);
        }
    }

    public bool IsWithinBaseline(Vector3 position)
    {
        float halfBaselineWidth = baselineWidth / 2f;
        return Mathf.Abs(position.x) <= halfBaselineWidth;
    }

    public void SetPosition(Vector3 pos)
    {
        transform.position = pos;
        Rigidbody.linearVelocity = Vector3.zero;
        Rigidbody.angularVelocity = Vector3.zero;
    }

    private Shader GetShader()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        return shader;
    }

    private Material CreateStrikerMaterial()
    {
        Material mat = new Material(GetShader());
        mat.color = new Color(0.92f, 0.88f, 0.82f);
        mat.SetFloat("_Metallic", 0.8f);
        mat.SetFloat("_Smoothness", 0.92f);
        return mat;
    }

    private Material CreateGoldRimMaterial()
    {
        Material mat = new Material(GetShader());
        mat.color = new Color(0.85f, 0.7f, 0.2f);
        mat.SetFloat("_Metallic", 0.85f);
        mat.SetFloat("_Smoothness", 0.95f);
        return mat;
    }
}
