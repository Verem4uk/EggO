using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public interface IAudioTrackRequest : IDisposable
{
    Task<AudioClip> Completion { get; }
}

public interface IAudioTrackLoader
{
    IAudioTrackRequest Load(string url);
}

public sealed class UnityAudioTrackLoader : IAudioTrackLoader
{
    public IAudioTrackRequest Load(string url) => new Request(url);

    private sealed class Request : IAudioTrackRequest
    {
        private readonly UnityWebRequest Download;
        private readonly TaskCompletionSource<AudioClip> Result = new TaskCompletionSource<AudioClip>();
        private bool Disposed;
        public Task<AudioClip> Completion => Result.Task;

        public Request(string url)
        {
            Download = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG);
            ((DownloadHandlerAudioClip)Download.downloadHandler).streamAudio = false;
            try
            {
                var operation = Download.SendWebRequest();
                operation.completed += OnCompleted;
                if (operation.isDone) OnCompleted(operation);
            }
            catch
            {
                Download.Dispose();
                throw;
            }
        }

        private void OnCompleted(AsyncOperation operation)
        {
            if (Disposed || Result.Task.IsCompleted) return;
            if (Download.result != UnityWebRequest.Result.Success)
            {
                Result.TrySetException(new InvalidOperationException(Download.error));
                return;
            }
            try { Result.TrySetResult(DownloadHandlerAudioClip.GetContent(Download)); }
            catch (Exception error) { Result.TrySetException(error); }
        }

        public void Dispose()
        {
            if (Disposed) return;
            Disposed = true;
            if (!Download.isDone) Download.Abort();
            Download.Dispose();
            Result.TrySetCanceled();
        }
    }
}
