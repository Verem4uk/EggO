using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RandomBlock", menuName = "SO/RandomBlock")]
public class RandomQuestionsBlock : LevelsElement
{
    [SerializeField]
    public int AmountForOneSession = 5;

    [SerializeField]
    public Question[] Questions;

    private Question[] PreparedQuestions;
    private int nextIndex = 0;

    public override void PrepareQuestions()
    {
        List<Question> tempList = new List<Question>(Questions);
        PreparedQuestions = new Question[AmountForOneSession];

        for (int i = 0; i < AmountForOneSession; i++)
        {
            int randomIndex = Random.Range(0, tempList.Count);
            var question = tempList[randomIndex];
            PreparedQuestions[i] = question;
            tempList.RemoveAt(randomIndex);

            if (question.HasImage())
            {
                ((ImageQuestion)question).PrepareImagesAsync();
            }                
        }

        nextIndex = 0;
    }

    public override IQuestion GetNextElement()
    {
        if (PreparedQuestions == null || nextIndex >= PreparedQuestions.Length)
            return null;

        return PreparedQuestions[nextIndex++];
    }
}