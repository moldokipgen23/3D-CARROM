using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Small helper that builds UGUI objects at runtime. The project ships no
/// Canvas / EventSystem in any scene and Cloud Build cannot wire Inspector
/// references, so every screen builds its own UI through this class.
/// </summary>
public static class RuntimeUI
{
    private static Font cachedFont;

    public static Font GetFont()
    {
        if (cachedFont != null) return cachedFont;

        // Unity 2022.2+ renamed the built-in font to LegacyRuntime.ttf;
        // Arial.ttf is kept as a fallback for older editors.
        cachedFont = TryBuiltinFont("LegacyRuntime.ttf");
        if (cachedFont == null) cachedFont = TryBuiltinFont("Arial.ttf");

        if (cachedFont == null)
        {
            try
            {
                cachedFont = Font.CreateDynamicFontFromOSFont(
                    new[] { "Arial", "Helvetica", "Roboto", "Segoe UI" }, 32);
            }
            catch
            {
                cachedFont = null;
            }
        }

        return cachedFont;
    }

    private static Font TryBuiltinFont(string resourceName)
    {
        try
        {
            return Resources.GetBuiltinResource<Font>(resourceName);
        }
        catch
        {
            return null;
        }
    }

    public static void EnsureEventSystem()
    {
        if (EventSystem.current != null) return;

        GameObject existing = GameObject.Find("EventSystem");
        if (existing != null && existing.GetComponent<EventSystem>() != null) return;

        GameObject go = new GameObject("EventSystem");
        go.AddComponent<EventSystem>();
        go.AddComponent<StandaloneInputModule>();
    }

    public static Canvas CreateCanvas(string name, int sortOrder)
    {
        EnsureEventSystem();

        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Canvas),
            typeof(CanvasScaler), typeof(GraphicRaycaster));

        Canvas canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortOrder;

        CanvasScaler scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        return canvas;
    }

    public static RectTransform CreateUIObject(Transform parent, string name)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.localScale = Vector3.one;
        return rt;
    }

    public static Image CreatePanel(Transform parent, string name, Color color)
    {
        RectTransform rt = CreateUIObject(parent, name);
        Image image = rt.gameObject.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    public static Text CreateText(Transform parent, string name, string content, int fontSize,
        TextAnchor alignment, Color color, FontStyle style = FontStyle.Normal)
    {
        RectTransform rt = CreateUIObject(parent, name);
        Text text = rt.gameObject.AddComponent<Text>();
        text.font = GetFont();
        text.text = content;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = color;
        text.fontStyle = style;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;
        return text;
    }

    public static Button CreateButton(Transform parent, string name, string label,
        Color background, Color textColor)
    {
        RectTransform rt = CreateUIObject(parent, name);

        Image image = rt.gameObject.AddComponent<Image>();
        image.color = background;
        image.raycastTarget = true;

        Button button = rt.gameObject.AddComponent<Button>();
        button.targetGraphic = image;

        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
        colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        colors.selectedColor = Color.white;
        colors.fadeDuration = 0.08f;
        button.colors = colors;

        Text text = CreateText(rt, "Label", label, 34, TextAnchor.MiddleCenter, textColor, FontStyle.Bold);
        Stretch(text.rectTransform, 8f, 8f, 4f, 4f);

        return button;
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

    /// <summary>Stretch to fill the parent, with pixel insets.</summary>
    public static void Stretch(RectTransform rt, float left, float right, float top, float bottom)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = new Vector2(left, bottom);
        rt.offsetMax = new Vector2(-right, -top);
    }

    public static Color PanelDark { get { return new Color(0.05f, 0.04f, 0.03f, 0.62f); } }
    public static Color TextLight { get { return new Color(0.96f, 0.94f, 0.88f); } }
    public static Color AccentGold { get { return new Color(0.92f, 0.74f, 0.25f); } }
    public static Color ButtonGreen { get { return new Color(0.16f, 0.45f, 0.24f, 0.95f); } }
    public static Color ButtonBrown { get { return new Color(0.38f, 0.22f, 0.10f, 0.95f); } }
    public static Color ButtonRed { get { return new Color(0.48f, 0.14f, 0.12f, 0.95f); } }
}
