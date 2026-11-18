using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class SessionView : MonoBehaviour
{
    [SerializeField] private Text CurrentQuestionText;
    [SerializeField] private Image[] Images;
    [SerializeField] private NextButton NextButton;
    [SerializeField] private Button BackButton;
    [SerializeField] private Controller Controller;
    [SerializeField] private WanningPanelView WarningPanel;
    [SerializeField] private Material SmokeMaterial;
    [SerializeField] private GridLayoutGroup ImageHolder;
    [SerializeField] private int MinCellSize = 250;
    [SerializeField] private int MidCellSize = 300;
    [SerializeField] private int MaxCellSize = 800;

    private SessionCoordinator Coordinator;
    private Session Session => Coordinator?.Session;
    private IQuestion CurrentQuestion;
    private int DisplayVersion;
    private bool ImagesLoading;
    private bool ImageLoadFailed;

    public void Initialize(SessionCoordinator coordinator, Level level)
    {
        ClearSession();
        Coordinator = coordinator;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        NextButton.UpdateButton(level.Icon, level.ButtonSound);
        SmokeMaterial.SetColor("_EmissionColor", level.SmokeColor);
        CurrentQuestionText.color = level.TextColor;
        Next();
    }

    public void ClearSession()
    {
        DisplayVersion++;
        Coordinator = null;
        CurrentQuestion = null;
        ImagesLoading = false;
        ImageLoadFailed = false;
        ClearImages();
        WarningPanel.gameObject.SetActive(false);
        NextButton.SetInteractable(true);
        BackButton.interactable = false;
        Screen.sleepTimeout = SleepTimeout.SystemSetting;
    }

    public void Back()
    {
        if (Session == null || !Session.CanGoBack()) return;
        CurrentQuestion = Session.GetPreviousQuestion();
        DisplayQuestion();
    }

    public void ChangeLanguage()
    {
        if (CurrentQuestion != null) DisplayQuestion();
    }

    public void Next()
    {
        if (Session == null || ImagesLoading) return;
        if (ImageLoadFailed)
        {
            DisplayQuestion(retry: true);
            return;
        }

        CurrentQuestion = Session.GetQuestion();
        if (CurrentQuestion == null)
        {
            Controller.GoToCoffeeScreen();
            return;
        }
        DisplayQuestion();
    }

    private void DisplayQuestion(bool retry = false)
    {
        // Async exceptions are handled inside this operation; UI entry points stay synchronous.
        _ = DisplayQuestionAsync(++DisplayVersion, Coordinator, CurrentQuestion, retry);
    }

    private async Task DisplayQuestionAsync(int version, SessionCoordinator coordinator,
        IQuestion question, bool retry)
    {
        if (coordinator == null || question == null) return;
        ClearImages();
        ImageLoadFailed = false;
        ImagesLoading = question.HasImage();
        NextButton.SetInteractable(!ImagesLoading);
        BackButton.interactable = coordinator.Session.CanGoBack();
        UpdateQuestionText();

        try
        {
            if (ImagesLoading)
            {
                var result = await coordinator.Images.GetAsync(question, retry);
                if (version != DisplayVersion || coordinator != Coordinator) return;
                ImagesLoading = false;
                if (result.IsCancelled) return;
                if (!result.IsSuccess)
                {
                    ImageLoadFailed = true;
                    Debug.LogWarning("Question images could not be loaded: " + result.Error.Message);
                }
                else
                    ShowImages(result.Sprites);
            }

            if (version != DisplayVersion || coordinator != Coordinator) return;
            NextButton.SetInteractable(true);
            UpdateQuestionText();
            if (!ImageLoadFailed) coordinator.Session.ConfirmCurrentQuestion();
        }
        catch (Exception error)
        {
            if (version != DisplayVersion || coordinator != Coordinator) return;
            ImagesLoading = false;
            ImageLoadFailed = true;
            NextButton.SetInteractable(true);
            UpdateQuestionText();
            Debug.LogException(error);
        }
    }

    private void UpdateQuestionText()
    {
        var text = CurrentQuestion?.GetText();
        if (string.IsNullOrEmpty(text)) text = Session?.GetCounterInfo() ?? "";
        if (ImagesLoading) text += "\n" + LoadingText();
        else if (ImageLoadFailed) text += "\n" + RetryText();
        CurrentQuestionText.text = text;
    }

    private static string LoadingText() => Root.GetLanguageIndex() switch
    {
        1 => "Ładowanie obrazów…",
        2 => "Загрузка изображений…",
        _ => "Loading images…"
    };

    private static string RetryText() => Root.GetLanguageIndex() switch
    {
        1 => "Nie udało się załadować obrazów. Naciśnij Dalej, aby spróbować ponownie.",
        2 => "Не удалось загрузить изображения. Нажмите «Далее», чтобы повторить.",
        _ => "Images could not be loaded. Press Next to retry."
    };

    private void ClearImages()
    {
        foreach (var image in Images)
        {
            image.sprite = null;
            image.gameObject.SetActive(false);
        }
        ImageHolder.gameObject.SetActive(false);
    }

    private void ShowImages(IReadOnlyList<Sprite> sprites)
    {
        for (int i = 0; i < Images.Length; i++)
        {
            if (i >= sprites.Count) continue;
            Images[i].sprite = sprites[i];
            Images[i].gameObject.SetActive(true);
        }

        int size = sprites.Count > 2 ? MinCellSize : sprites.Count == 1 ? MaxCellSize : MidCellSize;
        ImageHolder.cellSize = new Vector2(size, size);
        ImageHolder.gameObject.SetActive(true);
    }

    public void TryFinishSession()
    {
        if (Session == null) return;
        if (Session.IsTheLastQuestion())
        {
            Controller.GoToCoffeeScreen();
            return;
        }
        if (Session.IsFinishing) return;
        WarningPanel.gameObject.SetActive(true);
    }

    public void FinishSession()
    {
        if (Session == null) return;
        WarningPanel.gameObject.SetActive(false);
        CurrentQuestion = Session.Interupt();
        if (CurrentQuestion == null)
        {
            Controller.GoToCoffeeScreen();
            return;
        }
        DisplayQuestion();
    }

    private void OnDestroy()
    {
        DisplayVersion++;
        Coordinator = null;
    }
}
