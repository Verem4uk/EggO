using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class PortraitTextArea : MonoBehaviour
{
    [SerializeField] private Text Text;
    [SerializeField, Min(1)] private int maximumFontSize = 100;
    private string CachedText;
    private string[] Words = System.Array.Empty<string>();
    public RectTransform Viewport => (RectTransform)transform;
    public string TextValue => Text.text;
    public int FontSize => Text.fontSize;

    public void SetMaximumFontSize(int size) => maximumFontSize = Mathf.Max(1, size);
    public float PreferredHeight(float width) => HeightAt(width, FontAtWidth(width)) + 4f;
    public bool Fits => WordsFit(Viewport.rect.width, Text.fontSize) &&
        HeightAt(Viewport.rect.width, Text.fontSize) <= Viewport.rect.height + 0.5f;

    private bool WordsFit(float width, int fontSize)
    {
        if (CachedText != Text.text)
        {
            CachedText = Text.text;
            // Keep non-breaking spaces inside the token: the entire phrase must fit.
            Words = (CachedText ?? "").Split(new[] { ' ', '\t', '\r', '\n' },
                System.StringSplitOptions.RemoveEmptyEntries);
        }
        var settings = Text.GetGenerationSettings(Vector2.zero);
        settings.fontSize = fontSize;
        settings.resizeTextForBestFit = false;
        settings.horizontalOverflow = HorizontalWrapMode.Overflow;
        float available = Mathf.Max(1f, width - 2f);
        foreach (var word in Words)
            if (Text.cachedTextGeneratorForLayout.GetPreferredWidth(word, settings) /
                Text.pixelsPerUnit > available) return false;
        return true;
    }

    private int FontAtWidth(float width)
    {
        int low = 1, high = maximumFontSize, chosen = 1;
        while (low <= high)
        {
            int candidate = (low + high) / 2;
            if (WordsFit(width, candidate)) { chosen = candidate; low = candidate + 1; }
            else high = candidate - 1;
        }
        return chosen;
    }

    private float HeightAt(float width, int fontSize)
    {
        var settings = Text.GetGenerationSettings(new Vector2(Mathf.Max(1f, width - 2f), 0f));
        settings.fontSize = fontSize;
        settings.resizeTextForBestFit = false;
        settings.horizontalOverflow = HorizontalWrapMode.Wrap;
        settings.verticalOverflow = VerticalWrapMode.Overflow;
        return Text.cachedTextGeneratorForLayout.GetPreferredHeight(Text.text, settings) / Text.pixelsPerUnit;
    }

    public void SetArea(float width, float height)
    {
        var content = Text.rectTransform;
        content.anchorMin = Vector2.zero;
        content.anchorMax = Vector2.one;
        content.pivot = Vector2.one * 0.5f;
        content.offsetMin = content.offsetMax = Vector2.zero;
        content.localScale = Vector3.one;
        Text.resizeTextForBestFit = false;
        Text.horizontalOverflow = HorizontalWrapMode.Wrap;
        Text.verticalOverflow = VerticalWrapMode.Overflow;

        // Unity otherwise falls back to character wrapping for an oversized word.
        // First fit whole words horizontally, then fit the full question vertically.
        int low = 1, high = FontAtWidth(width), chosen = 1;
        float available = Mathf.Max(1f, height - 4f);
        while (low <= high)
        {
            int candidate = (low + high) / 2;
            if (HeightAt(width, candidate) <= available)
            {
                chosen = candidate;
                low = candidate + 1;
            }
            else high = candidate - 1;
        }
        Text.fontSize = chosen;
    }
}
