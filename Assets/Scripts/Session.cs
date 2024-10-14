using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Session
{
    [System.Serializable]
    public struct SessionData
    {
        public Dictionary<int, List<int>> UsedIndexes;
    }

    public SessionData Data { private set; get; }
    private List<int> AvailableQuestionsIndexes;
    private List<int> AvailableImagesQuestionsIndexes;
    private readonly int ImageProbability;

    private int NoImageCounter;
    
    public Session(SessionData data)
    {
        ImageProbability = Root.Questions.ImageProbability;
        
        if (data.UsedIndexes == null)
        {
            data.UsedIndexes = new Dictionary<int, List<int>>();
            data.UsedIndexes.Add(0, new List<int>());
            data.UsedIndexes.Add(1, new List<int>());
        }

        Data = data;
        var allBaseIndexes = Root.Questions.FreeQuestionsPack.GetIndexes();
        AvailableQuestionsIndexes = allBaseIndexes.Except(data.UsedIndexes[0]).ToList();
        CheckBaseRestIndexes();
        
        var allImageIndexes = Root.Questions.FreeImagesPack.GetIndexes();
        AvailableImagesQuestionsIndexes = allImageIndexes.Except(data.UsedIndexes[1]).ToList();
        CheckImageRestIndexes();
    }
    
    public IQuestion GetRandomQuestion()
    {
        Debug.Log("GetRandomQuestion");
        if (NoImageCounter < ImageProbability)
        {
            NoImageCounter++;
            
            var randomNumber = Random.Range(0, AvailableQuestionsIndexes.Count);
            var newUsedIndex = AvailableQuestionsIndexes[randomNumber];
            AvailableQuestionsIndexes.Remove(newUsedIndex);
        
            Data.UsedIndexes[0].Add(newUsedIndex);
            var updatedData = Data;
            Data = updatedData;
        
            CheckBaseRestIndexes();
            return Root.Questions.FreeQuestionsPack.GetQuestionByID(newUsedIndex);
        }

        NoImageCounter = 0;
        
        var randomImageNumber = Random.Range(0, AvailableImagesQuestionsIndexes.Count);
        var newImageUsedIndex = AvailableImagesQuestionsIndexes[randomImageNumber];
        AvailableImagesQuestionsIndexes.Remove(newImageUsedIndex);
        
        Data.UsedIndexes[1].Add(newImageUsedIndex);
        var updatedImageData = Data;
        Data = updatedImageData;

        CheckImageRestIndexes();
        return Root.Questions.FreeImagesPack.GetQuestionByID(newImageUsedIndex);
    }

    private void CheckBaseRestIndexes()
    {
        if (AvailableQuestionsIndexes.Count == 0)
        {
            Data.UsedIndexes[0].Clear();
            AvailableQuestionsIndexes = Root.Questions.FreeQuestionsPack.GetIndexes();
        }
    }
    
    private void CheckImageRestIndexes()
    {
        if (AvailableImagesQuestionsIndexes.Count == 0)
        {
            Data.UsedIndexes[1].Clear();
            AvailableImagesQuestionsIndexes = Root.Questions.FreeImagesPack.GetIndexes();
        }
    }
}


