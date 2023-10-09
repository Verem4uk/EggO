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

    [SerializeField] 
    private QuestionsCategory Practises;

    [SerializeField] 
    private int PracticeFrequency = 5;

    [SerializeField] 
    private ScreenManager ScreenManager;

    private int CounterForPracticeAppearance;
    private bool UsePractises;
    private string[] Players;
    private int CurrentPlayerIndex;
    private List<int> UnusedIndexes = new List<int>();
    private List<int> UnusedPractisesIndexes;
    private int FirstAnsweredPlayerIndex;
    private int CurrentQuestionCounter;

    public void Initialize(string[] players, bool usePractises)
    {
        Players = players;
        foreach (var question in QuestionsCategory.Questions)
        {
            UnusedIndexes.Add(question.GetID());
        }

        UsePractises = usePractises;
        InitializePractises();
        UpdatePlayerName();
        GenerateNewQuestion();
    }

    private void InitializePractises()
    {
        if (!UsePractises)
        {
            return;
        }

        UnusedPractisesIndexes = new List<int>();
        foreach (var practice in Practises.Questions)
        {
            UnusedPractisesIndexes.Add(practice.GetID());
        }
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
        CounterForPracticeAppearance = 0;
        CurrentQuestionCounter = 0;
        CurrentPlayerIndex = 0;
        FirstAnsweredPlayerIndex = 0;
        ScreenManager.SwitchToMainMenu();
    }

    private void GenerateNewQuestion()
    {
        CounterForPracticeAppearance++;
        CurrentQuestionCounter = 0;

        if (UsePractises && CounterForPracticeAppearance >= PracticeFrequency && UnusedPractisesIndexes.Count > 0)
        {
            CounterForPracticeAppearance = 0;
            var newNumber = Random.Range(0, UnusedPractisesIndexes.Count);
            var newQuestion = Practises.GetQuestionByID(UnusedPractisesIndexes[newNumber]);
            UnusedPractisesIndexes.Remove(newNumber);
            CurrentQuestion.text = newQuestion.GetRussianText;
            return;
        }
        
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
