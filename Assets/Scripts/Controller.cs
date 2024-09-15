using UnityEngine;

public class Controller : MonoBehaviour
{
    [SerializeField]
    public QuestionsPack BaseQuestions;
    
    [SerializeField] 
    private SessionView SessionView;
    
    [SerializeField] 
    private GameObject MainMenuView;

    private bool UnfinishedSession;

    public void Play()
    {
        MainMenuView.SetActive(false);
        SessionView.gameObject.SetActive(true);
        SessionView.Initialize(Root.CurrentSession, UnfinishedSession);
    }

    public void BackToMenu()
    {
        SessionView.gameObject.SetActive(false);
        MainMenuView.SetActive(true);
    }

    //Entry Point
    private void Start()
    {
        Root.Initialize(BaseQuestions);
        if (UnfinishedSession)
        {
            Play();
        }
    }

    //Exit Point
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            Root.Save();
            UnfinishedSession = true;
        }
        else
        {
            UnfinishedSession = false;
        }
    }
}

