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
    private CanvasGroup MainMenuView;
    
    [SerializeField]
    private ColorChanger Background;

    [SerializeField]
    private AudioController AudioController;    

    public void OpenRoadMap()
    {
        StartCoroutine(FadeOutAndSwitch());
    }

    public void PlaySession(int levelIndex)
    {
        Debug.Log("Play session " + levelIndex);
        StartCoroutine(PlayAfterHide());        
        var level = Root.Levels[--levelIndex]; 
        SessionView.Initialize(new Session(++levelIndex), level);

        AudioController.HandleAudio(level);
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
    }

    private IEnumerator PlayAfterHide()
    {
        yield return StartCoroutine(routine: RoadMap.HideSequence());
        yield return new WaitForSeconds(0.5f);
        RoadMap.gameObject.SetActive(false);
        SessionView.gameObject.SetActive(true);        
    }

    public void BackToMenu()
    {
        Debug.Log("BackToMainMenu");
        Background.ToggleColor(Color.black);
        RoadMap.gameObject.SetActive(false);
        SessionView.gameObject.SetActive(false);
        MainMenuView.gameObject.SetActive(true);
        MainMenuView.alpha = 1;

        AudioController.SetDefault();
    }
        
    public void SwitchLanguage(int index)
    {
        Root.ChangeLanguage(index);
        if (SessionView.isActiveAndEnabled)
        {
            SessionView.ChangeLanguage();
        }        
    }

    public void ClearPrefs()
    {
        Saver.Clear();
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

