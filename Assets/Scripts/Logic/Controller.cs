using DG.Tweening;
using UnityEngine;

public class Controller : MonoBehaviour
{
    public enum ScreenState { MainMenu, RoadMap, Session, Coffee }

    [SerializeField] private Level[] Levels;
    [SerializeField] private SessionView SessionView;
    [SerializeField] private RoadMapScreen RoadMap;
    [SerializeField] private CoffeeView CoffeeView;
    [SerializeField] private CanvasGroup MainMenuView;
    [SerializeField] private ColorChanger Background;
    [SerializeField] private AudioController AudioController;
    [SerializeField] private GameObject Buttons;
    [SerializeField, Min(0)] private float menuExitDuration = 0.5f;
    [SerializeField, Min(0)] private float menuExitPause = 0.5f;
    [SerializeField, Min(0)] private float buttonsFadeDuration = 1f;

    public ScreenState CurrentScreen { get; private set; } = ScreenState.MainMenu;
    public bool IsTransitioning => Transition != null && Transition.IsActive();

    private Sequence Transition;
    private CanvasGroup SharedButtons;
    private CanvasGroupAutoFadeWithImages MenuFade;
    private CanvasGroupAutoFadeWithImages SessionFade;
    private CanvasGroupAutoFadeWithImages CoffeeFade;
    private SessionCoordinator ActiveSession;
    private bool Configured;
    private bool Started;

    private void Awake() => Configure();

    private void Configure()
    {
        if (Configured) return;
        MenuFade = MainMenuView.GetComponent<CanvasGroupAutoFadeWithImages>();
        SessionFade = SessionView.GetComponent<CanvasGroupAutoFadeWithImages>();
        CoffeeFade = CoffeeView.GetComponent<CanvasGroupAutoFadeWithImages>();
        MenuFade?.SetManagedExternally();
        SessionFade?.SetManagedExternally();
        CoffeeFade?.SetManagedExternally();
        RoadMap.GetComponent<CanvasGroupAutoFadeWithImages>()?.SetManagedExternally();
        SharedButtons = Buttons.GetComponent<CanvasGroup>();
        if (SharedButtons == null) SharedButtons = Buttons.AddComponent<CanvasGroup>();
        Configured = true;
    }

