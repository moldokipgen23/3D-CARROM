using UnityEngine;

public class BoardSetup : MonoBehaviour
{
    [Header("Board Dimensions (ICF: 74 cm playing surface)")]
    public float boardSize = 2.9f;
    public float borderWidth = 0.12f;
    public float boardThickness = 0.08f;
    public float frameHeight = 0.086f;

    [Header("Pockets (ICF: 4.45 cm)")]
    // Scaled with the 1.5x pieces so the striker (r = 0.131) still drops in.
    public float pocketRadius = 0.140f;
    public float pocketRimWidth = 0.016f;

    [Header("Base Lines (ICF: 47 cm lines, 3.18 cm apart)")]
    public float baseLineDistance = 0.99f;
    public float baseLineLength = 1.842f;
    public float baseLineGap = 0.125f;
    public float baseCircleRadius = 0.0623f;
    public float baseRedRadius = 0.0498f;

    [Header("Centre (ICF: 3.18 cm centre circle, 17 cm outer circle)")]
    public float centerCircleRadius = 0.0623f;
    public float outerCircleRadius = 0.333f;
    public float innerCircleRadius = 0.032f;

    [Header("Corner Arrows")]
    public float arrowLength = 0.34f;
    public float arrowWidth = 0.022f;

    [Header("Layers")]
    public LayerMask coinLayer;

    // Traditional carrom colours: near-black lines with oxblood red circles.
    private static readonly Color LineBlack = new Color(0.075f, 0.055f, 0.042f);
    private static readonly Color LineRed = new Color(0.56f, 0.085f, 0.065f);
    private static readonly Color LineRedDark = new Color(0.36f, 0.05f, 0.04f);
    // Muted wood tones for the decorative centre mandala, so the rosette reads
    // as a carved board detail instead of bright paint under the coins.
    private static readonly Color LineBrown = new Color(0.600f, 0.420f, 0.245f);
    private static readonly Color LineBrownDark = new Color(0.480f, 0.330f, 0.195f);

    /// <summary>Top of the playing surface (surface cube: centre 0.041, half 0.0025).</summary>
    private float SurfaceTopY
    {
        get { return boardThickness / 2f + 0.0035f; }
    }

    private void Start()
    {
        BuildProfessionalBoard();
    }

    private void BuildProfessionalBoard()
    {
        GameObject boardRoot = new GameObject("CarromBoard");
        boardRoot.transform.SetParent(transform, false);
        boardRoot.transform.localPosition = Vector3.zero;
        boardRoot.transform.localRotation = Quaternion.identity;
        boardRoot.transform.localScale = Vector3.one;

        CreateTableSurface(boardRoot);
        CreateBoardBody(boardRoot);
        CreatePlayingSurface(boardRoot);
        CreateWoodFrame(boardRoot);
        CreatePockets(boardRoot);
        // Base lines, circles, arrows and the centre rosette are baked into the
        // high-resolution playing-surface texture. Separate floating quads used
        // to z-fight at grazing angles, which read as shimmering "animated"
        // lines, and they were limited to the texture-free 512 px surface.

        // The playing-surface cube provides the collision floor.  A second
        // full-size collider here would overlap the coins and cause jitter.
        BoardController boardController = GetComponent<BoardController>();
        if (boardController == null)
            boardController = boardRoot.AddComponent<BoardController>();

        boardController.SetupPockets();
        boardController.SetPocketLayer(coinLayer);

        SetupLighting(boardRoot);
    }

    private void CreateTableSurface(GameObject parent)
    {
        GameObject table = GameObject.CreatePrimitive(PrimitiveType.Cube);
        table.name = "Table";
        table.transform.parent = parent.transform;
        table.transform.localPosition = new Vector3(0, -0.075f, 0);
        table.transform.localScale = new Vector3(boardSize + 2.6f, 0.15f, boardSize + 2.6f);
        // Darken the table a touch so the board frame reads as a separate object.
        // Dark wine table under a bright orange-rimmed board: the playcourt is
        // the only lit object, exactly like the reference layout.
        Material tableMat = CreateMaterial(new Color(0.245f, 0.055f, 0.045f), 0.0f, 0.30f);
        table.GetComponent<MeshRenderer>().sharedMaterial = tableMat;

        // Wide floor so the portrait frame is filled with deep maroon instead
        // of black void above and below the board.
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "Floor";
        floor.transform.parent = parent.transform;
        floor.transform.localPosition = new Vector3(0, -1.15f, 0);
        floor.transform.localScale = new Vector3(24f, 0.12f, 24f);
        floor.GetComponent<MeshRenderer>().sharedMaterial =
            CreateMaterial(new Color(0.145f, 0.032f, 0.028f), 0.0f, 0.30f);
    }

    /// <summary>Solid slab the playing surface sits on, so the board looks thick from the side.</summary>
    private void CreateBoardBody(GameObject parent)
    {
        float outer = boardSize + borderWidth * 2f;
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "BoardBody";
        body.transform.parent = parent.transform;
        // Top must stay below the playing-surface cube (which ends at 0.0385).
        body.transform.localPosition = new Vector3(0f, 0.0185f, 0f);
        body.transform.localScale = new Vector3(outer - 0.01f, 0.037f, outer - 0.01f);
        body.GetComponent<MeshRenderer>().sharedMaterial = CreateRosewoodMaterial();
        RemoveCollider(body);
    }

    private void CreatePlayingSurface(GameObject parent)
    {
        GameObject surface = GameObject.CreatePrimitive(PrimitiveType.Cube);
        surface.name = "PlayingSurface";
        surface.transform.parent = parent.transform;
        surface.transform.localPosition = new Vector3(0, boardThickness / 2f + 0.001f, 0);
        surface.transform.localScale = new Vector3(boardSize, 0.005f, boardSize);

        Material surfaceMat = CreateSurfaceMaterial();
        surface.GetComponent<MeshRenderer>().sharedMaterial = surfaceMat;
    }

