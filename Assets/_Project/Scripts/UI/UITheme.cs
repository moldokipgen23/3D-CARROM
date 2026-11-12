using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Procedural UI art + layout helpers. The project ships no imported art, so
/// every panel, button, icon and gradient is generated at runtime from code.
/// Sprites are cached, so repeated builds are cheap.
/// </summary>
public static class UITheme
{
    // ---------------------------------------------------------------- palette
    public static readonly Color BgDark = new Color(0.055f, 0.040f, 0.030f, 1f);
    public static readonly Color PanelBg = new Color(0.115f, 0.080f, 0.052f, 0.95f);
    public static readonly Color PanelSoft = new Color(0.165f, 0.115f, 0.072f, 0.92f);
    public static readonly Color Gold = new Color(0.960f, 0.770f, 0.340f, 1f);
    public static readonly Color GoldDeep = new Color(0.760f, 0.530f, 0.150f, 1f);
    public static readonly Color Cream = new Color(0.975f, 0.940f, 0.860f, 1f);
    public static readonly Color Muted = new Color(0.720f, 0.660f, 0.560f, 1f);
    public static readonly Color Ink = new Color(0.150f, 0.095f, 0.045f, 1f);
    public static readonly Color Green = new Color(0.180f, 0.520f, 0.300f, 1f);
    public static readonly Color Red = new Color(0.620f, 0.170f, 0.140f, 1f);

    private static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

