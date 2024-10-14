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
    private bool InsideImageQuestion;

    private IQuestion CurrentQuestion;
    
    public void Initialize(Session session)
    {
        Session = session;
        CurrentQuestion = session.GetRandomQuestion();
        CurrentQuestionText.text = CurrentQuestion.GetText();
    }

    public void ChangeLanguage()
    {
        CurrentQuestionText.text = CurrentQuestion.GetText();
    }
        
    public void Next()
    {
        if (InsideImageQuestion)
        {
            InsideImageQuestion = false;
            ImageHolder.gameObject.SetActive(true);
            CurrentQuestion = Root.Questions.QuestionAfterImage;
            CurrentQuestionText.text = CurrentQuestion.GetText();
            return;
        }
        
        CurrentQuestion = Session.GetRandomQuestion();
        CurrentQuestionText.text = CurrentQuestion.GetText();
        
        if (CurrentQuestion is IImageQuestion imageQuestion)
        {
            ImageHolder.sprite = imageQuestion.GetImage();
            CurrentQuestionText.text = imageQuestion.GetText();
            InsideImageQuestion = true;
            return;
        }
        ImageHolder.gameObject.SetActive(false);
    }

    public void Exit()
    {
        ImageHolder.gameObject.SetActive(false);
        CurrentQuestion = Root.Questions.LastQuestionInSession;
        CurrentQuestionText.text = CurrentQuestion.GetText();
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
