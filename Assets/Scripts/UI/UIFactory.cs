using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Petites fonctions pour construire l'interface par code (Canvas, textes, boutons, panneaux).
/// </summary>
public static class UIFactory
{
    public static readonly Color Cyan = new Color(0.25f, 0.85f, 1f);
    public static readonly Color PanelColor = new Color(0.03f, 0.07f, 0.09f, 0.82f);
    public static readonly Color ButtonColor = new Color(0.06f, 0.2f, 0.26f, 0.95f);

    static Font font;

    public static Font DefaultFont
    {
        get
        {
            if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return font;
        }
    }

    public static Canvas CreateCanvas(string name, int sortingOrder = 0)
    {
        var go = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;
        canvas.pixelPerfect = true;   // texte net
        var scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1440f, 810f);   // texte plus grand dans les petites fenêtres
        scaler.matchWidthOrHeight = 0.5f;
        EnsureEventSystem();
        return canvas;
    }

    public static void EnsureEventSystem()
    {
        if (Object.FindObjectOfType<EventSystem>() == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
    }

    public static RectTransform CreateRect(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 position, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return rect;
    }

    public static Image CreatePanel(Transform parent, string name, Color color, Vector2 anchor, Vector2 position, Vector2 size, bool glowBorder = true)
    {
        RectTransform rect = CreateRect(parent, name, anchor, anchor, anchor, position, size);
        var image = rect.gameObject.AddComponent<Image>();
        image.color = color;
        if (glowBorder)
        {
            var outline = rect.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(Cyan.r, Cyan.g, Cyan.b, 0.45f);
            outline.effectDistance = new Vector2(2f, -2f);
        }
        return image;
    }

    public static Image CreateFullscreen(Transform parent, string name, Color color)
    {
        RectTransform rect = CreateRect(parent, name, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        var image = rect.gameObject.AddComponent<Image>();
        image.color = color;
        return image;
    }

    public static Text CreateText(Transform parent, string name, string content, int fontSize, TextAnchor alignment, Color color,
        Vector2 anchor, Vector2 position, Vector2 size, FontStyle style = FontStyle.Normal, bool shadow = false)
    {
        RectTransform rect = CreateRect(parent, name, anchor, anchor, anchor, position, size);
        var text = rect.gameObject.AddComponent<Text>();
        text.font = DefaultFont;
        text.text = content;
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.alignment = alignment;
        text.color = color;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;
        // Ombre seulement pour le texte posé directement sur la scène 3D (sinon le texte paraît flou)
        if (shadow)
        {
            var effect = rect.gameObject.AddComponent<Shadow>();
            effect.effectColor = new Color(0f, 0f, 0f, 0.9f);
            effect.effectDistance = new Vector2(1f, -1f);
        }
        return text;
    }

    public static Button CreateButton(Transform parent, string label, Vector2 anchor, Vector2 position, Vector2 size, UnityAction onClick, Color? color = null, int fontSize = 28)
    {
        Image image = CreatePanel(parent, "Button " + label, color ?? ButtonColor, anchor, position, size, false);
        var button = image.gameObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.65f, 0.95f, 1f, 1f);
        colors.selectedColor = colors.highlightedColor;
        colors.pressedColor = new Color(0.55f, 0.7f, 0.75f, 1f);
        colors.disabledColor = new Color(0.45f, 0.5f, 0.52f, 0.65f);
        colors.fadeDuration = 0.08f;
        button.colors = colors;
        button.navigation = new Navigation { mode = Navigation.Mode.None };
        button.onClick.AddListener(() =>
        {
            // Évite que Espace/Entrée ne re-déclenche le bouton après un clic
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
        });
        if (onClick != null) button.onClick.AddListener(onClick);

        Text text = CreateText(image.transform, "Label", label, fontSize, TextAnchor.MiddleCenter, Color.white,
            new Vector2(0.5f, 0.5f), Vector2.zero, size);
        text.fontStyle = FontStyle.Bold;
        return button;
    }

    public static void SetLabel(Button button, string label)
    {
        Text text = button.GetComponentInChildren<Text>();
        if (text != null) text.text = label;
    }
}
