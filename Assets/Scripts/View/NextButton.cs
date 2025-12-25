using UnityEngine;
using UnityEngine.UI;

public class NextButton : MonoBehaviour
{
    [SerializeField]
    private Image Image;

    [SerializeField]
    private AudioSource AudioSource;
    
    public void UpdateButton(Sprite sprite, AudioClip clip)
    {
        Image.sprite = sprite;

        if(clip != null)
        {
            AudioSource.clip = clip;
        }        
    }
}