    private void CreateWoodFrame(GameObject parent)
    {
        float half = boardSize / 2f;
        float outerSize = boardSize + borderWidth * 2f;

        // The frame body is 2.2 cm above the playing surface (ICF allows 1.9-2.54 cm).
        float capHeight = 0.013f;
        float bodyTop = SurfaceTopY + frameHeight - capHeight;
        float bodyBottom = 0.012f;
        float bodyHeight = bodyTop - bodyBottom;
        float bodyCenterY = bodyBottom + bodyHeight / 2f;

        string[] frameNames = { "Frame_Top", "Frame_Bottom", "Frame_Left", "Frame_Right" };
        Vector3[] positions = {
            new Vector3(0, bodyCenterY, half + borderWidth / 2f),
            new Vector3(0, bodyCenterY, -(half + borderWidth / 2f)),
            new Vector3(-(half + borderWidth / 2f), bodyCenterY, 0),
            new Vector3(half + borderWidth / 2f, bodyCenterY, 0)
        };
        Vector3[] scales = {
            new Vector3(outerSize, bodyHeight, borderWidth),
            new Vector3(outerSize, bodyHeight, borderWidth),
            new Vector3(borderWidth, bodyHeight, outerSize),
            new Vector3(borderWidth, bodyHeight, outerSize)
        };

        // Painted orange rim, like the tournament boards in commercial carrom
        // games. The lighter cap and the dark inner lip give it a bevel.
        Material frameMat = CreateMaterial(new Color(0.905f, 0.505f, 0.150f), 0.0f, 0.55f);

        for (int i = 0; i < 4; i++)
        {
            GameObject frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frame.name = frameNames[i];
            frame.transform.parent = parent.transform;
            frame.transform.localPosition = positions[i];
            frame.transform.localScale = scales[i];
            frame.GetComponent<MeshRenderer>().sharedMaterial = frameMat;
        }

        CreateFrameCap(parent, capHeight);
        CreateFrameInlay(parent);
        CreateInnerLip(parent);
    }

    /// <summary>Slightly lighter, slightly proud top rail that gives the frame its bevel.</summary>
    private void CreateFrameCap(GameObject parent, float capHeight)
    {
        float half = boardSize / 2f;
        float capTop = SurfaceTopY + frameHeight;
        float capCenterY = capTop - capHeight / 2f;
        float capWidth = borderWidth + 0.008f;
        float capSize = boardSize + capWidth * 2f;
        float axis = half + borderWidth / 2f + 0.004f;

        Material capMat = CreateMaterial(new Color(0.985f, 0.610f, 0.215f), 0.0f, 0.60f);

        Vector3[] positions = {
            new Vector3(0, capCenterY, axis),
            new Vector3(0, capCenterY, -axis),
            new Vector3(-axis, capCenterY, 0),
            new Vector3(axis, capCenterY, 0)
        };
        Vector3[] scales = {
            new Vector3(capSize, capHeight, capWidth),
            new Vector3(capSize, capHeight, capWidth),
            new Vector3(capWidth, capHeight, capSize),
            new Vector3(capWidth, capHeight, capSize)
        };

        for (int i = 0; i < 4; i++)
        {
            GameObject cap = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cap.name = "FrameCap_" + i;
            cap.transform.parent = parent.transform;
            cap.transform.localPosition = positions[i];
            cap.transform.localScale = scales[i];
            cap.GetComponent<MeshRenderer>().sharedMaterial = capMat;
            RemoveCollider(cap);
        }
    }

    /// <summary>Painted groove + mitre joints on the frame top, like a moulded rim.</summary>
    private void CreateFrameInlay(GameObject parent)
    {
        float half = boardSize / 2f;
        float capTop = SurfaceTopY + frameHeight;
        float inset = 0.075f;
        float dist = half + inset;
        float length = boardSize + borderWidth * 2f - inset * 2f;
        Material groove = CreateMaterial(new Color(0.735f, 0.375f, 0.100f), 0.0f, 0.45f);

        for (int i = 0; i < 4; i++)
        {
            bool horizontal = i < 2;
            GameObject strip = GameObject.CreatePrimitive(PrimitiveType.Cube);
            strip.name = "FrameInlay_" + i;
            strip.transform.parent = parent.transform;
            strip.transform.localPosition = new Vector3(
                horizontal ? 0f : (i == 2 ? -dist : dist),
                capTop + 0.0015f,
                horizontal ? (i == 0 ? dist : -dist) : 0f);
            strip.transform.localScale = horizontal
                ? new Vector3(length, 0.002f, 0.009f)
                : new Vector3(0.009f, 0.002f, length);
            strip.GetComponent<MeshRenderer>().sharedMaterial = groove;
            RemoveCollider(strip);
        }
    }

    /// <summary>Shadow chamfer where the frame meets the playing surface.</summary>
    private void CreateInnerLip(GameObject parent)
    {
        float half = boardSize / 2f;
        float y = SurfaceTopY + 0.006f;
        float length = boardSize - 0.004f;
        float lip = 0.018f;
        Material lipMat = CreateMaterial(new Color(0.10f, 0.045f, 0.025f), 0.15f, 0.55f);

        Vector3[] positions = {
            new Vector3(0, y, half + lip / 2f - 0.001f),
            new Vector3(0, y, -(half + lip / 2f - 0.001f)),
            new Vector3(-(half + lip / 2f - 0.001f), y, 0),
            new Vector3(half + lip / 2f - 0.001f, y, 0)
        };
        Vector3[] scales = {
            new Vector3(length, 0.012f, lip),
            new Vector3(length, 0.012f, lip),
            new Vector3(lip, 0.012f, length),
            new Vector3(lip, 0.012f, length)
        };

        for (int i = 0; i < 4; i++)
        {
            GameObject strip = GameObject.CreatePrimitive(PrimitiveType.Cube);
            strip.name = "InnerLip_" + i;
            strip.transform.parent = parent.transform;
            strip.transform.localPosition = positions[i];
            strip.transform.localScale = scales[i];
            strip.GetComponent<MeshRenderer>().sharedMaterial = lipMat;
            RemoveCollider(strip);
        }
    }

