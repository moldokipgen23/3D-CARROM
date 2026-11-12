using UnityEngine;

public static class ProceduralTextures
{
    private static Texture2D cachedPlywood;
    private static Texture2D cachedRosewood;

    /// <summary>Light warm plywood used for the carrom playing surface.</summary>
    public static Texture2D GetPlywoodTexture()
    {
        if (cachedPlywood != null) return cachedPlywood;
        cachedPlywood = CreatePlywoodTexture(512);
        return cachedPlywood;
    }

    /// <summary>Deep reddish hardwood used for the board frame.</summary>
    public static Texture2D GetRosewoodTexture()
    {
        if (cachedRosewood != null) return cachedRosewood;
        cachedRosewood = CreateRosewoodTexture(512);
        return cachedRosewood;
    }

    public static Texture2D CreatePlywoodTexture(int size = 512)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color light = new Color(0.78f, 0.58f, 0.35f);
        Color mid = new Color(0.67f, 0.47f, 0.26f);
        Color dark = new Color(0.52f, 0.34f, 0.17f);

        for (int y = 0; y < size; y++)
        {
            float v = (float)y / size;
            for (int x = 0; x < size; x++)
            {
                float u = (float)x / size;

                // Long, soft grain running across the board.
                // Long, soft grain running along the board (x). Three octaves
                // keep it organic - a regular wave reads as ugly banding.
                float grain = Mathf.PerlinNoise(x * 0.006f, y * 0.05f) * 0.62f;
                float streak = Mathf.PerlinNoise(x * 0.022f + 13f, y * 0.17f + 7f) * 0.30f;
                float fine = Mathf.PerlinNoise(x * 0.11f + 31f, y * 0.62f + 19f) * 0.16f;
                float pore = Mathf.PerlinNoise(x * 0.85f + 5f, y * 0.12f + 2f) * 0.06f;

                float t = Mathf.Clamp01(grain + streak + fine + pore);
                Color c = Color.Lerp(light, mid, t * 0.85f);
                c = Color.Lerp(c, dark, Mathf.Clamp01((t - 0.62f) * 1.4f));

                // Gentle centre brightness so the playing area feels lit.
                float dx = u - 0.5f;
                float dy = v - 0.5f;
                float vig = Mathf.Clamp01(1f - (dx * dx + dy * dy) * 1.5f);
                c *= 0.80f + vig * 0.16f;

                c.a = 1f;
                tex.SetPixel(x, y, c);
            }
        }

