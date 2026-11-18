#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// Batch entry point. Uses real MainScene bindings and UI, with controlled image downloads.
// It never saves the scene, edits level assets or writes user progress.
public static class SessionViewSmokeTests
{
    public static async void Run()
    {
        try
        {
            await ValidateView();
            Debug.Log("PASS: SessionView loading, retry, history and disposal smoke tests.");
            EditorApplication.Exit(0);
        }
        catch (Exception error)
        {
            Debug.LogException(error);
            EditorApplication.Exit(1);
        }
    }

    private static T Field<T>(object instance, string name) =>
        (T)instance.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)
            .GetValue(instance);

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private static async Task Settle()
    {
        // Let both the resource and view continuations run on Unity's editor context.
        await Task.Yield();
        await Task.Yield();
    }

    private static async Task ValidateView()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/MainScene.unity");
        var view = scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<SessionView>(true)).Single();
        var level = AssetDatabase.LoadAssetAtPath<Level>("Assets/BD/Level6Love.asset");
        var plan = new SessionFactory(count => 0).Create(level, 6);
        var session = new Session(plan);
        var loader = new ControlledLoader();
        var effects = new NoEffects();
        // Initialize sets a shader color. Keep the scene's material asset untouched.
        var smokeField = typeof(SessionView).GetField("SmokeMaterial",
            BindingFlags.Instance | BindingFlags.NonPublic);
        var originalSmoke = (Material)smokeField.GetValue(view);
        var testSmoke = new Material(originalSmoke);
        smokeField.SetValue(view, testSmoke);
        using (var coordinator = new SessionCoordinator(session, new SessionImages(loader), effects, () => 0))
        {
            try
            {
                view.Initialize(coordinator, level);
                var next = Field<NextButton>(view, "NextButton").GetComponent<Button>();
                var images = Field<Image[]>(view, "Images");
                var text = Field<Text>(view, "CurrentQuestionText");

                // Position 5 is Overeat, which requires two images.
                for (int i = 0; i < 4; i++) view.Next();
                var question = plan.Questions[4].Question;
                Check(!next.interactable, "Next remains enabled while images are pending.");
                view.Next();
                Check(ReferenceEquals(Field<IQuestion>(view, "CurrentQuestion"), question),
                    "Next skipped a pending question.");

                loader.Latest(question.ImageKeys[0]).Fail();
                await Settle();
                Check(next.interactable, "Next did not offer retry after an image failure.");
                view.Next();
                Check(ReferenceEquals(Field<IQuestion>(view, "CurrentQuestion"), question),
                    "Retry advanced to a different question.");
                var retried = coordinator.Images.GetAsync(question);
                loader.Latest(question.ImageKeys[0]).Succeed();
                await Settle();
                loader.Latest(question.ImageKeys[1]).Succeed();
                await retried;
                await Settle();
                Check(next.interactable && images[0].sprite != null && images[1].sprite != null,
                    "Successful retry did not display both images.");

                // Tasty is still pending. Going back must keep Overeat visible when Tasty arrives.
                view.Next();
                var tasty = plan.Questions[5].Question;
                var tastyLoad = coordinator.Images.GetAsync(tasty);
                Check(!next.interactable, "Next was not blocked on a subsequent pending image.");
                view.Back();
                var previousSprite = images[0].sprite;
                var previousText = text.text;
                loader.Latest(tasty.ImageKeys[0]).Succeed();
                await tastyLoad;
                await Settle();
                Check(images[0].sprite == previousSprite && text.text == previousText &&
                    ReferenceEquals(Field<IQuestion>(view, "CurrentQuestion"), question),
                    "A late image replaced the question after Back.");

                // Exit with other preloads in flight; all arriving leases must be released.
                var loads = plan.Questions.Where(e => e.Question.HasImage())
                    .Select(e => coordinator.Images.GetAsync(e.Question)).ToArray();
                view.ClearSession();
                coordinator.Dispose();
                loader.CompletePending();
                await Task.WhenAll(loads);
                await Settle();
                Check(images.All(image => image.sprite == null),
                    "The closed view retained session sprites.");
                Check(loader.Requests.Where(request => request.Succeeded)
                    .All(request => request.Releases == 1),
                    "A downloaded image leaked or was released twice.");
                Check(effects.Saves == 0 && effects.Finishes == 0,
                    "Closing an incomplete view incorrectly completed the level.");
            }
            finally
            {
                view.ClearSession();
                coordinator.Dispose();
                loader.CompletePending();
                smokeField.SetValue(view, originalSmoke);
                UnityEngine.Object.DestroyImmediate(testSmoke);
            }
        }
    }

    private sealed class NoEffects : ISessionEffects
    {
        public int Saves, Finishes;
        public void Start(int levelNumber) { }
        public void SaveProgress(int levelNumber) { Saves++; }
        public void Finish(int levelNumber, int duration) { Finishes++; }
    }

    private sealed class ControlledLoader : ISessionImageLoader
    {
        public sealed class Request
        {
            public string Key;
            public readonly TaskCompletionSource<SessionImage> Completion =
                new TaskCompletionSource<SessionImage>();
            public bool Succeeded;
            public int Releases;

            public void Fail() =>
                Completion.SetException(new InvalidOperationException("Simulated image download failure."));

            public void Succeed()
            {
                Succeeded = true;
                var texture = new Texture2D(2, 2);
                var sprite = Sprite.Create(texture, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f));
                Completion.SetResult(new SessionImage(sprite, () =>
                {
                    Releases++;
                    UnityEngine.Object.DestroyImmediate(sprite);
                    UnityEngine.Object.DestroyImmediate(texture);
                }));
            }
        }

        public readonly List<Request> Requests = new List<Request>();

        public Task<SessionImage> LoadAsync(string key)
        {
            var request = new Request { Key = key };
            Requests.Add(request);
            return request.Completion.Task;
        }

        public Request Latest(string key) => Requests.Last(request => request.Key == key);

        public void CompletePending()
        {
            foreach (var request in Requests.ToArray())
                if (!request.Completion.Task.IsCompleted) request.Succeed();
        }
    }
}
#endif
