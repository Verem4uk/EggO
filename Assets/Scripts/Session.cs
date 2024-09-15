using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Session
{
    [System.Serializable]
    public struct SessionData
    {
        public int CurrentQuestionIndex;
        public List<int> UsedIndexes;
        
        public SessionData(int currentQuestionIndex)
        {
            CurrentQuestionIndex = currentQuestionIndex;
            UsedIndexes = new List<int>();
        }
    }

    public SessionData Data { private set; get; }
    private List<int> AvailableQuestionsIndexes;
    
    public Session(SessionData data, List<int> allQuestionsIndexes)
    {
        Data = data;
        AvailableQuestionsIndexes = allQuestionsIndexes.Except(data.UsedIndexes).ToList();
        CheckRestIndexes();
    }

    public Question GetCurrentQuestion()
    {
        return Root.BaseQuestions.GetQuestionByID(AvailableQuestionsIndexes[Data.CurrentQuestionIndex]);
    }
    
    public Question GetRandomQuestion()
    {
        var randomNumber = Random.Range(0, AvailableQuestionsIndexes.Count);
        var newUsedIndex = AvailableQuestionsIndexes[randomNumber];
        
        Data.UsedIndexes.Add(newUsedIndex);
        var updatedData = Data;
        updatedData.CurrentQuestionIndex = newUsedIndex;
        Data = updatedData;
        
        CheckRestIndexes();
        return Root.BaseQuestions.GetQuestionByID(newUsedIndex);
    }

    private void CheckRestIndexes()
    {
        if (AvailableQuestionsIndexes.Count == 0)
        {
            Data.UsedIndexes.Clear();
            AvailableQuestionsIndexes = Root.BaseQuestions.GetIndexes();
        }
    }
}


