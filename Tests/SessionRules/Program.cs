using System.Text.RegularExpressions;
using UnityEngine;

internal static class Program
{
    private static int Assertions;
    private static SessionCoordinator ActiveCoordinator;
    private static readonly int[] Boundaries = { 3, 10, 14, 10, 10, 8, 8 };

    internal static void Check(bool condition, string message)
    {
        Assertions++;
        if (!condition) throw new Exception(message);
    }

    private static (Level[], Session) Start(int level, Action<Level[]> configure = null, int seed = 42)
    {
        ActiveCoordinator?.Dispose();
        var levels = new AssetLoader().LoadLevels();
        configure?.Invoke(levels);
        Saver.Clear();
        Debug.Messages.Clear();
        UnityEngine.Random.Reset(seed);
        Time.time = 100;
        Root.Initialize(levels);
        var plan = new SessionFactory(count => UnityEngine.Random.Range(0, count)).Create(levels[level - 1], level);
        ActiveCoordinator = new SessionCoordinator(new Session(plan),
            new SessionImages(new ImmediateImageLoader()), new UnitySessionEffects(), () => Time.time);
        return (levels, ActiveCoordinator.Session);
    }

    internal static IQuestion Next(Session session)
    {
        var question = session.GetQuestion();
        if (question != null) session.ConfirmCurrentQuestion();
        return question;
    }

    internal static IQuestion Interrupt(Session session)
    {
        var question = session.Interupt();
        if (question != null) session.ConfirmCurrentQuestion();
        return question;
    }

    private static void Open(Session session, int amount)
    {
        for (int i = 0; i < amount; i++)
            Check(Next(session) != null, "Unexpected end while opening question " + (i + 1));
    }

    private static void Drain(Session session)
    {
        int limit = 100;
        while (Next(session) != null && --limit > 0) { }
        Check(limit > 0, "Session did not terminate");
    }

    private static void CheckEvents(int level)
    {
        var starts = Debug.Messages.Where(m => m.StartsWith("[Analytics] session_start")).ToArray();
        var ends = Debug.Messages.Where(m => m.StartsWith("[Analytics] session_complete")).ToArray();
        Check(starts.Length == 1 && ends.Length == 1, "Analytics must start and finish exactly once");
        Check(starts[0].Contains("| level: " + level + " |") &&
              ends[0].Contains("| level: " + level + " |"), "Analytics level number changed");
    }

