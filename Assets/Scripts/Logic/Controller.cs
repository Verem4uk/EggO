using System.Collections;
using UnityEngine;

public class Controller : MonoBehaviour
{
    [SerializeField] 
    private Level[] Questions;
    
    [SerializeField] 
    private SessionView SessionView;
    
    [SerializeField] 
    private CanvasGroup MainMenuView;
    
    [SerializeField]
    private ColorChanger Background;

    public void Play()
    {
        StartCoroutine(FadeOutAndSwitch());
    }

    private IEnumerator FadeOutAndSwitch()
    {
        Background.ToggleColor();

        yield return new WaitForSeconds(0.5f);

        float duration = 0.5f; 
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, time / duration);
            MainMenuView.alpha = alpha;
            yield return null;
        }

        MainMenuView.alpha = 0f;
        MainMenuView.gameObject.SetActive(false); 
        SessionView.gameObject.SetActive(true);
        SessionView.Initialize(Root.CurrentSession);
    }

    public void BackToMenu()
    {
        SessionView.gameObject.SetActive(false);
        MainMenuView.gameObject.SetActive(true);
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
            Root.Save();
        }
    }

    private void OnApplicationQuit()
    {
        Debug.Log("Data was saved");
        Root.Save();
    }
}

