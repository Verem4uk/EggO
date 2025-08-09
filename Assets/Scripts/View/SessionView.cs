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
        if(CurrentQuestion == null)
        {
            ExitButton.GetComponent<Button>().onClick.Invoke();
            return;
        }
        CurrentQuestionText.text = CurrentQuestion.GetText();
        
        if (CurrentQuestion.HasImage())
        {
            ImageHolder.sprite = CurrentQuestion.GetImage();
            ImageHolder.gameObject.SetActive(true);
            return;
        }
        ImageHolder.gameObject.SetActive(false);
    }    
}
