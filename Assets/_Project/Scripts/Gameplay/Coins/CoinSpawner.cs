using UnityEngine;

public class CoinSpawner : MonoBehaviour
{
    [Header("Board Settings")]
    public float boardSize = 2.9f;
    public float boardThickness = 0.08f;

    [Header("Coin Counts")]
    public int whiteCoinCount = 9;
    public int blackCoinCount = 9;

    [Header("Coin Dimensions")]
    // 1.5x ICF scale for phone readability, matching commercial carrom titles.
    public float coinRadius = 0.1008f;
    public float coinThickness = 0.055f;
    public float coinSpacing = 0.0027f;
    public float coinBevel = 0.012f;

    [Header("Spawn")]
    public Vector3[] spawnPositions = new Vector3[19];

    private bool spawned = false;

    private void Start()
    {
        GenerateSpawnPositions();
        SpawnCoins();
    }

    private float BoardTopY()
    {
        // Playing surface cube: centre boardThickness/2 + 0.001, half-height 0.0025.
        return boardThickness / 2f + 0.0035f;
    }

    /// <summary>
    /// Official carrom formation, fully deterministic: queen in the centre,
    /// a ring of 6 alternating coins, then a ring of 12 alternating coins.
    /// </summary>
    private void GenerateSpawnPositions()
    {
        float y = BoardTopY() + coinThickness / 2f + 0.0005f;
        float step = coinRadius * 2f + coinSpacing;

        spawnPositions[0] = new Vector3(0f, y, 0f);

        int index = 1;
        index = AddRing(1, 6, step, index, y);
        AddRing(2, 12, step, index, y);
    }

    private int AddRing(int ring, int count, float step, int index, float y)
    {
        float radius = ring * step;

        for (int i = 0; i < count && index < spawnPositions.Length; i++)
        {
            float angle = (Mathf.PI * 2f * i / count) + (ring == 1 ? 0f : Mathf.PI / count);
            spawnPositions[index] = new Vector3(
                Mathf.Cos(angle) * radius,
                y,
                Mathf.Sin(angle) * radius);
            index++;
        }

        return index;
    }

    public void SpawnCoins()
    {
        if (spawned) return;
        spawned = true;

        if (spawnPositions == null || spawnPositions.Length < 19 || spawnPositions[1] == Vector3.zero)
            GenerateSpawnPositions();

        Material whiteMat = CreateCoinMaterial(
            new Color(0.95f, 0.92f, 0.85f),
            new Color(0.85f, 0.8f, 0.7f),
            0.6f, 0.85f);
        Material blackMat = CreateCoinMaterial(
            new Color(0.12f, 0.1f, 0.08f),
            new Color(0.25f, 0.2f, 0.15f),
            0.5f, 0.8f);
        Material queenMat = CreateQueenMaterial();
        Material goldRimMat = CreateMaterial(new Color(0.85f, 0.7f, 0.2f), 0.85f, 0.95f);

        // Centre: queen.
        CreateQueen("Queen", spawnPositions[0], queenMat, goldRimMat);

        int whiteMade = 0;
        int blackMade = 0;

        for (int i = 1; i < 19; i++)
        {
            bool isWhite = (i % 2) == 0;

            // Inner ring (1-6) and outer ring (7-18) each alternate colours.
            if (isWhite && whiteMade < whiteCoinCount)
            {
                CreateCoin($"WhiteCoin_{whiteMade + 1}", spawnPositions[i], whiteMat, goldRimMat, CoinType.White);
                whiteMade++;
            }
            else if (!isWhite && blackMade < blackCoinCount)
            {
                CreateCoin($"BlackCoin_{blackMade + 1}", spawnPositions[i], blackMat, goldRimMat, CoinType.Black);
                blackMade++;
            }
            else if (whiteMade < whiteCoinCount)
            {
                CreateCoin($"WhiteCoin_{whiteMade + 1}", spawnPositions[i], whiteMat, goldRimMat, CoinType.White);
                whiteMade++;
            }
            else if (blackMade < blackCoinCount)
            {
                CreateCoin($"BlackCoin_{blackMade + 1}", spawnPositions[i], blackMat, goldRimMat, CoinType.Black);
                blackMade++;
            }
        }

        Debug.Log($"CoinSpawner: spawned {whiteMade} white, {blackMade} black and 1 queen");
    }

    private void CreateCoin(string name, Vector3 pos, Material bodyMat, Material rimMat, CoinType type)
    {
        GameObject coinGroup = BuildCoinObject(name, pos, type);

        Coin coinScript = coinGroup.AddComponent<Coin>();
        coinScript.Type = type;
    }

    private void CreateQueen(string name, Vector3 pos, Material bodyMat, Material rimMat)
    {
        // The queen motif is printed on the face, so no extra geometry is needed.
        GameObject coinGroup = BuildCoinObject(name, pos, CoinType.Queen);

        Coin coinScript = coinGroup.AddComponent<Coin>();
        coinScript.Type = CoinType.Queen;
    }

