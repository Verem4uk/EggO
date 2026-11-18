using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public sealed class AddressableSessionImageLoader : ISessionImageLoader
{
    public async Task<SessionImage> LoadAsync(string key)
    {
        var handle = Addressables.LoadAssetAsync<Texture2D>(key);
        Sprite sprite = null;
        try
        {
            // Use Completed instead of handle.Task, which is unavailable on WebGL.
            var completion = new TaskCompletionSource<Texture2D>();
            void Complete(AsyncOperationHandle<Texture2D> operation)
            {
                if (operation.Status == AsyncOperationStatus.Succeeded)
                    completion.TrySetResult(operation.Result);
                else
                    completion.TrySetException(operation.OperationException ??
                        new InvalidOperationException("Unable to load image: " + key));
            }

            if (handle.IsDone) Complete(handle);
            else handle.Completed += Complete;

            var texture = await completion.Task;
            sprite = Sprite.Create(texture,
                new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            var loadedSprite = sprite;
            return new SessionImage(sprite, () =>
            {
                UnityEngine.Object.Destroy(loadedSprite);
                if (handle.IsValid()) Addressables.Release(handle);
            });
        }
        catch
        {
            if (sprite != null) UnityEngine.Object.Destroy(sprite);
            if (handle.IsValid()) Addressables.Release(handle);
            throw;
        }
    }
}
