using UnityEngine;

internal sealed class ImmediateImageLoader : ISessionImageLoader
{
    public readonly List<string> Keys = new();
    public int Released;
    public Task<SessionImage> LoadAsync(string key)
    {
        Keys.Add(key);
        return Task.FromResult(new SessionImage(new Sprite(), () => Released++));
    }
}

internal sealed class RecordingEffects : ISessionEffects
{
    public int Starts, Saves, Finishes, Duration, Level;
    public void Start(int level) { Starts++; Level = level; }
    public void SaveProgress(int level) { Saves++; Level = level; }
    public void Finish(int level, int duration) { Finishes++; Level = level; Duration = duration; }
}

internal static class SessionIsolationTests
{
    public static void Run()
    {
        var levels = new AssetLoader().LoadLevels();
        var factory = new SessionFactory(count => 0);
        var firstPlan = factory.Create(levels[1], 2);
        var snapshot = firstPlan.Questions.Select(e => e.Question).ToArray();
        var first = new Session(firstPlan);
        for (int i = 0; i < 9; i++) Program.Next(first);
        var secondPlan = new SessionFactory(count => count - 1).Create(levels[1], 2);
        var second = new Session(secondPlan);
        while (Program.Next(second) != null) { }
        Program.Check(firstPlan.Questions.Select(e => e.Question).SequenceEqual(snapshot),
            "Generating or advancing another session changed the first plan");
        Program.Check(first.GetCounterInfo() == "4/5", "Another session changed the first cursor");
        Program.Next(first);
        Program.Interrupt(first);
        while (Program.Next(first) != null) { }
        Program.Check(first.Result.LevelCompleted && second.Result.LevelCompleted,
            "Sessions sharing the same assets interfere with completion");

        var randomBlocks = levels[1].Elements.OfType<RandomQuestionsBlock>()
            .Where(b => b.Collection == ((RandomQuestionsBlock)levels[1].Elements[1]).Collection).ToArray();
        var sharedQuestions = firstPlan.Questions.Where(e => randomBlocks.Contains(e.Element))
            .Select(e => e.Question).ToArray();
        Program.Check(sharedQuestions.Length == 11 && sharedQuestions.Distinct().Count() == 11,
            "Blocks using a shared collection repeat questions within the same session");
        Program.Check(!firstPlan.Questions.Select(e => e.Question)
            .SequenceEqual(secondPlan.Questions.Select(e => e.Question)), "Random source is ignored");

        // A plan snapshots required counts and selected positions.
        levels = new AssetLoader().LoadLevels();
        var plan = factory.Create(levels[0], 1);
        levels[0].CompletionRequirements[0].MinimumQuestions = 999;
        ((RandomQuestionsBlock)levels[0].Elements[2]).AmountForOneSession = 0;
        var isolated = new Session(plan);
        for (int i = 0; i < 3; i++) Program.Next(isolated);
        Program.Interrupt(isolated);
        Program.Check(isolated.Result.LevelCompleted, "Asset changes mutated an existing plan");
        Program.Check(isolated.Result.Reason == SessionEndReason.EarlyFinish, "Wrong result reason");

        // Domain sessions run without Root, PlayerPrefs, clocks or analytics.
        Saver.Clear();
        Debug.Messages.Clear();
        var pure = new Session(factory.Create(new AssetLoader().LoadLevels()[0], 1));
        while (Program.Next(pure) != null) { }
        Program.Check(pure.Result.LevelCompleted && Saver.GetLevel() == 0 && Debug.Messages.Count == 0,
            "Session performs external effects directly");

        // A requested image which never became visible must not count toward completion.
        var pending = new Session(factory.Create(new AssetLoader().LoadLevels()[5], 6));
        for (int i = 0; i < 7; i++) Program.Next(pending);
        Program.Check(pending.GetQuestion() is ImageQuestion, "Expected the second block's final image");
        Program.Interrupt(pending);
        Program.Check(!pending.Result.LevelCompleted, "An unconfirmed image counted toward completion");

        var effects = new RecordingEffects();
        var loader = new ImmediateImageLoader();
        float now = 10;
        var coordinated = new Session(factory.Create(new AssetLoader().LoadLevels()[0], 1));
        using (var coordinator = new SessionCoordinator(coordinated, new SessionImages(loader), effects, () => now))
        {
            Program.Check(loader.Keys.Count > 0, "Images are not preloaded when the session starts");
            now = 27.9f;
            while (Program.Next(coordinated) != null) { }
            Program.Interrupt(coordinated);
            Program.Check(effects.Starts == 1 && effects.Saves == 1 && effects.Finishes == 1 &&
                effects.Duration == 17 && effects.Level == 1, "Coordinator applied an incorrect result");
        }
        Program.Check(loader.Released == loader.Keys.Count, "Coordinator did not release all image leases");

        effects = new RecordingEffects();
        var abandoned = new Session(factory.Create(new AssetLoader().LoadLevels()[0], 1));
        var disposed = new SessionCoordinator(abandoned, new SessionImages(new ImmediateImageLoader()), effects, () => 0);
        disposed.Dispose();
        disposed.Dispose();
        while (Program.Next(abandoned) != null) { }
        Program.Check(effects.Saves == 0 && effects.Finishes == 0, "Disposed coordinator applied a stale result");
    }
}
