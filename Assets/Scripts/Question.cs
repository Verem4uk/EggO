using UnityEngine;

[CreateAssetMenu(fileName = "Question", menuName = "SO/Question")]
public class Question : ScriptableObject
{
    public string GetID() => name;
    [SerializeField, TextAreaAttribute]
    public string RussianText;
    
    [SerializeField, TextAreaAttribute]
    public string EnglishText;
    
    [SerializeField, TextAreaAttribute]
    public string PolishText;
}
