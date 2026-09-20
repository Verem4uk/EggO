using System.Collections.Generic;
using UnityEngine;

public class Session
{
    private Level Level;
    private int LevelIndex;

    private int CurrentElementIndex;
    private LevelsElement CurrentElement;
    private LevelsElement MaxElement;

    private List<int> CurrentElementIndexes = new List<int>();

    private bool LevelFailed;
    private bool LevelCompleted;
    private float StartTime;

    // History of all received questions
    private List<IQuestion> History = new List<IQuestion>();

    // Index of the current question in history
    private int HistoryIndex = -1;

    // Current question
    private IQuestion CurrentQuestion;

    // Session was interrupted through Interupt()
    private bool IsInterrupted;

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
            if (element is RandomQuestionsBlock block)
            {
                block.Collection.ResetPool();
            }
        }

        foreach (var element in Level.Elements)
        {
            element.PrepareQuestions();
        }
    }

    public string GetCounterInfo() =>
        CurrentElement is RandomQuestionsBlock randomBlock
            ? randomBlock.GetCounterInfo()
            : "";

    public bool CanGoBack()
    {
        return HistoryIndex > 0;
    }

    public IQuestion GetQuestion()
    {
        // If there are previously received questions ahead,
        // return them without generating new questions.
        if (HistoryIndex < History.Count - 1)
        {
            HistoryIndex++;
            CurrentQuestion = History[HistoryIndex];

            return CurrentQuestion;
        }

        // Do not generate new questions after interruption.
        // The history already contains the final question.
        if (IsInterrupted)
        {
            return null;
        }

        // Get the next question using the existing generation logic.
        var nextQuestion = GetNewQuestion();

        if (nextQuestion == null)
        {
            return null;
        }

        // Add the newly generated question to history.
        History.Add(nextQuestion);
        HistoryIndex = History.Count - 1;

        CurrentQuestion = nextQuestion;

        return CurrentQuestion;
    }

    private IQuestion GetNewQuestion()
    {
        if (CurrentElement == null)
        {
            if (CurrentElementIndex >= Level.Elements.Length)
            {
                CompleteLevel();

                Analytics.FinishSession(
                    LevelIndex,
                    (int)(Time.time - StartTime)
                );

                return null;
            }

            CurrentElement = Level.Elements[CurrentElementIndex];
            CurrentElementIndex++;

            // Remember the furthest element reached by the session.
            MaxElement = CurrentElement;

            CurrentElementIndexes.Clear();
        }

        if (CurrentElement is RandomQuestionsBlock randomBlock)
        {
            if (CurrentElementIndexes.Count >= randomBlock.AmountForOneSession)
            {
                CurrentElement = null;

                // End of the random questions block
                return GetNewQuestion();
            }

            var nextQuestion = randomBlock.GetNextElement();

            if (nextQuestion == null)
            {
                CurrentElement = null;

                // End of the random questions block
                return GetNewQuestion();
            }

            var questionID = nextQuestion.GetID();

            CurrentElementIndexes.Add(questionID);

            return nextQuestion;
        }

        var question = CurrentElement.GetNextElement();

        if (question == null)
        {
            CurrentElement = null;

            return GetNewQuestion();
        }

        CurrentElementIndexes.Add(question.GetID());

        return question;
    }

    public IQuestion GetPreviousQuestion()
    {
        if (!CanGoBack())
        {
            return null;
        }

        HistoryIndex--;

        CurrentQuestion = History[HistoryIndex];

        return CurrentQuestion;
    }

    public IQuestion Interupt()
    {
        if (!IsInterrupted)
        {
            var elementToCheck = MaxElement ?? CurrentElement;

            LevelFailed =
                !Level.IsTheSecondToLastQuestion(elementToCheck) &&
                !Level.IsTheLastQuestion(elementToCheck);

            if (!LevelFailed)
            {
                CompleteLevel();
            }

            Analytics.FinishSession(
                LevelIndex,
                (int)(Time.time - StartTime)
            );

            IsInterrupted = true;
        }

        // Generate the final question of the session.
        CurrentElement = Level.Elements[Level.Elements.Length - 1];

        var finalQuestion = CurrentElement.GetNextElement();

        if (finalQuestion == null)
        {
            return null;
        }

        History.Add(finalQuestion);
        HistoryIndex = History.Count - 1;

        CurrentQuestion = finalQuestion;

        return CurrentQuestion;
    }

    private void CompleteLevel()
    {
        if (LevelCompleted)
        {
            return;
        }

        LevelCompleted = true;
        Saver.SetLevel(++LevelIndex);
    }

    public bool IsTheLastQuestion() =>
        Level.IsTheLastQuestion(CurrentElement);
}
