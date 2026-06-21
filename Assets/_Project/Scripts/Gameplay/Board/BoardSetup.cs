using UnityEngine;

public class BoardSetup : MonoBehaviour
{
    [Header("Board Dimensions (standard carrom: 74cm x 74cm)")]
    public float boardSize = 2.9f;
    public float borderWidth = 0.18f;
    public float boardThickness = 0.08f;
    public float frameHeight = 0.04f;

    [Header("Pockets")]
    public float pocketRadius = 0.065f;
    public float pocketDepth = 0.03f;
    public float pocketRimWidth = 0.012f;

    [Header("Markings")]
    public float centerCircleRadius = 0.18f;
    public float innerCircleRadius = 0.06f;
    public float smallCircleRadius = 0.015f;
    public float baseLineDistance = 0.45f;
    public float arrowLength = 0.25f;

    [Header("Layers")]
    public LayerMask coinLayer;

    private void Start()
    {
        BuildProfessionalBoard();
    }

    private void BuildProfessionalBoard()
    {
        GameObject boardRoot = new GameObject("CarromBoard");
        boardRoot.transform.position = Vector3.zero;

        CreateTableSurface(boardRoot);
        CreatePlayingSurface(boardRoot);
        CreateWoodFrame(boardRoot);
        CreatePockets(boardRoot);
        CreateCenterDesign(boardRoot);
        CreateBaseLines(boardRoot);
        CreateOuterLines(boardRoot);
        CreateArrowMarkers(boardRoot);
        CreateCornerDecorations(boardRoot);

        BoxCollider boardCollider = boardRoot.AddComponent<BoxCollider>();
        boardCollider.center = new Vector3(0, boardThickness / 2f, 0);
        boardCollider.size = new Vector3(boardSize + borderWidth * 2, boardThickness, boardSize + borderWidth * 2);

        BoardController boardController = boardRoot.AddComponent<BoardController>();
        boardController.SetPocketLayer(coinLayer);

        SetupLighting(boardRoot);
    }

    private void CreateTableSurface(GameObject parent)
    {
        GameObject table = GameObject.CreatePrimitive(PrimitiveType.Cube);
        table.name = "Table";
        table.transform.parent = parent.transform;
        table.transform.localPosition = new Vector3(0, -0.05f, 0);
        table.transform.localScale = new Vector3(boardSize + 1.2f, 0.1f, boardSize + 1.2f);

        Material tableMat = CreateMaterial(new Color(0.18f, 0.11f, 0.06f), 0.2f, 0.6f);
        table.GetComponent<MeshRenderer>().sharedMaterial = tableMat;
    }

    private void CreatePlayingSurface(GameObject parent)
    {
        GameObject surface = GameObject.CreatePrimitive(PrimitiveType.Cube);
        surface.name = "PlayingSurface";
        surface.transform.parent = parent.transform;
        surface.transform.localPosition = new Vector3(0, boardThickness / 2f + 0.001f, 0);
        surface.transform.localScale = new Vector3(boardSize, 0.005f, boardSize);

        Material surfaceMat = CreateMaterial(new Color(0.15f, 0.38f, 0.18f), 0.05f, 0.7f);
        surface.GetComponent<MeshRenderer>().sharedMaterial = surfaceMat;
    }

    private void CreateWoodFrame(GameObject parent)
    {
        Color woodColor = new Color(0.45f, 0.25f, 0.12f);
        Color woodDark = new Color(0.35f, 0.18f, 0.08f);
        float outerSize = boardSize + borderWidth * 2f;

        string[] frameNames = { "Frame_Top", "Frame_Bottom", "Frame_Left", "Frame_Right" };
        Vector3[] positions = {
            new Vector3(0, boardThickness / 2f, boardSize / 2f + borderWidth / 2f),
            new Vector3(0, boardThickness / 2f, -(boardSize / 2f + borderWidth / 2f)),
            new Vector3(-(boardSize / 2f + borderWidth / 2f), boardThickness / 2f, 0),
            new Vector3(boardSize / 2f + borderWidth / 2f, boardThickness / 2f, 0)
        };
        Vector3[] scales = {
            new Vector3(outerSize, frameHeight, borderWidth),
            new Vector3(outerSize, frameHeight, borderWidth),
            new Vector3(borderWidth, frameHeight, outerSize),
            new Vector3(borderWidth, frameHeight, outerSize)
        };

        for (int i = 0; i < 4; i++)
        {
            GameObject frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frame.name = frameNames[i];
            frame.transform.parent = parent.transform;
            frame.transform.localPosition = positions[i];
            frame.transform.localScale = scales[i];

            Material frameMat = CreateMaterial(i % 2 == 0 ? woodColor : woodDark, 0.15f, 0.55f);
            frame.GetComponent<MeshRenderer>().sharedMaterial = frameMat;
        }

        CreateInnerRim(parent, woodDark);
    }

