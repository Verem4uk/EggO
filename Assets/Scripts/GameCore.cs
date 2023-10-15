using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

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
    
    [System.Serializable]
    public struct GameProcessStructure
    {
        public string[] Players;
        public int CurrentPlayerIndex;
        public int FirstAnsweredPlayerIndex;
        public int CurrentQuestionCounter;
        public Question CurrentLogicQuestion;
        public List<int> UnusedIndexes;
        public List<int> UnusedPractisesIndexes;
        public int CounterForPracticeAppearance;
        public bool UsePractises;
    }

    private GameProcessStructure GameProcess;
    
    public void Initialize(string[] players, bool usePractises)
    {
        GameProcess = new GameProcessStructure
        {
            Players = players,
            UsePractises = usePractises
        };

        InitializeQuestions();
        InitializePractises();
        UpdatePlayerName();
        GenerateNewQuestion();

        void InitializeQuestions()
        {
            GameProcess.UnusedIndexes = new List<int>();
            foreach (var question in QuestionsCategory.Questions)
            {
                GameProcess.UnusedIndexes.Add(question.GetID());
            }
        }
        
        void InitializePractises()
        {
            if (!GameProcess.UsePractises)
            {
                return;
            }

            GameProcess.UnusedPractisesIndexes = new List<int>();
            foreach (var practice in Practises.Questions)
            {
                GameProcess.UnusedPractisesIndexes.Add(practice.GetID());
            }
        }
    }

    public void InitializeFromSave(GameProcessStructure gameProcessStructure)
    {
        GameProcess = gameProcessStructure;
        UpdatePlayerName();
        UpdateQuestionText();
    }
    
    private void UpdatePlayerName() => CurrentPlayersName.text = GameProcess.Players[GameProcess.CurrentPlayerIndex];
    
    private void UpdateQuestionText() => CurrentQuestion.text = 
        Localization.Instance.GetTextAccordingLanguage(GameProcess.CurrentLogicQuestion.GetTextTranslations());

    public void Next()
    {
        if (++GameProcess.CurrentPlayerIndex >= GameProcess.Players.Length)
        {
            GameProcess.CurrentPlayerIndex = 0;
        }
        
        if (++GameProcess.CurrentQuestionCounter >= GameProcess.Players.Length)
        {
            if (++GameProcess.FirstAnsweredPlayerIndex >= GameProcess.Players.Length)
            {
                GameProcess.FirstAnsweredPlayerIndex = 0;
            }
            GameProcess.CurrentPlayerIndex = GameProcess.FirstAnsweredPlayerIndex;
            GenerateNewQuestion();
        }

        UpdatePlayerName();
    }

    public void ChangeLanguage(int position)
    {
        ScreenManager.ChangeLanguage(position);
        CurrentQuestion.text = 
            Localization.Instance.GetTextAccordingLanguage(GameProcess.CurrentLogicQuestion.GetTextTranslations());
    }

    private void GenerateNewQuestion()
    {
        GameProcess.CounterForPracticeAppearance++;
        GameProcess.CurrentQuestionCounter = 0;

        if (GameProcess.UsePractises && GameProcess.CounterForPracticeAppearance >= 
            PracticeFrequency && GameProcess.UnusedPractisesIndexes.Count > 0)
        {
            GameProcess.CounterForPracticeAppearance = 0;
            var newNumber = Random.Range(0, GameProcess.UnusedPractisesIndexes.Count);
            GameProcess.CurrentLogicQuestion = Practises.GetQuestionByID(GameProcess.UnusedPractisesIndexes[newNumber]);
            GameProcess.UnusedPractisesIndexes.Remove(newNumber);
            CurrentQuestion.text = 
                Localization.Instance.GetTextAccordingLanguage(GameProcess.CurrentLogicQuestion.GetTextTranslations());
            return;
        }
        
        if (GameProcess.UnusedIndexes.Count > 0)
        {
            var newNumber = Random.Range(0, GameProcess.UnusedIndexes.Count);
            GameProcess.CurrentLogicQuestion = QuestionsCategory.GetQuestionByID(GameProcess.UnusedIndexes[newNumber]);
            GameProcess.UnusedIndexes.Remove(newNumber);
            CurrentQuestion.text = 
                Localization.Instance.GetTextAccordingLanguage(GameProcess.CurrentLogicQuestion.GetTextTranslations());
            return;
        }
        
        FinishSession();
    }
    
    public void FinishSession()
    {
        GameProcess.CounterForPracticeAppearance = 0;
        GameProcess.CurrentQuestionCounter = 0;
        GameProcess.CurrentPlayerIndex = 0;
        GameProcess.FirstAnsweredPlayerIndex = 0;
        Saver.Instance.Clear();
        ScreenManager.SwitchToMainMenu();
    }
    
    private void OnApplicationQuit()
    {
        Debug.LogError("Quit and save");
        Saver.Instance.Save(GameProcess);
    }
}
