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

    private void Awake()
    {
        SetupPhysics();
        SetupVisuals();
    }

    private void SetupPhysics()
    {
        if (Rigidbody == null)
        {
            Rigidbody = gameObject.AddComponent<Rigidbody>();
        }

        Rigidbody.useGravity = true;
        Rigidbody.constraints = RigidbodyConstraints.FreezePositionY;
        Rigidbody.mass = 5f;

        if (PhysicsMaterial == null)
        {
            PhysicsMaterial = new PhysicsMaterial();
            PhysicsMaterial.dynamicFriction = 0.3f;
            PhysicsMaterial.staticFriction = 0.3f;
            PhysicsMaterial.bounciness = 0.1f;
            PhysicsMaterial.frictionCombine = PhysicsMaterialCombine.Minimum;
            PhysicsMaterial.bounceCombine = PhysicsMaterialCombine.Minimum;
        }

        Collider col = GetComponent<Collider>();
        if (col != null)
            col.sharedMaterial = PhysicsMaterial;
    }

    private void SetupVisuals()
    {
        if (MeshRenderer == null)
        {
            MeshRenderer = GetComponent<MeshRenderer>();
        }

        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        Color coinColor = Type == CoinType.White ? Color.white :
                         Type == CoinType.Black ? Color.black :
                         Color.red;

        if (MeshRenderer != null)
        {
            MeshRenderer.material.color = coinColor;
            MeshRenderer.material.SetFloat("_Smoothness", 0.6f);
        }
    }

    public void Pocket()
    {
        if (!IsPocketed)
        {
            IsPocketed = true;
            Rigidbody.velocity = Vector3.zero;
            Rigidbody.angularVelocity = Vector3.zero;
            transform.parent = transform.root.Find("Board").Find("Pocket0");
            Debug.Log($"Coin {Type} pocketed");
        }
    }

    public void ResetCoin()
    {
        IsPocketed = false;
        Rigidbody.velocity = Vector3.zero;
        Rigidbody.angularVelocity = Vector3.zero;
        transform.parent = null;
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        Debug.Log($"Coin {Type} reset");
    }
}