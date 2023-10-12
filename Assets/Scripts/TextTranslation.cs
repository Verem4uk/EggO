using UnityEngine;

[CreateAssetMenu(fileName = "TextTranslation", menuName = "SO/TextTranslation")]
public class TextTranslation : ScriptableObject
{
    [SerializeField, TextAreaAttribute]
    private string RussianText;
    
    [SerializeField, TextAreaAttribute]
    private string EnglishText;
    
    [SerializeField, TextAreaAttribute]
    private string PolishText;
    
    public string GetRussianText => RussianText;
    public string GetEnglishText => EnglishText;
    public string GetPolishText => PolishText;
}
