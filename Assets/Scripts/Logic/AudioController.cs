using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;

public class AudioController : MonoBehaviour
{
    [SerializeField] 
    private AudioSource AudioSource;

    [SerializeField] 
    private AudioClip DefaultAudio;

    [SerializeField] 
    private Image SoundImage;

    [SerializeField]
    private float FadeTime = 3f;

    private Coroutine fadeCoroutine;

    public void ToggleMute()
    {
        AudioSource.mute = !AudioSource.mute;
        UpdateView();
    }

    public void ChangeVolume(float value)
    {
        AudioSource.volume = value;
        if(value == 0)
        {
            AudioSource.mute = true;
        }
        else 
        {
            AudioSource.mute = false;
        }

        UpdateView();
    }

    public void HandleAudio(string filename)
    {
        if (string.IsNullOrEmpty(filename))
        {
            AudioSource.Stop();
            return;
        }

        string url = $"https://verlema.life/EggO/StreamingAssets/Sounds/{filename}";
        Debug.Log($"Start loading audio from: {url}");

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(LoadAudio(url));
    }

    private void UpdateView()
    {
        SoundImage.color = AudioSource.mute ? new Color(1, 1, 1, 0.5f) : new Color(1, 1, 1, 1);        
    }

    private IEnumerator LoadAudio(string url)
    {
        yield return StopAudio();
        
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
        {
            DownloadHandlerAudioClip dlHandler = (DownloadHandlerAudioClip)www.downloadHandler;
            dlHandler.streamAudio = false; 

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Failed to load audio: {www.error}");
                yield break;
            }

            AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
            AudioSource.clip = clip;
            AudioSource.volume = 1f;

            StartCoroutine(PlayAudio());
        }
    }

    private IEnumerator PlayAudio()
    {
        AudioSource.Play();

        float t = 0f;
        while (t < FadeTime)
        {
            t += Time.deltaTime;
            AudioSource.volume = Mathf.Lerp(0f, 1f, t / FadeTime);
            yield return null;
        }
        AudioSource.volume = 1f;
    }

    private IEnumerator StopAudio()
    {
        if (AudioSource.isPlaying)
        {
            float startVol = AudioSource.volume;
            float t = 0f;
            while (t < FadeTime)
            {
                t += Time.deltaTime;
                AudioSource.volume = Mathf.Lerp(startVol, 0f, t / FadeTime);
                yield return null;
            }
            AudioSource.Stop();
        }
    }

    public void SetDefault()
    {
        AudioSource.clip = DefaultAudio;
        StartCoroutine(PlayAudio());
    }

    public void Stop()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        StartCoroutine(StopAudio());
    }
}