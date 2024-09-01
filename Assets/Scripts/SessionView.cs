using UnityEngine;
using UnityEngine.UI;

public class SessionView : MonoBehaviour
{
    [SerializeField] 
    private Text CurrentQuestionText;
    
    [SerializeField] 
    private Dropdown LanguageDropdown;

    private Session Session;
    
    public void Initialize(Session session, bool pausedSession)
    {
        Session = session;
        CurrentQuestionText.text = pausedSession ? 
            session.GetCurrentQuestion().RussianText : session.GetRandomQuestion().RussianText;
    }
        
    public void Next()
    {
        CurrentQuestionText.text = Session.GetRandomQuestion().RussianText;
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
