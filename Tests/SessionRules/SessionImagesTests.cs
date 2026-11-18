using UnityEngine;
using UnityEngine.AddressableAssets;

internal sealed class DeferredImageLoader : ISessionImageLoader
{
    public sealed class Request
    {
        public string Key;
        public TaskCompletionSource<SessionImage> Completion = new();
        public int Released;
        public void Succeed() =>
            Completion.SetResult(new SessionImage(new Sprite(), () => Released++));
        public void Fail() => Completion.SetException(new InvalidOperationException("Test download failure"));
    }

    public readonly List<Request> Requests = new();
    public Task<SessionImage> LoadAsync(string key)
    {
        var request = new Request { Key = key };
        Requests.Add(request);
        return request.Completion.Task;
    }
}

internal static class SessionImagesTests
{
    private static ImageQuestion Image(params string[] keys)
    {
        var question = new ImageQuestion();
        typeof(ImageQuestion).GetField("imageKeys",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(question, keys);
        return question;
    }

    public static async Task Run()
    {
        var question = Image("first", "second");
        var loader = new DeferredImageLoader();
        using (var images = new SessionImages(loader))
        {
            var loading = images.GetAsync(question);
            Program.Check(ReferenceEquals(loading, images.GetAsync(question)), "Duplicate in-session request");
            Program.Check(!loading.IsCompleted && loader.Requests.Count == 1, "Load did not wait for network");
            loader.Requests[0].Succeed();
            Program.Check(!loading.IsCompleted && loader.Requests.Count == 2, "Question completed before all images");
            loader.Requests[1].Succeed();
            var result = await loading;
            Program.Check(result.IsSuccess && result.Sprites.Count == 2, "Missing loaded sprites");
            Program.Check(ReferenceEquals(loading, images.GetAsync(question, retry: true)),
                "Retry unnecessarily reloaded successful sprites");
        }
        Program.Check(loader.Requests.All(r => r.Released == 1), "Loaded sprites were not released exactly once");

        // Release both a partial result and a result arriving after disposal.
        loader = new DeferredImageLoader();
        var disposed = new SessionImages(loader);
        var pending = disposed.GetAsync(question);
        loader.Requests[0].Succeed();
        disposed.Dispose();
        disposed.Dispose();
        Program.Check(loader.Requests[0].Released == 1, "Partially loaded image leaked on exit");
        loader.Requests[1].Succeed();
        var cancelled = await pending;
        Program.Check(cancelled.IsCancelled && loader.Requests.All(r => r.Released == 1),
            "Late request leaked or supplied sprites to a disposed session");
        Program.Check((await disposed.GetAsync(question)).IsCancelled && loader.Requests.Count == 2,
            "Disposed resources started a new request");

        // A failed preload is observed, releases partial data and can be retried.
        loader = new DeferredImageLoader();
        using (var images = new SessionImages(loader))
        {
            var failed = images.GetAsync(question);
            loader.Requests[0].Succeed();
            loader.Requests[1].Fail();
            Program.Check(!(await failed).IsSuccess && loader.Requests[0].Released == 1,
                "Failure retained a partial image or escaped as an unobserved exception");
            Program.Check(ReferenceEquals(failed, images.GetAsync(question)), "Failed preload silently retried");
            var retry = images.GetAsync(question, retry: true);
            loader.Requests[2].Succeed();
            loader.Requests[3].Succeed();
            Program.Check((await retry).IsSuccess, "Retry did not recover");
        }
        Program.Check(loader.Requests[2].Released == 1 && loader.Requests[3].Released == 1,
            "Retry images leaked");

        // Session resource owners do not retain an application-wide cache.
        var immediate = new ImmediateImageLoader();
        using (var first = new SessionImages(immediate)) await first.GetAsync(question);
        using (var second = new SessionImages(immediate)) await second.GetAsync(question);
        Program.Check(immediate.Keys.Count == 4 && immediate.Released == 4, "Images leaked across sessions");

        // Compile and exercise the real Addressables adapter without relying on handle.Task.
        Addressables.Reset();
        var adapter = new AddressableSessionImageLoader();
        var lease = await adapter.LoadAsync("ready");
        lease.Dispose();
        lease.Dispose();
        Program.Check(Addressables.Releases == 1 && UnityEngine.Object.Destroyed == 1,
            "Adapter did not release its handle and generated sprite exactly once");

        Addressables.Reset();
        Addressables.Defer = true;
        var delayed = adapter.LoadAsync("delayed");
        Program.Check(!delayed.IsCompleted, "Adapter did not wait for Completed");
        Addressables.CompleteNext(success: true);
        (await delayed).Dispose();
        Program.Check(Addressables.Releases == 1, "Delayed handle was not released");

        Addressables.Reset();
        Addressables.Defer = true;
        var failure = adapter.LoadAsync("failure");
        Addressables.CompleteNext(success: false);
        bool threw = false;
        try { await failure; }
        catch (InvalidOperationException) { threw = true; }
        Program.Check(threw && Addressables.Releases == 1, "Failed Addressables handle leaked");
    }
}