    private void CreatePockets(GameObject parent)
    {
        Vector3[] pocketPositions = CalculatePocketPositions();

        Material holeMat = CreateMaterial(new Color(0.012f, 0.010f, 0.008f), 0.0f, 0.25f);
        // Dark turned rim: the reference pockets are plain black holes, so no
        // brass ring stealing attention from the pieces.
        Material rimMat = CreateMaterial(new Color(0.185f, 0.098f, 0.050f), 0.15f, 0.45f);
        Material throatMat = CreateMaterial(new Color(0.10f, 0.065f, 0.04f), 0.15f, 0.55f);

        for (int i = 0; i < 4; i++)
        {
            GameObject pocketGroup = new GameObject($"Pocket{i}");
            pocketGroup.transform.parent = parent.transform;
            pocketGroup.transform.localPosition = new Vector3(pocketPositions[i].x, 0f, pocketPositions[i].z);

            float holeTop = SurfaceTopY + 0.0005f;
            float holeHeight = 0.07f;

            GameObject pocketHole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pocketHole.name = "Hole";
            pocketHole.transform.parent = pocketGroup.transform;
            pocketHole.transform.localPosition = new Vector3(0, holeTop - holeHeight / 2f, 0);
            pocketHole.transform.localScale = new Vector3(pocketRadius * 2f, holeHeight / 2f, pocketRadius * 2f);
            pocketHole.GetComponent<MeshRenderer>().sharedMaterial = holeMat;
            RemoveCollider(pocketHole);

            // Subtle inner throat ring: sells the depth of the hole.
            GameObject throat = CreateTorusRing(pocketGroup.transform, pocketRadius * 0.80f,
                0.007f, throatMat);
            throat.name = "Throat";
            throat.transform.localPosition = new Vector3(0, holeTop + 0.0008f, 0);

            GameObject pocketRim = CreateTorusRing(pocketGroup.transform,
                pocketRadius + pocketRimWidth * 0.5f, pocketRimWidth, rimMat);
            pocketRim.name = "Rim";
            pocketRim.transform.localPosition = new Vector3(0, SurfaceTopY + 0.0025f, 0);

            SphereCollider pocketCollider = pocketGroup.AddComponent<SphereCollider>();
            pocketCollider.radius = pocketRadius * 0.98f;
            pocketCollider.isTrigger = true;
            pocketCollider.center = new Vector3(0f, SurfaceTopY + 0.015f, 0f);

            PocketTrigger pocketTrigger = pocketGroup.AddComponent<PocketTrigger>();
            pocketTrigger.coinLayer = coinLayer;
            pocketTrigger.pocketRadius = pocketRadius;
        }
    }

    private void CreateCenterDesign(GameObject parent)
    {
        float y = SurfaceTopY + 0.0008f;
        float y2 = y + 0.0003f;
        float y3 = y2 + 0.0003f;

        // ICF outer circle: 17 cm diameter.
        AddRing(parent.transform, "OuterCircle", 0f, 0f, outerCircleRadius - 0.0075f,
            outerCircleRadius, y, LineBlack);
        AddRing(parent.transform, "OuterCircleInner", 0f, 0f, outerCircleRadius - 0.032f,
            outerCircleRadius - 0.028f, y2, LineBrownDark);

        // Fine decorative ring inside the outer circle.
        AddRing(parent.transform, "DecoRing", 0f, 0f, outerCircleRadius - 0.040f,
            outerCircleRadius - 0.036f, y2, LineBrownDark);

        // Classic rosette: sixteen tapered petals alternating red and black,
        // held clear of the central red disc by a small inner ring.
        const int petals = 16;
        for (int i = 0; i < petals; i++)
        {
            float angle = i * Mathf.PI * 2f / petals;
            bool major = i % 2 == 0;
            float inner = 0.106f;
            float outer = major ? outerCircleRadius - 0.050f : outerCircleRadius - 0.090f;
            AddSpoke(parent.transform, "Petal_" + i, inner, outer,
                major ? 0.030f : 0.017f, major ? 0.009f : 0.006f,
                angle, y3, major ? LineBrown : LineBrownDark);
        }

        AddRing(parent.transform, "InnerCircle", 0f, 0f, 0.100f, 0.106f, y3, LineBlack);
        AddRing(parent.transform, "InnerCircle2", 0f, 0f, 0.072f, 0.076f, y3, LineRedDark);

        // ICF centre circle: 3.18 cm black outline, 2.54 cm red heart.
        AddDisc(parent.transform, "CentreOutline", 0f, 0f, centerCircleRadius, y2, LineBlack, 0.0012f);
        AddDisc(parent.transform, "CentreRed", 0f, 0f, baseRedRadius, y3, LineRed, 0.0012f);
    }

