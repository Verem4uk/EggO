using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "QuestionsCollection", menuName = "SO/QuestionsCollection")]
public class QuestionsCollection : ScriptableObject
{
    [SerializeField]
    public Question[] Questions;
    
    private List<Question> availableQuestions;

    public int AvailableCount => availableQuestions?.Count ?? 0;

    public void ResetPool()
    {
        availableQuestions = new List<Question>(Questions);
    }

    public Question TakeRandomQuestion()
    {        
        if (availableQuestions.Count == 0)
        {
            return null;
        }            

        int randomIndex = UnityEngine.Random.Range(0, availableQuestions.Count);

        Question question = availableQuestions[randomIndex];
        availableQuestions.RemoveAt(randomIndex);

        return question;
    }
}

