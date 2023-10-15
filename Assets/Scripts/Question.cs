using UnityEngine;

[CreateAssetMenu(fileName = "Question", menuName = "SO/Question")]
public class Question : ScriptableObject
{
    [SerializeField] 
    private int ID;

    [SerializeField] 
    private TextTranslation TextTranslation;
    
    public int GetID() => ID;
    public TextTranslation GetTextTranslations() => TextTranslation;
}
