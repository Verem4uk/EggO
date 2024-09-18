using UnityEngine;

[CreateAssetMenu(fileName = "Question", menuName = "SO/Question")]
public class Question : ScriptableObject, IQuestion
{
    public int GetID() => int.Parse(name);
    public string GetText() => RussianText;
    
    [SerializeField, TextAreaAttribute]
    public string RussianText;
    
    [SerializeField, TextAreaAttribute]
    public string EnglishText;
    
    [SerializeField, TextAreaAttribute]
    public string PolishText;
}
