using UnityEngine;

[CreateAssetMenu(fileName = "TextTranslation", menuName = "SO/TextTranslation")]
public class TextTranslation : ScriptableObject
{
    [SerializeField, TextAreaAttribute]
    public string RussianText;
    
    [SerializeField, TextAreaAttribute]
    public string EnglishText;
    
    [SerializeField, TextAreaAttribute]
    public string PolishText;
    
    public string GetRussianText => RussianText;
    public string GetEnglishText => EnglishText;
    public string GetPolishText => PolishText;
}
