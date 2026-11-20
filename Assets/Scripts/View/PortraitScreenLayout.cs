using UnityEngine;

[DisallowMultipleComponent]
public sealed class PortraitScreenLayout : MonoBehaviour
{
    public enum ScreenKind { Menu, RoadMap, Coffee, Warning, SharedControls }
    [SerializeField] private PortraitCanvas Frame;
    [SerializeField] private ScreenKind Kind;
    [SerializeField] private RectTransform Primary;
    [SerializeField] private RectTransform Secondary;
    [SerializeField] private RectTransform LeftButton;
    [SerializeField] private RectTransform RightButton;
    [SerializeField] private PortraitTextArea TextArea;
    private Vector2 LastSize;
    private string LastText;

    private void OnEnable() => Refresh();
    private void LateUpdate()
    {
        if (Frame != null && (LastSize != Frame.ContentRect.rect.size ||
            (TextArea != null && LastText != TextArea.TextValue))) Refresh();
    }

    public void Refresh()
    {
        if (Frame == null) return;
        Vector2 size = Frame.ContentRect.rect.size;
        if (size.x <= 0 || size.y <= 0) return;
        LastSize = size;
        LastText = TextArea != null ? TextArea.TextValue : null;
        switch (Kind)
        {
            case ScreenKind.Menu:
                SetRect(Primary, new Vector2(0.5f, 1f), new Vector2(0f, -150f), new Vector2(518f, 215f));
                float playWidth = Mathf.Min(600f, size.x - 80f, size.y - 440f);
                // The wrapper owns layout; Pulsation keeps its own transform and base scale.
                SetRect(Secondary, new Vector2(0.5f, 0.5f), new Vector2(0f, -150f),
                    new Vector2(playWidth, playWidth * 620f / 600f));
                if (RightButton != null)
                    // Keep the transparent reset hit area on the logo's egg, as authored
                    // on the Nothing Phone reference, independently of the screen height.
                    SetRect(RightButton, new Vector2(0.5f, 1f),
                        Primary.anchoredPosition + new Vector2(174.61f, -16.47f),
                        new Vector2(89.26f, 84.85f));
                break;
            case ScreenKind.RoadMap:
                const float low = -490f, high = 650f;
                float top = size.y - 220f, bottom = 35f;
                float scale = Mathf.Min(1f, (size.x - 40f) / 740f, (top - bottom) / (high - low));
                float center = size.y * 0.5f;
                float offset = Mathf.Clamp(0f, bottom - center - low * scale, top - center - high * scale);
                Primary.localScale = Vector3.one * scale;
                Primary.anchoredPosition = new Vector2(0f, offset);
                break;
            case ScreenKind.Coffee:
                float textWidth = Mathf.Min(675f, size.x - 100f);
                float textHeight = Mathf.Min(TextArea.PreferredHeight(textWidth), size.y * 0.34f);
                SetRect(TextArea.Viewport, new Vector2(0.5f, 1f),
                    new Vector2(0f, -70f - textHeight * 0.5f), new Vector2(textWidth, textHeight));
                TextArea.SetArea(textWidth, textHeight);
                float button = Mathf.Min(300f, size.y * 0.20f, (size.x - 100f) * 0.5f);
                float offsetX = button * 0.5f + 50f;
                SetRect(LeftButton, new Vector2(0.5f, 0f), new Vector2(-offsetX, size.y * 0.46f),
                    new Vector2(button, button));
                SetRect(RightButton, new Vector2(0.5f, 0f), new Vector2(offsetX, size.y * 0.46f),
                    new Vector2(button, button));
                // The authored sprite includes large margins around the egg.
                float artHeight = Mathf.Min(800f, size.y * 0.46f);
                SetRect(Primary, new Vector2(0.5f, 0f), new Vector2(0f, 35f + artHeight * 0.5f),
                    new Vector2(size.x - 100f, artHeight));
                break;
            case ScreenKind.Warning:
                float width = Mathf.Min(650f, size.x - 100f);
                float height = Mathf.Min(TextArea.PreferredHeight(width), size.y - 450f);
                SetRect(TextArea.Viewport, new Vector2(0.5f, 0.5f), new Vector2(0f, 100f),
                    new Vector2(width, height));
                TextArea.SetArea(width, height);
                BottomButton(LeftButton, false);
                BottomButton(RightButton, true);
                break;
            case ScreenKind.SharedControls:
                SetRect(LeftButton, new Vector2(0f, 1f), new Vector2(120f, -120f), new Vector2(150f, 150f));
                SetRect(RightButton, Vector2.one, new Vector2(-120f, -120f), new Vector2(150f, 150f));
                break;
        }
    }

    private static void BottomButton(RectTransform rect, bool right) =>
        SetRect(rect, new Vector2(right ? 1f : 0f, 0f),
            new Vector2(right ? -120f : 120f, 120f), new Vector2(150f, 150f));

    private static void SetRect(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size)
    {
        if (rect == null) return;
        rect.anchorMin = rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        // Layout wrappers are not animated; existing loop effects retain their own base transforms.
    }
}