    private void CreateInnerRim(GameObject parent, Color rimColor)
    {
        float rimWidth = 0.025f;
        float innerSize = boardSize - rimWidth * 2f;

        string[] rimNames = { "Rim_Top", "Rim_Bottom", "Rim_Left", "Rim_Right" };
        Vector3[] positions = {
            new Vector3(0, boardThickness / 2f + 0.003f, boardSize / 2f - rimWidth / 2f),
            new Vector3(0, boardThickness / 2f + 0.003f, -(boardSize / 2f - rimWidth / 2f)),
            new Vector3(-(boardSize / 2f - rimWidth / 2f), boardThickness / 2f + 0.003f, 0),
            new Vector3(boardSize / 2f - rimWidth / 2f, boardThickness / 2f + 0.003f, 0)
        };
        Vector3[] scales = {
            new Vector3(innerSize, 0.006f, rimWidth),
            new Vector3(innerSize, 0.006f, rimWidth),
            new Vector3(rimWidth, 0.006f, innerSize),
            new Vector3(rimWidth, 0.006f, innerSize)
        };

        for (int i = 0; i < 4; i++)
        {
            GameObject rim = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rim.name = rimNames[i];
            rim.transform.parent = parent.transform;
            rim.transform.localPosition = positions[i];
            rim.transform.localScale = scales[i];

            Material rimMat = CreateMaterial(new Color(0.55f, 0.32f, 0.15f), 0.1f, 0.7f);
            rim.GetComponent<MeshRenderer>().sharedMaterial = rimMat;
        }
    }

    private void CreatePockets(GameObject parent)
    {
        Vector3[] pocketPositions = CalculatePocketPositions();
        Material pocketMat = CreateMaterial(new Color(0.02f, 0.02f, 0.02f), 0.1f, 0.3f);
        Material rimMat = CreateMaterial(new Color(0.75f, 0.65f, 0.2f), 0.8f, 0.9f);

        for (int i = 0; i < 4; i++)
        {
            GameObject pocketGroup = new GameObject($"Pocket_{i}");
            pocketGroup.transform.parent = parent.transform;
            pocketGroup.transform.position = pocketPositions[i];

            GameObject pocketHole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pocketHole.name = "Hole";
            pocketHole.transform.parent = pocketGroup.transform;
            pocketHole.transform.localPosition = new Vector3(0, -0.005f, 0);
            pocketHole.transform.localScale = new Vector3(pocketRadius * 2f, 0.02f, pocketRadius * 2f);
            pocketHole.GetComponent<MeshRenderer>().sharedMaterial = pocketMat;

            GameObject pocketRim = GameObject.CreatePrimitive(PrimitiveType.Torus);
            pocketRim.name = "Rim";
            pocketRim.transform.parent = pocketGroup.transform;
            pocketRim.transform.localPosition = new Vector3(0, 0.003f, 0);
            pocketRim.transform.localScale = new Vector3(pocketRadius + pocketRimWidth, pocketRimWidth, pocketRadius + pocketRimWidth);
            pocketRim.GetComponent<MeshRenderer>().sharedMaterial = rimMat;

            SphereCollider pocketCollider = pocketGroup.AddComponent<SphereCollider>();
            pocketCollider.radius = pocketRadius;
            pocketCollider.isTrigger = true;

            PocketTrigger pocketTrigger = pocketGroup.AddComponent<PocketTrigger>();
            pocketTrigger.coinLayer = coinLayer;
            pocketTrigger.pocketRadius = pocketRadius;

            GameObject pocketLight = new GameObject("PocketGlow");
            pocketLight.transform.parent = pocketGroup.transform;
            pocketLight.transform.localPosition = new Vector3(0, -0.01f, 0);
            Light pLight = pocketLight.AddComponent<Light>();
            pLight.type = LightType.Point;
            pLight.color = new Color(1f, 0.85f, 0.3f);
            pLight.intensity = 0.3f;
            pLight.range = 0.15f;
        }
    }

