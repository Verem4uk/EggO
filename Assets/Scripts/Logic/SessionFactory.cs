using System;
using System.Collections.Generic;

public sealed class SessionFactory
{
    private readonly Func<int, int> RandomIndex;

    public SessionFactory(Func<int, int> randomIndex)
    {
        RandomIndex = randomIndex ?? throw new ArgumentNullException(nameof(randomIndex));
    }

    public SessionPlan Create(Level level, int levelNumber)
    {
        if (level == null) throw new ArgumentNullException(nameof(level));
        if (levelNumber < 1) throw new ArgumentOutOfRangeException(nameof(levelNumber));
        if (level.Elements == null || level.Elements.Length == 0)
            throw new ArgumentException("A level must contain a closing element.", nameof(level));

        var pools = new Dictionary<QuestionsCollection, List<Question>>();
        var entriesByElement = new Dictionary<LevelsElement, List<SessionPlan.Entry>>();
        var questions = new List<SessionPlan.Entry>();

        for (int elementIndex = 0; elementIndex < level.Elements.Length; elementIndex++)
        {
            var element = level.Elements[elementIndex];
            if (element == null || entriesByElement.ContainsKey(element))
                throw new ArgumentException("Level elements must be non-null and occur once.", nameof(level));

            var selected = Select(element, pools);
            var entries = new List<SessionPlan.Entry>();
            for (int i = 0; i < selected.Count; i++)
            {
                if (selected[i] == null)
                    throw new ArgumentException("A question definition is missing.", nameof(level));

                var entry = new SessionPlan.Entry(selected[i], element, i + 1,
                    element is RandomQuestionsBlock ? $"{i + 1}/{selected.Count}" : "",
                    elementIndex == level.Elements.Length - 1 && i == selected.Count - 1);
                entries.Add(entry);
                questions.Add(entry);
            }
            entriesByElement.Add(element, entries);
        }

        var requiredCounts = new Dictionary<LevelsElement, int>();
        foreach (var requirement in level.CompletionRequirements ?? Array.Empty<Level.CompletionRequirement>())
        {
            if (requirement == null || requirement.Element == null ||
                !entriesByElement.TryGetValue(requirement.Element, out var entries))
                throw new ArgumentException("A completion requirement must reference a level element.", nameof(level));

            int count = requirement.MinimumQuestions > 0 ? requirement.MinimumQuestions : entries.Count;
            if (!requiredCounts.TryGetValue(requirement.Element, out int previous) || previous < count)
                requiredCounts[requirement.Element] = count;
        }

        var finish = new List<SessionPlan.Entry>();
        var finishElements = level.FinishElements != null && level.FinishElements.Length > 0
            ? level.FinishElements
            : new[] { level.Elements[level.Elements.Length - 1] };
        var added = new HashSet<SessionPlan.Entry>();
        foreach (var element in finishElements)
        {
            if (element == null || !entriesByElement.TryGetValue(element, out var entries))
                throw new ArgumentException("A finish element must belong to the level.", nameof(level));
            foreach (var entry in entries)
                if (added.Add(entry)) finish.Add(entry);
        }

        return new SessionPlan(levelNumber, questions, finish, requiredCounts);
    }

    private List<Question> Select(LevelsElement element,
        Dictionary<QuestionsCollection, List<Question>> pools)
    {
        if (element is Question question)
            return new List<Question> { question };

        if (element is QuestionsBlock block)
            return new List<Question>(block.Questions ?? Array.Empty<Question>());

        if (element is RandomQuestionsBlock random)
        {
            if (random.Collection == null)
                throw new ArgumentException("A random block must reference a collection.");
            if (!pools.TryGetValue(random.Collection, out var pool))
            {
                // The pool is local to this generated session, shared only by its blocks.
                pool = new List<Question>();
                foreach (var candidate in random.Collection.Questions ?? Array.Empty<Question>())
                    if (candidate != null && !pool.Contains(candidate)) pool.Add(candidate);
                pools.Add(random.Collection, pool);
            }

            int amount = Math.Min(Math.Max(0, random.AmountForOneSession), pool.Count);
            var selected = new List<Question>(amount);
            for (int i = 0; i < amount; i++)
            {
                int index = RandomIndex(pool.Count);
                if (index < 0 || index >= pool.Count)
                    throw new InvalidOperationException("The random source returned an invalid index.");
                selected.Add(pool[index]);
                pool.RemoveAt(index);
            }
            return selected;
        }

        throw new ArgumentException("Unsupported level element: " + element.GetType().Name);
    }
}
