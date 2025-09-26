using UnityEngine;

[CreateAssetMenu(fileName = "Question", menuName = "SO/Question")]
public class Question : ScriptableObject, IQuestion
{  
    public string GetText()
    {
        var languageIndex = Root.GetLanguageIndex();
        return languageIndex switch
        {
            1 => PolishText,
            2 => RussianText,
            _ => EnglishText
        };
    }

    [SerializeField, TextAreaAttribute]
    public string RussianText;
    
    [SerializeField, TextAreaAttribute]
    public string EnglishText;
    
    [SerializeField, TextAreaAttribute]
    public string PolishText;
       
    public virtual bool HasImage() => false;
    public virtual Sprite[] GetImages() => null;
}
