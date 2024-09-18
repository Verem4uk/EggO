using UnityEngine;

public class Controller : MonoBehaviour
{
    [SerializeField]
    public QuestionsPack BaseQuestions;

    [SerializeField] 
    public ImagesQuestionsPack ImagesQuestions;

    [SerializeField] 
    private int ImageProbability = 5;

    [SerializeField] 
    private Question LastQuestion;
    
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
        Root.Initialize(BaseQuestions, ImagesQuestions, LastQuestion, ImageProbability);
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

