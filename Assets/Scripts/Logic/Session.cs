using System.Collections.Generic;
using UnityEngine;

public class Session
{       
    private Level Level;
    private int LevelIndex;

    private int CurrentElementIndex;
    private LevelsElement CurrentElement;
    private List<int> CurrentElementIndexes = new List<int>();

    private float StartTime;

    public Session(int level)
    {   
        Level = Root.Levels[--level];
        LevelIndex = level;
        CurrentElementIndex = 0;

        PrepareQuestions();

        StartTime = Time.time;
        Analytics.StartSession(LevelIndex);
    }

    public void PrepareQuestions()
    {
        foreach (var element in Level.Elements)
        {
            element.PrepareQuestions();            
        }
    }

    public string GetCounterInfo()
    {
        return CurrentElement is RandomQuestionsBlock randomBlock
            ? (CurrentElementIndexes.Count).ToString() + "/" + randomBlock.AmountForOneSession.ToString()
            : "";
    }

    public IQuestion GetQuestion()
    {
        if (CurrentElement == null)
        {
            if (CurrentElementIndex >= Level.Elements.Length)
            {
                Saver.SetLevel(++LevelIndex);
                Analytics.FinishSession(LevelIndex, (int)(Time.time - StartTime));
                return null; // the end of the session
            }

            CurrentElement = Level.Elements[CurrentElementIndex];
            CurrentElementIndex++;
            CurrentElementIndexes.Clear();
        }
                
        if (CurrentElement is RandomQuestionsBlock randomBlock)
        {            
            if (CurrentElementIndexes.Count >= randomBlock.AmountForOneSession)
            {
                CurrentElement = null;
                return GetQuestion();
            }

            var nextQuestion = randomBlock.GetNextElement();
            if (nextQuestion == null)
            {                
                CurrentElement = null;
                return GetQuestion();
            }

            CurrentElementIndexes.Add(nextQuestion.GetID());
            return nextQuestion;
        }

        var question = CurrentElement.GetNextElement();
        if (question == null)
        {
            CurrentElement = null;
            return GetQuestion();
        }

        CurrentElementIndexes.Add(question.GetID());
        return question;
    }
}


