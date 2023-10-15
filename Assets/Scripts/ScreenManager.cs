using System.Collections.Generic;
using UnityEngine;

public class ScreenManager : MonoBehaviour
{
    [SerializeField] 
    private GameObject StartScreen;
    
    [SerializeField] 
    private GameObject MainMenu;

    [SerializeField] 
    private GameObject PlayersMenu;

    [SerializeField] 
    private GameCore MainGame;

    [SerializeField]
    private List<TranslatableText> TranslatableTexts;

    public void StartApplication()
    {
        if (StartScreen)
        {
            Destroy(StartScreen);
        }
        ChangeLanguage(Saver.Instance.GetLanguageIdentifier());
        if (Saver.Instance.HasUnfinishedSession())
        {
            MainGame.InitializeFromSave(Saver.Instance.Load());
            OpenGameScreen();
            return;
        }
        SwitchToMainMenu();
    }

    public void SwitchToMainMenu()
    {
        MainMenu.gameObject.SetActive(true);
        MainGame.gameObject.SetActive(false);
    }

    public void OpenChoosePlayersScreen()
    {
        PlayersMenu.SetActive(true);
        MainMenu.SetActive(false);
    }

    public void OpenGameScreen()
    {
        MainGame.gameObject.SetActive(true);
        PlayersMenu.SetActive(false);
    }

    public void ChangeLanguage(int position)
    {
        Localization.Instance.ChangeLanguage(position);
        foreach (var translatableText in TranslatableTexts)
        {
            translatableText.ApplyLanguage();
        }
    }

    public void Quit() => Application.Quit();
}
