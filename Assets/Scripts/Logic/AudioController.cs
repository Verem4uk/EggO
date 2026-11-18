using System;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class AudioController : MonoBehaviour
{
    [SerializeField] private AudioSource AudioSource;
    [SerializeField] private AudioClip DefaultAudio;
    [SerializeField] private Image SoundImage;
    [SerializeField] private float FadeTime = 3f;

    public IAudioTrackLoader TrackLoader { private get; set; }
    private IAudioTrackRequest PendingRequest;
    private Tween VolumeFade;
    private AudioClip OwnedClip;
    private int OperationVersion;
    private float UserVolume;
    private float Envelope = 1f;
    private bool Initialized;

    private void Awake() => Initialize();

    private void Initialize()
    {
        if (Initialized) return;
        UserVolume = AudioSource.volume;
        TrackLoader = TrackLoader ?? new UnityAudioTrackLoader();
        Initialized = true;
    }

    public void ToggleMute()
    {
        Initialize();
        AudioSource.mute = !AudioSource.mute;
        UpdateView();
    }

    public void ChangeVolume(float value)
    {
        Initialize();
        UserVolume = Mathf.Clamp01(value);
        AudioSource.mute = UserVolume == 0f;
        ApplyVolume();
        UpdateView();
    }

    public void HandleAudio(string filename)
    {
        Initialize();
        if (string.IsNullOrEmpty(filename)) { Stop(); return; }
        CancelOperation();
        _ = LoadAudioAsync(
            $"https://verlema.life/EggO/StreamingAssets/Sounds/{filename}", OperationVersion);
    }

    private async Task LoadAudioAsync(string url, int version)
    {
        IAudioTrackRequest request = null;
        try
        {
            await FadeToAsync(0f);
            if (version != OperationVersion) return;
            ClearClip();
            request = TrackLoader.Load(url);
            PendingRequest = request;
            var clip = await request.Completion;
            if (version != OperationVersion)
            {
                DestroyClip(clip);
                return;
            }
            if (clip == null) throw new InvalidOperationException("The downloaded audio clip is empty.");
            OwnedClip = clip;
            AudioSource.clip = clip;
            Envelope = 0f;
            ApplyVolume();
            AudioSource.Play();
            await FadeToAsync(1f);
        }
        catch (OperationCanceledException) { }
        catch (Exception error)
        {
            if (version == OperationVersion)
                Debug.LogWarning("Background audio could not be loaded: " + error.Message);
        }
        finally
        {
            if (PendingRequest == request) PendingRequest = null;
            request?.Dispose();
        }
    }

    public void SetDefault()
    {
        Initialize();
        CancelOperation();
        ClearClip();
        if (DefaultAudio == null) return;
        AudioSource.clip = DefaultAudio;
        Envelope = 0f;
        ApplyVolume();
        AudioSource.Play();
        FadeTo(1f);
    }

    public void Stop()
    {
        Initialize();
        CancelOperation();
        int version = OperationVersion;
        FadeTo(0f).OnComplete(() =>
        {
            if (version == OperationVersion) ClearClip();
        });
    }

    public void StopImmediate()
    {
        if (AudioSource == null) return;
        Initialize();
        CancelOperation();
        ClearClip();
        Envelope = 0f;
        ApplyVolume();
    }

    private Tween FadeTo(float target)
    {
        VolumeFade?.Kill();
        VolumeFade = DOTween.To(() => Envelope, value =>
        {
            Envelope = value;
            ApplyVolume();
        }, target, Mathf.Max(0f, FadeTime)).SetEase(Ease.Linear).SetUpdate(true);
        return VolumeFade;
    }

    private Task FadeToAsync(float target)
    {
        var completion = new TaskCompletionSource<bool>();
        FadeTo(target).OnComplete(() => completion.TrySetResult(true))
            .OnKill(() => completion.TrySetCanceled());
        return completion.Task;
    }

    private void CancelOperation()
    {
        OperationVersion++;
        var request = PendingRequest;
        PendingRequest = null;
        VolumeFade?.Kill();
        VolumeFade = null;
        request?.Dispose();
    }

    private void ApplyVolume() => AudioSource.volume = UserVolume * Envelope;

    private void UpdateView()
    {
        var color = SoundImage.color;
        color.a = AudioSource.mute ? 0.5f : 1f;
        SoundImage.color = color;
    }

    private void ClearClip()
    {
        AudioSource.Stop();
        AudioSource.clip = null;
        DestroyClip(OwnedClip);
        OwnedClip = null;
    }

    private static void DestroyClip(AudioClip clip)
    {
        if (clip == null) return;
#if UNITY_EDITOR
        if (!Application.isPlaying) { DestroyImmediate(clip); return; }
#endif
        Destroy(clip);
    }

    private void OnDisable() => StopImmediate();
    private void OnDestroy() => StopImmediate();
}