    // ------------------------------------------------------------- primitives
    public static Sprite Rounded(int radius, Color fill, Color stroke, int strokeWidth)
    {
        string key = "r" + radius + "_" + ColorKey(fill) + "_" + ColorKey(stroke) + "_" + strokeWidth;
        Sprite cached;
        if (Cache.TryGetValue(key, out cached)) return cached;

        int size = radius * 2 + 8;
        Texture2D tex = NewTexture(size, size);
        float half = size * 0.5f - 1f;
        float r = radius;
        float feather = 1.1f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float px = x + 0.5f - size * 0.5f;
                float py = y + 0.5f - size * 0.5f;

                float dx = Mathf.Abs(px) - (half - r);
                float dy = Mathf.Abs(py) - (half - r);
                float qx = Mathf.Max(dx, 0f);
                float qy = Mathf.Max(dy, 0f);
                float d = Mathf.Sqrt(qx * qx + qy * qy) + Mathf.Min(Mathf.Max(dx, dy), 0f) - r;

                float alpha = Mathf.Clamp01(0.5f - d / feather);
                Color c = (strokeWidth > 0 && d > -strokeWidth) ? stroke : fill;
                c.a *= alpha;
                tex.SetPixel(x, y, c);
            }
        }

        tex.Apply(false, true);
        int border = radius + 2;
        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f),
            100f, 0, SpriteMeshType.FullRect, new Vector4(border, border, border, border));
        Cache[key] = sprite;
        return sprite;
    }

    public static Sprite Circle(Color fill, Color stroke, int strokeWidth)
    {
        string key = "c_" + ColorKey(fill) + "_" + ColorKey(stroke) + "_" + strokeWidth;
        Sprite cached;
        if (Cache.TryGetValue(key, out cached)) return cached;

        int size = 128;
        Texture2D tex = NewTexture(size, size);
        float half = size * 0.5f - 1f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float px = x + 0.5f - size * 0.5f;
                float py = y + 0.5f - size * 0.5f;
                float d = Mathf.Sqrt(px * px + py * py) - half;
                float alpha = Mathf.Clamp01(0.5f - d);
                Color c = (strokeWidth > 0 && d > -strokeWidth) ? stroke : fill;
                c.a *= alpha;
                tex.SetPixel(x, y, c);
            }
        }

        tex.Apply(false, true);
        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        Cache[key] = sprite;
        return sprite;
    }

    public static Sprite VerticalGradient(Color top, Color bottom, float topAlpha = 1f, float bottomAlpha = 1f)
    {
        string key = "g_" + ColorKey(top) + "_" + ColorKey(bottom) + "_" + topAlpha + "_" + bottomAlpha;
        Sprite cached;
        if (Cache.TryGetValue(key, out cached)) return cached;

        int h = 256;
        Texture2D tex = new Texture2D(4, h, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < h; y++)
        {
            float t = (float)y / (h - 1);
            Color c = Color.Lerp(bottom, top, t);
            c.a = Mathf.Lerp(bottomAlpha, topAlpha, t);
            for (int x = 0; x < 4; x++) tex.SetPixel(x, y, c);
        }

        tex.Apply(false, true);
        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, 4, h), new Vector2(0.5f, 0.5f), 100f);
        Cache[key] = sprite;
        return sprite;
    }

    public static Sprite Vignette()
    {
        Sprite cached;
        if (Cache.TryGetValue("vignette", out cached)) return cached;

        int size = 256;
        Texture2D tex = NewTexture(size, size);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float px = (x + 0.5f) / size * 2f - 1f;
                float py = (y + 0.5f) / size * 2f - 1f;
                float r = Mathf.Sqrt(px * px + py * py) / 1.35f;
                float a = Mathf.SmoothStep(0f, 0.85f, Mathf.Clamp01(r));
                tex.SetPixel(x, y, new Color(0.02f, 0.012f, 0.008f, a));
            }
        }

        tex.Apply(false, true);
        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        Cache["vignette"] = sprite;
        return sprite;
    }

    /// <summary>Glossy carrom coin icon (white or black) used in score chips.</summary>
    public static Sprite CoinIcon(bool white)
    {
        string key = white ? "coin_white" : "coin_black";
        Sprite cached;
        if (Cache.TryGetValue(key, out cached)) return cached;

        int size = 128;
        Texture2D tex = NewTexture(size, size);
        float half = size * 0.5f - 1f;

        Color face = white ? new Color(0.97f, 0.94f, 0.86f) : new Color(0.16f, 0.13f, 0.10f);
        Color faceDark = white ? new Color(0.80f, 0.75f, 0.64f) : new Color(0.06f, 0.05f, 0.04f);
        Color rim = white ? new Color(0.86f, 0.68f, 0.28f) : new Color(0.72f, 0.55f, 0.22f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float px = x + 0.5f - size * 0.5f;
                float py = y + 0.5f - size * 0.5f;
                float dist = Mathf.Sqrt(px * px + py * py);
                float alpha = Mathf.Clamp01(half - dist);

                Color c;
                if (dist > half - 7f)
                {
                    float shade = Mathf.Lerp(0.9f, 1.15f, (py + half) / (half * 2f));
                    c = rim * shade;
                }
                else
                {
                    float t = Mathf.Clamp01((dist / half) * 1.1f);
                    c = Color.Lerp(face, faceDark, t * 0.55f);
                }

                // subtle top-left specular sheen
                float sheen = Mathf.Clamp01(1f - Vector2.Distance(
                    new Vector2(px, py), new Vector2(-half * 0.35f, half * 0.35f)) / (half * 0.75f));
                c = Color.Lerp(c, Color.white, sheen * (white ? 0.22f : 0.10f));

                c.a = alpha;
                tex.SetPixel(x, y, c);
            }
        }

        tex.Apply(false, true);
        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        Cache[key] = sprite;
        return sprite;
    }

    /// <summary>
    /// Procedural top-down striker used as the baseline slider knob. It samples
    /// the same printed face as the 3D piece (ivory lacquer + red rosette),
    /// overlays a turned gold rim and carries a soft drop shadow so it lifts
    /// off the rail.
    /// </summary>
    public static Sprite StrikerIcon()
    {
        const string key = "striker_icon";
        Sprite cached;
        if (Cache.TryGetValue(key, out cached)) return cached;

        int size = 256;
        Texture2D face = ProceduralTextures.CreateStrikerFaceTexture(size);
        Color32[] facePixels = face.GetPixels32();
        Texture2D tex = NewTexture(size, size);
        float half = size * 0.5f;

        Color gold = new Color(0.900f, 0.700f, 0.330f);
        Color goldDeep = new Color(0.500f, 0.335f, 0.095f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float px = (x + 0.5f - half) / half;
                float py = (y + 0.5f - half) / half;
                float r = Mathf.Sqrt(px * px + py * py);

                Color c;
                float alpha;

                if (r > 1f)
                {
                    // Soft shadow just outside the rim, biased downwards.
                    float sr = Mathf.Sqrt(px * px + (py + 0.055f) * (py + 0.055f));
                    alpha = Mathf.Clamp01((1.13f - sr) / 0.13f) * 0.34f;
                    c = new Color(0.03f, 0.02f, 0.012f);
                }
                else
                {
                    c = facePixels[y * size + x];

                    if (r > 0.900f)
                    {
                        // Turned gold rim, brightest near the outer edge.
                        c = Color.Lerp(goldDeep, gold, Mathf.Clamp01((1f - r) / 0.100f));
                    }

                    float light = Mathf.Clamp01(0.55f - px * 0.30f + py * 0.42f);
                    c *= 0.92f + light * 0.14f;

                    // Crisp anti-aliased circular edge.
                    alpha = Mathf.Clamp01((1f - r) * half * 0.9f);
                }

                c.a = alpha;
                tex.SetPixel(x, y, c);
            }
        }

        tex.Apply(false, true);
        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        Cache[key] = sprite;
        return sprite;
    }

    /// <summary>
    /// Circular player portrait generated from code: a lit human bust for the
    /// local player and a metallic robot for the AI. Used in the game HUD so
    /// both sides have a proper profile instead of a plain coin face.
    /// </summary>
    public static Sprite PlayerPortrait(bool human)
    {
        string key = human ? "portrait_human" : "portrait_ai";
        Sprite cached;
        if (Cache.TryGetValue(key, out cached)) return cached;

        int size = 256;
        Texture2D tex = NewTexture(size, size);
        float half = size * 0.5f;

        Color humanBgTop = new Color(0.16f, 0.30f, 0.24f);
        Color humanBgBottom = new Color(0.05f, 0.10f, 0.08f);
        Color aiBgTop = new Color(0.20f, 0.26f, 0.34f);
        Color aiBgBottom = new Color(0.06f, 0.08f, 0.11f);

        Color skin = new Color(0.86f, 0.66f, 0.48f);
        Color skinShade = new Color(0.64f, 0.45f, 0.31f);
        Color hair = new Color(0.13f, 0.085f, 0.06f);
        Color cloth = new Color(0.22f, 0.36f, 0.46f);
        Color clothShade = new Color(0.11f, 0.20f, 0.28f);

        Color metal = new Color(0.76f, 0.80f, 0.84f);
        Color metalShade = new Color(0.35f, 0.40f, 0.46f);
        Color eye = new Color(1f, 0.72f, 0.26f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float px = (x + 0.5f - half) / half;
                float py = (y + 0.5f - half) / half;
                float r = Mathf.Sqrt(px * px + py * py);

                // Feather the circular mask to nothing at the sprite edge. The
                // texture starts uninitialised, so transparent pixels must be
                // written explicitly or they can render as opaque grey corners.
                float alpha = Mathf.Clamp01((1f - r) * half * 1.6f);
                if (alpha <= 0.001f)
                {
                    tex.SetPixel(x, y, new Color(0f, 0f, 0f, 0f));
                    continue;
                }

                float v = Mathf.Clamp01((py + 1f) * 0.5f);
                Color c = Color.Lerp(human ? humanBgBottom : aiBgBottom,
                                     human ? humanBgTop : aiBgTop, v);

                // Soft vignette so the portrait frame reads as a lens.
                float vig = Mathf.Clamp01(1f - r * r * 0.75f);
                c *= Mathf.Lerp(0.55f, 1f, vig);

                if (human)
                {
                    // Shoulders and jacket.
                    float sx = px / 0.78f;
                    float sy = (py + 0.72f) / 0.55f;
                    float shoulder = sx * sx + sy * sy;
                    if (sy > -0.2f && shoulder < 1f)
                    {
                        float t = Mathf.Clamp01((px + 0.4f) * 0.6f + (py + 0.7f) * 0.35f);
                        c = Color.Lerp(clothShade, cloth, t);

                        // Jacket collar framing the neck.
                        if (Mathf.Abs(px) > 0.13f && Mathf.Abs(px) < 0.36f &&
                            py > -0.64f && py < -0.40f)
                            c = clothShade * 0.72f;
                    }

                    // Neck.
                    if (Mathf.Abs(px) < 0.16f && py > -0.42f && py < -0.08f)
                        c = Color.Lerp(skinShade, skin, Mathf.Clamp01((px + 0.16f) * 2f));

                    // Head.
                    float hx = px / 0.285f;
                    float hy = (py - 0.12f) / 0.33f;
                    float head = hx * hx + hy * hy;
                    if (head < 1f)
                    {
                        float lit = Mathf.Clamp01(1f - Mathf.Sqrt(
                            (px + 0.20f) * (px + 0.20f) + (py - 0.30f) * (py - 0.30f)) / 0.55f);
                        c = Color.Lerp(skinShade, skin, 0.35f + lit * 0.65f);

                        // Cheek warmth.
                        float cheek = Mathf.Abs(px);
                        if (cheek > 0.16f && cheek < 0.26f && py > -0.16f && py < 0.02f)
                            c = Color.Lerp(c, new Color(0.82f, 0.50f, 0.38f), 0.18f);

                        // Hair cap with a side part.
                        float hairLine = 0.20f - px * 0.18f;
                        if (py > hairLine)
                            c = Color.Lerp(hair, new Color(0.22f, 0.15f, 0.10f), lit * 0.7f);
                        if (Mathf.Abs(px) > 0.20f && py > -0.10f && py < 0.22f)
                            c = Color.Lerp(c, hair, 0.55f);

                        // Eyes: sclera, dark iris, catch light.
                        float eyeX = 0.105f;
                        float eyeY = 0.115f;
                        for (int e = -1; e <= 1; e += 2)
                        {
                            float ex = px - e * eyeX;
                            float ey = py - eyeY;
                            float white = ex * ex / 0.0026f + ey * ey / 0.0011f;
                            if (white < 1f)
                            {
                                c = new Color(0.96f, 0.94f, 0.90f);
                                float iris = ex * ex / 0.0011f + ey * ey / 0.0011f;
                                if (iris < 1f)
                                    c = new Color(0.13f, 0.09f, 0.065f);
                                float spark = (ex + 0.012f) * (ex + 0.012f) +
                                               (ey - 0.009f) * (ey - 0.009f);
                                if (spark < 0.00010f) c = Color.white;
                            }

                            // Brow.
                            float brow = ex * ex / 0.0030f + (py - 0.175f) * (py - 0.175f) / 0.00035f;
                            if (brow < 1f && py > eyeY)
                                c = Color.Lerp(hair, skinShade, 0.25f);
                        }

                        // Nose and mouth.
                        if (Mathf.Abs(px - 0.008f) < 0.026f && py > -0.02f && py < 0.075f)
                            c = Color.Lerp(c, skinShade, 0.45f);
                        if (Mathf.Abs(px) < 0.085f && py > -0.105f && py < -0.085f)
                            c = Color.Lerp(skinShade, hair, 0.42f);
                        if (Mathf.Abs(px) < 0.090f && py > -0.135f && py < -0.115f)
                            c = Color.Lerp(c, skinShade, 0.35f);
                    }

                    // Ears.
                    float earL = (px + 0.29f) * (px + 0.29f) + (py - 0.10f) * (py - 0.10f);
                    float earR = (px - 0.29f) * (px - 0.29f) + (py - 0.10f) * (py - 0.10f);
                    if (earL < 0.0035f || earR < 0.0035f)
                        c = skinShade;
                }
                else
                {
                    // Shoulders: dark armour rather than a pale bust.
                    float sx = px / 0.82f;
                    float sy = (py + 0.78f) / 0.52f;
                    if (sy > -0.2f && sx * sx + sy * sy < 1f)
                    {
                        float t = Mathf.Clamp01(1f - sy * 0.7f);
                        c = Color.Lerp(metalShade * 0.30f, metalShade * 0.62f, t);

                        // Shoulder plate highlights.
                        if (Mathf.Abs(px) > 0.46f && Mathf.Abs(px) < 0.72f &&
                            py > -0.86f && py < -0.50f)
                            c = Color.Lerp(c, metal, 0.35f);
                    }

                    // Chest reactor.
                    float core = px * px + (py + 0.50f) * (py + 0.50f);
                    if (core < 0.0045f)
                        c = Color.Lerp(eye, Color.white, 0.35f);
                    else if (core < 0.0075f)
                        c = metalShade * 0.55f;

                    // Neck joint.
                    if (Mathf.Abs(px) < 0.18f && py > -0.50f && py < -0.30f)
                        c = metalShade * 0.65f;

                    // Antenna.
                    if (Mathf.Abs(px) < 0.018f && py > 0.34f && py < 0.58f)
                        c = metal * 0.85f;
                    if (px * px + (py - 0.60f) * (py - 0.60f) < 0.0022f)
                        c = eye;

                    // Head plate.
                    float hx = px / 0.34f;
                    float hy = (py - 0.08f) / 0.30f;
                    float head = hx * hx + hy * hy;
                    if (head < 1f)
                    {
                        float lit = Mathf.Clamp01(1f - Mathf.Sqrt(
                            (px + 0.18f) * (px + 0.18f) + (py - 0.28f) * (py - 0.28f)) / 0.62f);
                        c = Color.Lerp(metalShade, metal, 0.25f + lit * 0.75f);

                        // Dark visor band.
                        if (Mathf.Abs(px) < 0.255f && py > 0.055f && py < 0.205f)
                            c = new Color(0.055f, 0.085f, 0.115f);

                        // Eye sockets.
                        float eyeL = (px + 0.135f) * (px + 0.135f) + (py - 0.12f) * (py - 0.12f);
                        float eyeR = (px - 0.135f) * (px - 0.135f) + (py - 0.12f) * (py - 0.12f);
                        if (eyeL < 0.0085f || eyeR < 0.0085f)
                            c = Color.Lerp(eye, Color.white, Mathf.Clamp01(0.55f - eyeL * 55f));

                        // Mouth grille.
                        if (Mathf.Abs(px) < 0.14f && py > -0.145f && py < -0.115f)
                            c = metalShade * 0.7f;
                    }
                }

                c.a = alpha;
                tex.SetPixel(x, y, c);
            }
        }

        tex.Apply(false, true);
        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        Cache[key] = sprite;
        return sprite;
    }

    public static Sprite Sparkle(Color color)
    {
        string key = "spark_" + ColorKey(color);
        Sprite cached;
        if (Cache.TryGetValue(key, out cached)) return cached;

        int size = 96;
        Texture2D tex = NewTexture(size, size);
        float half = size * 0.5f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = Mathf.Abs((x + 0.5f - half) / half);
                float ny = Mathf.Abs((y + 0.5f - half) / half);
                float v = Mathf.Pow(nx, 0.55f) + Mathf.Pow(ny, 0.55f);
                float a = Mathf.Clamp01((1f - v) * 4f);
                Color c = color;
                c.a = a;
                tex.SetPixel(x, y, c);
            }
        }

        tex.Apply(false, true);
        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        Cache[key] = sprite;
        return sprite;
    }

    private static Texture2D NewTexture(int w, int h)
    {
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;
        return tex;
    }

    private static string ColorKey(Color c)
    {
        return Mathf.RoundToInt(c.r * 255f) + "_" + Mathf.RoundToInt(c.g * 255f) + "_" +
               Mathf.RoundToInt(c.b * 255f) + "_" + Mathf.RoundToInt(c.a * 255f);
    }

    // ---------------------------------------------------------------- builders
    public static RectTransform Rect(Transform parent, string name)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.localScale = Vector3.one;
        return rt;
    }

    public static Image Panel(Transform parent, string name, Color color, int radius = 36,
        bool border = false, float borderAlpha = 0.55f)
    {
        RectTransform rt = Rect(parent, name);
        Image img = rt.gameObject.AddComponent<Image>();

        if (radius > 0)
        {
            Color stroke = new Color(Gold.r, Gold.g, Gold.b, borderAlpha);
            img.sprite = Rounded(radius, color, border ? stroke : color, border ? 3 : 0);
            img.type = Image.Type.Sliced;
            img.color = Color.white;
        }
        else
        {
            // No sprite: the image is a flat quad, so the colour must be used
            // directly (a white tint here would paint a solid white screen).
            img.sprite = null;
            img.color = color;
        }

        img.raycastTarget = false;
        return img;
    }

    public static Image Circle(Transform parent, string name, Color color, int strokeWidth = 0,
        Color? stroke = null)
    {
        RectTransform rt = Rect(parent, name);
        Image img = rt.gameObject.AddComponent<Image>();
        img.sprite = UITheme.Circle(color, stroke.HasValue ? stroke.Value : color, strokeWidth);
        img.color = Color.white;
        img.raycastTarget = false;
        return img;
    }

    public static Image Gradient(Transform parent, string name, Color top, Color bottom,
        float topAlpha = 1f, float bottomAlpha = 1f)
    {
        RectTransform rt = Rect(parent, name);
        Image img = rt.gameObject.AddComponent<Image>();
        img.sprite = VerticalGradient(top, bottom, topAlpha, bottomAlpha);
        img.color = Color.white;
        img.raycastTarget = false;
        return img;
    }

    public static Text Label(Transform parent, string name, string content, int size,
        TextAnchor anchor, Color color, FontStyle style = FontStyle.Bold, bool shadow = true)
    {
        RectTransform rt = Rect(parent, name);
        Text text = rt.gameObject.AddComponent<Text>();
        text.font = RuntimeUI.GetFont();
        text.text = content;
        text.fontSize = size;
        text.alignment = anchor;
        text.color = color;
        text.fontStyle = style;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;
        text.supportRichText = true;

        if (shadow)
        {
            Shadow sh = rt.gameObject.AddComponent<Shadow>();
            sh.effectColor = new Color(0f, 0f, 0f, 0.45f);
            sh.effectDistance = new Vector2(0f, -3f);
        }

        return text;
    }

    public static Button Button(Transform parent, string name, string label, Color fill,
        Color labelColor, int fontSize = 38, int radius = 40, bool goldBorder = true)
    {
        RectTransform rt = Rect(parent, name);

        Image bg = rt.gameObject.AddComponent<Image>();
        bg.sprite = Rounded(radius, fill,
            goldBorder ? new Color(Gold.r, Gold.g, Gold.b, 0.75f) : new Color(0f, 0f, 0f, 0f),
            goldBorder ? 3 : 0);
        bg.type = Image.Type.Sliced;
        bg.color = Color.white;
        bg.raycastTarget = true;

        Button button = rt.gameObject.AddComponent<Button>();
        button.targetGraphic = bg;

        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.08f, 1.08f, 1.08f, 1f);
        colors.pressedColor = new Color(0.82f, 0.82f, 0.82f, 1f);
        colors.selectedColor = Color.white;
        colors.fadeDuration = 0.08f;
        button.colors = colors;

        Text text = Label(rt, "Label", label, fontSize, TextAnchor.MiddleCenter, labelColor,
            FontStyle.Bold);
        Stretch(text.rectTransform, 10f, 10f, 0f, 0f);

        return button;
    }

    public static Image Icon(Transform parent, string name, Sprite sprite, float size)
    {
        RectTransform rt = Rect(parent, name);
        Image img = rt.gameObject.AddComponent<Image>();
        img.sprite = sprite;
        img.preserveAspect = true;
        img.raycastTarget = false;
        rt.sizeDelta = new Vector2(size, size);
        return img;
    }

    public static void Stretch(RectTransform rt, float left, float right, float top, float bottom)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = new Vector2(left, bottom);
        rt.offsetMax = new Vector2(-right, -top);
    }

    public static void SetRect(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax,
        Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = anchoredPosition;
        rt.sizeDelta = sizeDelta;
    }

    /// <summary>Keeps UI clear of notches / rounded corners on real phones.</summary>
    public static void ApplySafeArea(RectTransform rt)
    {
        Rect safe = Screen.safeArea;
        Vector2 min = safe.position;
        Vector2 max = safe.position + safe.size;
        min.x /= Screen.width;
        min.y /= Screen.height;
        max.x /= Screen.width;
        max.y /= Screen.height;
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}
