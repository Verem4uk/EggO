using UnityEngine;
using UnityEngine.UI;

public class SessionView : MonoBehaviour
{
    [SerializeField] 
    private Text CurrentQuestionText;

    [SerializeField] 
    private Image[] ImageHolders;

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
        Debug.Log("Click text");
        CurrentQuestion = Session.GetQuestion();
        if(CurrentQuestion == null)
        {
            ExitButton.GetComponent<Button>().onClick.Invoke();
            return;
        }
        Debug.Log("Text "+ CurrentQuestion.GetText());
        CurrentQuestionText.text = CurrentQuestion.GetText();
        
        if (CurrentQuestion.HasImage())
        {
            var images = CurrentQuestion.GetImages();
            for(int i = 0; i < images.Length; i++)
            {
                ImageHolders[i].sprite = images[i];
            }

            ImageHolders[0].transform.parent.gameObject.SetActive(true);
            return;
        }
        ImageHolders[0].transform.parent.gameObject.SetActive(false);
    }    
}
