using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "QuestionBlock", menuName = "SO/QuestionBlock")]
public class QuestionsBlock : LevelsElement
{
    [SerializeField]
    public Question[] Questions;

    private int NextIndex = 0;

    public override void PrepareQuestions()
    {
        for (int i = 0; i < Questions.Length; i++)
        {
            if (Questions[i].HasImage())
            {
                ((ImageQuestion)Questions[i]).PrepareImagesAsync();
            }               
        }
        NextIndex = 0;
    }

    public override IQuestion GetNextElement()
    {
        if (Questions == null || Questions.Length == 0)
            return null;

        if (NextIndex >= Questions.Length)
            return null; 

        return Questions[NextIndex++];
    }
}