    private static async Task Main()
    {
        // Every early-exit position around each boundary, with different generated selections.
        for (int seed = 0; seed < 5; seed++)
        for (int level = 1; level <= 7; level++)
        for (int position = 0; position <= Boundaries[level - 1] + 1; position++)
        {
            var (_, session) = Start(level, seed: seed);
            Open(session, position);
            var firstClosing = Interrupt(session);
            Check(firstClosing != null, "Missing early-exit closing question");
            Check(ReferenceEquals(firstClosing, Interrupt(session)),
                "Repeated interruption skipped a closing question");
            Drain(session);
            int expected = position >= Boundaries[level - 1] ? level : 0;
            Check(Saver.GetLevel() == expected,
                $"Level {level}, position {position}: expected progress {expected}, got {Saver.GetLevel()}");
            CheckEvents(level);
        }

        for (int level = 1; level <= 7; level++)
        {
            var (_, session) = Start(level);
            Drain(session);
            Check(Saver.GetLevel() == level, "Full completion was not saved");
            Check(session.IsTheLastQuestion(), "Final card was not recognized");
            Check(Next(session) == null && Next(session) == null,
                "Questions generated after completion");
            Interrupt(session);
            CheckEvents(level);

            // Replaying completed history must neither generate new questions nor finish twice.
            Check(session.GetPreviousQuestion() != null, "Cannot revisit completed history");
            Check(!session.IsTheLastQuestion(), "An earlier history entry is treated as the final card");
            Next(session);
            Check(session.IsTheLastQuestion(), "Final card lost after replay");
            CheckEvents(level);
        }

        // Backtracking preserves the furthest reached milestone and counter snapshots.
        {
            var (_, session) = Start(4);
            Open(session, 10);
            Check(session.GetCounterInfo() == "5/5", "Last image counter incorrect");
            session.GetPreviousQuestion();
            Check(session.GetCounterInfo() == "4/5", "Back counter follows generator instead of history");
            Interrupt(session);
            Drain(session);
            Check(Saver.GetLevel() == 4, "Backtracking revoked reached completion");
        }
        {
            var (_, session) = Start(1);
            var first = Next(session);
            Next(session);
            for (int i = 0; i < 10; i++)
            {
                Check(ReferenceEquals(first, session.GetPreviousQuestion()), "History question changed");
                Next(session);
            }
            Interrupt(session);
            Drain(session);
            Check(Saver.GetLevel() == 0, "Repeated views counted toward progress");
        }

        // The level 7 card is mandatory even on the earliest exit, but cannot replace Important questions.
        foreach (int position in new[] { 1, 7, 8, 9, 10 })
        {
            var (levels, session) = Start(7);
            Open(session, position);
            var route = levels[6].FinishElements;
            var instruction = route[0] as Question;
            var closing = Interrupt(session);
            if (position < 9)
            {
                Check(ReferenceEquals(closing, instruction), "Missing final-card instruction");
                closing = Next(session);
            }
            if (position < 10)
            {
                Check(closing is ImageQuestion, "Mandatory final image card skipped");
                Check(ReferenceEquals(closing, Interrupt(session)), "Repeated finish skipped final image");
                Check(Saver.GetLevel() == 0, "Progress saved before closing sequence finished");
                closing = Next(session);
            }
            Check(ReferenceEquals(closing, levels[6].Elements.Last()), "Missing final reflection question");
            Check(Next(session) == null, "Unexpected question after final reflection");
            Check(Saver.GetLevel() == (position >= 8 ? 7 : 0), "Level 7 mandatory questions bypassed");
            CheckEvents(7);
        }

        // All-of-block uses the actual generated count, even with a shortened collection.
        {
            var (_, session) = Start(2, levels =>
            {
                var block = (RandomQuestionsBlock)levels[1].CompletionRequirements[0].Element;
                block.Collection.Questions = block.Collection.Questions.Take(2).ToArray();
            });
            Open(session, 7); // mood + 3 text + instruction + 2 generated images
            Interrupt(session);
            Drain(session);
            Check(Saver.GetLevel() == 2, "Shorter generated block was not recognized");
        }
        {
            var (_, session) = Start(2, levels =>
            {
                var block = (RandomQuestionsBlock)levels[1].CompletionRequirements[0].Element;
                block.Collection.Questions = Array.Empty<Question>();
            });
            Drain(session);
            Check(Saver.GetLevel() == 0, "Empty required block incorrectly granted completion");
        }
        {
            var (_, session) = Start(1);
            Saver.SetLevel(6);
            Open(session, 3);
            Interrupt(session);
            Drain(session);
            Check(Saver.GetLevel() == 6, "Existing progress was downgraded");
            CheckEvents(1);
        }

        ActiveCoordinator.Dispose();
        SessionIsolationTests.Run();
        await SessionImagesTests.Run();
        Console.WriteLine($"PASS: {Assertions} assertions; actual seven level assets, five random seeds, " +
            "completion boundaries, history, isolated plans, resource lifecycle and coordinator effects.");
    }
}

// Read the fields used by the session from the checked-in Unity YAML assets.
// Unity serialization and scenes still require an editor smoke test.
internal sealed class AssetLoader
{
    private readonly string RootPath;
    private static readonly Dictionary<string, string> Paths = new();
    private static readonly Dictionary<string, string> RawAssets = new();
    private readonly Dictionary<string, ScriptableObject> Loaded = new();

    public AssetLoader()
    {
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (directory != null && !Directory.Exists(Path.Combine(directory.FullName, "Assets", "BD")))
            directory = directory.Parent;
        RootPath = directory?.FullName ?? throw new Exception("Run inside the EggO repository");
        if (Paths.Count > 0) return;
        foreach (var meta in Directory.EnumerateFiles(Path.Combine(RootPath, "Assets"), "*.asset.meta",
                     SearchOption.AllDirectories))
        {
            string guid = Regex.Match(File.ReadAllText(meta), @"(?m)^guid: (\w+)").Groups[1].Value;
            Paths.Add(guid, meta[..^5]);
        }
    }

