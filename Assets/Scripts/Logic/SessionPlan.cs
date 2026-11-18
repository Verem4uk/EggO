using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

// A snapshot of one generated session. Entries are shared with the finish route,
// so a card can be recognized without using question IDs or mutable asset state.
public sealed class SessionPlan
{
    public sealed class Entry
    {
        public IQuestion Question { get; }
        public LevelsElement Element { get; }
        public int PositionInElement { get; }
        public string CounterInfo { get; }
        public bool IsClosing { get; }

        internal Entry(IQuestion question, LevelsElement element, int position,
            string counterInfo, bool isClosing)
        {
            Question = question;
            Element = element;
            PositionInElement = position;
            CounterInfo = counterInfo;
            IsClosing = isClosing;
        }
    }

    public int LevelNumber { get; }
    public IReadOnlyList<Entry> Questions { get; }
    public IReadOnlyList<Entry> FinishQuestions { get; }
    internal IReadOnlyDictionary<LevelsElement, int> RequiredCounts { get; }

    internal SessionPlan(int levelNumber, List<Entry> questions, List<Entry> finishQuestions,
        Dictionary<LevelsElement, int> requiredCounts)
    {
        LevelNumber = levelNumber;
        Questions = questions.AsReadOnly();
        FinishQuestions = finishQuestions.AsReadOnly();
        RequiredCounts = new ReadOnlyDictionary<LevelsElement, int>(requiredCounts);
    }
}
