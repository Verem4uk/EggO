using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Session
{
    [System.Serializable]
    public struct SessionData
    {
        public (int, int) CurrentQuestionIndex;
        public Dictionary<int, List<int>> UsedIndexes;
        
        public SessionData((int, int) currentQuestionIndex)
        {
            CurrentQuestionIndex = currentQuestionIndex;
            UsedIndexes = new Dictionary<int, List<int>>
            {
                [0] = new List<int>(),
                [1] = new List<int>()
            };
        }
    }

    public SessionData Data { private set; get; }
    private List<int> AvailableQuestionsIndexes;
    private List<int> AvailableImagesQuestionsIndexes;
    private readonly int ImageProbability;

    private int NoImageCounter;
    
    public Session(SessionData data, List<int> baseQuestionsIndexes, List<int> imagesQuestionsIndexes, int imageProbability)
    {
        Data = data;
        AvailableQuestionsIndexes = baseQuestionsIndexes.Except(data.UsedIndexes[0]).ToList();
        AvailableImagesQuestionsIndexes = imagesQuestionsIndexes.Except(data.UsedIndexes[1]).ToList();
        ImageProbability = imageProbability;
        CheckBaseRestIndexes();
    }

    public IQuestion GetCurrentQuestion()
    {
        Debug.Log("GetCurrentSession");
        return Data.CurrentQuestionIndex.Item1 == 0 ? 
            Root.BaseQuestions.GetQuestionByID(AvailableQuestionsIndexes[Data.CurrentQuestionIndex.Item2]) : 
            Root.ImagesQuestions.GetQuestionByID(AvailableImagesQuestionsIndexes[Data.CurrentQuestionIndex.Item2]);
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
            updatedData.CurrentQuestionIndex = (0, newUsedIndex);
            Data = updatedData;
        
            CheckBaseRestIndexes();
            return Root.BaseQuestions.GetQuestionByID(newUsedIndex);
        }

        NoImageCounter = 0;
        
        var randomImageNumber = Random.Range(0, AvailableImagesQuestionsIndexes.Count);
        var newImageUsedIndex = AvailableImagesQuestionsIndexes[randomImageNumber];
        AvailableQuestionsIndexes.Remove(newImageUsedIndex);
        
        Data.UsedIndexes[1].Add(newImageUsedIndex);
        var updatedImageData = Data;
        updatedImageData.CurrentQuestionIndex = (1, newImageUsedIndex);
        Data = updatedImageData;

        CheckImageRestIndexes();
        return Root.ImagesQuestions.GetQuestionByID(newImageUsedIndex);
    }

    private void CheckBaseRestIndexes()
    {
        if (AvailableQuestionsIndexes.Count == 0)
        {
            Data.UsedIndexes[0].Clear();
            AvailableQuestionsIndexes = Root.BaseQuestions.GetIndexes();
        }
    }
    
    private void CheckImageRestIndexes()
    {
        if (AvailableImagesQuestionsIndexes.Count == 0)
        {
            Data.UsedIndexes[1].Clear();
            AvailableImagesQuestionsIndexes = Root.ImagesQuestions.GetIndexes();
        }
    }
}


