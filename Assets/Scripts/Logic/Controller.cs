using System.Collections;
using UnityEngine;

public class Controller : MonoBehaviour
{
    [SerializeField] 
    private Level[] Questions;
    
    [SerializeField] 
    private SessionView SessionView;

    [SerializeField]
    private RoadMapScreen RoadMap;

    [SerializeField] 
    private CanvasGroup MainMenuView;
    
    [SerializeField]
    private ColorChanger Background;
        
    public void OpenRoadMap()
    {
        StartCoroutine(FadeOutAndSwitch());
    }
    public void PlaySession()
    {
        StartCoroutine(PlayAfterHide());
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
        SessionView.Initialize(Root.CurrentSession);
    }

    public void BackToMenu()
    {
        Background.ToggleColor(Color.black);
        RoadMap.gameObject.SetActive(false);
        SessionView.gameObject.SetActive(false);
        MainMenuView.gameObject.SetActive(true);
        MainMenuView.alpha = 1;
    }
        
    public void SwitchLanguage(int index)
    {
        Root.ChangeLanguage(index);
        if (SessionView.isActiveAndEnabled)
        {
            SessionView.ChangeLanguage();
        }        
    }

    //Entry Point
    private void Start()
    {
        Root.Initialize(Questions);
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

