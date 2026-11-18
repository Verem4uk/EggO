#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class AnimationSmokeTests
{
    private static int Checks;

    public static async void Run()
    {
        try
        {
            await Validate();
            Debug.Log($"PASS: {Checks} animation, navigation and audio checks.");
            EditorApplication.Exit(0);
        }
        catch (Exception error)
        {
            Debug.LogException(error);
            EditorApplication.Exit(1);
        }
    }

    private static T Field<T>(object instance, string name) =>
        (T)instance.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(instance);

    private static void Set(object instance, string name, object value) =>
        instance.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(instance, value);

    private static void Invoke(object instance, string name) =>
        instance.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(instance, null);

    private static void Check(bool condition, string message)
    {
        Checks++;
        if (!condition) throw new InvalidOperationException(message);
    }

    private static async Task Settle() { await Task.Yield(); await Task.Yield(); }

    private static void FinishTransition(Controller controller)
    {
        Field<Sequence>(controller, "Transition")?.Complete(withCallbacks: true);
        Check(!controller.IsTransitioning, "A completed transition left input locked.");
    }

    private static void CheckScreen(Controller controller, Controller.ScreenState expected)
    {
        Check(controller.CurrentScreen == expected, "Unexpected navigation state.");
        Check(Field<CanvasGroup>(controller, "MainMenuView").gameObject.activeSelf ==
            (expected == Controller.ScreenState.MainMenu), "Incorrect menu visibility.");
        Check(Field<RoadMapScreen>(controller, "RoadMap").gameObject.activeSelf ==
            (expected == Controller.ScreenState.RoadMap), "Incorrect map visibility.");
        Check(Field<SessionView>(controller, "SessionView").gameObject.activeSelf ==
            (expected == Controller.ScreenState.Session), "Incorrect session visibility.");
        Check(Field<CoffeeView>(controller, "CoffeeView").gameObject.activeSelf ==
            (expected == Controller.ScreenState.Coffee), "Incorrect coffee visibility.");
    }

    private static async Task Validate()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/MainScene.unity");
        // DOTween.Init deliberately does nothing outside Play Mode. Enable its manager
        // for deterministic Complete/Goto/Kill calls without creating a live update loop.
        typeof(DOTween).GetField("initialized", BindingFlags.Static | BindingFlags.NonPublic)
            .SetValue(null, true);
        DOTween.useSafeMode = false; // Fail the test on an invalid tween target.
        var controller = scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<Controller>(true)).Single();
        var view = Field<SessionView>(controller, "SessionView");
        var audio = Field<AudioController>(controller, "AudioController");
        var source = Field<AudioSource>(audio, "AudioSource");
        var loader = new ControlledAudioLoader();
        audio.TrackLoader = loader;
        var originalSmoke = Field<Material>(view, "SmokeMaterial");
        var testSmoke = new Material(originalSmoke);
        Set(view, "SmokeMaterial", testSmoke);
        var originalLevels = Field<Level[]>(controller, "Levels");
        var testLevel = UnityEngine.Object.Instantiate(originalLevels[0]);
        // Text-only fixture: no image or audio requests, and no level completion/prefs writes.
        testLevel.Elements = new[] { originalLevels[0].Elements[0], originalLevels[0].Elements.Last() };
        testLevel.CompletionRequirements = new[] { new Level.CompletionRequirement
            { Element = testLevel.Elements[0], MinimumQuestions = 1 } };
        testLevel.FinishElements = new[] { testLevel.Elements[1] };
        testLevel.BackGroundAudio = "";
        Set(controller, "Levels", new[] { testLevel });

        try
        {
            controller.BackToMenu();
            FinishTransition(controller);
            CheckScreen(controller, Controller.ScreenState.MainMenu);
            var menu = Field<CanvasGroup>(controller, "MainMenuView");
            Check(menu.alpha == 1f && menu.interactable, "Menu did not regain visibility and input.");

            var map = Field<RoadMapScreen>(controller, "RoadMap");
            var mapShow = map.ShowSequence(0);
            mapShow.Goto(map.delayBeforeStart * 0.75f);
            var firstPointImage = Field<Button>(map.pointsMap[0], "ActiveEggO").GetComponent<Image>();
            Check(firstPointImage.color.a == 0f,
                "The canvas fader animated a map point before its reveal sequence.");
            mapShow.Kill();

            controller.OpenRoadMap();
            var interrupted = Field<Sequence>(controller, "Transition");
            controller.OpenRoadMap();
            Check(ReferenceEquals(interrupted, Field<Sequence>(controller, "Transition")),
                "Repeated map opening created competing transitions.");
            interrupted.Goto(0.25f);
            bool wasCancelled = false;
            interrupted.OnKill(() => wasCancelled = true);
            controller.BackToMenu();
            Check(wasCancelled, "Return to menu did not cancel the old transition.");
            FinishTransition(controller);
            CheckScreen(controller, Controller.ScreenState.MainMenu);

            var soundImage = Field<Image>(audio, "SoundImage");
            float soundAlpha = soundImage.color.a;
            controller.OpenRoadMap();
            FinishTransition(controller);
            CheckScreen(controller, Controller.ScreenState.RoadMap);
            Check(Mathf.Approximately(soundImage.color.a, soundAlpha),
                "A screen fader changed the shared sound button's opacity.");
            var shared = Field<CanvasGroup>(controller, "SharedButtons");
            Check(shared.alpha == 1f && shared.interactable, "Shared buttons remained faded or blocked.");

            controller.PlaySession(1);
            interrupted = Field<Sequence>(controller, "Transition");
            var cancelledSession = Field<SessionCoordinator>(controller, "ActiveSession");
            interrupted.Goto(Mathf.Min(0.1f, interrupted.Duration() / 2f));
            wasCancelled = false;
            interrupted.OnKill(() => wasCancelled = true);
            controller.BackToMenu();
            FinishTransition(controller);
            Check(wasCancelled && Field<SessionCoordinator>(controller, "ActiveSession") == null,
                "Interrupted session transition retained its tween or session.");
            Check((await cancelledSession.Images.GetAsync(testLevel.Elements[0] as IQuestion)).IsCancelled,
                "Interrupted navigation retained session resources.");
            CheckScreen(controller, Controller.ScreenState.MainMenu);

            controller.OpenRoadMap();
            FinishTransition(controller);
            controller.PlaySession(1);
            FinishTransition(controller);
            CheckScreen(controller, Controller.ScreenState.Session);
            var sessionCanvas = view.GetComponent<CanvasGroup>();
            Check(sessionCanvas.alpha == 1f && sessionCanvas.interactable, "Session input was not restored.");
            controller.GoToCoffeeScreen();
            FinishTransition(controller);
            CheckScreen(controller, Controller.ScreenState.Coffee);
            controller.BackToMenu();
            FinishTransition(controller);

            // A disabled owner cannot later activate an old destination.
            controller.OpenRoadMap();
            interrupted = Field<Sequence>(controller, "Transition");
            Invoke(controller, "OnDisable");
            Check(!interrupted.IsActive() && !controller.IsTransitioning,
                "Disabling the controller left a navigation tween alive.");
            controller.BackToMenu();
            FinishTransition(controller);
            CheckScreen(controller, Controller.ScreenState.MainMenu);

            await ValidateAudio(audio, source, loader);
            ValidateLocalFader();
            ValidateLoopEffects();
            ValidateExitFade();
        }
        finally
        {
            controller.BackToMenu();
            Field<Sequence>(controller, "Transition")?.Kill();
            audio.StopImmediate();
            DOTween.KillAll();
            Set(controller, "Levels", originalLevels);
            Set(view, "SmokeMaterial", originalSmoke);
            UnityEngine.Object.DestroyImmediate(testSmoke);
            UnityEngine.Object.DestroyImmediate(testLevel);
        }
    }

    private static void CompleteVolumeFade(AudioController audio) =>
        Field<Tween>(audio, "VolumeFade")?.Complete(withCallbacks: true);

    private static async Task ValidateAudio(AudioController audio, AudioSource source, ControlledAudioLoader loader)
    {
        audio.ChangeVolume(0.25f);
        audio.SetDefault();
        Field<Tween>(audio, "VolumeFade").Goto(1f);
        audio.ChangeVolume(0.6f);
        CompleteVolumeFade(audio);
        Check(Mathf.Approximately(source.volume, 0.6f), "Fade overwrote the user's volume.");

        audio.HandleAudio("first.mp3");
        CompleteVolumeFade(audio);
        await Settle();
        var first = loader.Requests.Last();
        audio.HandleAudio("second.mp3");
        CompleteVolumeFade(audio);
        await Settle();
        var second = loader.Requests.Last();
        Check(first != second && first.Disposals == 1, "Replacing audio did not dispose the old request.");
        var stale = AudioClip.Create("stale", 64, 1, 44100, false);
        first.CompletionSource.SetResult(stale);
        await Settle();
        Check(stale == null && source.clip == null, "A stale request restarted background audio.");

        var current = AudioClip.Create("current", 64, 1, 44100, false);
        second.CompletionSource.SetResult(current);
        await Settle();
        CompleteVolumeFade(audio);
        await Settle();
        Check(source.clip == current && Mathf.Approximately(source.volume, 0.6f),
            "The current audio did not respect the user's volume.");
        audio.StopImmediate();
        Check(source.clip == null && current == null && second.Disposals == 1,
            "Stopping audio leaked the downloaded clip or request.");

        audio.HandleAudio("late.mp3");
        CompleteVolumeFade(audio);
        await Settle();
        var lateRequest = loader.Requests.Last();
        audio.HandleAudio("");
        var late = AudioClip.Create("late", 64, 1, 44100, false);
        lateRequest.CompletionSource.SetResult(late);
        await Settle();
        CompleteVolumeFade(audio);
        Check(source.clip == null && late == null && lateRequest.Disposals == 1,
            "An empty track failed to cancel an outstanding download.");
    }

    private static void ValidateLocalFader()
    {
        var root = new GameObject("Fader test", typeof(CanvasGroup), typeof(CanvasGroupAutoFadeWithImages));
        var local = new GameObject("Inactive local image", typeof(RectTransform), typeof(Image));
        local.transform.SetParent(root.transform);
        var external = new GameObject("Shared image", typeof(RectTransform), typeof(Image));
        try
        {
            var fader = root.GetComponent<CanvasGroupAutoFadeWithImages>();
            var shared = external.GetComponent<Image>();
            var color = shared.color;
            color.a = 0.4f;
            shared.color = color;
            Set(fader, "images", new[] { local.GetComponent<Image>(), shared });
            Invoke(fader, "Awake");
            local.SetActive(false);
            fader.Show(immediate: true);
            Check(!local.activeSelf && Mathf.Approximately(shared.color.a, 0.4f),
                "Fader activated hidden content or modified another screen's image.");
            fader.HideAndDeactivate();
            var hide = Field<Tween>(fader, "AutomaticFade");
            hide.Goto(0.1f);
            bool wasCancelled = false;
            hide.OnKill(() => wasCancelled = true);
            fader.Show();
            Check(wasCancelled, "Show did not cancel a pending hide.");
            Field<Tween>(fader, "AutomaticFade").Complete(withCallbacks: true);
            Check(root.activeSelf && root.GetComponent<CanvasGroup>().interactable,
                "A cancelled hide deactivated the newly shown screen.");
            Invoke(fader, "OnDisable");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(root);
            UnityEngine.Object.DestroyImmediate(external);
        }
    }

    private static void ValidateLoopEffects()
    {
        var root = new GameObject("Loop test");
        try
        {
            var basePosition = new Vector3(1f, 2f, 3f);
            root.transform.localPosition = basePosition;
            var levitation = root.AddComponent<Levitation>();
            Invoke(levitation, "Awake");
            for (int i = 0; i < 5; i++)
            {
                Invoke(levitation, "OnEnable");
                Set(levitation, "Elapsed", 1f);
                Invoke(levitation, "Update");
                Invoke(levitation, "OnDisable");
                Check(root.transform.localPosition == basePosition, "Levitation drifted after re-enabling.");
            }
            var baseScale = new Vector3(2f, 3f, 4f);
            root.transform.localScale = baseScale;
            var pulsation = root.AddComponent<Pulsation>();
            pulsation.minScale = pulsation.maxScale = 1f;
            Invoke(pulsation, "Awake");
            Invoke(pulsation, "OnEnable");
            Invoke(pulsation, "Update");
            Check(root.transform.localScale == baseScale, "Pulsation produced NaN or flattened the Z scale.");
            Invoke(pulsation, "OnDisable");
            Check(root.transform.localScale == baseScale, "Pulsation did not restore its base scale.");
        }
        finally { UnityEngine.Object.DestroyImmediate(root); }
    }

    private static void ValidateExitFade()
    {
        var root = new GameObject("Exit fade test", typeof(RectTransform), typeof(Image), typeof(FadeScreen));
        try
        {
            var fade = root.GetComponent<FadeScreen>();
            var image = root.GetComponent<Image>();
            Set(fade, "BlackScreen", image);
            fade.FadeOut();
            var quitting = Field<Tween>(fade, "Fade");
            quitting.Goto(0.2f);
            fade.FadeIn();
            Check(!quitting.IsActive() && !image.raycastTarget, "FadeIn did not cancel the exit fade.");
            Invoke(fade, "OnDisable");
            Check(!Field<Tween>(fade, "Fade")?.IsActive() ?? true, "Disabled overlay retained a tween.");
        }
        finally { UnityEngine.Object.DestroyImmediate(root); }
    }

    private sealed class ControlledAudioLoader : IAudioTrackLoader
    {
        public sealed class Request : IAudioTrackRequest
        {
            public readonly TaskCompletionSource<AudioClip> CompletionSource = new TaskCompletionSource<AudioClip>();
            public int Disposals;
            public Task<AudioClip> Completion => CompletionSource.Task;
            public void Dispose() { if (Disposals == 0) Disposals++; }
        }
        public readonly List<Request> Requests = new List<Request>();
        public IAudioTrackRequest Load(string url)
        {
            var request = new Request();
            Requests.Add(request);
            return request;
        }
    }
}
#endif
