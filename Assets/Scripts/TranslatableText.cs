using UnityEngine;
using UnityEngine.UI;

public class TranslatableText : MonoBehaviour
{
    [SerializeField]
    private Text TextComponent;
    
    [SerializeField, TextAreaAttribute]
    public string EnglishText;
    
    [SerializeField, TextAreaAttribute]
    public string PolishText;
    
    [SerializeField, TextAreaAttribute]
    public string RussianText;

    /*
    public void ApplyLanguage()
    {
        var currentLanguage =  Root.Saver.GetLanguageIdentifier();
        TextComponent.text = currentLanguage switch
        {
            1 => PolishText,
            2 => RussianText,
            _ => EnglishText
        };
    }
    */
}
