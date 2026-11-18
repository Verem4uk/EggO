using System.Collections;
using UnityEngine;

public class Controller : MonoBehaviour
{
    [SerializeField] 
    private Level[] Levels;
    
    [SerializeField] 
    private SessionView SessionView;

    [SerializeField]
    private RoadMapScreen RoadMap;

    [SerializeField]
    private CoffeeView CoffeeView;

    [SerializeField] 
    private CanvasGroup MainMenuView;
    
    [SerializeField]
    private ColorChanger Background;

    [SerializeField]
    private AudioController AudioController;

    [SerializeField]
    private GameObject Buttons;

    private bool InputIsBlocked;

    private SessionCoordinator ActiveSession;

    private void OnEnable()
    {
#if UNITY_WEBGL && !UNITY_EDITOR

        int level = WebGLBridge.GetLevel();
        bool trial = WebGLBridge.IsTrial();

        Debug.Log("EGGO level from server: " + level);

        Saver.InitLevel(level);
        Saver.InitTrial(trial);
#endif
    }

    public void OpenRoadMap()
    {
        if (InputIsBlocked)
        {
            return;
        }

        InputIsBlocked = true;
        StartCoroutine(FadeOutAndSwitch());
    }

    public void PlaySession(int levelIndex)
    {
        if(InputIsBlocked)
        {
            return;
        }

        if (levelIndex < 1 || levelIndex > Levels.Length)
            return;

        if(levelIndex > Saver.MaxAvailableLevel())
        {
            GoToCoffeeScreen();
            return;
        }

        InputIsBlocked = true;
        Debug.Log("Play session " + levelIndex);
        ReleaseSession();
        var level = Levels[levelIndex - 1];
        var plan = new SessionFactory(count => Random.Range(0, count)).Create(level, levelIndex);
        ActiveSession = new SessionCoordinator(new Session(plan),
            new SessionImages(new AddressableSessionImageLoader()),
            new UnitySessionEffects(), () => Time.time);
        SessionView.Initialize(ActiveSession, level);
        StartCoroutine(PlayAfterHide());
        Background.ToggleColor(level.BackgroundColor);

        AudioController.HandleAudio(level.BackGroundAudio);
    }    

    private IEnumerator FadeOutAndSwitch()
    {        
        float duration = 0.5f; 
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, time / duration);
            MainMenuView.alpha = alpha;
            yield return null;
        }

        Background.ToggleColor();

        yield return new WaitForSeconds(0.5f);

        MainMenuView.alpha = 0f;
        MainMenuView.gameObject.SetActive(false);
        RoadMap.gameObject.SetActive(true);
        Buttons.gameObject.SetActive(true);
        InputIsBlocked = false;
    }

    private IEnumerator PlayAfterHide()
    {
        yield return StartCoroutine(routine: RoadMap.HideSequence());
        yield return new WaitForSeconds(1.5f);
        RoadMap.gameObject.SetActive(false);
        SessionView.gameObject.SetActive(true);
        InputIsBlocked = false;
    }

    public void GoToCoffeeScreen()
    {
        ReleaseSession();
        Background.ToggleColor(Color.white);
        RoadMap.gameObject.SetActive(false);
        SessionView.gameObject.SetActive(false);
        Buttons.gameObject.SetActive(false);
        CoffeeView.gameObject.SetActive(true);

        AudioController.Stop();        
    }

    public void BackToMenu()
    {
        ReleaseSession();
        Debug.Log("BackToMainMenu");
        Background.ToggleColor(Background.startColor);
        RoadMap.gameObject.SetActive(false);
        SessionView.gameObject.SetActive(false);
        Buttons.gameObject.SetActive(false);
        CoffeeView.gameObject.SetActive(false);
        MainMenuView.gameObject.SetActive(true);
        MainMenuView.alpha = 1;

        AudioController.Stop();
    }
        
    public void SwitchLanguage(int index)
    {
        Debug.Log("Change language to " + index);
        Root.ChangeLanguage(index);
        
        SessionView.ChangeLanguage();
        RoadMap.UpdateTexts();
    }

    public void ClearPrefs()
    {
        Saver.Clear();
    }

    private void ReleaseSession()
    {
        SessionView.ClearSession();
        ActiveSession?.Dispose();
        ActiveSession = null;
    }

    private void OnDestroy()
    {
        ActiveSession?.Dispose();
        ActiveSession = null;
    }

    //Entry Point
    private void Start()
    {
        Root.Initialize(Levels);
    }
    
    //Exit Point
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            Debug.Log("Data was saved");
            //Root.Save();
        }
    }

    private void OnApplicationQuit()
    {
        Debug.Log("Data was saved");
        //Root.Save();
    }
}

