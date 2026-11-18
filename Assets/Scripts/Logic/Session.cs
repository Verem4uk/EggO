using System;
using System.Collections.Generic;

public enum SessionEndReason
{
    CompletedSequence,
    EarlyFinish
}

public sealed class SessionResult
{
    public int LevelNumber { get; }
    public bool LevelCompleted { get; }
    public SessionEndReason Reason { get; }

    internal SessionResult(int levelNumber, bool levelCompleted, SessionEndReason reason)
    {
        LevelNumber = levelNumber;
        LevelCompleted = levelCompleted;
        Reason = reason;
    }
}

public class Session
{
    public SessionPlan Plan { get; }
    public SessionResult Result { get; private set; }
    public event Action<SessionResult> Finished;

    private int NextIndex;
    private int FinishIndex;
    private readonly Dictionary<LevelsElement, int> OpenedQuestions =
        new Dictionary<LevelsElement, int>();
    private readonly HashSet<SessionPlan.Entry> OpenedEntries = new HashSet<SessionPlan.Entry>();
    private readonly List<SessionPlan.Entry> History = new List<SessionPlan.Entry>();
    private int HistoryIndex = -1;

    public bool IsFinishing { get; private set; }

    public Session(SessionPlan plan)
    {
        Plan = plan ?? throw new ArgumentNullException(nameof(plan));
    }

    public string GetCounterInfo() =>
        HistoryIndex >= 0 ? History[HistoryIndex].CounterInfo : "";

    public bool CanGoBack() => HistoryIndex > 0;

    public IQuestion GetQuestion()
    {
        if (HistoryIndex < History.Count - 1)
            return History[++HistoryIndex].Question;
        if (Result != null)
            return null;

        SessionPlan.Entry entry = null;
        if (IsFinishing)
        {
            while (FinishIndex < Plan.FinishQuestions.Count)
            {
                var candidate = Plan.FinishQuestions[FinishIndex++];
                if (!OpenedEntries.Contains(candidate))
                {
                    entry = candidate;
                    break;
                }
            }
        }
        else if (NextIndex < Plan.Questions.Count)
            entry = Plan.Questions[NextIndex++];

        if (entry == null)
        {
            Finish();
            return null;
        }

        History.Add(entry);
        HistoryIndex = History.Count - 1;
        return entry.Question;
    }

    // The view confirms after the text and all required images are ready to display.
    public void ConfirmCurrentQuestion()
    {
        if (Result != null || HistoryIndex < 0) return;
        var entry = History[HistoryIndex];
        if (OpenedEntries.Add(entry))
        {
            OpenedQuestions.TryGetValue(entry.Element, out int count);
            OpenedQuestions[entry.Element] = count + 1;
        }
        if (entry.IsClosing) Finish();
    }

    public IQuestion GetPreviousQuestion() =>
        CanGoBack() ? History[--HistoryIndex].Question : null;

    public IQuestion Interupt()
    {
        if (IsFinishing || Result != null)
            return HistoryIndex >= 0 ? History[HistoryIndex].Question : null;

        IsFinishing = true;
        HistoryIndex = History.Count - 1;
        return GetQuestion();
    }

    private void Finish()
    {
        if (Result != null) return;

        bool completed = Plan.RequiredCounts.Count > 0;
        foreach (var requirement in Plan.RequiredCounts)
        {
            if (requirement.Value <= 0 ||
                !OpenedQuestions.TryGetValue(requirement.Key, out int count) ||
                count < requirement.Value)
                completed = false;
        }

        Result = new SessionResult(Plan.LevelNumber, completed,
            IsFinishing ? SessionEndReason.EarlyFinish : SessionEndReason.CompletedSequence);
        Finished?.Invoke(Result);
    }

    public bool IsTheLastQuestion() =>
        HistoryIndex >= 0 && History[HistoryIndex].IsClosing;
}
