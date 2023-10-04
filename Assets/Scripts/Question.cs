using UnityEngine;

[CreateAssetMenu(fileName = "Question", menuName = "SO/Question")]
public class Question : ScriptableObject
{
    [SerializeField] 
    private int ID;
    
    [SerializeField, TextAreaAttribute]
    private string RussianText;

    public int GetID() => ID;

    public string GetRussianText => RussianText;
}