    private static Texture2D whiteFaceTexture;
    private static Texture2D blackFaceTexture;
    private static Texture2D queenFaceTexture;

    private Texture2D FaceTexture(CoinType type)
    {
        if (type == CoinType.White)
        {
            if (whiteFaceTexture == null)
                whiteFaceTexture = ProceduralTextures.CreateCoinFaceTexture(
                    new Color(0.97f, 0.94f, 0.87f), new Color(0.79f, 0.67f, 0.40f), false);
            return whiteFaceTexture;
        }

        if (type == CoinType.Black)
        {
            if (blackFaceTexture == null)
                blackFaceTexture = ProceduralTextures.CreateCoinFaceTexture(
                    new Color(0.14f, 0.12f, 0.10f), new Color(0.42f, 0.33f, 0.19f), false);
            return blackFaceTexture;
        }

        if (queenFaceTexture == null)
            queenFaceTexture = ProceduralTextures.CreateCoinFaceTexture(
                new Color(0.72f, 0.10f, 0.09f), new Color(0.96f, 0.80f, 0.36f), true);
        return queenFaceTexture;
    }

    private GameObject BuildCoinObject(string name, Vector3 pos, CoinType type)
    {
        bool white = type == CoinType.White;
        bool queen = type == CoinType.Queen;

        Color faceColor = queen ? new Color(0.72f, 0.10f, 0.09f)
                        : white ? new Color(0.97f, 0.94f, 0.87f)
                                : new Color(0.13f, 0.11f, 0.09f);

        Color rimColor = queen ? new Color(0.91f, 0.73f, 0.31f)
                       : white ? new Color(0.86f, 0.79f, 0.64f)
                               : new Color(0.30f, 0.23f, 0.15f);

        Material rimMat = CreateMaterial(rimColor, queen ? 0.80f : 0.35f, 0.92f);
        Material faceMat = CreateMaterial(Color.white, 0.05f, 0.74f);
        faceMat.mainTexture = FaceTexture(type);

        GameObject coinGroup = new GameObject(name);
        coinGroup.transform.position = pos;
        coinGroup.tag = "Coin";
        coinGroup.layer = LayerMask.NameToLayer("Default");

        // One beveled disc mesh: a rounded rim that catches the key light plus a
        // domed printed face. Two submeshes give the rim and face their own look.
        GameObject body = new GameObject("Body");
        body.transform.SetParent(coinGroup.transform, false);

        MeshFilter filter = body.AddComponent<MeshFilter>();
        filter.sharedMesh = PieceMeshFactory.GetDisc(
            "Coin_" + type, coinRadius, coinThickness, coinBevel);

        MeshRenderer renderer = body.AddComponent<MeshRenderer>();
        renderer.sharedMaterials = new Material[] { rimMat, faceMat };
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        renderer.receiveShadows = true;

        // Soft contact shadow so the coin sits on the board instead of floating.
        PieceMeshFactory.CreateContactShadow(
            coinGroup.transform, coinRadius, -coinThickness * 0.5f + 0.0008f, 2.5f);

        Rigidbody rb = coinGroup.AddComponent<Rigidbody>();
        rb.mass = 0.8f;
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezePositionY |
                         RigidbodyConstraints.FreezeRotationX |
                         RigidbodyConstraints.FreezeRotationZ;
        rb.linearDamping = 0.85f;
        rb.angularDamping = 1.10f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        // Round convex footprint: box colliders snag on their corners and make
        // cut shots unpredictable.
        MeshCollider coinCollider = coinGroup.AddComponent<MeshCollider>();
        coinCollider.convex = true;
        coinCollider.sharedMesh = PieceMeshFactory.GetColliderCylinder(
            "CoinCollider_" + type, coinRadius * 0.98f, coinThickness);

        return coinGroup;
    }

    private void RemoveCollider(GameObject go)
    {
        if (go == null) return;
        Collider collider = go.GetComponent<Collider>();
        if (collider == null) return;
        collider.enabled = false;
        Destroy(collider);
    }

    private Material CreateCoinMaterial(Color mainColor, Color darkColor, float metallic, float smoothness)
    {
        Material mat = CreateMaterial(mainColor, metallic, smoothness);
        return mat;
    }

    private Material CreateQueenMaterial()
    {
        return CreateMaterial(new Color(0.8f, 0.12f, 0.12f), 0.5f, 0.9f);
    }

    private Material CreateMaterial(Color color, float metallic, float smoothness)
    {
        Shader shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Diffuse");
        Material mat = new Material(shader);
        mat.color = color;
        mat.SetFloat("_Metallic", metallic);
        mat.SetFloat("_Glossiness", smoothness);
        return mat;
    }

    public void ResetCoins()
    {
        Coin[] coins = FindObjectsOfType<Coin>(true);
        for (int i = 0; i < coins.Length; i++)
        {
            if (coins[i] != null)
                coins[i].ResetCoin();
        }
    }
}
