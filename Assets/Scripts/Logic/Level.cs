using UnityEngine;

[CreateAssetMenu(fileName = "Level", menuName = "SO/Level")]
public class Level : ScriptableObject
{
    [SerializeField] 
    public int ID;

    [SerializeField]
    public Sprite Icon;

    [SerializeField]
    public int QuestionsToComplete;

    [SerializeField]
    public AudioClip ButtonSound;

    [SerializeField]
    public Color BackgroundColor;

    [SerializeField]
    public Color SmokeColor;

    [SerializeField]
    public Color TextColor;

    [SerializeField]
    public string BackGroundAudio;

    [SerializeField]
    public LevelsElement[] Elements;

    public bool IsCompleted(int countOfPastQuestion) => countOfPastQuestion >= QuestionsToComplete;

    public bool IsTheLastQuestion(LevelsElement element) => Elements[Elements.Length - 1] == element;
}
