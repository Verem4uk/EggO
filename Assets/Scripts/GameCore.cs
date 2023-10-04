using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameCore : MonoBehaviour
{
    [SerializeField] 
    private Text CurrentPlayersName;

    [SerializeField] 
    private Text CurrentQuestion;
    
    [SerializeField]
    private QuestionsCategory QuestionsCategory;
    
    private string[] Players;
    private int CurrentPlayerIndex;
    private List<int> UnusedIndexes = new List<int>();
    private int FirstAnsweredPlayerIndex;
    private int CurrentQuestionCounter;

    public void Initialize(string[] players)
    {
        Players = players;
        foreach (var t in QuestionsCategory.Questions)
        {
            UnusedIndexes.Add(t.GetID());
        }

        UpdatePlayerName();
        GenerateNewQuestion();
    }

    private void UpdatePlayerName() => CurrentPlayersName.text = Players[CurrentPlayerIndex];

    public void Next()
    {
        if (++CurrentPlayerIndex >= Players.Length)
        {
            CurrentPlayerIndex = 0;
        }
        
        if (++CurrentQuestionCounter >= Players.Length)
        {
            if (++FirstAnsweredPlayerIndex >= Players.Length)
            {
                FirstAnsweredPlayerIndex = 0;
            }
            CurrentPlayerIndex = FirstAnsweredPlayerIndex;
            GenerateNewQuestion();
        }

        UpdatePlayerName();
    }

    public void FinishSession()
    {
        Application.Quit();
    }

    private void GenerateNewQuestion()
    {
        CurrentQuestionCounter = 0;
        if (UnusedIndexes.Count > 0)
        {
            var newNumber = Random.Range(0, UnusedIndexes.Count);
            var newQuestion = QuestionsCategory.GetQuestionByID(UnusedIndexes[newNumber]);
            UnusedIndexes.Remove(newNumber);
            CurrentQuestion.text = newQuestion.GetRussianText;
            return;
        }
        FinishSession();
    }
}
