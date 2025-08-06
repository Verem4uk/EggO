using UnityEngine;
using UnityEngine.UI;

public class SessionView : MonoBehaviour
{
    [SerializeField] 
    private Text CurrentQuestionText;

    [SerializeField] 
    private Image ImageHolder;

    [SerializeField] 
    private GameObject NextButton;
    
    [SerializeField] 
    private GameObject ExitButton;
    
    [SerializeField] 
    private GameObject TrueExitButton;

    private Session Session;
    
    private IQuestion CurrentQuestion;
    
    public void Initialize(Session session)
    {
        Session = session;        
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Next();
    }

    public void ChangeLanguage()
    {
        CurrentQuestionText.text = CurrentQuestion.GetText();
    }
        
    public void Next()
    {               
        CurrentQuestion = Session.GetQuestion();
        CurrentQuestionText.text = CurrentQuestion.GetText();
        
        if (CurrentQuestion.HasImage())
        {
            ImageHolder.sprite = CurrentQuestion.GetImage();
            CurrentQuestionText.text = CurrentQuestion.GetText();
            return;
        }
        ImageHolder.gameObject.SetActive(false);
    }

    public void Exit()
    {
        ImageHolder.gameObject.SetActive(false);
        CurrentQuestionText.text = CurrentQuestion.GetText();
        NextButton.gameObject.SetActive(false);
        ExitButton.gameObject.SetActive(false);
        TrueExitButton.gameObject.SetActive(true);
        Screen.sleepTimeout = SleepTimeout.SystemSetting;
    }
}
