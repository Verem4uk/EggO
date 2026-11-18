using System;
using System.Collections.Generic;
using UnityEngine;

public class Session
{
    private readonly Level Level;
    private readonly int LevelNumber;
    private readonly float StartTime;
    private readonly LevelsElement[] FinishElements;

    private int CurrentElementIndex;
    private int FinishElementIndex;
    private LevelsElement CurrentElement;
    private bool IsFinished;

    // Count sequence positions, not IDs: different questions may share an ID.
    private readonly Dictionary<LevelsElement, int> OpenedQuestions =
        new Dictionary<LevelsElement, int>();

    private sealed class HistoryEntry
    {
        public IQuestion Question;
        public LevelsElement Element;
        public int PositionInElement;
        public string CounterInfo;
    }

    private readonly List<HistoryEntry> History = new List<HistoryEntry>();
    private int HistoryIndex = -1;

    public bool IsFinishing { get; private set; }

    public Session(int level)
    {
        if (Root.Levels == null || level < 1 || level > Root.Levels.Length)
            throw new ArgumentOutOfRangeException(nameof(level));

        Level = Root.Levels[level - 1];
        LevelNumber = level;
        FinishElements = Level.FinishElements != null && Level.FinishElements.Length > 0
            ? Level.FinishElements
            : new[] { Level.Elements[Level.Elements.Length - 1] };

        PrepareQuestions();

        StartTime = Time.time;
        Analytics.StartSession(LevelNumber);
    }

    private void PrepareQuestions()
    {
        foreach (var element in Level.Elements)
        {
            if (element is RandomQuestionsBlock block)
                block.Collection.ResetPool();
        }

        foreach (var element in Level.Elements)
            element.PrepareQuestions();
    }

    public string GetCounterInfo() =>
        HistoryIndex >= 0 ? History[HistoryIndex].CounterInfo : "";

    public bool CanGoBack() => HistoryIndex > 0;

    public IQuestion GetQuestion()
    {
        if (HistoryIndex < History.Count - 1)
            return History[++HistoryIndex].Question;

        if (IsFinished)
            return null;

        var question = GetNewQuestion();
        if (question == null)
        {
            Finish();
            return null;
        }

        OpenedQuestions.TryGetValue(CurrentElement, out int openedCount);
        openedCount++;
        OpenedQuestions[CurrentElement] = openedCount;

        History.Add(new HistoryEntry
        {
            Question = question,
            Element = CurrentElement,
            PositionInElement = openedCount,
            CounterInfo = CurrentElement is RandomQuestionsBlock randomBlock
                ? randomBlock.GetCounterInfo()
                : ""
        });
        HistoryIndex = History.Count - 1;

        // Both normal and early completion end on the same closing question.
        if (IsTheLastQuestion())
            Finish();

        return question;
    }

    private IQuestion GetNewQuestion()
    {
        while (true)
        {
            if (CurrentElement == null)
            {
                var elements = IsFinishing ? FinishElements : Level.Elements;
                int index = IsFinishing ? FinishElementIndex : CurrentElementIndex;
                if (index >= elements.Length)
                    return null;

                CurrentElement = elements[index];
                if (IsFinishing)
                    FinishElementIndex++;
                else
                    CurrentElementIndex++;

                // A closing card already opened in this session is not generated again.
                if (IsFinishing &&
                    OpenedQuestions.TryGetValue(CurrentElement, out int openedCount) &&
                    openedCount >= CurrentElement.QuestionCount)
                {
                    CurrentElement = null;
                    continue;
                }
            }

            var question = CurrentElement.GetNextElement();
            if (question != null)
                return question;

            CurrentElement = null;
        }
    }

    public IQuestion GetPreviousQuestion()
    {
        return CanGoBack() ? History[--HistoryIndex].Question : null;
    }

    // Keep the existing name because the view calls it when confirming early completion.
    public IQuestion Interupt()
    {
        if (IsFinishing || IsFinished)
            return HistoryIndex >= 0 ? History[HistoryIndex].Question : null;

        IsFinishing = true;
        CurrentElement = null;
        // Finish from the furthest reached position, even when viewing older history.
        HistoryIndex = History.Count - 1;
        return GetQuestion();
    }

    private void Finish()
    {
        if (IsFinished)
            return;

        IsFinished = true;
        if (Level.IsCompleted(OpenedQuestions))
            Saver.SetLevel(LevelNumber);

        Analytics.FinishSession(LevelNumber, (int)(Time.time - StartTime));
    }

    public bool IsTheLastQuestion() =>
        HistoryIndex >= 0 &&
        Level.IsTheLastQuestion(History[HistoryIndex].Element) &&
        History[HistoryIndex].PositionInElement >= History[HistoryIndex].Element.QuestionCount;
}