    private void CreateBaseLines(GameObject parent)
    {
        // Group 0: far side (+z). 1: near side (-z). 2: left. 3: right.
        Vector3[] positions = {
            new Vector3(0f, 0f, baseLineDistance),
            new Vector3(0f, 0f, -baseLineDistance),
            new Vector3(-baseLineDistance, 0f, 0f),
            new Vector3(baseLineDistance, 0f, 0f)
        };
        float[] rotations = { 0f, 180f, 90f, -90f };

        float y = SurfaceTopY + 0.0006f;
        float lineThickness = 0.020f;
        float circleX = baseLineLength * 0.5f - baseCircleRadius;

        for (int i = 0; i < 4; i++)
        {
            GameObject group = new GameObject($"BaseLine_{i}");
            group.transform.parent = parent.transform;
            group.transform.localPosition = positions[i];
            group.transform.localRotation = Quaternion.Euler(0f, rotations[i], 0f);

            for (int side = -1; side <= 1; side += 2)
            {
                AddQuad(group.transform, "Line", 0f, side * baseLineGap * 0.5f,
                    baseLineLength, lineThickness, y, LineBlack);
            }

            // Base circles: 3.18 cm black ring, 2.54 cm red heart, tangent to
            // both base lines and to the adjacent side's base lines.
            for (int end = -1; end <= 1; end += 2)
            {
                float cx = end * circleX;
                AddDisc(group.transform, "BaseCircle", cx, 0f, baseCircleRadius,
                    y + 0.0002f, LineBlack, 0.0012f);
                AddDisc(group.transform, "BaseCircleRed", cx, 0f, baseRedRadius,
                    y + 0.0005f, LineRed, 0.0012f);
            }
        }
    }

    /// <summary>
    /// Four ICF corner arrows: 45 degrees, through the gap between the two
    /// base circles, pointing at the pocket and stopping 5 cm short of it.
    /// </summary>
    private void CreateCornerArrows(GameObject parent)
    {
        float y = SurfaceTopY + 0.0011f;

        for (int i = 0; i < 4; i++)
        {
            float sx = (i == 0 || i == 3) ? -1f : 1f;
            float sz = (i < 2) ? 1f : -1f;

            // Start in the gap between the corner's two base circles.
            float circleX = baseLineLength * 0.5f - baseCircleRadius;
            Vector2 gap = new Vector2(
                sx * (circleX + baseLineDistance) * 0.5f,
                sz * (circleX + baseLineDistance) * 0.5f);

            Vector2 pocket = new Vector2(
                sx * (boardSize / 2f - pocketRadius - PocketInset),
                sz * (boardSize / 2f - pocketRadius - PocketInset));

            Vector2 dir = (pocket - gap).normalized;
            Vector2 end = pocket - dir * (pocketRadius + 0.196f);
            Vector2 start = end - dir * Mathf.Min(arrowLength, Vector2.Distance(gap, end));

            AddArrow(parent.transform, "CornerArrow_" + i, start, end, arrowWidth,
                arrowWidth * 2.6f, 0.075f, y, LineRed);
        }
    }

    private const float PocketInset = 0.006f;

    private Vector3[] CalculatePocketPositions()
    {
        float p = boardSize / 2f - pocketRadius - PocketInset;
        return new Vector3[]
        {
            new Vector3(-p, 0f, p),
            new Vector3(p, 0f, p),
            new Vector3(-p, 0f, -p),
            new Vector3(p, 0f, -p)
        };
    }

    // ---------------------------------------------------------------- markings

    private void AddQuad(Transform parent, string name, float cx, float cz,
        float width, float length, float y, Color color)
    {
        Vector3[] verts = {
            new Vector3(cx - width * 0.5f, y, cz - length * 0.5f),
            new Vector3(cx - width * 0.5f, y, cz + length * 0.5f),
            new Vector3(cx + width * 0.5f, y, cz + length * 0.5f),
            new Vector3(cx + width * 0.5f, y, cz - length * 0.5f)
        };
        int[] tris = { 0, 1, 2, 0, 2, 3 };
        AddFlatMesh(parent, name, verts, tris, color);
    }

    private void AddDisc(Transform parent, string name, float cx, float cz,
        float radius, float y, Color color, float thickness)
    {
        GameObject disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        disc.name = name;
        disc.transform.SetParent(parent, false);
        disc.transform.localPosition = new Vector3(cx, y + thickness * 0.5f, cz);
        disc.transform.localScale = new Vector3(radius * 2f, thickness * 0.5f, radius * 2f);
        disc.GetComponent<MeshRenderer>().sharedMaterial = CreateMarkingMaterial(color);
        RemoveCollider(disc);
    }

    private void AddRing(Transform parent, string name, float cx, float cz,
        float inner, float outer, float y, Color color)
    {
        const int segments = 72;
        Vector3[] verts = new Vector3[(segments + 1) * 2];
        int[] tris = new int[segments * 6];

        for (int i = 0; i <= segments; i++)
        {
            float angle = (float)i / segments * Mathf.PI * 2f;
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);
            verts[i * 2] = new Vector3(cx + cos * inner, y, cz + sin * inner);
            verts[i * 2 + 1] = new Vector3(cx + cos * outer, y, cz + sin * outer);
        }

        for (int i = 0; i < segments; i++)
        {
            int v = i * 2;
            int t = i * 6;
            tris[t] = v;
            tris[t + 1] = v + 2;
            tris[t + 2] = v + 1;
            tris[t + 3] = v + 1;
            tris[t + 4] = v + 2;
            tris[t + 5] = v + 3;
        }

