using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "QuestionCategory", menuName = "SO/QuestionCategory")]
public class QuestionsCategory : ScriptableObject
{
    [SerializeField]
    public List<Question> Questions;

    public Question GetQuestionByID(int ID)
    {
        foreach (var question in Questions)
        {
            if (question.GetID() == ID)
            {
                return question;
            }
        }
        
        Debug.LogError("There is no an element with ID: "+ID);
        return null;
    }
}
