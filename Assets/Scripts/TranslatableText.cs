using UnityEngine;
using UnityEngine.UI;

public class TranslatableText : MonoBehaviour
{
    [SerializeField]
    private Text TextComponent;
    
    [SerializeField]
    private TextTranslation Translation;

    public void ApplyLanguage() => TextComponent.text = Localization.Instance.GetTextAccordingLanguage(Translation);
}
