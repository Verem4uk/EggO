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
    private GameObject MainGame;

    public void SwitchToMainMenu()
    {
        MainMenu.gameObject.SetActive(true);
        MainGame.gameObject.SetActive(false);
        if (StartScreen)
        {
            Destroy(StartScreen);
        }
    }

    public void OpenChoosePlayersScreen()
    {
        PlayersMenu.SetActive(true);
        MainMenu.SetActive(false);
    }

    public void OpenGameScreen()
    {
        MainGame.SetActive(true);
        PlayersMenu.SetActive(false);
    }

    public void Quit() => Application.Quit();
}
