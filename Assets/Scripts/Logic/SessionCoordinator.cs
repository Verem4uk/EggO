using System;

public interface ISessionEffects
{
    void Start(int levelNumber);
    void SaveProgress(int levelNumber);
    void Finish(int levelNumber, int duration);
}

public sealed class UnitySessionEffects : ISessionEffects
{
    public void Start(int levelNumber) => Analytics.StartSession(levelNumber);
    public void SaveProgress(int levelNumber) => Saver.SetLevel(levelNumber);
    public void Finish(int levelNumber, int duration) => Analytics.FinishSession(levelNumber, duration);
}

public sealed class SessionCoordinator : IDisposable
{
    public Session Session { get; }
    public SessionImages Images { get; }

    private readonly ISessionEffects Effects;
    private readonly Func<float> Clock;
    private readonly float StartTime;
    private bool IsDisposed;
    private bool ResultApplied;

    public SessionCoordinator(Session session, SessionImages images,
        ISessionEffects effects, Func<float> clock)
    {
        Session = session ?? throw new ArgumentNullException(nameof(session));
        Images = images ?? throw new ArgumentNullException(nameof(images));
        Effects = effects ?? throw new ArgumentNullException(nameof(effects));
        Clock = clock ?? throw new ArgumentNullException(nameof(clock));
        StartTime = Clock();
        Session.Finished += ApplyResult;
        Effects.Start(Session.Plan.LevelNumber);
        Images.Preload(Session.Plan);
        if (Session.Result != null) ApplyResult(Session.Result);
    }

    private void ApplyResult(SessionResult result)
    {
        if (IsDisposed || ResultApplied) return;
        ResultApplied = true;
        if (result.LevelCompleted) Effects.SaveProgress(result.LevelNumber);
        Effects.Finish(result.LevelNumber, Math.Max(0, (int)(Clock() - StartTime)));
    }

    public void Dispose()
    {
        if (IsDisposed) return;
        IsDisposed = true;
        Session.Finished -= ApplyResult;
        Images.Dispose();
    }
}
