using UnityEngine;
using UnityEngine.UI;

public class SessionView : MonoBehaviour
{
    [SerializeField] 
    private Text CurrentQuestionText;

    [SerializeField] 
    private Image[] Images;

    [SerializeField] 
    private NextButton NextButton;
    
    [SerializeField] 
    private GameObject ExitButton;
    
    [SerializeField] 
    private GameObject TrueExitButton;

    [SerializeField]
    private GridLayoutGroup ImageHolder;

    [SerializeField]
    private int MinCellSize = 250;

    [SerializeField]
    private int MidCellSize = 300;

    [SerializeField]
    private int MaxCellSize = 800;

    private Session Session;
    
    private IQuestion CurrentQuestion;
    
    public void Initialize(Session session, Level level)
    {
        Session = session;        
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Next();
        NextButton.UpdateButton(level.Icon, level.Audio);        
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
            var images = CurrentQuestion.GetImages();
            for(int i = 0; i < Images.Length; i++)
            {
                if(i < images.Length)
                {
                    Images[i].sprite = images[i];
                    Images[i].gameObject.SetActive(true);
                    continue;
                }                
                Images[i].gameObject.SetActive(false);
            }

            switch (images.Length)
            {
                case > 2:
                    ImageHolder.cellSize = new Vector2(MinCellSize, MinCellSize);
                    CurrentQuestionText.gameObject.SetActive(true);
                    break;
                case 1:
                    ImageHolder.cellSize = new Vector2(MaxCellSize, MaxCellSize);
                    CurrentQuestionText.gameObject.SetActive(false);
                    break;
                default:
                    ImageHolder.cellSize = new Vector2(MidCellSize, MidCellSize);
                    CurrentQuestionText.gameObject.SetActive(true);
                    break;
            }

            ImageHolder.gameObject.SetActive(true);
            return;
        }
        ImageHolder.gameObject.SetActive(false);
        CurrentQuestionText.gameObject.SetActive(true);
    }    
}
