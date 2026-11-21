using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class ResponsiveSessionLayout : MonoBehaviour
{
    [SerializeField] private PortraitCanvas Frame;
    [SerializeField] private PortraitTextArea Question;
    [SerializeField] private GridLayoutGroup ImageGrid;
    [SerializeField] private RectTransform Next;
    [SerializeField] private RectTransform Back;
    [SerializeField] private RectTransform Exit;
    [SerializeField] private float singleImageMaximum = 800f;
    [SerializeField] private float pairImageMaximum = 300f;
    [SerializeField] private float gridImageMaximum = 250f;
    [SerializeField] private float imageGap = 50f;
    [SerializeField] private float textImageGap = 24f;
    [SerializeField] private int questionFontSize = 100;
    [SerializeField] private int minimumImageQuestionFontSize = 72;

    public int ImageCount { get; private set; }
    public PortraitTextArea QuestionArea => Question;
    public GridLayoutGroup Grid => ImageGrid;
    private Vector2 LastSize;

    public void SetImageCount(int count)
    {
        ImageCount = Mathf.Clamp(count, 0, 4);
        Refresh();
    }

    private void OnEnable() => Refresh();
    private void LateUpdate()
    {
        if (Frame != null && LastSize != Frame.ContentRect.rect.size) Refresh();
    }

    public void Refresh()
    {
        if (Frame == null || Question == null) return;
        var size = Frame.ContentRect.rect.size;
        if (size.x <= 0f || size.y <= 0f) return;
        LastSize = size;
        float navigationHeight = Mathf.Min(340f, size.y * 0.20f);
        float nextHeight = Mathf.Max(120f, navigationHeight - 40f);
        SetRect(Next, new Vector2(0.5f, 0f), new Vector2(0f, 40f + nextHeight * 0.5f),
            new Vector2(nextHeight * 0.775f, nextHeight));
        float side = Mathf.Min(150f, navigationHeight * 0.5f);
        SetRect(Back, Vector2.zero, new Vector2(45f + side * 0.5f, 40f + side * 0.5f),
            new Vector2(side, side));
        SetRect(Exit, new Vector2(1f, 0f), new Vector2(-45f - side * 0.5f, 40f + side * 0.5f),
            new Vector2(side, side));

        // Header buttons end at 195 UI units; leave room before the question.
        float top = size.y - 280f;
        float bottom = navigationHeight + 24f;
        float bodyHeight = Mathf.Max(1f, top - bottom);
        Question.SetMaximumFontSize(ImageCount == 0 ? questionFontSize : Mathf.RoundToInt(Mathf.Lerp(
            minimumImageQuestionFontSize, questionFontSize, Mathf.InverseLerp(850f, 1200f, bodyHeight))));
        float textWidth = Mathf.Max(1f, size.x - 160f);
        float textPreferred = Question.PreferredHeight(textWidth);
        if (ImageCount == 0)
        {
            float height = Mathf.Min(Mathf.Max(100f, textPreferred), bodyHeight);
            Place(Question.Viewport, textWidth, height, bottom + (bodyHeight + height) * 0.5f, size.y);
            Question.SetArea(textWidth, height);
            return;
        }

        int columns = ImageCount <= 2 ? 1 : 2;
        int rows = Mathf.CeilToInt(ImageCount / (float)columns);
        float gap = Mathf.Min(imageGap, size.x * 0.0625f);
        float maximum = ImageCount == 1 ? singleImageMaximum :
            ImageCount == 2 ? pairImageMaximum : gridImageMaximum;
        float idealGridHeight = rows * maximum + gap * (rows - 1);
        float textHeight = Mathf.Min(Mathf.Max(90f, textPreferred),
            Mathf.Max(bodyHeight * 0.38f, bodyHeight - idealGridHeight - textImageGap));
        float cell = Mathf.Max(1f, Mathf.Min(maximum,
            (size.x - 48f - gap * (columns - 1)) / columns,
            (bodyHeight - textHeight - textImageGap - gap * (rows - 1)) / rows));
        float gridWidth = columns * cell + gap * (columns - 1);
        float gridHeight = rows * cell + gap * (rows - 1);
        float combined = textHeight + textImageGap + gridHeight;
        float blockTop = bottom + (bodyHeight + combined) * 0.5f;
        Place(Question.Viewport, textWidth, textHeight, blockTop, size.y);
        Question.SetArea(textWidth, textHeight);
        Place((RectTransform)ImageGrid.transform, gridWidth, gridHeight,
            blockTop - textHeight - textImageGap, size.y);
        ImageGrid.startAxis = GridLayoutGroup.Axis.Horizontal;
        ImageGrid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        ImageGrid.childAlignment = TextAnchor.MiddleCenter;
        ImageGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        ImageGrid.constraintCount = columns;
        ImageGrid.spacing = new Vector2(gap, gap);
        ImageGrid.padding = new RectOffset();
        ImageGrid.cellSize = new Vector2(cell, cell);
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)ImageGrid.transform);
    }

    // The common content frame and the question/grid parent share the same full-size rect.
    private static void Place(RectTransform rect, float width, float height, float top, float frameHeight)
    {
        SetRect(rect, new Vector2(0.5f, 1f), new Vector2(0f, top - frameHeight),
            new Vector2(width, height), new Vector2(0.5f, 1f));
    }

    private static void SetRect(RectTransform rect, Vector2 anchor, Vector2 position,
        Vector2 size, Vector2? pivot = null)
    {
        if (rect == null) return;
        rect.anchorMin = rect.anchorMax = anchor;
        rect.pivot = pivot ?? new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        rect.localScale = Vector3.one;
    }
}
