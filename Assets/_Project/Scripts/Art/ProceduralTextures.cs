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

    /// <summary>Face print for a coin: concentric rings plus a small motif.</summary>
    public static Texture2D CreateCoinFaceTexture(Color face, Color ring, bool queen = false, int size = 128)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float half = size * 0.5f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float px = (x + 0.5f - half) / half;
                float py = (y + 0.5f - half) / half;
                float r = Mathf.Sqrt(px * px + py * py);

                Color c = face;

                // printed concentric bands
                if (r > 0.34f && r < 0.40f) c = Color.Lerp(c, ring, 0.55f);
                if (r > 0.62f && r < 0.70f) c = Color.Lerp(c, ring, 0.85f);
                if (r > 0.86f) c = Color.Lerp(c, ring, 0.35f);

                if (queen)
                {
                    // simple crown silhouette
                    float crownTop = 0.30f;
                    float crownBottom = 0.34f;
                    bool inCrown = py > -0.18f && py < crownTop &&
                                   Mathf.Abs(px) < crownBottom * (1f - Mathf.Clamp01((py + 0.18f) / 0.5f) * 0.35f);
                    if (inCrown && (py < 0.10f || Mathf.Abs(Mathf.Sin(px * 18f)) > 0.55f))
                        c = Color.Lerp(c, new Color(1f, 0.86f, 0.42f), 0.9f);
                }
                else
                {
                    // small centre star
                    float star = Mathf.Pow(Mathf.Abs(px), 0.6f) + Mathf.Pow(Mathf.Abs(py), 0.6f);
                    if (star < 0.28f) c = Color.Lerp(c, ring, 0.55f);
                }

                // soft sheen
                float sheen = Mathf.Clamp01(1f - Vector2.Distance(
                    new Vector2(px, py), new Vector2(-0.35f, 0.35f)) / 0.9f);
                c = Color.Lerp(c, Color.white, sheen * 0.18f);

                c.a = 1f;
                tex.SetPixel(x, y, c);
            }
        }

        tex.Apply(false, true);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;
        return tex;
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
