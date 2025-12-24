using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "QuestionBlock", menuName = "SO/RandomBlock")]
public class QuestionsBlock : LevelsElement
{
    [SerializeField]
    public Question[] Questions;

    public override IQuestion GetNextElement(List<int> exeptIndexes = null)
    {
        if (Questions == null || Questions.Length == 0)
            return null;

        foreach (var question in Questions)
        {
            if (exeptIndexes == null || !exeptIndexes.Contains(question.ID))
            {
                return question;
            }
        }

        return null;
    }
}