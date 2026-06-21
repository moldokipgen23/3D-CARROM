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

    [Header("Physics Settings")]
    public PhysicMaterial strikerPhysicsMaterial;

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

    private void Awake()
    {
        mainCamera = Camera.main;
        SetupPhysics();
    }

    private void SetupPhysics()
    {
        if (Rigidbody == null)
        {
            Rigidbody = gameObject.AddComponent<Rigidbody>();
        }

        Rigidbody.mass = strikerMass;
        Rigidbody.useGravity = true;
        Rigidbody.constraints = RigidbodyConstraints.FreezePositionY;

        if (strikerPhysicsMaterial == null)
        {
            strikerPhysicsMaterial = new PhysicMaterial();
            strikerPhysicsMaterial.dynamicFriction = 0.5f;
            strikerPhysicsMaterial.staticFriction = 0.5f;
            strikerPhysicsMaterial.bounciness = 0.1f;
            strikerPhysicsMaterial.frictionCombine = PhysicMaterialCombine.Minimum;
            strikerPhysicsMaterial.bounceCombine = PhysicMaterialCombine.Minimum;
        }

        Rigidbody.sharedMaterial = strikerPhysicsMaterial;

        if (StrikerCollider == null)
        {
            StrikerCollider = GetComponent<Collider>();
        }
    }

    private void Update()
    {
        HandleInput();
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

        Debug.Log("Aiming started");
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
        {
            shotDirection = shotDirection.normalized;
        }

        Debug.Log($"Aiming - Power: {shotPower:F1}, Direction: {shotDirection}");
    }

    private void StopAimingAndShoot()
    {
        isAiming = false;

        if (shotDirection != Vector3.zero && shotPower > 0)
        {
            Vector3 worldDirection = mainCamera.transform.TransformDirection(shotDirection);
            Vector3 worldForce = worldDirection * shotPower;

            Rigidbody.AddForce(worldForce, ForceMode.Impulse);

            Debug.Log($"Shot fired - Force: {worldForce.magnitude:F1}, Direction: {worldDirection}");

            OnShotFired?.Invoke(new Vector2(worldDirection.x, worldDirection.z), worldForce.magnitude);
        }
        else
        {
            Debug.Log("Shot cancelled - insufficient power or direction");
        }
    }

    public bool IsWithinBaseline(Vector3 position)
    {
        float halfBaselineWidth = baselineWidth / 2f;
        return Mathf.Abs(position.x) <= halfBaselineWidth;
    }
}