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

    [SerializeField] 
    private Dropdown LanguageDropdown;

    private Session Session;
    
    public void Initialize(Session session, bool pausedSession)
    {
        Session = session;
        if (session == null)
        {
            Debug.Log("Session is null");
        }
        CurrentQuestionText.text = pausedSession ? 
            session.GetCurrentQuestion().GetText() : session.GetRandomQuestion().GetText();
    }
        
    public void Next()
    {
        var newQuestion = Session.GetRandomQuestion();
        CurrentQuestionText.text = newQuestion.GetText();
        if (newQuestion is IImageQuestion imageQuestion)
        {
            ImageHolder.sprite = imageQuestion.GetImage();
            ImageHolder.gameObject.SetActive(true);
            return;
        }
        ImageHolder.gameObject.SetActive(false);
    }

    public void Exit()
    {
        ImageHolder.gameObject.SetActive(false);
        CurrentQuestionText.text = Root.LastQuestion.GetText();
        NextButton.gameObject.SetActive(false);
        ExitButton.gameObject.SetActive(false);
        TrueExitButton.gameObject.SetActive(true);
    }

    /*
    public void ChangeLanguage(int position)
    {
        screenView.ChangeLanguage(position);
        CurrentQuestionText.text = 
            Localization.Instance.GetTextAccordingLanguage(GameProcess.CurrentQuestion.GetTextTranslations());
    }
    */
}
