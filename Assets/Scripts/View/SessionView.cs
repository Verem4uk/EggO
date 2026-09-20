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
    private Button BackButton;

    [SerializeField]
    private Controller Controller;

    [SerializeField]
    private WanningPanelView WarningPanel;

    [SerializeField]
    private Material SmokeMaterial;

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

        NextButton.UpdateButton(level.Icon, level.ButtonSound);

        SmokeMaterial.SetColor("_EmissionColor", level.SmokeColor);

        CurrentQuestionText.color = level.TextColor;

        BackButton.interactable = false;
    }


    public void Back()
    {
        CurrentQuestion = Session.GetPreviousQuestion();

        if (CurrentQuestion == null)
        {
            BackButton.interactable = false;
            return;
        }

        DisplayQuestion(CurrentQuestion);

        BackButton.interactable = Session.CanGoBack();
    }


    public void ChangeLanguage()
    {
        if (CurrentQuestion != null)
        {
            CurrentQuestionText.text = CurrentQuestion.GetText();
        }
    }


    public void Next()
    {
        CurrentQuestion = Session.GetQuestion();

        if (CurrentQuestion == null)
        {
            Controller.GoToCoffeeScreen();
            return;
        }

        DisplayQuestion(CurrentQuestion);

        BackButton.interactable = Session.CanGoBack();
    }


    private void DisplayQuestion(IQuestion question)
    {
        if (question == null)
        {
            Controller.GoToCoffeeScreen();
            return;
        }

        var text = question.GetText();

        if (string.IsNullOrEmpty(text))
        {
            text = Session.GetCounterInfo();
        }

        CurrentQuestionText.text = text;

        if (question.HasImage())
        {
            var images = question.GetImages();

            for (int i = 0; i < Images.Length; i++)
            {
                if (i < images.Length)
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
                    ImageHolder.cellSize = new Vector2(
                        MinCellSize,
                        MinCellSize
                    );
                    break;

                case 1:
                    ImageHolder.cellSize = new Vector2(
                        MaxCellSize,
                        MaxCellSize
                    );
                    break;

                default:
                    ImageHolder.cellSize = new Vector2(
                        MidCellSize,
                        MidCellSize
                    );
                    break;
            }

            ImageHolder.gameObject.SetActive(true);
            return;
        }

        ImageHolder.gameObject.SetActive(false);
    }


    public void TryFinishSession()
    {
        if (Session.IsTheLastQuestion())
        {
            Controller.GoToCoffeeScreen();
            return;
        }

        WarningPanel.gameObject.SetActive(true);
    }


    public void FinishSession()
    {
        CurrentQuestion = Session.Interupt();

        if (CurrentQuestion == null)
        {
            Controller.GoToCoffeeScreen();
            return;
        }

        DisplayQuestion(CurrentQuestion);

        BackButton.interactable = Session.CanGoBack();
    }
}