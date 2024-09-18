using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class FadeScreen : MonoBehaviour
{
    [SerializeField] 
    private Image BlackScreen;

    private void Start()
    {
        FadeIn();
    }

    public void FadeIn()
    {
        DOTween.Sequence().Append(BlackScreen.DOFade(0, .8f));
    }

    public void FadeOut()
    {
        DOTween.Sequence().Append(BlackScreen.DOFade(1, .8f));
        Application.Quit();
    }
}
