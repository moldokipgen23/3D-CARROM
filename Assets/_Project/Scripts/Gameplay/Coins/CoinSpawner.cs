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
    public float coinRadius = 0.038f;
    public float coinThickness = 0.012f;
    public float coinSpacing = 0.005f;

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
        GameObject coinGroup = BuildCoinObject(name, pos, bodyMat, rimMat, type == CoinType.White
            ? new Color(0.9f, 0.85f, 0.75f)
            : new Color(0.18f, 0.15f, 0.1f));

        Coin coinScript = coinGroup.AddComponent<Coin>();
        coinScript.Type = type;
    }

    private void CreateQueen(string name, Vector3 pos, Material bodyMat, Material rimMat)
    {
        GameObject coinGroup = BuildCoinObject(name, pos, bodyMat, rimMat, new Color(0.75f, 0.1f, 0.1f));

        GameObject crownDot = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        crownDot.name = "CrownDot";
        crownDot.transform.parent = coinGroup.transform;
        crownDot.transform.localPosition = new Vector3(0, coinThickness * 0.5f, 0);
        crownDot.transform.localScale = new Vector3(coinRadius * 0.6f, coinThickness * 0.1f, coinRadius * 0.6f);
        crownDot.GetComponent<MeshRenderer>().sharedMaterial = CreateMaterial(new Color(0.9f, 0.75f, 0.2f), 0.85f, 0.95f);
        RemoveCollider(crownDot);

        Coin coinScript = coinGroup.AddComponent<Coin>();
        coinScript.Type = CoinType.Queen;
    }

    private GameObject BuildCoinObject(string name, Vector3 pos, Material bodyMat, Material rimMat, Color topColor)
    {
        GameObject coinGroup = new GameObject(name);
        coinGroup.transform.position = pos;
        coinGroup.tag = "Coin";
        coinGroup.layer = LayerMask.NameToLayer("Default");

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        body.name = "Body";
        body.transform.parent = coinGroup.transform;
        body.transform.localPosition = Vector3.zero;
        body.transform.localScale = new Vector3(coinRadius * 2f, coinThickness / 2f, coinRadius * 2f);
        body.GetComponent<MeshRenderer>().sharedMaterial = bodyMat;
        RemoveCollider(body);

        GameObject rim = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        rim.name = "Rim";
        rim.transform.parent = coinGroup.transform;
        rim.transform.localPosition = Vector3.zero;
        rim.transform.localScale = new Vector3(coinRadius * 2.15f, coinThickness / 2f * 0.95f, coinRadius * 2.15f);
        rim.GetComponent<MeshRenderer>().sharedMaterial = rimMat;
        RemoveCollider(rim);

        GameObject topDetail = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        topDetail.name = "TopDetail";
        topDetail.transform.parent = coinGroup.transform;
        topDetail.transform.localPosition = new Vector3(0, coinThickness * 0.35f, 0);
        topDetail.transform.localScale = new Vector3(coinRadius * 1.4f, coinThickness * 0.15f, coinRadius * 1.4f);
        topDetail.GetComponent<MeshRenderer>().sharedMaterial = CreateMaterial(topColor, 0.3f, 0.6f);
        RemoveCollider(topDetail);

        Rigidbody rb = coinGroup.AddComponent<Rigidbody>();
        rb.mass = 1f;
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezePositionY |
                         RigidbodyConstraints.FreezeRotationX |
                         RigidbodyConstraints.FreezeRotationZ;
        rb.linearDamping = 0.12f;
        rb.angularDamping = 0.5f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        // A thin box collider behaves like a disc and rests flat on the board.
        BoxCollider coinCollider = coinGroup.AddComponent<BoxCollider>();
        coinCollider.size = new Vector3(coinRadius * 2f, coinThickness, coinRadius * 2f);
        coinCollider.center = Vector3.zero;

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
