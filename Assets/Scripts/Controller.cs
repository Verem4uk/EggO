using UnityEngine;

public class Controller : MonoBehaviour
{
    [SerializeField] 
    private QuestionSet QuestionSet;
    
    [SerializeField] 
    private SessionView SessionView;
    
    [SerializeField] 
    private GameObject MainMenuView;

    public void Play()
    {
        MainMenuView.SetActive(false);
        SessionView.gameObject.SetActive(true);
        SessionView.Initialize(Root.CurrentSession);
    }

    public void BackToMenu()
    {
        SessionView.gameObject.SetActive(false);
        MainMenuView.SetActive(true);
    }

    public void SwitchLanguage(int index)
    {
        Root.ChangeLanguage(index);
        SessionView.ChangeLanguage();
    }

    //Entry Point
    private void Start()
    {
        Root.Initialize(QuestionSet);
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
}

