using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class AudioController : MonoBehaviour
{
    [SerializeField]
    private AudioSource AudioSource;

    [SerializeField]
    private AudioClip DefaultAudio;

    [SerializeField]
    private Image SoundImage;

    public void ToogleMute()
    {
        if(AudioSource.mute)
        {
            AudioSource.mute = false;
            SoundImage.color = new Color(1, 1, 1, 1);
            return;
        }

        AudioSource.mute = true;
        SoundImage.color = new Color(1, 1, 1, .5f);
    }

    public async void HandleAudio(string key)
    {        
        if (string.IsNullOrEmpty(key))
        {
            AudioSource.Stop();
            return;
        }

        Debug.Log($"Start loading audio: {key}");

        var handle = Addressables.LoadAssetAsync<AudioClip>(key);
        var downloadedAudio = await handle.Task;

        if (downloadedAudio != null)
        {
            Debug.Log($"Audio loaded: {downloadedAudio.name}");
            AudioSource.clip = downloadedAudio;
            AudioSource.Play();
        }
        else
        {
            Debug.LogError("Failed to load audio.");
        }
    }

    public void SetDefault()
    {
        AudioSource.clip = DefaultAudio;
        AudioSource.Play();
    }

    public void Stop()
    {
        AudioSource.Stop();
    }    
}