    private void OnEnable()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        Saver.InitLevel(WebGLBridge.GetLevel());
        Saver.InitTrial(WebGLBridge.IsTrial());
#endif
        if (Started) BackToMenu();
    }

    private void Start()
    {
        Root.Initialize(Levels);
        Started = true;
        BackToMenu();
    }

    public void OpenRoadMap()
    {
        Configure();
        if (IsTransitioning || CurrentScreen != ScreenState.MainMenu) return;
        MenuFade?.SetInput(false);
        SetButtonsInput(false);
        var sequence = DOTween.Sequence().Pause();
        sequence.Append(MainMenuView.DOFade(0f, menuExitDuration).SetEase(Ease.Linear));
        sequence.AppendCallback(() => Background.ToggleColor());
        sequence.AppendInterval(menuExitPause);
        sequence.AppendCallback(() =>
        {
            SetScreens(ScreenState.RoadMap);
            SharedButtons.alpha = 0f;
        });
        float showStart = sequence.Duration();
        sequence.Append(RoadMap.ShowSequence(Saver.GetLevel()));
        sequence.Insert(showStart, SharedButtons.DOFade(1f, buttonsFadeDuration).SetEase(Ease.Linear));
        StartTransition(sequence, ScreenState.RoadMap);
    }

    public void PlaySession(int levelIndex)
    {
        Configure();
        if (IsTransitioning || CurrentScreen != ScreenState.RoadMap ||
            levelIndex < 1 || levelIndex > Levels.Length) return;
        if (levelIndex > Saver.MaxAvailableLevel())
        {
            GoToCoffeeScreen();
            return;
        }

        var level = Levels[levelIndex - 1];
        var plan = new SessionFactory(count => Random.Range(0, count)).Create(level, levelIndex);
        ReleaseSession();
        ActiveSession = new SessionCoordinator(new Session(plan),
            new SessionImages(new AddressableSessionImageLoader()),
            new UnitySessionEffects(), () => Time.time);
        SessionView.Initialize(ActiveSession, level);
        SetButtonsInput(false);

        var sequence = DOTween.Sequence().Pause();
        sequence.Append(RoadMap.HideSequence());
        sequence.AppendCallback(() => SetScreens(ScreenState.Session));
        if (SessionFade != null) sequence.Append(SessionFade.ShowSequence(enableInput: false));
        StartTransition(sequence, ScreenState.Session);
        Background.ToggleColor(level.BackgroundColor);
        AudioController.HandleAudio(level.BackGroundAudio);
    }

    public void GoToCoffeeScreen()
    {
        Configure();
        CancelTransition();
        ReleaseSession();
        SetScreens(ScreenState.Coffee);
        Background.ToggleColor(Color.white);
        AudioController.Stop();
        if (CoffeeFade != null)
            StartTransition(CoffeeFade.ShowSequence(enableInput: false), ScreenState.Coffee);
    }

    public void BackToMenu()
    {
        Configure();
        CancelTransition();
        ReleaseSession();
        SetScreens(ScreenState.MainMenu);
        Background.ToggleColor(Background.startColor);
        AudioController.Stop();
        if (MenuFade != null)
            StartTransition(MenuFade.ShowSequence(enableInput: false), ScreenState.MainMenu);
        else MainMenuView.alpha = 1f;
    }

    private void SetScreens(ScreenState screen)
    {
        CurrentScreen = screen;
        MainMenuView.gameObject.SetActive(screen == ScreenState.MainMenu);
        RoadMap.gameObject.SetActive(screen == ScreenState.RoadMap);
        SessionView.gameObject.SetActive(screen == ScreenState.Session);
        CoffeeView.gameObject.SetActive(screen == ScreenState.Coffee);
        Buttons.SetActive(screen == ScreenState.RoadMap || screen == ScreenState.Session);
        SetButtonsInput(false);
    }

    private void StartTransition(Sequence sequence, ScreenState destination)
    {
        Transition = sequence;
        sequence.SetUpdate(true).OnComplete(() =>
        {
            if (Transition != sequence) return;
            Transition = null;
            CurrentScreen = destination;
            MenuFade?.SetInput(destination == ScreenState.MainMenu);
            SessionFade?.SetInput(destination == ScreenState.Session);
            CoffeeFade?.SetInput(destination == ScreenState.Coffee);
            RoadMap.SetInput(destination == ScreenState.RoadMap);
            SetButtonsInput(destination == ScreenState.RoadMap || destination == ScreenState.Session);
        }).Play();
    }

    private void SetButtonsInput(bool enabled)
    {
        SharedButtons.interactable = enabled;
        SharedButtons.blocksRaycasts = enabled;
    }

    private void CancelTransition()
    {
        var previous = Transition;
        Transition = null;
        previous?.Kill();
    }

    public void SwitchLanguage(int index)
    {
        Root.ChangeLanguage(index);
        SessionView.ChangeLanguage();
        RoadMap.UpdateTexts();
    }

    public void ClearPrefs() => Saver.Clear();

    private void ReleaseSession()
    {
        SessionView.ClearSession();
        ActiveSession?.Dispose();
        ActiveSession = null;
    }

    private void OnDisable()
    {
        CancelTransition();
        if (!Configured) return;
        MenuFade?.SetInput(false);
        SessionFade?.SetInput(false);
        CoffeeFade?.SetInput(false);
        RoadMap.SetInput(false);
        SetButtonsInput(false);
    }

    private void OnDestroy()
    {
        CancelTransition();
        ActiveSession?.Dispose();
        ActiveSession = null;
    }
}

