using UnityEngine;

public static class ProceduralTextures
{
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
