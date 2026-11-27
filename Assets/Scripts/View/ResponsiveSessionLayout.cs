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
    private float[] SmokeBaseLifetimes;
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
            SmokeBaseLifetimes = new float[systems.Length];
            SmokeEmitters = new Transform[systems.Length];
            for (int i = 0; i < systems.Length; i++)
            {
                SmokeEmitters[i] = systems[i].transform;
                SmokeBaseLifetimes[i] = systems[i].main.startLifetimeMultiplier;
            }
        }

        if (SmokeBounds.rect.width <= 0f || SmokeBounds.rect.height <= 0f || SmokeEmitters == null) return;

        // Anchor the two emitters to the actual camera viewport, not to the
        // narrower portrait content rect. This keeps them in the screen's
        // upper-left and lower-left corners on portrait and landscape screens.
        for (int i = 0; i < SmokeEmitters.Length; i++)
        {
            var smoke = SmokeEmitters[i];
            if (smoke == null) continue;
            bool topEmitter = smoke.localPosition.y >= 0f;
            bool repositioned = PlaceSmokeAtViewportCorner(smoke,
                -0.04f, topEmitter ? 0.96f : 0.04f);
            bool lifetimeChanged = FitSmokeLifetime(SmokeSystems[i], SmokeBaseLifetimes[i]);

            // Clear particles created at the old emitter position after a
            // resolution change or a layout repositioning.
            if (restartParticles || repositioned || lifetimeChanged)
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

    private bool FitSmokeLifetime(ParticleSystem system, float authoredLifetime)
    {
        var canvas = Frame.GetComponentInParent<Canvas>();
        var camera = canvas != null ? canvas.worldCamera : Camera.main;
        if (camera == null || system == null || authoredLifetime <= 0f) return false;
        var main = system.main;
        // SmokeEffect uses constant speed/lifetime and emits along local +Z.
        // Keep its speed and density; only extend its travel before particles die.
        if (main.startLifetime.mode != ParticleSystemCurveMode.Constant ||
            main.startSpeed.mode != ParticleSystemCurveMode.Constant) return false;
        float horizontalSpeed = main.startSpeed.constant *
            Vector3.Dot(system.transform.forward, camera.transform.right);
        if (horizontalSpeed <= 0.0001f) return false;

        float depth = camera.WorldToViewportPoint(system.transform.position).z;
        if (depth <= camera.nearClipPlane) return false;
        var exit = camera.ViewportToWorldPoint(new Vector3(1.04f, 0.5f, depth));
        float distance = Vector3.Dot(exit - system.transform.position, camera.transform.right);
        // Move the whole puff beyond the edge before its abrupt lifetime cutoff.
        float radius = main.startSize.constantMax * 0.5f;
        float requiredLifetime = (Mathf.Max(0f, distance) + radius) / horizontalSpeed * 1.05f;
        float lifetime = Mathf.Max(authoredLifetime, requiredLifetime);
        if (Mathf.Abs(main.startLifetimeMultiplier - lifetime) < 0.001f) return false;
        // Always compare against the cached prefab value, not the last adjusted
        // value: repeated refreshes must not compound, and phones regain 20s.
        main.startLifetimeMultiplier = lifetime;
        return true;
    }

    private bool PlaceSmokeAtViewportCorner(Transform smoke, float viewportX, float viewportY)
    {
        var canvas = Frame.GetComponentInParent<Canvas>();
        var camera = canvas != null ? canvas.worldCamera : Camera.main;
        if (camera == null || smoke.parent == null) return false;

        var pointOnCanvasPlane = smoke.parent.TransformPoint(Vector3.zero);
        float canvasDepth = camera.WorldToViewportPoint(pointOnCanvasPlane).z;
        if (canvasDepth <= camera.nearClipPlane) return false;

        float targetDepth = Mathf.Clamp(canvasDepth - smokeCameraDepthOffset,
            camera.nearClipPlane + 0.1f, camera.farClipPlane - 0.1f);
        var targetWorldPosition = camera.ViewportToWorldPoint(
            new Vector3(viewportX, viewportY, targetDepth));
        var targetLocalPosition = smoke.parent.InverseTransformPoint(targetWorldPosition);
        if ((smoke.localPosition - targetLocalPosition).sqrMagnitude < 0.01f) return false;

        smoke.localPosition = targetLocalPosition;
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
                    $"lifetime={system.main.startLifetimeMultiplier:F1}s, " +
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
