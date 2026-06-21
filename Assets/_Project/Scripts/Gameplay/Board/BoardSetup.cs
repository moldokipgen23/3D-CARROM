using UnityEngine;

public class BoardSetup : MonoBehaviour
{
    [Header("Board Prefab References")]
    public GameObject boardPrefab;
    public GameObject pocketPrefab;
    public GameObject centerCirclePrefab;

    [Header("Board Settings")]
    public float boardWidth = 2.9f;
    public float boardHeight = 2.9f;
    public float borderWidth = 0.15f;
    public float pocketRadius = 0.05f;
    public float pocketInset = 0.05f;
    public float centerCircleDiameter = 0.16f;

    [Header("Materials")]
    public Material boardMaterial;
    public Material pocketMaterial;

    [Header("Coin Layer")]
    public LayerMask coinLayer;

    private void Start()
    {
        CreateBoard();
    }

    private void CreateBoard()
    {
        if (boardPrefab == null)
        {
            Debug.LogError("Board prefab is not assigned");
            return;
        }

        GameObject board = new GameObject("Board");
        board.transform.position = Vector3.zero;

        MeshRenderer boardRenderer = board.AddComponent<MeshRenderer>();
        boardRenderer.sharedMaterial = boardMaterial != null ? boardMaterial : GetDefaultBoardMaterial();

        MeshFilter boardFilter = board.AddComponent<MeshFilter>();
        boardFilter.sharedMesh = CreateBoardMesh();

        BoxCollider boardCollider = board.AddComponent<BoxCollider>();
        boardCollider.center = Vector3.zero;
        boardCollider.size = new Vector3(boardWidth, 0.01f, boardHeight);

        CreatePockets(board);
        CreateCenterCircle(board);

        BoardController boardController = board.AddComponent<BoardController>();
        boardController.SetPocketLayer(coinLayer);

        Debug.Log($"Board created with dimensions: {boardWidth}x{boardHeight}");
    }

    private void CreatePockets(GameObject board)
    {
        Vector3[] pocketPositions = CalculatePocketPositions();

        for (int i = 0; i < 4; i++)
        {
            GameObject pocket = new GameObject($"Pocket{i}");
            pocket.transform.parent = board.transform;
            pocket.transform.position = pocketPositions[i];

            SphereCollider pocketCollider = pocket.AddComponent<SphereCollider>();
            pocketCollider.radius = pocketRadius;
            pocketCollider.isTrigger = true;

            MeshRenderer pocketRenderer = pocket.AddComponent<MeshRenderer>();
            pocketRenderer.sharedMaterial = pocketMaterial != null ? pocketMaterial : GetDefaultPocketMaterial();

            MeshFilter pocketFilter = pocket.AddComponent<MeshFilter>();
            pocketFilter.sharedMesh = CreateSphereMesh(pocketRadius);

            PocketTrigger pocketTrigger = pocket.AddComponent<PocketTrigger>();
            pocketTrigger.coinLayer = coinLayer;
            pocketTrigger.pocketRadius = pocketRadius;
        }
    }

    private void CreateCenterCircle(GameObject board)
    {
        float centerCircleRadius = centerCircleDiameter / 2f;
        Vector3 centerCirclePosition = Vector3.zero;

        GameObject centerCircle = new GameObject("CenterCircle");
        centerCircle.transform.parent = board.transform;
        centerCircle.transform.position = centerCirclePosition;

        MeshRenderer circleRenderer = centerCircle.AddComponent<MeshRenderer>();
        circleRenderer.sharedMaterial = GetDefaultCenterCircleMaterial();

        MeshFilter circleFilter = centerCircle.AddComponent<MeshFilter>();
        circleFilter.sharedMesh = CreateCircleMesh(centerCircleRadius);
    }

    private Vector3[] CalculatePocketPositions()
    {
        Vector3[] positions = new Vector3[4];

        float halfWidth = boardWidth / 2f - pocketInset - pocketRadius;
        float halfHeight = boardHeight / 2f - pocketInset - pocketRadius;

        positions[0] = new Vector3(-halfWidth, 0, halfHeight);
        positions[1] = new Vector3(halfWidth, 0, halfHeight);
        positions[2] = new Vector3(-halfWidth, 0, -halfHeight);
        positions[3] = new Vector3(halfWidth, 0, -halfHeight);

        return positions;
    }

    private Mesh CreateBoardMesh()
    {
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[4]
        {
            new Vector3(-boardWidth / 2f, 0, -boardHeight / 2f),
            new Vector3(boardWidth / 2f, 0, -boardHeight / 2f),
            new Vector3(boardWidth / 2f, 0, boardHeight / 2f),
            new Vector3(-boardWidth / 2f, 0, boardHeight / 2f)
        };

        int[] triangles = new int[]
        {
            0, 1, 2,
            2, 3, 0
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }

    private Mesh CreateSphereMesh(float radius)
    {
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[20]
        {
            new Vector3(0, radius, 0),
            new Vector3(radius * 0.951f, radius * 0.309f, 0),
            new Vector3(radius * 0.587f, -radius * 0.809f, 0),
            new Vector3(-radius * 0.587f, -radius * 0.809f, 0),
            new Vector3(-radius * 0.951f, radius * 0.309f, 0),
            new Vector3(0, -radius, 0)
        };

        int[] triangles = new int[]
        {
            0, 1, 4,
            1, 2, 4,
            2, 3, 4,
            3, 0, 4,
            0, 3, 2,
            0, 2, 1
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }

    private Mesh CreateCircleMesh(float radius)
    {
        Mesh mesh = new Mesh();

        int segments = 32;
        Vector3[] vertices = new Vector3[segments + 1];
        int[] triangles = new int[segments * 3];

        vertices[0] = Vector3.zero;

        for (int i = 0; i <= segments; i++)
        {
            float angle = (i * 360f / segments) * Mathf.Deg2Rad;
            vertices[i] = new Vector3(Mathf.Sin(angle) * radius, 0, Mathf.Cos(angle) * radius);
        }

        for (int i = 0; i < segments; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = (i + 1) % (segments + 1);
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }

    private Material GetDefaultBoardMaterial()
    {
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = new Color(0.3f, 0.2f, 0.15f);
        mat.metallic = 0.1f;
        mat.smoothness = 0.4f;
        return mat;
    }

    private Material GetDefaultPocketMaterial()
    {
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = Color.black;
        mat.metallic = 0.1f;
        mat.smoothness = 0.4f;
        return mat;
    }

    private Material GetDefaultCenterCircleMaterial()
    {
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = Color.white;
        mat.metallic = 0.1f;
        mat.smoothness = 0.4f;
        return mat;
    }
}