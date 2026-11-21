#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public static class PortraitLayoutSmokeTests
{
    private static int Checks;
    private static readonly Vector2Int[] Profiles =
    {
        new Vector2Int(1080, 2400), new Vector2Int(1080, 1920),
        new Vector2Int(320, 568), new Vector2Int(390, 844),
        new Vector2Int(768, 1024), new Vector2Int(1536, 2048), new Vector2Int(1200, 1920)
    };

    public static async void Run()
    {
        try
        {
            Checks += PortraitWordWrapTests.Validate();
            await Validate();
            foreach (var suite in new[] { typeof(SessionViewSmokeTests), typeof(AnimationSmokeTests) })
            {
                string method = suite == typeof(SessionViewSmokeTests) ? "ValidateView" : "Validate";
                await (Task)suite.GetMethod(method, BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
            }
            Debug.Log($"PASS: {Checks} portrait layout checks across seven phone/tablet profiles; " +
                "session view and navigation/audio regressions passed.");
            EditorApplication.Exit(0);
        }
        catch (Exception error) { Debug.LogException(error); EditorApplication.Exit(1); }
    }

    private static T Field<T>(object instance, string name) =>
        (T)instance.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(instance);

    private static void Check(bool condition, string message)
    {
        Checks++;
        if (!condition) throw new InvalidOperationException(message);
    }

    private static Rect Bounds(RectTransform rect, RectTransform parent)
    {
        var corners = new Vector3[4];
        rect.GetWorldCorners(corners);
        var local = corners.Select(parent.InverseTransformPoint).ToArray();
        return Rect.MinMaxRect(local.Min(v => v.x), local.Min(v => v.y),
            local.Max(v => v.x), local.Max(v => v.y));
    }

    private static bool Contains(Rect outer, Rect inner) =>
        inner.xMin >= outer.xMin - 1f && inner.xMax <= outer.xMax + 1f &&
        inner.yMin >= outer.yMin - 1f && inner.yMax <= outer.yMax + 1f;

    private static async Task Validate()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/MainScene.unity");
        var roots = scene.GetRootGameObjects();
        var frames = roots.Select(root => root.GetComponent<PortraitCanvas>()).Where(f => f != null).ToArray();
        var canvases = roots.Select(root => root.GetComponent<Canvas>()).Where(c => c != null).ToArray();
        var camera = roots.SelectMany(root => root.GetComponentsInChildren<Camera>(true)).First();
        var session = roots.Select(root => root.GetComponent<SessionView>()).First(v => v != null);
        var layout = session.GetComponent<ResponsiveSessionLayout>();
        var text = Field<Text>(session, "CurrentQuestionText");
        var images = Field<Image[]>(session, "Images");
        var frame = session.GetComponent<PortraitCanvas>();
        var next = Field<NextButton>(session, "NextButton").GetComponent<RectTransform>();
        var map = roots.Select(root => root.GetComponent<RoadMapScreen>()).First(m => m != null);
        string output = Path.GetFullPath(Path.Combine(Application.dataPath, "../Logs/PortraitLayout"));
        Directory.CreateDirectory(output);
        var allQuestions = AssetDatabase.FindAssets("t:Question").Select(guid =>
            AssetDatabase.LoadAssetAtPath<Question>(AssetDatabase.GUIDToAssetPath(guid))).Where(q => q != null).ToArray();
        var longTexts = new[]
        {
            allQuestions.Select(q => q.EnglishText).Where(s => !string.IsNullOrEmpty(s)).OrderByDescending(s => s.Length).First(),
            allQuestions.Select(q => q.PolishText).Where(s => !string.IsNullOrEmpty(s)).OrderByDescending(s => s.Length).First(),
            allQuestions.Select(q => q.RussianText).Where(s => !string.IsNullOrEmpty(s)).OrderByDescending(s => s.Length).First()
        };
        var example = AssetDatabase.LoadAssetAtPath<ImageQuestion>("Assets/BD/2Doubts/ImageQuestion0.asset");
        var addressables = AddressableAssetSettingsDefaultObject.Settings;
        var entries = addressables.groups.Where(group => group != null).SelectMany(group => group.entries).ToArray();
        var sprites = example.ImageKeys.Select(key =>
        {
            var entry = entries.First(e => e.address == key);
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(entry.AssetPath);
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
        }).ToArray();

        camera.transform.position = new Vector3(0, 0, -10);
        camera.transform.rotation = Quaternion.identity;
        camera.orthographic = true;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.white;
        camera.cullingMask = -1;
        foreach (var renderer in roots.SelectMany(root => root.GetComponentsInChildren<ParticleSystemRenderer>(true)))
            renderer.enabled = false;
        foreach (var canvas in canvases)
        {
            canvas.gameObject.SetActive(true);
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = camera;
            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null) scaler.enabled = false;
            canvas.transform.position = Vector3.zero;
            canvas.transform.rotation = Quaternion.identity;
            canvas.transform.localScale = Vector3.one;
        }
        foreach (var group in roots.SelectMany(root => root.GetComponentsInChildren<CanvasGroup>(true)))
            group.alpha = 1f;
        text.color = Color.black;
        var background = roots.SelectMany(root => root.GetComponentsInChildren<ColorChanger>(true)).Single();
        var originalBackground = background.targetImage.color;
        var sharedControls = frames.First(item => item.name == "Canvas").ContentRect.Find("CanvasButtons");

        foreach (var profile in Profiles)
        {
            float scale = Mathf.Min(profile.x / 800f, profile.y / 1400f);
            var logical = new Vector2(profile.x / scale, profile.y / scale);
            camera.orthographicSize = logical.y * 0.5f;
            camera.aspect = profile.x / (float)profile.y;
            foreach (var canvas in canvases)
            {
                var rect = (RectTransform)canvas.transform;
                rect.anchorMin = rect.anchorMax = Vector2.one * 0.5f;
                rect.pivot = Vector2.one * 0.5f;
                rect.sizeDelta = logical;
            }
            foreach (bool inset in new[] { false, true })
            {
                var safe = inset
                    ? new Rect(0f, profile.y * 0.035f, profile.x, profile.y * 0.92f)
                    : new Rect(0f, 0f, profile.x, profile.y);
                foreach (var item in frames) item.Apply(profile, safe);
                Canvas.ForceUpdateCanvases();
                foreach (var screen in roots.Select(root => root.GetComponent<PortraitScreenLayout>()).Where(v => v != null))
                    screen.Refresh();
                layout.Refresh();
                Canvas.ForceUpdateCanvases();
                foreach (var area in roots.SelectMany(root => root.GetComponentsInChildren<PortraitTextArea>(true)))
                {
                    Check(area.GetComponent<ScrollRect>() == null, "A text area still contains scrolling.");
                    Check(area.Fits, $"Text does not fit: {area.name}, font={area.FontSize}, area={area.Viewport.rect}.");
                }
                Check(frame.ContentRect.rect.width <= 800.1f, "Tablet content was stretched beyond its maximum width.");

                foreach (int count in new[] { 0, 1, 2, 4 })
                foreach (var message in longTexts.Concat(new[]
                {
                    "What does this picture remind you of?",
                    "Какие напитки предпочитаете?",
                    "Если бы у вас была возможность переместиться во времени, что бы вы сделали?",
                    "В чём вы черпаете энергию и находите вдохновение?"
                }))
                {
                    for (int i = 0; i < images.Length; i++)
                    {
                        images[i].gameObject.SetActive(i < count);
                        images[i].sprite = i < count ? sprites[i] : null;
                        images[i].preserveAspect = true;
                    }
                    layout.Grid.gameObject.SetActive(count > 0);
                    text.text = message;
                    layout.SetImageCount(count);
                    Canvas.ForceUpdateCanvases();
                    layout.Refresh();
                    Canvas.ForceUpdateCanvases();
                    var content = frame.ContentRect;
                    Check(layout.QuestionArea.Fits, $"Full question did not fit at {profile}, images={count}.");
                    var questionBounds = Bounds(layout.QuestionArea.Viewport, content);
                    Check(Contains(content.rect, questionBounds), "Question viewport escaped the safe content frame.");
                    Check(questionBounds.yMax <= content.rect.yMax - 279f,
                        "Question does not leave enough space below header controls.");
                    var nextBounds = Bounds(next, content);
                    Check(Contains(content.rect, nextBounds), "Next button escaped the safe frame.");
                    Check(questionBounds.yMin >= nextBounds.yMax + 20f,
                        $"Question overlaps navigation at {profile}, images={count}.");
                    if (count > 0)
                    {
                        Check(layout.Grid.constraintCount == (count <= 2 ? 1 : 2), "Image column count changed.");
                        for (int i = 0; i < count; i++)
                        {
                            var bounds = Bounds(images[i].rectTransform, content);
                            Check(Contains(content.rect, bounds), "An image escaped the safe frame.");
                            Check(bounds.yMin >= nextBounds.yMax + 20f, "Image overlaps the Next button.");
                            Check(bounds.yMax <= questionBounds.yMin - 20f, "Image overlaps the question.");
                            Check(images[i].preserveAspect, "Image aspect ratio is not preserved.");
                        }
                        if (count == 2)
                        {
                            var a = Bounds(images[0].rectTransform, content);
                            var b = Bounds(images[1].rectTransform, content);
                            Check(Mathf.Abs(a.center.x - b.center.x) < 1f && a.yMin > b.yMax,
                                "Two images must remain vertically stacked.");
                        }
                        if (count == 4)
                        {
                            var a = Bounds(images[0].rectTransform, content);
                            var b = Bounds(images[1].rectTransform, content);
                            var c = Bounds(images[2].rectTransform, content);
                            Check(a.xMax < b.xMin && a.yMin > c.yMax &&
                                Mathf.Abs(a.center.y - b.center.y) < 1f,
                                "Four images must remain a two-by-two grid.");
                        }
                        if (profile == Profiles[0] && !inset && count >= 2)
                            Check(Mathf.Abs(layout.Grid.cellSize.x - (count == 2 ? 300f : 250f)) < 1f,
                                "Nothing Phone image sizes changed unnecessarily.");
                    }
                    Check(!string.IsNullOrEmpty(message), "Missing language fixture.");
                }
                // Long text stays fully visible by fitting the font, without any scrolling.
                text.text = string.Join("\n", Enumerable.Repeat(longTexts[0], 3));
                layout.SetImageCount(4);
                layout.Refresh();
                Check(layout.QuestionArea.Fits && text.fontSize < 100,
                    "Long text did not fit through font reduction.");
                Check(layout.QuestionArea.GetComponent<ScrollRect>() == null,
                    "Question text must not have a ScrollRect.");
            }

            if (profile == Profiles[0] || profile == Profiles[4])
            {
                foreach (int count in new[] { 1, 2, 4 })
                {
                    background.targetImage.color = Color.white;
                    sharedControls.gameObject.SetActive(true);
                    var levelStyle = AssetDatabase.LoadAssetAtPath<Level>("Assets/BD/Level3Doubts.asset");
                    next.GetComponent<Image>().sprite = levelStyle.Icon;
                    foreach (var root in roots)
                        if (root.GetComponent<PortraitCanvas>() != null)
                            root.SetActive(root.name == "Canvas" || root.name == "SessionScreen");
                    for (int i = 0; i < images.Length; i++)
                    {
                        images[i].gameObject.SetActive(i < count);
                        images[i].sprite = i < count ? sprites[i] : null;
                    }
                    text.text = count == 4 ? example.EnglishText : "Was there a little sin?";
                    foreach (var item in frames) item.Apply(profile, new Rect(0, 0, profile.x, profile.y));
                    layout.Grid.gameObject.SetActive(true);
                    layout.SetImageCount(count);
                    Canvas.ForceUpdateCanvases();
                    layout.Refresh();
                    await Task.Yield();
                    Save(camera, profile, Path.Combine(output,
                        (profile == Profiles[0] ? "NothingPhone" : "Tablet") + "_" + count + ".png"));
                }
                // Capture the other screens using their real sprites and labels.
                foreach (string screenName in new[] { "MainMenuScreen", "RoadMapScreen", "CoffeeScreen", "WarningPanel" })
                {
                    background.targetImage.color = originalBackground;
                    sharedControls.gameObject.SetActive(screenName == "RoadMapScreen");
                    foreach (var item in frames)
                    {
                        item.gameObject.SetActive(item.name == "Canvas" || item.name == screenName);
                        item.Apply(profile, new Rect(0, 0, profile.x, profile.y));
                    }
                    var screen = frames.First(item => item.name == screenName);
                    screen.GetComponent<PortraitScreenLayout>().Refresh();
                    foreach (var image in screen.GetComponentsInChildren<Graphic>(true))
                    {
                        if (image.GetComponentInParent<Scrollbar>() != null) continue;
                        var color = image.color;
                        color.a = image.GetComponent<PortraitTextArea>() != null ? 0f : 1f;
                        image.color = color;
                    }
                    if (screenName == "RoadMapScreen")
                    {
                        for (int i = 0; i < map.pointsMap.Length; i++)
                        {
                            var point = map.pointsMap[i];
                            var show = point.ShowSequence(0);
                            show.Complete(withCallbacks: true);
                        }
                    }
                    Canvas.ForceUpdateCanvases();
                    await Task.Yield();
                    Save(camera, profile, Path.Combine(output,
                        (profile == Profiles[0] ? "NothingPhone" : "Tablet") + "_" + screenName + ".png"));
                }
                foreach (var item in frames) item.gameObject.SetActive(true);
            }
        }

        foreach (var sprite in sprites) UnityEngine.Object.DestroyImmediate(sprite);
        Check(PlayerSettings.defaultInterfaceOrientation == UIOrientation.Portrait,
            "Native orientation is not locked to portrait.");
        Check(SystemInfo.graphicsDeviceType != GraphicsDeviceType.Null, "No graphics device for visual verification.");
    }

    private static void Save(Camera camera, Vector2Int size, string path)
    {
        if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null) return;
        var render = new RenderTexture(size.x, size.y, 24);
        var previous = camera.targetTexture;
        var active = RenderTexture.active;
        try
        {
            camera.targetTexture = render;
            Canvas.ForceUpdateCanvases();
            camera.Render();
            RenderTexture.active = render;
            var image = new Texture2D(size.x, size.y, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0, 0, size.x, size.y), 0, 0);
            image.Apply();
            File.WriteAllBytes(path, image.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(image);
        }
        finally
        {
            camera.targetTexture = previous;
            RenderTexture.active = active;
            UnityEngine.Object.DestroyImmediate(render);
        }
    }
}
#endif
