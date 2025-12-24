using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Question", menuName = "SO/Question")]
public class Question : LevelsElement, IQuestion
{
    public int ID;
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

    public int GetID() => ID;       
    public virtual bool HasImage() => false;
    public virtual Sprite[] GetImages() => null;

    public override IQuestion GetNextElement(List<int> exceptIndexes = null)
    {
        return exceptIndexes == null || exceptIndexes.Count == 0 ? this : null;
    }
}
