using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "QuestionCategory", menuName = "SO/QuestionCategory")]
public class QuestionsPack : ScriptableObject
{
    [SerializeField]
    public List<Question> Questions;

    public List<int> GetIndexes() => Questions.Select(question => question.GetInstanceID()).ToList();

    public Question GetQuestionByID(int id)
    {
        foreach (var question in Questions)
        {
            if (question.GetID() == id.ToString())
            {
                return question;
            }
        }
        
        Debug.LogError("There is no an element with ID: "+id);
        return null;
    }
}
