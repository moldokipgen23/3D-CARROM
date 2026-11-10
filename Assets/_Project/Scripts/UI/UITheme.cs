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

        // glossy top highlight
        Image gloss = Panel(rt, "Gloss", new Color(1f, 1f, 1f, 0.10f), radius - 4);
        RectTransform grt = gloss.rectTransform;
        grt.anchorMin = new Vector2(0f, 0.45f);
        grt.anchorMax = new Vector2(1f, 1f);
        grt.offsetMin = new Vector2(8f, 0f);
        grt.offsetMax = new Vector2(-8f, -6f);

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
