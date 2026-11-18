using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public interface ISessionImageLoader
{
    Task<SessionImage> LoadAsync(string key);
}

// Owns one loaded texture handle and the sprite created from it.
public sealed class SessionImage : IDisposable
{
    public Sprite Sprite { get; }
    private Action Release;

    public SessionImage(Sprite sprite, Action release)
    {
        Sprite = sprite ?? throw new ArgumentNullException(nameof(sprite));
        Release = release ?? throw new ArgumentNullException(nameof(release));
    }

    public void Dispose()
    {
        var release = Release;
        Release = null;
        release?.Invoke();
    }
}

public sealed class SessionImages : IDisposable
{
    public sealed class LoadResult
    {
        public IReadOnlyList<Sprite> Sprites { get; }
        public Exception Error { get; }
        public bool IsCancelled { get; }
        public bool IsSuccess => Error == null && !IsCancelled;

        internal LoadResult(Sprite[] sprites, Exception error = null, bool cancelled = false)
        {
            Sprites = Array.AsReadOnly(sprites);
            Error = error;
            IsCancelled = cancelled;
        }
    }

    private sealed class LoadState
    {
        public readonly List<SessionImage> Images = new List<SessionImage>();
        public Task<LoadResult> Task;
    }

    private readonly ISessionImageLoader Loader;
    private readonly Dictionary<IQuestion, LoadState> Loads = new Dictionary<IQuestion, LoadState>();
    private bool IsDisposed;

    public SessionImages(ISessionImageLoader loader)
    {
        Loader = loader ?? throw new ArgumentNullException(nameof(loader));
    }

    public void Preload(SessionPlan plan)
    {
        if (IsDisposed) throw new ObjectDisposedException(nameof(SessionImages));
        foreach (var entry in plan.Questions)
            if (entry.Question.HasImage()) GetAsync(entry.Question);
    }

    // Reuses work only within this session. A retry replaces a failed operation.
    public Task<LoadResult> GetAsync(IQuestion question, bool retry = false)
    {
        if (IsDisposed)
            return Task.FromResult(new LoadResult(Array.Empty<Sprite>(), cancelled: true));
        if (question == null) throw new ArgumentNullException(nameof(question));

        if (Loads.TryGetValue(question, out var existing) &&
            (!retry || !existing.Task.IsCompleted || existing.Task.Result.IsSuccess))
            return existing.Task;

        var state = new LoadState();
        Loads[question] = state;
        // Snapshot keys as well: an in-flight request never reads changing asset data.
        var keys = new string[question.ImageKeys.Count];
        for (int i = 0; i < keys.Length; i++) keys[i] = question.ImageKeys[i];
        state.Task = LoadAsync(keys, state);
        return state.Task;
    }

    private async Task<LoadResult> LoadAsync(string[] keys, LoadState state)
    {
        try
        {
            foreach (var key in keys)
            {
                var image = await Loader.LoadAsync(key);
                // Addressables requests cannot be cancelled. Release late results on arrival.
                if (IsDisposed)
                {
                    image.Dispose();
                    Release(state);
                    return new LoadResult(Array.Empty<Sprite>(), cancelled: true);
                }
                state.Images.Add(image);
            }

            var sprites = new Sprite[state.Images.Count];
            for (int i = 0; i < sprites.Length; i++) sprites[i] = state.Images[i].Sprite;
            return new LoadResult(sprites);
        }
        catch (Exception error)
        {
            Release(state);
            return new LoadResult(Array.Empty<Sprite>(), error, IsDisposed);
        }
    }

    private static void Release(LoadState state)
    {
        foreach (var image in state.Images) image.Dispose();
        state.Images.Clear();
    }

    public void Dispose()
    {
        if (IsDisposed) return;
        IsDisposed = true;
        foreach (var state in Loads.Values) Release(state);
        Loads.Clear();
    }
}