    private void CreateCenterDesign(GameObject parent)
    {
        float surfaceY = boardThickness / 2f + 0.003f;

        CreateCircleOnBoard(parent, "CenterCircle_Outer", centerCircleRadius, surfaceY,
            new Color(0.9f, 0.85f, 0.7f), 0.012f);

        CreateCircleOnBoard(parent, "CenterCircle_Inner", innerCircleRadius, surfaceY,
            new Color(0.9f, 0.85f, 0.7f), 0.012f);

        for (int i = 0; i < 4; i++)
        {
            float angle = i * 90f * Mathf.Deg2Rad;
            float x = Mathf.Cos(angle) * (centerCircleRadius + 0.05f);
            float z = Mathf.Sin(angle) * (centerCircleRadius + 0.05f);

            CreateSmallDot(parent, $"CenterDot_{i}", new Vector3(x, surfaceY, z), smallCircleRadius,
                new Color(0.9f, 0.85f, 0.7f));
        }

        CreateCircleOnBoard(parent, "CenterDot_Main", smallCircleRadius * 1.5f, surfaceY,
            new Color(0.8f, 0.15f, 0.1f), 0.015f);
    }

    private void CreateBaseLines(GameObject parent)
    {
        float surfaceY = boardThickness / 2f + 0.003f;
        float lineLength = 0.35f;
        float lineThickness = 0.012f;

        Vector3[] basePositions = {
            new Vector3(0, surfaceY, baseLineDistance),
            new Vector3(0, surfaceY, -baseLineDistance),
            new Vector3(baseLineDistance, surfaceY, 0),
            new Vector3(-baseLineDistance, surfaceY, 0)
        };

        for (int i = 0; i < 4; i++)
        {
            GameObject line = new GameObject($"BaseLine_{i}");
            line.transform.parent = parent.transform;
            line.transform.position = basePositions[i];

            bool isHorizontal = i < 2;
            line.transform.rotation = Quaternion.Euler(0, isHorizontal ? 0 : 90f, 0);

            CreateLineSegment(line, lineLength, lineThickness, surfaceY, new Color(0.9f, 0.85f, 0.7f));
            CreateArcAtBase(line, isHorizontal, surfaceY);
        }
    }

    private void CreateOuterLines(GameObject parent)
    {
        float surfaceY = boardThickness / 2f + 0.003f;
        float offset = 0.06f;

        float outerDist = boardSize / 2f - offset;
        float lineThickness = 0.008f;

        Vector3[] outerPositions = {
            new Vector3(0, surfaceY, outerDist),
            new Vector3(0, surfaceY, -outerDist),
            new Vector3(outerDist, surfaceY, 0),
            new Vector3(-outerDist, surfaceY, 0)
        };

        for (int i = 0; i < 4; i++)
        {
            GameObject line = new GameObject($"OuterLine_{i}");
            line.transform.parent = parent.transform;
            line.transform.position = outerPositions[i];

            bool isHorizontal = i < 2;
            line.transform.rotation = Quaternion.Euler(0, isHorizontal ? 0 : 90f, 0);

            float segLength = boardSize - offset * 4f;
            CreateLineSegment(line, segLength, lineThickness, surfaceY, new Color(0.85f, 0.8f, 0.65f));
        }
    }

