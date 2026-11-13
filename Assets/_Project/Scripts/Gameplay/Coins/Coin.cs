using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("Coin Settings")]
    public CoinType Type;
    public bool IsPocketed = false;

    [Header("Physics Settings")]
    public Rigidbody Rigidbody;
    public PhysicsMaterial PhysicsMaterial;

    [Header("Visual Settings")]
    public MeshRenderer MeshRenderer;

    private Vector3 spawnPosition;

    private void Awake()
    {
        spawnPosition = transform.position;
        SetupPhysics();
        SetupVisuals();
    }

    private void SetupPhysics()
    {
        if (Rigidbody == null)
            Rigidbody = GetComponent<Rigidbody>();

        if (Rigidbody == null)
            Rigidbody = gameObject.AddComponent<Rigidbody>();

        Rigidbody.useGravity = true;
        Rigidbody.constraints = RigidbodyConstraints.FreezePositionY |
                                RigidbodyConstraints.FreezeRotationX |
                                RigidbodyConstraints.FreezeRotationZ;

        // A real carrom coin weighs ~5 g against a ~15 g striker, so the
        // striker must stay heavier than the coins to transfer momentum.
        // Coins are much lighter than the striker so a hard shot scatters the pile.
        Rigidbody.mass = 0.8f;
        Rigidbody.linearDamping = 0.25f;
        Rigidbody.angularDamping = 0.70f;

        if (PhysicsMaterial == null)
        {
            PhysicsMaterial = new PhysicsMaterial();
            // Powdered carrom board: coins glide a long way and keep their
            // speed instead of stopping dead after a few centimetres.
            PhysicsMaterial.dynamicFriction = 0.04f;
            PhysicsMaterial.staticFriction = 0.04f;
            // Hard acrylic coins rebound off each other, so the pack bursts
            // open on a solid break instead of absorbing the whole impulse.
            PhysicsMaterial.bounciness = 0.55f;
            PhysicsMaterial.frictionCombine = PhysicsMaterialCombine.Minimum;
            PhysicsMaterial.bounceCombine = PhysicsMaterialCombine.Maximum;
        }

        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            BoxCollider box = gameObject.AddComponent<BoxCollider>();
            box.size = new Vector3(0.076f, 0.012f, 0.076f);
            col = box;
        }

        col.sharedMaterial = PhysicsMaterial;
    }

    private void SetupVisuals()
    {
        if (MeshRenderer == null)
            MeshRenderer = GetComponent<MeshRenderer>();

        // Coins are built from child primitives; those already have their own
        // materials, so nothing is recoloured here.
    }

    public void Pocket()
    {
        if (IsPocketed) return;

        IsPocketed = true;

        if (Rigidbody != null)
        {
            Rigidbody.linearVelocity = Vector3.zero;
            Rigidbody.angularVelocity = Vector3.zero;
            Rigidbody.isKinematic = true;
        }

        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = false;

        gameObject.SetActive(false);
        Debug.Log($"Coin {Type} pocketed");
    }

    public void ResetCoin()
    {
        IsPocketed = false;

        gameObject.SetActive(true);

        if (Rigidbody != null)
        {
            Rigidbody.isKinematic = false;
            Rigidbody.linearVelocity = Vector3.zero;
            Rigidbody.angularVelocity = Vector3.zero;
        }

        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = true;

        transform.position = spawnPosition;
        transform.rotation = Quaternion.identity;

        Debug.Log($"Coin {Type} reset");
    }

    public void SetSpawnPoint(Vector3 position)
    {
        spawnPosition = position;
    }
}
