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
    [SerializeField, Min(0.5f)] private float smokeCameraDepthOffset = 40f;

    public int ImageCount { get; private set; }
    public PortraitTextArea QuestionArea => Question;
    public GridLayoutGroup Grid => ImageGrid;
    private Vector2 LastSize;
    private RectTransform SmokeBounds;
    private Transform[] SmokeEmitters;
    private ParticleSystem[] SmokeSystems;
    private Vector2Int LastLoggedViewport = new Vector2Int(-1, -1);
    private Vector2Int LastSmokeViewport = new Vector2Int(-1, -1);

    public void SetImageCount(int count)
    {
        ImageCount = Mathf.Clamp(count, 0, 4);
        Refresh();
    }

    private void OnEnable() => Refresh();
    private void LateUpdate()
    {
        // Canvas layout can settle a frame after Screen/Game View changes.
        // Read the actual background rect so smoke stays visible at every size.
        var viewport = new Vector2Int(Screen.width, Screen.height);
        bool viewportChanged = viewport != LastSmokeViewport;
        LastSmokeViewport = viewport;
        ArrangeSmoke(viewportChanged);
        LogSmokeLayoutWhenViewportChanges();
        if (Frame != null && LastSize != Frame.ContentRect.rect.size) Refresh();
    }

    public void Refresh()
    {
        if (Frame == null || Question == null) return;
        var size = Frame.ContentRect.rect.size;
        if (size.x <= 0f || size.y <= 0f) return;
        LastSize = size;
        ArrangeSmoke(false);
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

    private void ArrangeSmoke(bool restartParticles)
    {
        if (Frame == null) return;
        if (SmokeBounds == null)
        {
            SmokeBounds = Frame.ContentRect.Find("Background") as RectTransform;
            if (SmokeBounds == null) return;

            var systems = SmokeBounds.GetComponentsInChildren<ParticleSystem>(true);
            SmokeSystems = systems;
            SmokeEmitters = new Transform[systems.Length];
            for (int i = 0; i < systems.Length; i++) SmokeEmitters[i] = systems[i].transform;
        }

        float frameHeight = SmokeBounds.rect.height;
        if (frameHeight <= 0f || SmokeEmitters == null) return;

        // Keep the emitters close to the top and bottom, matching their original
        // portrait placement while adapting to the current background height.
        float topY = frameHeight * 0.46f;
        float bottomY = -frameHeight * 0.42f;
        foreach (var smoke in SmokeEmitters)
        {
            if (smoke == null) continue;
            var position = smoke.localPosition;
            float targetY = position.y >= 0f ? topY : bottomY;
            bool repositioned = Mathf.Abs(position.y - targetY) > 1f;
            if (repositioned)
            {
                position.y = targetY;
                smoke.localPosition = position;
            }

            // The scene stores these emitters at local Z=-16000. That value
            // was tuned for one Canvas scale; on another viewport the scaled
            // world position can end up behind the camera. Keep the emitter
            // on the same screen point, but place it a fixed distance in
            // front of the Canvas plane and therefore inside the camera.
            repositioned |= KeepSmokeInFrontOfCanvas(smoke);

            // Clear particles created at the old emitter position after a
            // resolution change or a layout repositioning.
            if (restartParticles || repositioned)
            {
                var particleSystem = smoke.GetComponent<ParticleSystem>();
                if (particleSystem != null)
                {
                    particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    particleSystem.Play(true);
                }
            }
        }
    }

    private bool KeepSmokeInFrontOfCanvas(Transform smoke)
    {
        var canvas = Frame.GetComponentInParent<Canvas>();
        var camera = canvas != null ? canvas.worldCamera : Camera.main;
        if (camera == null || smoke.parent == null) return false;

        var local = smoke.localPosition;
        var pointOnCanvasPlane = smoke.parent.TransformPoint(new Vector3(local.x, local.y, 0f));
        var viewportPoint = camera.WorldToViewportPoint(pointOnCanvasPlane);
        if (viewportPoint.z <= camera.nearClipPlane) return false;

        float targetDepth = Mathf.Clamp(viewportPoint.z - smokeCameraDepthOffset,
            camera.nearClipPlane + 0.1f, camera.farClipPlane - 0.1f);
        var targetWorldPosition = camera.ViewportToWorldPoint(
            new Vector3(viewportPoint.x, viewportPoint.y, targetDepth));
        if ((smoke.position - targetWorldPosition).sqrMagnitude < 0.01f) return false;

        smoke.position = targetWorldPosition;
        return true;
    }

    private void LogSmokeLayoutWhenViewportChanges()
    {
#if UNITY_EDITOR
        var viewport = new Vector2Int(Screen.width, Screen.height);
        if (viewport == LastLoggedViewport) return;
        LastLoggedViewport = viewport;
        var mainCamera = Camera.main;
        var cameraPlanes = mainCamera != null
            ? GeometryUtility.CalculateFrustumPlanes(mainCamera)
            : null;

        string emitters = SmokeSystems == null || SmokeSystems.Length == 0
            ? "none found"
            : string.Join(" | ", System.Array.ConvertAll(SmokeSystems, system =>
            {
                if (system == null) return "destroyed";
                var renderer = system.GetComponent<ParticleSystemRenderer>();
                var position = system.transform.localPosition;
                bool inMainCamera = renderer != null && cameraPlanes != null &&
                    GeometryUtility.TestPlanesAABB(cameraPlanes, renderer.bounds);
                var viewportPoint = mainCamera != null && renderer != null
                    ? mainCamera.WorldToViewportPoint(renderer.bounds.center)
                    : Vector3.zero;
                var emitterPoint = mainCamera != null
                    ? mainCamera.WorldToViewportPoint(system.transform.position)
                    : Vector3.zero;
                return $"{system.name}: active={system.gameObject.activeInHierarchy}, " +
                    $"playing={system.isPlaying}, emitting={system.isEmitting}, particles={system.particleCount}, " +
                    $"renderer={(renderer != null && renderer.enabled)}, anyCamera={(renderer != null && renderer.isVisible)}, " +
                    $"mainCameraFrustum={inMainCamera}, boundsPoint=({viewportPoint.x:F2},{viewportPoint.y:F2},{viewportPoint.z:F1}), " +
                    $"emitterPoint=({emitterPoint.x:F2},{emitterPoint.y:F2},{emitterPoint.z:F1}), " +
                    $"local=({position.x:F0},{position.y:F0},{position.z:F0})";
            }));
        string backgroundSize = SmokeBounds != null
            ? $"{SmokeBounds.rect.width:F1}x{SmokeBounds.rect.height:F1}"
            : "not found";
        Debug.Log($"[SmokeLayout] viewport={viewport.x}x{viewport.y}, " +
            $"background={backgroundSize}, emitters={emitters}");
#endif
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
