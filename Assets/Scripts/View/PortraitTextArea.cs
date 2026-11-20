using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class PortraitTextArea : MonoBehaviour
{
    [SerializeField] private Text Text;
    [SerializeField, Min(1)] private int maximumFontSize = 100;
    public RectTransform Viewport => (RectTransform)transform;
    public string TextValue => Text.text;
    public int FontSize => Text.fontSize;

    public void SetMaximumFontSize(int size) => maximumFontSize = Mathf.Max(1, size);
    public float PreferredHeight(float width) => HeightAt(width, maximumFontSize) + 4f;
    public bool Fits => HeightAt(Viewport.rect.width, Text.fontSize) <= Viewport.rect.height + 0.5f;

    private float HeightAt(float width, int fontSize)
    {
        var settings = Text.GetGenerationSettings(new Vector2(Mathf.Max(1f, width - 2f), 0f));
        settings.fontSize = fontSize;
        settings.resizeTextForBestFit = false;
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

        // Keep the largest font that displays the whole question in the fixed area.
        int low = 1, high = maximumFontSize, chosen = 1;
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
