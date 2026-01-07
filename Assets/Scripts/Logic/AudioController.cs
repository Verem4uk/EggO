using UnityEngine;
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

    public void HandleAudio(Level level)
    {
        var audio = level.BackGroundAudio;
        if (audio == null)
        {
            AudioSource.Stop();
            return;
        }
        AudioSource.clip = level.BackGroundAudio;
        AudioSource.Play();
    }

    public void SetDefault()
    {
        AudioSource.clip = DefaultAudio;
        AudioSource.Play();
    }
    
}