        AddFlatMesh(parent, name, verts, tris, color);
    }

    private void AddSpoke(Transform parent, string name, float r0, float r1,
        float width0, float width1, float angle, float y, Color color)
    {
        Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        Vector2 perp = new Vector2(-dir.y, dir.x);
        Vector2 outer0 = dir * r0;
        Vector2 outer1 = dir * r1;

        Vector3[] verts = {
            new Vector3(outer0.x + perp.x * width0 * 0.5f, y, outer0.y + perp.y * width0 * 0.5f),
            new Vector3(outer1.x + perp.x * width1 * 0.5f, y, outer1.y + perp.y * width1 * 0.5f),
            new Vector3(outer1.x - perp.x * width1 * 0.5f, y, outer1.y - perp.y * width1 * 0.5f),
            new Vector3(outer0.x - perp.x * width0 * 0.5f, y, outer0.y - perp.y * width0 * 0.5f)
        };
        int[] tris = { 0, 1, 2, 0, 2, 3 };
        AddFlatMesh(parent, name, verts, tris, color);
    }

    private void AddArrow(Transform parent, string name, Vector2 from, Vector2 to,
        float shaftWidth, float headWidth, float headLength, float y, Color color)
    {
        Vector2 dir = (to - from).normalized;
        Vector2 perp = new Vector2(-dir.y, dir.x);
        Vector2 headBase = to - dir * headLength;
        Vector2 shoulder0 = from + perp * shaftWidth * 0.5f;
        Vector2 shoulder1 = headBase + perp * shaftWidth * 0.5f;
        Vector2 shoulder2 = headBase - perp * shaftWidth * 0.5f;
        Vector2 shoulder3 = from - perp * shaftWidth * 0.5f;
        Vector2 wing0 = headBase + perp * headWidth * 0.5f;
        Vector2 wing1 = headBase - perp * headWidth * 0.5f;

        Vector3[] verts = {
            new Vector3(shoulder0.x, y, shoulder0.y),
            new Vector3(shoulder1.x, y, shoulder1.y),
            new Vector3(shoulder2.x, y, shoulder2.y),
            new Vector3(shoulder3.x, y, shoulder3.y),
            new Vector3(wing0.x, y, wing0.y),
            new Vector3(wing1.x, y, wing1.y),
            new Vector3(to.x, y, to.y)
        };

        // Shaft (quad), then head (wing0, wing1, tip).
        int[] tris = {
            0, 1, 2, 0, 2, 3,
            4, 5, 6
        };

        AddFlatMesh(parent, name, verts, tris, color);
    }

    private void AddFlatMesh(Transform parent, string name, Vector3[] verts,
        int[] tris, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        Mesh mesh = new Mesh();
        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.RecalculateNormals();

        go.AddComponent<MeshFilter>().sharedMesh = mesh;
        go.AddComponent<MeshRenderer>().sharedMaterial = CreateMarkingMaterial(color);
    }

    private Material CreateMarkingMaterial(Color color)
    {
        // Markings are paper-thin: draw them double sided so they can never
        // disappear from a grazing camera angle.
        Material mat = CreateMaterial(color, 0f, 0.35f);
        if (mat.HasProperty("_Cull")) mat.SetFloat("_Cull", 0f);
        return mat;
    }

    // ------------------------------------------------------------------- meshes

    private GameObject CreateTorusRing(Transform parent, float radius, float tubeRadius, Material mat)
    {
        int segments = 48;
        int tubeSegments = 10;
        GameObject torus = new GameObject("TorusRing");
        torus.transform.parent = parent;

        MeshFilter mf = torus.AddComponent<MeshFilter>();
        MeshRenderer mr = torus.AddComponent<MeshRenderer>();
        mr.sharedMaterial = mat;

        Mesh mesh = new Mesh();
        int vertCount = (segments + 1) * (tubeSegments + 1);
        Vector3[] vertices = new Vector3[vertCount];
        int[] triangles = new int[segments * tubeSegments * 6];

        for (int i = 0; i <= segments; i++)
        {
            float u = (float)i / segments * Mathf.PI * 2f;
            float cosU = Mathf.Cos(u);
            float sinU = Mathf.Sin(u);

            for (int j = 0; j <= tubeSegments; j++)
            {
                float v = (float)j / tubeSegments * Mathf.PI * 2f;
                float cosV = Mathf.Cos(v);
                float sinV = Mathf.Sin(v);

                float x = (radius + tubeRadius * cosV) * cosU;
                float y = tubeRadius * sinV;
                float z = (radius + tubeRadius * cosV) * sinU;

                vertices[i * (tubeSegments + 1) + j] = new Vector3(x, y, z);
            }
        }

        int triIdx = 0;
        for (int i = 0; i < segments; i++)
        {
            for (int j = 0; j < tubeSegments; j++)
            {
                int a = i * (tubeSegments + 1) + j;
                int b = a + tubeSegments + 1;
                int c = a + 1;
                int d = b + 1;

                // Counter-clockwise winding so the front faces point outwards;
                // otherwise the brass pocket rim is back-face culled and the
                // pocket reads as a flat black sticker.
                triangles[triIdx++] = a;
                triangles[triIdx++] = c;
                triangles[triIdx++] = b;

                triangles[triIdx++] = c;
                triangles[triIdx++] = d;
                triangles[triIdx++] = b;
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mf.sharedMesh = mesh;

        return torus;
    }

    private void SetupLighting(GameObject parent)
    {
        GameObject lightingObj = new GameObject("BoardLighting");
        lightingObj.transform.parent = parent.transform;
        lightingObj.transform.position = new Vector3(0, 2f, 0);

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.255f, 0.215f, 0.175f);

        Light mainLight = lightingObj.AddComponent<Light>();
        mainLight.type = LightType.Directional;
        mainLight.color = new Color(1f, 0.96f, 0.90f);
        mainLight.intensity = 1.02f;
        mainLight.shadows = LightShadows.Soft;
        mainLight.shadowStrength = 0.50f;
        mainLight.shadowBias = 0.012f;
        mainLight.shadowNormalBias = 0.18f;
        lightingObj.transform.rotation = Quaternion.Euler(52f, -30f, 0f);

        // Concentrate the shadow map on the board so small pieces get clean
        // contact shadows instead of a blurry smear.
        QualitySettings.shadowDistance = 12f;
        QualitySettings.shadowCascades = 4;

        // Warm overhead lamp: gives the varnished board and coins a soft hotspot.
        GameObject lampObj = new GameObject("OverheadLamp");
        lampObj.transform.parent = parent.transform;
        lampObj.transform.position = new Vector3(0f, 2.6f, -0.2f);
        Light lamp = lampObj.AddComponent<Light>();
        lamp.type = LightType.Point;
        lamp.color = new Color(1f, 0.90f, 0.76f);
        lamp.intensity = 0.30f;
        lamp.range = 5.0f;
        lamp.shadows = LightShadows.None;

        CreateFillLight(parent, new Vector3(-1.9f, 1.6f, -1.9f), new Color(0.55f, 0.68f, 0.95f), 0.38f);
        CreateFillLight(parent, new Vector3(1.9f, 1.6f, 1.9f), new Color(0.95f, 0.86f, 0.68f), 0.34f);
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

    private void RemoveCollider(GameObject go)
    {
        if (go == null) return;
        Collider collider = go.GetComponent<Collider>();
        if (collider == null) return;
        collider.enabled = false;
        Destroy(collider);
    }

    // --------------------------------------------------------------- materials

    private Material CreateSurfaceMaterial()
    {
        Texture2D texture = CreateBoardSurfaceTexture();
        Material mat = CreateTexturedMaterial(texture, 0.0f, 0.42f);
        return mat;
    }

    /// <summary>
    /// Builds one high-resolution texture containing the wood grain plus every
    /// printed marking. Baking the markings means they are anti-aliased by the
    /// texture sampler (crisp at any zoom) rather than razor-thin quads that
    /// flicker against the surface.
    /// </summary>
    private Texture2D CreateBoardSurfaceTexture()
    {
        // 1536 px across the playcourt keeps the printed lines sharper than the
        // screen can resolve at this zoom, so they never look soft or jagged.
        BoardSurfacePainter painter = new BoardSurfacePainter(1536, boardSize);
        painter.BuildWood();
        painter.PaintMarkings(this);
        return painter.ToTexture("CarromSurface");
    }

    /// <summary>Draws the playing surface directly into a colour buffer.</summary>
    private sealed class BoardSurfacePainter
    {
        private readonly int size;
        private readonly float half;
        private readonly float texel;
        private readonly Color32[] pixels;

        private static readonly Color LineBlack = BoardSetup.LineBlack;
        private static readonly Color LineRed = BoardSetup.LineRed;
        private static readonly Color LineRedDark = BoardSetup.LineRedDark;
        private static readonly Color LineBrown = BoardSetup.LineBrown;
        private static readonly Color LineBrownDark = BoardSetup.LineBrownDark;

        public BoardSurfacePainter(int size, float span)
        {
            this.size = size;
            half = span * 0.5f;
            texel = span / size;
            pixels = new Color32[size * size];
        }

        // -------------------------------------------------------------- wood

        public void BuildWood()
        {
            // Warm tan birch: light enough for brown ink to read, but not so
            // pale that the key light blows the face out to white.
            Color light = new Color(0.914f, 0.836f, 0.694f);
            Color mid = new Color(0.852f, 0.756f, 0.598f);
            Color dark = new Color(0.714f, 0.584f, 0.414f);

            for (int y = 0; y < size; y++)
            {
                float v = y / (float)(size - 1);
                for (int x = 0; x < size; x++)
                {
                    float u = x / (float)(size - 1);

                    // Warp the grain so the stripes undulate instead of
                    // repeating like a printed pattern.
                    float warp = Mathf.PerlinNoise(u * 2.2f + 9.1f, v * 6.4f + 2.7f) - 0.5f;

                    // Grain runs up the screen (along Z), like the reference.
                    float g1 = Mathf.PerlinNoise(u * 26f + warp * 2.2f,
                        v * 1.4f + warp * 0.30f);
                    float g2 = Mathf.PerlinNoise(u * 70f + 7.2f + warp * 0.5f,
                        v * 5.5f + 13.7f);
                    float g3 = Mathf.PerlinNoise(u * 220f + 18.6f, v * 18f + 31.4f);

                    float t = Mathf.Clamp01(g1 * 0.50f + g2 * 0.32f + g3 * 0.18f);
                    Color c = Color.Lerp(light, mid, Mathf.Clamp01(t * 0.95f));
                    c = Color.Lerp(c, dark, Mathf.Clamp01((t - 0.60f) * 1.4f) * 0.55f);

                    // Tight, long fibres: the fine figure of birch ply, not
                    // broad painted bands.
                    float fibre = Mathf.PerlinNoise(u * 260f + 5.7f, v * 3.1f + 21.3f);
                    c = Color.Lerp(c, dark, Mathf.Clamp01((fibre - 0.58f) * 1.6f) * 0.30f);

                    float dx = u - 0.5f;
                    float dy = v - 0.5f;
                    float vignette = Mathf.Clamp01(1f - (dx * dx + dy * dy) * 1.15f);
                    c *= 0.95f + vignette * 0.07f;

                    pixels[y * size + x] = c;
                }
            }
        }

        // ----------------------------------------------------------- markings

        public void PaintMarkings(BoardSetup b)
        {
            // Thin printed ink on pale birch, like a tournament board. The old
            // bold black ICF lines read as heavy stripes; these stay crisp but
            // light, and the base discs are the only saturated element.
            float lineThickness = 0.0075f;
            Color ink = new Color(0.400f, 0.232f, 0.132f);
            Color baseEdge = new Color(0.545f, 0.278f, 0.086f);
            Color baseFill = new Color(0.940f, 0.560f, 0.170f);
            Color baseHeart = new Color(0.330f, 0.158f, 0.062f);
            Color rosette = new Color(0.620f, 0.418f, 0.235f);

            // Base lines: one rounded double frame around the court.
            float outer = b.baseLineDistance + b.baseLineGap * 0.5f;
            float inner = b.baseLineDistance - b.baseLineGap * 0.5f;
            RoundedRectOutline(outer, 0.185f, lineThickness, ink);
            RoundedRectOutline(inner, 0.160f, lineThickness, ink);

            // Orange base discs at both ends of every base line, sitting in the
            // gap between the two printed lines exactly like the reference.
            float circleX = b.baseLineLength * 0.5f - b.baseCircleRadius;
            for (int side = 0; side < 4; side++)
            {
                bool horizontal = side < 2;
                float center = (horizontal ? (side == 0 ? 1f : -1f)
                                           : (side == 2 ? -1f : 1f)) * b.baseLineDistance;

                for (int end = -1; end <= 1; end += 2)
                {
                    float along = end * circleX;
                    float cx = horizontal ? along : center;
                    float cz = horizontal ? center : along;
                    Disc(cx, cz, b.baseCircleRadius + 0.0065f, baseEdge);
                    Disc(cx, cz, b.baseCircleRadius, baseFill);
                    Disc(cx, cz, 0.0125f, baseHeart);
                }
            }

            // Thin diagonal guide lines beside each pocket (no arrowhead).
            for (int i = 0; i < 4; i++)
            {
                float sx = (i == 0 || i == 3) ? -1f : 1f;
                float sz = (i < 2) ? 1f : -1f;

                Vector2 gap = new Vector2(
                    sx * (circleX + b.baseLineDistance) * 0.5f,
                    sz * (circleX + b.baseLineDistance) * 0.5f);

                Vector2 pocket = new Vector2(
                    sx * (b.boardSize * 0.5f - b.pocketRadius - PocketInset),
                    sz * (b.boardSize * 0.5f - b.pocketRadius - PocketInset));

                Vector2 dir = (pocket - gap).normalized;
                Vector2 end = pocket - dir * (b.pocketRadius + 0.205f);
                Vector2 start = end - dir * Mathf.Min(b.arrowLength, Vector2.Distance(gap, end));
                Line(start.x, start.y, end.x, end.y, lineThickness, ink);
            }

            // Centre: a wide printed circle with a faint carved rosette.
            Ring(0f, 0f, b.outerCircleRadius - 0.0055f, b.outerCircleRadius, ink);
            Ring(0f, 0f, b.outerCircleRadius - 0.024f, b.outerCircleRadius - 0.0215f, rosette);

            const int petals = 16;
            for (int i = 0; i < petals; i++)
            {
                float angle = i * Mathf.PI * 2f / petals;
                bool major = i % 2 == 0;
                float innerR = 0.098f;
                float outerR = major ? b.outerCircleRadius - 0.045f
                                     : b.outerCircleRadius - 0.082f;
                Petal(angle, innerR, outerR,
                    major ? 0.032f : 0.020f, major ? 0.011f : 0.007f,
                    rosette);
            }

            Ring(0f, 0f, 0.082f, 0.086f, ink);
            Disc(0f, 0f, 0.052f, new Color(0.605f, 0.150f, 0.100f));
            Ring(0f, 0f, 0.052f, 0.056f, ink);
        }

        // ------------------------------------------------------- draw helpers

        private void Line(float x0, float z0, float x1, float z1,
            float width, Color color)
        {
            float halfWidth = width * 0.5f;
            float feather = texel;

            int minX = ToTexelX(Mathf.Min(x0, x1) - halfWidth - feather * 2f);
            int maxX = ToTexelX(Mathf.Max(x0, x1) + halfWidth + feather * 2f);
            int minY = ToTexelY(Mathf.Min(z0, z1) - halfWidth - feather * 2f);
            int maxY = ToTexelY(Mathf.Max(z0, z1) + halfWidth + feather * 2f);

            Vector2 a = new Vector2(x0, z0);
            Vector2 b = new Vector2(x1, z1);
            Vector2 ab = b - a;
            float lengthSq = Mathf.Max(0.000001f, ab.sqrMagnitude);

            for (int y = minY; y <= maxY; y++)
            {
                float wz = ToWorldZ(y);
                for (int x = minX; x <= maxX; x++)
                {
                    Vector2 p = new Vector2(ToWorldX(x) - a.x, wz - a.y);
                    float t = Mathf.Clamp01(Vector2.Dot(p, ab) / lengthSq);
                    float distance = (p - ab * t).magnitude;
                    float coverage = Mathf.Clamp01((halfWidth - distance) / feather + 0.5f);
                    if (coverage > 0f)
                        Blend(x, y, color, coverage, 0.94f);
                }
            }
        }

        private void Disc(float cx, float cz, float radius, Color color)
        {
            float feather = texel;
            int minX = ToTexelX(cx - radius - feather * 2f);
            int maxX = ToTexelX(cx + radius + feather * 2f);
            int minY = ToTexelY(cz - radius - feather * 2f);
            int maxY = ToTexelY(cz + radius + feather * 2f);

            for (int y = minY; y <= maxY; y++)
            {
                float dz = ToWorldZ(y) - cz;
                for (int x = minX; x <= maxX; x++)
                {
                    float dx = ToWorldX(x) - cx;
                    float distance = Mathf.Sqrt(dx * dx + dz * dz);
                    float coverage = Mathf.Clamp01((radius - distance) / feather + 0.5f);
                    if (coverage > 0f)
                        Blend(x, y, color, coverage, 0.95f);
                }
            }
        }

        private void Ring(float cx, float cz, float inner, float outer, Color color)
        {
            float feather = texel;
            int minX = ToTexelX(cx - outer - feather * 2f);
            int maxX = ToTexelX(cx + outer + feather * 2f);
            int minY = ToTexelY(cz - outer - feather * 2f);
            int maxY = ToTexelY(cz + outer + feather * 2f);

            for (int y = minY; y <= maxY; y++)
            {
                float dz = ToWorldZ(y) - cz;
                for (int x = minX; x <= maxX; x++)
                {
                    float dx = ToWorldX(x) - cx;
                    float distance = Mathf.Sqrt(dx * dx + dz * dz);
                    float outside = Mathf.Clamp01((outer - distance) / feather + 0.5f);
                    float inside = Mathf.Clamp01((distance - inner) / feather + 0.5f);
                    float coverage = outside * inside;
                    if (coverage > 0f)
                        Blend(x, y, color, coverage, 0.92f);
                }
            }
        }

        private void RoundedRectOutline(float halfExtent, float cornerRadius,
            float thickness, Color color)
        {
            // Signed distance to a rounded square: negative inside. Stroking the
            // zero-set gives the soft printed corners used on commercial boards.
            float feather = texel;
            float h = halfExtent;
            float r = Mathf.Max(0.001f, cornerRadius);
            float halfWidth = thickness * 0.5f;

            int minX = ToTexelX(-h - thickness);
            int maxX = ToTexelX(h + thickness);
            int minY = ToTexelY(-h - thickness);
            int maxY = ToTexelY(h + thickness);

            for (int y = minY; y <= maxY; y++)
            {
                float pz = ToWorldZ(y);
                for (int x = minX; x <= maxX; x++)
                {
                    float px = ToWorldX(x);
                    float qx = Mathf.Abs(px) - (h - r);
                    float qz = Mathf.Abs(pz) - (h - r);
                    float outside = Mathf.Sqrt(Mathf.Max(qx, 0f) * Mathf.Max(qx, 0f) +
                                               Mathf.Max(qz, 0f) * Mathf.Max(qz, 0f))
                                    + Mathf.Min(Mathf.Max(qx, qz), 0f) - r;
                    float distance = Mathf.Abs(outside);
                    float coverage = Mathf.Clamp01((halfWidth - distance) / feather + 0.5f);
                    if (coverage > 0f)
                        Blend(x, y, color, coverage, 0.88f);
                }
            }
        }

        private void Petal(float angle, float r0, float r1, float w0, float w1, Color color)
        {
            // A tapered spoke drawn as overlapping stamped discs: simpler than
            // polygon rasterising and it softens the tip like real screen print.
            const int steps = 10;
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);

            for (int i = 0; i <= steps; i++)
            {
                float t = i / (float)steps;
                float radius = Mathf.Lerp(r0, r1, t);
                float width = Mathf.Lerp(w0, w1, t);
                Disc(cos * radius, sin * radius, width * 0.5f, color);
            }
        }

        private void Arrow(Vector2 start, Vector2 end, float shaftWidth,
            float headWidth, float headLength, Color color)
        {
            Vector2 dir = (end - start).normalized;
            Vector2 headBase = end - dir * headLength;
            Line(start.x, start.y, headBase.x, headBase.y, shaftWidth, color);

            const int steps = 8;
            for (int i = 0; i <= steps; i++)
            {
                float t = i / (float)steps;
                Vector2 center = Vector2.Lerp(headBase, end, t);
                Disc(center.x, center.y, Mathf.Lerp(headWidth * 0.5f, 0f, t), color);
            }
        }

        private int ToTexelX(float worldX) { return Mathf.Clamp(Mathf.RoundToInt((worldX + half) / texel), 0, size - 1); }
        private int ToTexelY(float worldZ) { return Mathf.Clamp(Mathf.RoundToInt((worldZ + half) / texel), 0, size - 1); }
        private float ToWorldX(int index) { return -half + (index + 0.5f) * texel; }
        private float ToWorldZ(int index) { return -half + (index + 0.5f) * texel; }

        private void Blend(int x, int y, Color mark, float coverage, float alpha)
        {
            // Keep some of the underlying grain so the print looks dyed into
            // the wood instead of sitting on top as flat vinyl.
            int index = y * size + x;
            Color32 under = pixels[index];
            float t = coverage * alpha;
            byte r = (byte)Mathf.Clamp(Mathf.RoundToInt(under.r + (mark.r * 255f - under.r) * t), 0, 255);
            byte g = (byte)Mathf.Clamp(Mathf.RoundToInt(under.g + (mark.g * 255f - under.g) * t), 0, 255);
            byte b = (byte)Mathf.Clamp(Mathf.RoundToInt(under.b + (mark.b * 255f - under.b) * t), 0, 255);
            pixels[index] = new Color32(r, g, b, 255);
        }

        public Texture2D ToTexture(string name)
        {
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, true);
            texture.name = name;
            texture.SetPixels32(pixels);
            texture.Apply(true, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Trilinear;
            texture.anisoLevel = 9;
            return texture;
        }
    }

    private Material CreateRosewoodMaterial()
    {
        return CreateTexturedMaterial(ProceduralTextures.GetRosewoodTexture(), 0.05f, 0.72f);
    }

    private Material CreateBrassMaterial()
    {
        // Heavy metalness goes almost black in Built-in without reflection
        // probes, so the pocket rim and frame inlay use a bright diffuse brass
        // with just enough metalness to catch the key light.
        return CreateMaterial(new Color(1f, 0.85f, 0.45f), 0.30f, 0.92f);
    }

    private Material CreateTexturedMaterial(Texture2D texture, float metallic, float smoothness)
    {
        Shader shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Diffuse");

        Material mat = new Material(shader);
        mat.color = Color.white;
        mat.mainTexture = texture;
        mat.mainTextureScale = Vector2.one;

        if (shader.name == "Standard")
        {
            mat.SetFloat("_Metallic", metallic);
            mat.SetFloat("_Glossiness", smoothness);
        }

        return mat;
    }

    private Material CreateMaterial(Color color, float metallic, float smoothness)
    {
        // Standard shader must be included in Project Settings > Graphics > Always Included Shaders.
        // Diffuse is the guaranteed fallback — always available on all platforms.
        // Never fall back to URP shaders; this project uses the Built-in render pipeline.
        Shader shader = Shader.Find("Standard");
        if (shader == null)
        {
            Debug.LogWarning("BoardSetup: Standard shader not found. Add it to Graphics Settings > Always Included Shaders. Using Diffuse fallback.");
            shader = Shader.Find("Diffuse");
        }
        Material mat = new Material(shader);
        mat.color = color;
        if (shader.name == "Standard")
        {
            mat.SetFloat("_Metallic", metallic);
            mat.SetFloat("_Glossiness", smoothness);
        }
        return mat;
    }
}
