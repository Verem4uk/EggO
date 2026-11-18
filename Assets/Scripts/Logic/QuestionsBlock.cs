using UnityEngine;

[CreateAssetMenu(fileName = "QuestionBlock", menuName = "SO/QuestionBlock")]
public class QuestionsBlock : LevelsElement
{
    [SerializeField]
    public Question[] Questions;
}
