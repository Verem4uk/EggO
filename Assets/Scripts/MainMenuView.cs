using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuView : MonoBehaviour
{
    [SerializeField] 
    private Image BlackScreen;

    public void ShowMainMenu()
    {
        AppearAnimation();
    }
    
    private void AppearAnimation()
    {
        DOTween.Sequence().Append(BlackScreen.DOFade(0, .8f));
    }
}
