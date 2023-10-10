using UnityEngine;

[CreateAssetMenu(fileName = "Question", menuName = "SO/Question")]
public class Question : ScriptableObject
{
    [SerializeField] 
    private int ID;
    
    [SerializeField] 
    private bool IsPractice;
    
    [SerializeField, TextAreaAttribute]
    private string RussianText;
    
    [SerializeField, TextAreaAttribute]
    private string EnglishText;
    
    [SerializeField, TextAreaAttribute]
    private string PolishText;

    public int GetID() => ID;

    public string GetRussianText => RussianText;
    public string GetEnglishText => EnglishText;
    public string GetPolishText => PolishText;
}
