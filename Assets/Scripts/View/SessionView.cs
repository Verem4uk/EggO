using UnityEngine;
using UnityEngine.UI;

public class SessionView : MonoBehaviour
{
    [SerializeField] 
    private Text CurrentQuestionText;

    [SerializeField] 
    private Image[] Images;

    [SerializeField] 
    private GameObject NextButton;
    
    [SerializeField] 
    private GameObject ExitButton;
    
    [SerializeField] 
    private GameObject TrueExitButton;

    [SerializeField]
    private GridLayoutGroup ImageHolder;

    [SerializeField]
    private int MinCellSize = 250;

    [SerializeField]
    private int MaxCellSize = 300;

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

            if(images.Length > 2)
            {
                ImageHolder.cellSize = new Vector2(MinCellSize, MinCellSize);
            }
            else
            {
                ImageHolder.cellSize = new Vector2(MaxCellSize, MaxCellSize);
            }

            ImageHolder.gameObject.SetActive(true);
            return;
        }
        ImageHolder.gameObject.SetActive(false);
    }    
}