    private void CreateArrowMarkers(GameObject parent)
    {
        float surfaceY = boardThickness / 2f + 0.003f;
        Color arrowColor = new Color(0.9f, 0.85f, 0.7f);

        Vector3[] arrowPositions = {
            new Vector3(0, surfaceY, baseLineDistance + 0.05f),
            new Vector3(0, surfaceY, -(baseLineDistance + 0.05f)),
            new Vector3(baseLineDistance + 0.05f, surfaceY, 0),
            new Vector3(-(baseLineDistance + 0.05f), surfaceY, 0)
        };

        float[] arrowRotations = { 0f, 180f, 270f, 90f };

        for (int i = 0; i < 4; i++)
        {
            GameObject arrowGroup = new GameObject($"Arrow_{i}");
            arrowGroup.transform.parent = parent.transform;
            arrowGroup.transform.position = arrowPositions[i];
            arrowGroup.transform.rotation = Quaternion.Euler(0, arrowRotations[i], 0);

            CreateArrowShape(arrowGroup, arrowLength, surfaceY, arrowColor);
        }
    }

    private void CreateCornerDecorations(GameObject parent)
    {
        float surfaceY = boardThickness / 2f + 0.003f;
        float cornerOffset = boardSize / 2f - 0.03f;
        Color decorColor = new Color(0.85f, 0.75f, 0.2f);

        Vector3[] corners = {
            new Vector3(-cornerOffset, surfaceY, cornerOffset),
            new Vector3(cornerOffset, surfaceY, cornerOffset),
            new Vector3(-cornerOffset, surfaceY, -cornerOffset),
            new Vector3(cornerOffset, surfaceY, -cornerOffset)
        };

        for (int i = 0; i < 4; i++)
        {
            CreateSmallDot(parent, $"CornerDecor_{i}", corners[i], 0.01f, decorColor);
        }
    }

    private void CreateCircleOnBoard(GameObject parent, string name, float radius, float y, Color color, float thickness)
    {
        GameObject circle = new GameObject(name);
        circle.transform.parent = parent.transform;
        circle.transform.position = new Vector3(0, y, 0);

        int segments = 48;
        Vector3[] vertices = new Vector3[segments * 2 + 2];
        int[] triangles = new int[segments * 6];

        for (int i = 0; i <= segments; i++)
        {
            float angle = (float)i / segments * Mathf.PI * 2f;
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);

            vertices[i * 2] = new Vector3(cos * radius, 0, sin * radius);
            vertices[i * 2 + 1] = new Vector3(cos * (radius + thickness), 0, sin * (radius + thickness));
        }

        for (int i = 0; i < segments; i++)
        {
            int baseIdx = i * 6;
            int vIdx = i * 2;

            triangles[baseIdx] = vIdx;
            triangles[baseIdx + 1] = vIdx + 1;
            triangles[baseIdx + 2] = vIdx + 2;

            triangles[baseIdx + 3] = vIdx + 1;
            triangles[baseIdx + 4] = vIdx + 3;
            triangles[baseIdx + 5] = vIdx + 2;
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        MeshFilter mf = circle.AddComponent<MeshFilter>();
        mf.sharedMesh = mesh;

        MeshRenderer mr = circle.AddComponent<MeshRenderer>();
        mr.sharedMaterial = CreateMaterial(color, 0.1f, 0.5f);
    }

    private void CreateSmallDot(GameObject parent, string name, Vector3 pos, float radius, Color color)
    {
        GameObject dot = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        dot.name = name;
        dot.transform.parent = parent.transform;
        dot.transform.position = pos;
        dot.transform.localScale = new Vector3(radius * 2f, 0.001f, radius * 2f);
        dot.GetComponent<MeshRenderer>().sharedMaterial = CreateMaterial(color, 0.1f, 0.5f);
    }

    private void CreateLineSegment(GameObject parent, float length, float thickness, float y, Color color)
    {
        GameObject line = GameObject.CreatePrimitive(PrimitiveType.Cube);
        line.name = "Line";
        line.transform.parent = parent.transform;
        line.transform.localPosition = new Vector3(0, 0.001f, 0);
        line.transform.localScale = new Vector3(length, 0.002f, thickness);
        line.GetComponent<MeshRenderer>().sharedMaterial = CreateMaterial(color, 0.1f, 0.5f);
    }