        tex.Apply(false, true);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;
        return tex;
    }

    public static Texture2D CreateRosewoodTexture(int size = 512)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color light = new Color(0.42f, 0.21f, 0.11f);
        Color mid = new Color(0.30f, 0.13f, 0.06f);
        Color dark = new Color(0.18f, 0.07f, 0.03f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float grain = Mathf.PerlinNoise(x * 0.02f, y * 0.16f);
                float fine = Mathf.PerlinNoise(x * 0.12f, y * 0.55f) * 0.35f;
                float streak = Mathf.PerlinNoise(x * 0.5f, y * 0.03f) * 0.18f;

                float t = Mathf.Clamp01(grain * 0.65f + fine + streak);
                Color c = Color.Lerp(mid, light, Mathf.Clamp01((t - 0.35f) * 1.6f));
                c = Color.Lerp(c, dark, Mathf.Clamp01((0.55f - t) * 1.8f));

                c.a = 1f;
                tex.SetPixel(x, y, c);
            }
        }

        tex.Apply(false, true);
        tex.wrapMode = TextureWrapMode.Repeat;
        tex.filterMode = FilterMode.Bilinear;
        return tex;
    }

    /// <summary>
    /// Lacquered face print for a coin: domed shading, turned grooves, embossed
    /// concentric rings and a centre motif. The relief shading is what makes the
    /// flat top read as a solid, polished 3D piece on a phone screen.
    /// </summary>
    public static Texture2D CreateCoinFaceTexture(Color face, Color ring, bool queen = false, int size = 256)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.name = queen ? "QueenCoinFace" : "CoinFace";
        float half = size * 0.5f;
        Color gold = new Color(1f, 0.84f, 0.38f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float px = (x + 0.5f - half) / half;
                float py = (y + 0.5f - half) / half;
                float r = Mathf.Sqrt(px * px + py * py);

                // Domed body: brighter towards the middle, darker at the edge.
                float relief = Mathf.Clamp01(1f - r * r);
                Color c = Color.Lerp(face, Color.Lerp(face, Color.white, 0.18f), relief * 0.6f);

                // Fine turned grooves left by the lathe.
                float groove = 0.5f + 0.5f * Mathf.Sin(r * 230f);
                c *= 0.972f + groove * 0.056f;

                // Raised rings: dark inner lip, bright outer lip.
                c = EmbossedBand(c, r, 0.845f, 0.955f, ring);
                c = EmbossedBand(c, r, 0.610f, 0.672f, ring);
                c = EmbossedBand(c, r, 0.300f, 0.352f, ring);

                if (queen)
                {
                    c = QueenMotif(c, px, py, r, gold, ring, face);
                }
                else
                {
                    // Twelve petal rosette stamped in the centre.
                    float angle = Mathf.Atan2(py, px);
                    float petal = 0.122f + Mathf.Cos(angle * 12f) * 0.048f;
                    if (r < 0.205f)
                    {
                        if (r > 0.185f)
                            c = Color.Lerp(c, ring, 0.8f);
                        else if (r < petal)
                            c = Color.Lerp(c, ring, 0.62f);
                        else
                            c = Color.Lerp(c, face, 0.45f);
                    }
                }

                // Glossy sheen from the key light (upper left of the face).
                float sheen = Mathf.Clamp01(1f - Vector2.Distance(
                    new Vector2(px, py), new Vector2(-0.33f, 0.35f)) / 1.05f);
                c = Color.Lerp(c, Color.white, sheen * sheen * 0.20f);

                // Recessed edge where the print meets the bevel.
                c = Color.Lerp(c, face * 0.62f, SmoothRange(0.94f, 1f, r));

                c.a = 1f;
                tex.SetPixel(x, y, c);
            }
        }

        tex.Apply(false, true);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;
        return tex;
    }

    /// <summary>Ivory striker face: polished lacquer with red and gold inlays.</summary>
    public static Texture2D CreateStrikerFaceTexture(int size = 256)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.name = "StrikerFace";

        Color ivory = new Color(0.98f, 0.95f, 0.88f);
        Color red = new Color(0.72f, 0.12f, 0.10f);
        Color gold = new Color(0.93f, 0.74f, 0.32f);

        float half = size * 0.5f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float px = (x + 0.5f - half) / half;
                float py = (y + 0.5f - half) / half;
                float r = Mathf.Sqrt(px * px + py * py);

                float relief = Mathf.Clamp01(1f - r * r);
                Color c = Color.Lerp(ivory, Color.white, relief * 0.35f);

                float groove = 0.5f + 0.5f * Mathf.Sin(r * 210f);
                c *= 0.978f + groove * 0.045f;

                c = EmbossedBand(c, r, 0.905f, 0.965f, gold);
                c = EmbossedBand(c, r, 0.640f, 0.780f, red);
                c = EmbossedBand(c, r, 0.435f, 0.470f, gold);

                if (r < 0.155f)
                {
                    float t = Mathf.Clamp01(r / 0.155f);
                    c = Color.Lerp(Color.Lerp(gold, Color.white, 0.25f), gold * 0.92f, t);
                    c = Color.Lerp(c, red, SmoothRange(0.72f, 1f, t) * 0.75f);
                }

                float sheen = Mathf.Clamp01(1f - Vector2.Distance(
                    new Vector2(px, py), new Vector2(-0.30f, 0.36f)) / 1.0f);
                c = Color.Lerp(c, Color.white, sheen * sheen * 0.30f);

                c = Color.Lerp(c, ivory * 0.70f, SmoothRange(0.94f, 1f, r));

                c.a = 1f;
                tex.SetPixel(x, y, c);
            }
        }

        tex.Apply(false, true);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;
        return tex;
    }

    /// <summary>GLSL-style smoothstep between two edges, returning 0..1.</summary>
    private static float SmoothRange(float edge0, float edge1, float x)
    {
        if (Mathf.Approximately(edge0, edge1)) return x >= edge1 ? 1f : 0f;
        float t = Mathf.Clamp01((x - edge0) / (edge1 - edge0));
        return t * t * (3f - 2f * t);
    }

    private static Color EmbossedBand(Color c, float r, float inner, float outer, Color bandColor)
    {
        if (r < inner || r > outer) return c;

        float t = (r - inner) / Mathf.Max(0.0001f, outer - inner);
        Color band = bandColor;

        // Dark inside lip.
        band = Color.Lerp(bandColor * 0.68f, band, Mathf.Clamp01(t * 4.5f));
        // Bright outside lip.
        band = Color.Lerp(band, Color.Lerp(bandColor, Color.white, 0.38f),
                          Mathf.Clamp01((t - 0.72f) * 3.6f));

        return Color.Lerp(c, band, 0.94f);
    }

    private static Color QueenMotif(Color c, float px, float py, float r, Color gold, Color ring, Color face)
    {
        // Solid gold disc behind the crown so the queen reads at a glance.
        if (r < 0.215f)
            c = Color.Lerp(c, gold * 0.92f, SmoothRange(0.215f, 0.195f, r) * 0.85f);

        float crownBase = -0.095f;
        float crownTop = 0.085f;
        float width = 0.145f;

        bool crownBar = py >= crownBase && py <= crownBase + 0.045f && Mathf.Abs(px) <= width;
        bool body = py > crownBase + 0.045f && py <= crownTop && Mathf.Abs(px) <= width;
        bool spikes = py > crownTop && py <= crownTop + 0.085f
                      && Mathf.Abs(px) <= width
                      && Mathf.Abs(Mathf.Sin(px * 24f)) > 0.35f;

        if (crownBar || body || spikes)
            c = Color.Lerp(c, new Color(0.48f, 0.12f, 0.09f), 0.92f);

        return c;
    }

    public static Texture2D CreateWoodTexture(int width = 256, int height = 256)
    {
        Texture2D tex = new Texture2D(width, height);
        Color baseColor = new Color(0.45f, 0.25f, 0.12f);
        Color darkGrain = new Color(0.32f, 0.17f, 0.07f);
        Color lightGrain = new Color(0.52f, 0.3f, 0.15f);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float grain = Mathf.PerlinNoise(x * 0.02f, y * 0.15f) * 0.3f;
                float ring = Mathf.PerlinNoise(x * 0.005f, y * 0.005f) * 0.15f;
                float noise = Mathf.PerlinNoise(x * 0.1f, y * 0.1f) * 0.05f;

                float t = grain + ring + noise;
                Color color = Color.Lerp(baseColor, darkGrain, Mathf.Clamp01(t));

                if ((x + y) % 80 < 2)
                    color = Color.Lerp(color, darkGrain, 0.3f);

                tex.SetPixel(x, y, color);
            }
        }
        tex.Apply();
        tex.wrapMode = TextureWrapMode.Repeat;
        return tex;
    }

    public static Texture2D CreateFeltTexture(int width = 256, int height = 256)
    {
        Texture2D tex = new Texture2D(width, height);
        Color baseColor = new Color(0.15f, 0.38f, 0.18f);
        Color darkFelt = new Color(0.12f, 0.3f, 0.14f);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float noise = Mathf.PerlinNoise(x * 0.3f, y * 0.3f) * 0.08f;
                float fiber = Mathf.PerlinNoise(x * 0.8f, y * 0.8f) * 0.04f;
                Color color = Color.Lerp(baseColor, darkFelt, noise + fiber);
                tex.SetPixel(x, y, color);
            }
        }
        tex.Apply();
        tex.wrapMode = TextureWrapMode.Repeat;
        return tex;
    }

    public static Texture2D CreateMetallicTexture(int width = 128, int height = 128, Color baseColor = default)
    {
        if (baseColor == default) baseColor = new Color(0.85f, 0.75f, 0.3f);

        Texture2D tex = new Texture2D(width, height);
        Color darkMetal = baseColor * 0.7f;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float radial = Mathf.Sqrt(Mathf.Pow((x - width / 2f) / (width / 2f), 2) +
                                          Mathf.Pow((y - height / 2f) / (height / 2f), 2));
                float highlight = Mathf.Clamp01(1f - radial) * 0.3f;
                float noise = Mathf.PerlinNoise(x * 0.2f, y * 0.2f) * 0.05f;

                Color color = Color.Lerp(darkMetal, baseColor, highlight + noise);
                tex.SetPixel(x, y, color);
            }
        }
        tex.Apply();
        return tex;
    }

    public static Texture2D CreateCoinTexture(int width = 128, int height = 128, Color mainColor = default, bool isWhite = true)
    {
        if (mainColor == default)
            mainColor = isWhite ? new Color(0.95f, 0.92f, 0.85f) : new Color(0.12f, 0.1f, 0.08f);

        Texture2D tex = new Texture2D(width, height);
        Color rimColor = new Color(0.85f, 0.7f, 0.2f);
        Color darkColor = mainColor * 0.8f;

        float center = width / 2f;
        float radius = width / 2f - 4;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                float normalizedDist = dist / radius;

                Color color;
                if (normalizedDist > 0.92f)
                {
                    color = rimColor;
                }
                else if (normalizedDist > 0.85f)
                {
                    color = Color.Lerp(mainColor, rimColor, (normalizedDist - 0.85f) / 0.07f);
                }
                else if (normalizedDist > 0.6f)
                {
                    float ring = Mathf.Sin(normalizedDist * 20f) * 0.05f;
                    color = Color.Lerp(mainColor, darkColor, ring + 0.1f);
                }
                else
                {
                    float shine = Mathf.Clamp01(1f - normalizedDist / 0.6f) * 0.15f;
                    color = Color.Lerp(mainColor, Color.white, shine);
                }

                float edge = Mathf.Clamp01((radius - dist) / 3f);
                color.a = edge;

                tex.SetPixel(x, y, color);
            }
        }
        tex.Apply();
        return tex;
    }
}
