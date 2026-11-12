using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Builds the chunky, beveled meshes and soft contact shadows used by the coins
/// and the striker. Real carrom pieces are thin, but on a phone screen a plain
/// cylinder reads as a flat 2D disc: a rounded bevel catches the key light, and
/// the contact shadow grounds the piece on the board.
/// </summary>
public static class PieceMeshFactory
{
    private static readonly Dictionary<string, Mesh> meshCache = new Dictionary<string, Mesh>();
    private static Texture2D shadowTexture;
    private static Material shadowMaterial;

    /// <summary>
    /// Disc with a flat bottom, rounded bevel, straight side wall, rounded top
    /// bevel and a gently domed top face.
    /// Submesh 0 is the rim/side, submesh 1 is the printed top face.
    /// </summary>
    public static Mesh CreateDisc(float radius, float thickness, float bevel, int segments = 56)
    {
        radius = Mathf.Max(0.004f, radius);
        thickness = Mathf.Max(0.003f, thickness);
        bevel = Mathf.Clamp(bevel, 0.0006f, Mathf.Min(radius * 0.42f, thickness * 0.45f));
        segments = Mathf.Clamp(segments, 12, 96);

        float half = thickness * 0.5f;
        float inner = radius - bevel;
        float dome = bevel * 0.45f;

        List<Vector3> verts = new List<Vector3>(segments * 16);
        List<Vector2> uvs = new List<Vector2>(segments * 16);
        List<int> rim = new List<int>(segments * 30);
        List<int> face = new List<int>(segments * 12);

        // --- rounded rim profile, bottom edge to top edge -----------------
        List<Vector2> profile = new List<Vector2>();
        profile.Add(new Vector2(inner, -half));
        for (int i = 1; i <= 3; i++)
        {
            float a = Mathf.PI * 0.5f * i / 3f;
            profile.Add(new Vector2(
                inner + Mathf.Sin(a) * bevel,
                -half + (1f - Mathf.Cos(a)) * bevel));
        }
        profile.Add(new Vector2(radius, half - bevel));
        for (int i = 1; i <= 3; i++)
        {
            float a = Mathf.PI * 0.5f * i / 3f;
            profile.Add(new Vector2(
                inner + Mathf.Cos(a) * bevel,
                half - bevel + Mathf.Sin(a) * bevel));
        }

        int previousRing = -1;
        for (int i = 0; i < profile.Count; i++)
        {
            int ring = AddRing(verts, uvs, profile[i].x, profile[i].y, segments);
            if (previousRing >= 0)
                AddBand(rim, previousRing, ring, segments, false);
            previousRing = ring;
        }

        // --- flat bottom cap ---------------------------------------------
        int bottomCenter = AddVertex(verts, uvs, new Vector3(0f, -half, 0f), new Vector2(0.5f, 0.5f));
        int bottomRing = AddRing(verts, uvs, inner, -half, segments);
        for (int i = 0; i < segments; i++)
        {
            int a = bottomRing + i;
            int b = bottomRing + (i + 1) % segments;
            rim.Add(bottomCenter);
            rim.Add(a);
            rim.Add(b);
        }

        // --- domed top face with its own crease ring ----------------------
        int faceCenter = AddVertex(verts, uvs, new Vector3(0f, half + dome, 0f), new Vector2(0.5f, 0.5f));
        int faceRingOuter = AddRing(verts, uvs, inner, half, segments, inner);
        int faceRingMid = AddRing(verts, uvs, inner * 0.52f, half + dome * 0.45f, segments, inner);

        AddBand(face, faceRingMid, faceRingOuter, segments, true);
        for (int i = 0; i < segments; i++)
        {
            int a = faceRingMid + i;
            int b = faceRingMid + (i + 1) % segments;
            face.Add(faceCenter);
            face.Add(b);
            face.Add(a);
        }

        Mesh mesh = new Mesh();
        mesh.name = "PieceDisc";
        mesh.SetVertices(verts);
        mesh.SetUVs(0, uvs);
        mesh.subMeshCount = 2;
        mesh.SetTriangles(rim, 0);
        mesh.SetTriangles(face, 1);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    /// <summary>Cached disc mesh so all pieces of a kind share one mesh.</summary>
    public static Mesh GetDisc(string key, float radius, float thickness, float bevel, int segments = 56)
    {
        Mesh mesh;
        if (meshCache.TryGetValue(key, out mesh) && mesh != null)
            return mesh;

        mesh = CreateDisc(radius, thickness, bevel, segments);
        meshCache[key] = mesh;
        return mesh;
    }

    /// <summary>
    /// Raised torus used for the gold inlay ring on the striker face. A real
    /// striker has a turned metal band, and the raised ring catches the light so
    /// the piece reads as a solid 3D object even under the near top-down camera.
    /// </summary>
    public static Mesh CreateTorus(float majorRadius, float tubeRadius,
        int segments = 72, int tubeSegments = 10)
    {
        majorRadius = Mathf.Max(0.002f, majorRadius);
        tubeRadius = Mathf.Clamp(tubeRadius, 0.0004f, majorRadius * 0.45f);
        segments = Mathf.Clamp(segments, 12, 128);
        tubeSegments = Mathf.Clamp(tubeSegments, 4, 24);

        List<Vector3> verts = new List<Vector3>(segments * tubeSegments);
        List<Vector2> uvs = new List<Vector2>(segments * tubeSegments);
        List<int> triangles = new List<int>(segments * tubeSegments * 6);

        for (int i = 0; i < segments; i++)
        {
            float theta = Mathf.PI * 2f * i / segments;
            float cosTheta = Mathf.Cos(theta);
            float sinTheta = Mathf.Sin(theta);

            for (int j = 0; j < tubeSegments; j++)
            {
                float phi = Mathf.PI * 2f * j / tubeSegments;
                float radial = majorRadius + Mathf.Cos(phi) * tubeRadius;

                verts.Add(new Vector3(radial * cosTheta, Mathf.Sin(phi) * tubeRadius,
                    radial * sinTheta));
                uvs.Add(new Vector2((float)i / segments, (float)j / tubeSegments));
            }
        }

        for (int i = 0; i < segments; i++)
        {
            int nextI = (i + 1) % segments;
            for (int j = 0; j < tubeSegments; j++)
            {
                int nextJ = (j + 1) % tubeSegments;

                int a = i * tubeSegments + j;
                int b = nextI * tubeSegments + j;
                int c = nextI * tubeSegments + nextJ;
                int d = i * tubeSegments + nextJ;

                // Winding chosen so the normals point away from the tube core.
                triangles.Add(a);
                triangles.Add(c);
                triangles.Add(b);

                triangles.Add(a);
                triangles.Add(d);
                triangles.Add(c);
            }
        }

        Mesh mesh = new Mesh();
        mesh.name = "PieceTorus";
        mesh.SetVertices(verts);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    /// <summary>Cached torus mesh so repeated striker builds share one mesh.</summary>
    public static Mesh GetTorus(string key, float majorRadius, float tubeRadius,
        int segments = 72, int tubeSegments = 10)
    {
        Mesh mesh;
        if (meshCache.TryGetValue(key, out mesh) && mesh != null)
            return mesh;

        mesh = CreateTorus(majorRadius, tubeRadius, segments, tubeSegments);
        meshCache[key] = mesh;
        return mesh;
    }

    /// <summary>
    /// Low-poly cylinder used only for collision. Convex mesh colliders give the
    /// pieces a round footprint, so cut shots and ghost-ball aim behave like real
    /// discs instead of snagging on box corners.
    /// </summary>
    public static Mesh GetColliderCylinder(string key, float radius, float thickness)
    {
        return GetDisc(key, radius, thickness, 0.0006f, 24);
    }

    /// <summary>
    /// Soft, unlit blob shadow parented to a piece. Sits just above the board so
    /// the piece looks like it rests on the surface instead of floating.
    /// </summary>
    public static GameObject CreateContactShadow(Transform parent, float radius, float localY, float spread = 2.6f)
    {
        GameObject shadow = GameObject.CreatePrimitive(PrimitiveType.Quad);
        shadow.name = "ContactShadow";
        shadow.transform.SetParent(parent, false);
        shadow.transform.localPosition = new Vector3(0f, localY, 0f);
        shadow.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        shadow.transform.localScale = new Vector3(radius * spread, radius * spread, 1f);

        MeshRenderer renderer = shadow.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = GetShadowMaterial();
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;

        Collider collider = shadow.GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
            Object.Destroy(collider);
        }

        return shadow;
    }

    private static int AddRing(
        List<Vector3> verts, List<Vector2> uvs, float radius, float y, int segments, float uvRadius = -1f)
    {
        int start = verts.Count;
        float safeRadius = Mathf.Max(uvRadius > 0f ? uvRadius : radius, 0.0001f);

        for (int i = 0; i < segments; i++)
        {
            float angle = Mathf.PI * 2f * i / segments;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;

            verts.Add(new Vector3(x, y, z));
            uvs.Add(new Vector2(0.5f + x / (safeRadius * 2f), 0.5f + z / (safeRadius * 2f)));
        }

        return start;
    }

    private static int AddVertex(List<Vector3> verts, List<Vector2> uvs, Vector3 position, Vector2 uv)
    {
        verts.Add(position);
        uvs.Add(uv);
        return verts.Count - 1;
    }

    /// <summary>Quads between two rings. <paramref name="faceUp"/> flips the winding for top faces.</summary>
    private static void AddBand(List<int> triangles, int ringA, int ringB, int segments, bool faceUp)
    {
        for (int i = 0; i < segments; i++)
        {
            int a = ringA + i;
            int b = ringA + (i + 1) % segments;
            int c = ringB + i;
            int d = ringB + (i + 1) % segments;

            if (faceUp)
            {
                // ringA = inner, ringB = outer
                triangles.Add(a);
                triangles.Add(b);
                triangles.Add(c);

                triangles.Add(b);
                triangles.Add(d);
                triangles.Add(c);
            }
            else
            {
                // ringA = lower, ringB = upper
                triangles.Add(a);
                triangles.Add(c);
                triangles.Add(b);

                triangles.Add(b);
                triangles.Add(c);
                triangles.Add(d);
            }
        }
    }

    private static Texture2D CreateShadowTexture()
    {
        const int size = 64;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.name = "ContactShadow";
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        float half = size * 0.5f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float px = (x + 0.5f - half) / half;
                float py = (y + 0.5f - half) / half;
                float distance = Mathf.Sqrt(px * px + py * py);

                // Dense core fading smoothly to nothing before the quad edge.
                float alpha = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.30f, 1f, distance));
                alpha *= Mathf.Lerp(0.95f, 0.62f, Mathf.Clamp01(distance));
                alpha *= 0.46f;

                tex.SetPixel(x, y, new Color(0.16f, 0.10f, 0.06f, alpha));
            }
        }

        tex.Apply(false, true);
        return tex;
    }

    private static Material GetShadowMaterial()
    {
        if (shadowMaterial != null) return shadowMaterial;

        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null) shader = Shader.Find("Unlit/Transparent");
        if (shader == null) shader = Shader.Find("Standard");

        shadowMaterial = new Material(shader);
        shadowMaterial.name = "ContactShadowMaterial";
        shadowMaterial.mainTexture = CreateShadowTexture();

        if (shadowMaterial.HasProperty("_Color"))
            shadowMaterial.SetColor("_Color", Color.white);

        shadowMaterial.renderQueue = 2900;
        return shadowMaterial;
    }
}