    private void CreateArcAtBase(GameObject parent, bool horizontal, float y)
    {
        float arcRadius = 0.08f;
        int segments = 24;

        Vector3[] verts = new Vector3[segments + 1];
        int[] tris = new int[segments * 3];

        for (int i = 0; i <= segments; i++)
        {
            float angle = ((float)i / segments - 0.5f) * Mathf.PI;
            float xPos = Mathf.Cos(angle) * arcRadius;
            float zPos = Mathf.Sin(angle) * arcRadius * 0.3f;
            verts[i] = new Vector3(xPos, 0.002f, zPos);
        }

        for (int i = 0; i < segments; i++)
        {
            tris[i * 3] = 0;
            tris[i * 3 + 1] = i;
            tris[i * 3 + 2] = i + 1;
        }

        Mesh mesh = new Mesh();
        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.RecalculateNormals();

        GameObject arc = new GameObject("Arc");
        arc.transform.parent = parent.transform;
        arc.AddComponent<MeshFilter>().sharedMesh = mesh;
        arc.AddComponent<MeshRenderer>().sharedMaterial = CreateMaterial(new Color(0.9f, 0.85f, 0.7f), 0.1f, 0.5f);
    }

    private void CreateArrowShape(GameObject parent, float length, float y, Color color)
    {
        float w = 0.03f;
        Mesh mesh = new Mesh();
        mesh.vertices = new Vector3[]
        {
            new Vector3(0, 0, 0),
            new Vector3(-w, 0, length),
            new Vector3(0, 0, length * 0.7f),
            new Vector3(w, 0, length)
        };
        mesh.triangles = new int[] { 0, 1, 2, 0, 2, 3 };
        mesh.RecalculateNormals();

        GameObject arrow = new GameObject("ArrowShape");
        arrow.transform.parent = parent.transform;
        arrow.transform.localPosition = new Vector3(0, 0.003f, 0);
        arrow.AddComponent<MeshFilter>().sharedMesh = mesh;
        arrow.AddComponent<MeshRenderer>().sharedMaterial = CreateMaterial(color, 0.1f, 0.5f);
    }

    private Vector3[] CalculatePocketPositions()
    {
        float half = boardSize / 2f - pocketRadius * 0.3f;
        return new Vector3[]
        {
            new Vector3(-half, boardThickness / 2f + 0.005f, half),
            new Vector3(half, boardThickness / 2f + 0.005f, half),
            new Vector3(-half, boardThickness / 2f + 0.005f, -half),
            new Vector3(half, boardThickness / 2f + 0.005f, -half)
        };
    }

    private void SetupLighting(GameObject parent)
    {
        GameObject lightingObj = new GameObject("BoardLighting");
        lightingObj.transform.parent = parent.transform;
        lightingObj.transform.position = new Vector3(0, 2f, 0);

        Light mainLight = lightingObj.AddComponent<Light>();
        mainLight.type = LightType.Directional;
        mainLight.color = new Color(1f, 0.97f, 0.92f);
        mainLight.intensity = 1.2f;
        mainLight.shadows = LightShadows.Soft;
        mainLight.shadowStrength = 0.7f;
        lightingObj.transform.rotation = Quaternion.Euler(50f, -30f, 0);

        CreateFillLight(parent, new Vector3(-1.5f, 1.5f, -1.5f), new Color(0.6f, 0.7f, 0.9f), 0.4f);
        CreateFillLight(parent, new Vector3(1.5f, 1.5f, 1.5f), new Color(0.9f, 0.85f, 0.7f), 0.3f);
    }

    private void CreateFillLight(GameObject parent, Vector3 pos, Color color, float intensity)
    {
        GameObject fillObj = new GameObject("FillLight");
        fillObj.transform.parent = parent.transform;
        fillObj.transform.position = pos;

        Light fill = fillObj.AddComponent<Light>();
        fill.type = LightType.Point;
        fill.color = color;
        fill.intensity = intensity;
        fill.range = 5f;
    }

    private Material CreateMaterial(Color color, float metallic, float smoothness)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        Material mat = new Material(shader);
        mat.color = color;
        mat.SetFloat("_Metallic", metallic);
        mat.SetFloat("_Smoothness", smoothness);
        return mat;
    }
}
