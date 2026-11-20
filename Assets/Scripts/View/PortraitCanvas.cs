using UnityEngine;

[DisallowMultipleComponent]
[DefaultExecutionOrder(-100)]
public sealed class PortraitCanvas : MonoBehaviour
{
    [SerializeField] private RectTransform SafeArea;
    [SerializeField] private RectTransform Content;
    [SerializeField, Min(1)] private float maximumContentWidth = 800f;
    public RectTransform ContentRect => Content;
    public RectTransform SafeAreaRect => SafeArea;

    private Vector2 LastSize;
    private Rect LastSafeArea;

    private void OnEnable() => Refresh();
    private void LateUpdate()
    {
        var size = new Vector2(Screen.width, Screen.height);
        if (size != LastSize || Screen.safeArea != LastSafeArea) Refresh();
        ApplyWidth();
    }

    public void Refresh()
    {
        Apply(new Vector2(Screen.width, Screen.height), Screen.safeArea);
    }

    // Pixel coordinates, including for browser viewport changes and test profiles.
    public void Apply(Vector2 viewport, Rect safeArea)
    {
        if (SafeArea == null || Content == null || viewport.x <= 0 || viewport.y <= 0) return;
        LastSize = viewport;
        LastSafeArea = safeArea;
        if (safeArea.width <= 0 || safeArea.height <= 0) safeArea = new Rect(Vector2.zero, viewport);
        SafeArea.anchorMin = new Vector2(Mathf.Clamp01(safeArea.xMin / viewport.x),
            Mathf.Clamp01(safeArea.yMin / viewport.y));
        SafeArea.anchorMax = new Vector2(Mathf.Clamp01(safeArea.xMax / viewport.x),
            Mathf.Clamp01(safeArea.yMax / viewport.y));
        SafeArea.offsetMin = SafeArea.offsetMax = Vector2.zero;
        ApplyWidth();
    }

    private void ApplyWidth()
    {
        if (SafeArea == null || Content == null) return;
        Content.anchorMin = new Vector2(0.5f, 0f);
        Content.anchorMax = new Vector2(0.5f, 1f);
        Content.pivot = new Vector2(0.5f, 0.5f);
        Content.anchoredPosition = Vector2.zero;
        Content.sizeDelta = new Vector2(Mathf.Min(maximumContentWidth, SafeArea.rect.width), 0f);
    }
}
