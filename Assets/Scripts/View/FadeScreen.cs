using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class FadeScreen : MonoBehaviour
{
    [SerializeField] private Image BlackScreen;
    [SerializeField, Min(0)] private float duration = 0.8f;
    private Tween Fade;

    private void Start() => FadeIn();

    public void FadeIn()
    {
        Fade?.Kill();
        BlackScreen.raycastTarget = false;
        Fade = BlackScreen.DOFade(0f, duration).SetEase(Ease.Linear).SetUpdate(true);
    }

    public void FadeOut()
    {
        Fade?.Kill();
        BlackScreen.raycastTarget = true;
        Fade = BlackScreen.DOFade(1f, duration).SetEase(Ease.Linear).SetUpdate(true)
            .OnComplete(Application.Quit);
    }

    private void OnDisable()
    {
        Fade?.Kill();
        Fade = null;
        BlackScreen.raycastTarget = false;
    }
}
