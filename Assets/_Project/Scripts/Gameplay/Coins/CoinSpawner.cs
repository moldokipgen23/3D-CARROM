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

    private void Start()
    {
        GenerateSpawnPositions();
        SpawnCoins();
    }

    private void GenerateSpawnPositions()
    {
        float centerX = 0f;
        float centerZ = 0f;
        float spacing = coinRadius * 2f + coinSpacing;
        float surfaceY = boardThickness / 2f + coinThickness / 2f + 0.002f;

        spawnPositions[0] = new Vector3(centerX, surfaceY, centerZ);

        int posIndex = 1;
        for (int ring = 1; ring <= 4; ring++)
        {
            int coinsInRing = ring * 6;
            if (posIndex >= 19) break;

            for (int i = 0; i < coinsInRing && posIndex < 19; i++)
            {
                float angle = (float)i / coinsInRing * Mathf.PI * 2f;
                float radius = ring * spacing;
                float x = centerX + Mathf.Cos(angle) * radius;
                float z = centerZ + Mathf.Sin(angle) * radius;
                spawnPositions[posIndex] = new Vector3(x, surfaceY, z);
                posIndex++;
            }
        }

        if (posIndex < 19)
        {
            float fallbackSpacing = spacing * 0.9f;
            for (int i = posIndex; i < 19; i++)
            {
                float row = (i - posIndex) / 3;
                float col = (i - posIndex) % 3 - 1;
                spawnPositions[i] = new Vector3(
                    centerX + col * fallbackSpacing,
                    surfaceY,
                    centerZ + (row + 2) * fallbackSpacing
                );
            }
        }
    }

    public void SpawnCoins()
    {
        float surfaceY = boardThickness / 2f + coinThickness / 2f + 0.003f;

        Material whiteMat = CreateCoinMaterial(
            new Color(0.95f, 0.92f, 0.85f),
            new Color(0.85f, 0.8f, 0.7f),
            0.6f, 0.85f
        );
        Material blackMat = CreateCoinMaterial(
            new Color(0.12f, 0.1f, 0.08f),
            new Color(0.25f, 0.2f, 0.15f),
            0.5f, 0.8f
        );
        Material queenMat = CreateQueenMaterial();
        Material goldRimMat = CreateMaterial(new Color(0.85f, 0.7f, 0.2f), 0.85f, 0.95f);

        int posIndex = 0;

        for (int i = 0; i < whiteCoinCount && posIndex < 19; i++)
        {
            CreateCoin($"WhiteCoin_{i + 1}", spawnPositions[posIndex], whiteMat, goldRimMat, CoinType.White);
            posIndex++;
        }

        for (int i = 0; i < blackCoinCount && posIndex < 19; i++)
        {
            CreateCoin($"BlackCoin_{i + 1}", spawnPositions[posIndex], blackMat, goldRimMat, CoinType.Black);
            posIndex++;
        }

        if (posIndex < 19)
        {
            CreateQueenCoin("Queen", spawnPositions[posIndex], queenMat, goldRimMat);
        }
    }

    private void CreateCoin(string name, Vector3 pos, Material bodyMat, Material rimMat, CoinType type)
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

        GameObject rim = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        rim.name = "Rim";
        rim.transform.parent = coinGroup.transform;
        rim.transform.localPosition = new Vector3(0, 0, 0);
        rim.transform.localScale = new Vector3(coinRadius * 2.15f, coinThickness / 2f * 0.95f, coinRadius * 2.15f);
        rim.GetComponent<MeshRenderer>().sharedMaterial = rimMat;

        GameObject topDetail = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        topDetail.name = "TopDetail";
        topDetail.transform.parent = coinGroup.transform;
        topDetail.transform.localPosition = new Vector3(0, coinThickness * 0.35f, 0);
        topDetail.transform.localScale = new Vector3(coinRadius * 1.4f, coinThickness * 0.15f, coinRadius * 1.4f);

        Color topColor = type == CoinType.White
            ? new Color(0.9f, 0.85f, 0.75f)
            : new Color(0.18f, 0.15f, 0.1f);
        topDetail.GetComponent<MeshRenderer>().sharedMaterial = CreateMaterial(topColor, 0.3f, 0.6f);

        Rigidbody rb = coinGroup.AddComponent<Rigidbody>();
        rb.mass = 0.015f;
        rb.drag = 0.5f;
        rb.angularDrag = 0.8f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        SphereCollider coinCollider = coinGroup.AddComponent<SphereCollider>();
        coinCollider.radius = coinRadius;
        coinCollider.center = Vector3.zero;

        Coin coinScript = coinGroup.AddComponent<Coin>();
        coinScript.Type = type;
    }

    private void CreateQueenCoin(string name, Vector3 pos, Material bodyMat, Material rimMat)
    {
        GameObject coinGroup = new GameObject(name);
        coinGroup.transform.position = pos;
        coinGroup.tag = "Coin";

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        body.name = "Body";
        body.transform.parent = coinGroup.transform;
        body.transform.localPosition = Vector3.zero;
        body.transform.localScale = new Vector3(coinRadius * 2f, coinThickness / 2f, coinRadius * 2f);
        body.GetComponent<MeshRenderer>().sharedMaterial = bodyMat;

        GameObject rim = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        rim.name = "Rim";
        rim.transform.parent = coinGroup.transform;
        rim.transform.localPosition = Vector3.zero;
        rim.transform.localScale = new Vector3(coinRadius * 2.2f, coinThickness / 2f * 0.95f, coinRadius * 2.2f);
        rim.GetComponent<MeshRenderer>().sharedMaterial = rimMat;

        GameObject topDetail = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        topDetail.name = "TopDetail";
        topDetail.transform.parent = coinGroup.transform;
        topDetail.transform.localPosition = new Vector3(0, coinThickness * 0.35f, 0);
        topDetail.transform.localScale = new Vector3(coinRadius * 1.3f, coinThickness * 0.15f, coinRadius * 1.3f);
        topDetail.GetComponent<MeshRenderer>().sharedMaterial = CreateMaterial(new Color(0.75f, 0.1f, 0.1f), 0.4f, 0.75f);

        GameObject crownDot = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        crownDot.name = "CrownDot";
        crownDot.transform.parent = coinGroup.transform;
        crownDot.transform.localPosition = new Vector3(0, coinThickness * 0.5f, 0);
        crownDot.transform.localScale = new Vector3(coinRadius * 0.6f, coinThickness * 0.1f, coinRadius * 0.6f);
        crownDot.GetComponent<MeshRenderer>().sharedMaterial = CreateMaterial(new Color(0.9f, 0.75f, 0.2f), 0.85f, 0.95f);

        Rigidbody rb = coinGroup.AddComponent<Rigidbody>();
        rb.mass = 0.02f;
        rb.drag = 0.5f;
        rb.angularDrag = 0.8f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        SphereCollider coinCollider = coinGroup.AddComponent<SphereCollider>();
        coinCollider.radius = coinRadius;

        Coin coinScript = coinGroup.AddComponent<Coin>();
        coinScript.Type = CoinType.Queen;
    }

    private Material CreateCoinMaterial(Color mainColor, Color darkColor, float metallic, float smoothness)
    {
        Shader shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
        Material mat = new Material(shader);
        mat.color = mainColor;
        mat.SetFloat("_Metallic", metallic);
        mat.SetFloat("_Glossiness", smoothness);
        return mat;
    }

    private Material CreateQueenMaterial()
    {
        Shader shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
        Material mat = new Material(shader);
        mat.color = new Color(0.8f, 0.12f, 0.12f);
        mat.SetFloat("_Metallic", 0.5f);
        mat.SetFloat("_Glossiness", 0.9f);
        return mat;
    }

    private Material CreateMaterial(Color color, float metallic, float smoothness)
    {
        Shader shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
        Material mat = new Material(shader);
        mat.color = color;
        mat.SetFloat("_Metallic", metallic);
        mat.SetFloat("_Glossiness", smoothness);
        return mat;
    }

    public void ResetCoins()
    {
        GameObject[] coins = GameObject.FindGameObjectsWithTag("Coin");
        for (int i = 0; i < coins.Length; i++)
        {
            if (coins[i] != null)
            {
                Coin coin = coins[i].GetComponent<Coin>();
                if (coin != null) coin.ResetCoin();
            }
        }
    }
}