    private static string ReadRaw(string guid)
    {
        if (!RawAssets.TryGetValue(guid, out var raw))
            RawAssets.Add(guid, raw = File.ReadAllText(Paths[guid]));
        return raw;
    }

    private static string Section(string raw, string field) =>
        Regex.Match(raw, @"(?ms)^  " + field +
            @":[^\r\n]*\r?\n(.*?)(?=^  [A-Za-z_]\w*:|\z)").Groups[1].Value;

    private static string[] Refs(string raw, string field) =>
        Regex.Matches(Section(raw, field), @"guid: (\w+)").Select(m => m.Groups[1].Value).ToArray();

    public Level[] LoadLevels() =>
        Directory.GetFiles(Path.Combine(RootPath, "Assets", "BD"), "Level*.asset")
            .OrderBy(p => p)
            .Select(p => (Level)Load(Paths.Single(pair => pair.Value == p).Key)).ToArray();

    private ScriptableObject Load(string guid)
    {
        if (Loaded.TryGetValue(guid, out var found)) return found;
        string raw = ReadRaw(guid);
        ScriptableObject asset;
        if (raw.Contains("\n  Elements:")) asset = new Level();
        else if (raw.Contains("\n  AmountForOneSession:")) asset = new RandomQuestionsBlock();
        else if (raw.Contains("\n  Questions:")) asset = new QuestionsBlock();
        else if (raw.Contains("\n  imageKeys:")) asset = new ImageQuestion();
        else asset = new Question();
        Loaded.Add(guid, asset);
        asset.name = Path.GetFileNameWithoutExtension(Paths[guid]);

        switch (asset)
        {
            case Level level:
                level.ID = int.Parse(Regex.Match(raw, @"(?m)^  ID: (\d+)").Groups[1].Value);
                level.Elements = Refs(raw, "Elements").Select(g => (LevelsElement)Load(g)).ToArray();
                level.FinishElements = Refs(raw, "FinishElements").Select(g => (LevelsElement)Load(g)).ToArray();
                level.CompletionRequirements = Regex.Matches(Section(raw, "CompletionRequirements"),
                        @"guid: (\w+), type: 2\}\r?\n    MinimumQuestions: (\d+)")
                    .Select(m => new Level.CompletionRequirement
                    {
                        Element = (LevelsElement)Load(m.Groups[1].Value),
                        MinimumQuestions = int.Parse(m.Groups[2].Value)
                    }).ToArray();
                if (level.CompletionRequirements.Length == 0) throw new Exception("Missing requirements");
                if (level.CompletionRequirements.Any(r => !level.Elements.Contains(r.Element)) ||
                    level.FinishElements.Any(e => !level.Elements.Contains(e)))
                    throw new Exception("Completion configuration references an element outside the level");
                break;
            case RandomQuestionsBlock block:
                block.AmountForOneSession = int.Parse(
                    Regex.Match(raw, @"AmountForOneSession: (\d+)").Groups[1].Value);
                string poolGuid = Regex.Match(raw, @"Collection: .*guid: (\w+)").Groups[1].Value;
                block.Collection = LoadCollection(poolGuid);
                break;
            case QuestionsBlock block:
                block.Questions = Refs(raw, "Questions").Select(g => (Question)Load(g)).ToArray();
                break;
            case ImageQuestion image:
                var keys = Section(raw, "imageKeys").Split('\n').Where(line => line.StartsWith("  - "))
                    .Select(line => line[4..].Trim()).ToArray();
                typeof(ImageQuestion).GetField("imageKeys",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    .SetValue(image, keys);
                break;
        }
        return asset;
    }

    private QuestionsCollection LoadCollection(string guid)
    {
        if (Loaded.TryGetValue(guid, out var found)) return (QuestionsCollection)found;
        var collection = new QuestionsCollection();
        Loaded.Add(guid, collection);
        collection.Questions = Refs(ReadRaw(guid), "Questions")
            .Select(g => (Question)Load(g)).ToArray();
        return collection;
    }
}
