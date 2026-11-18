using UnityEngine;
using UnityEngine.UI;

public class NextButton : MonoBehaviour
{
    [SerializeField]
    private Image Image;

    [SerializeField]
    private AudioSource AudioSource;

    public void SetInteractable(bool value)
    {
        var button = GetComponent<Button>();
        if (button != null) button.interactable = value;
    }
    
    public void UpdateButton(Sprite sprite, AudioClip clip)
    {
        Image.sprite = sprite;

        if(clip != null)
        {
            AudioSource.clip = clip;
        }        
    }
}
