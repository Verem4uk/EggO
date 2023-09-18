using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class StartAnimation : MonoBehaviour
{
    [SerializeField]
    private RectTransform Egg;

    [SerializeField] 
    private Image BlackScreen;
    
    [SerializeField] 
    private MainMenuView MainMenu;
    
    [SerializeField]
    private int NumberOfClick;

    [SerializeField] 
    private float ScaleMultiple = 20f;

    [SerializeField]
    private float AnimationSpeed;

    [SerializeField]
    private float PowerOfImpuls = 10f;

    [SerializeField] 
    private int FinalScale = 5;

    private int CounterOfClicks;
    private float IncrementScale;
    private bool InAnimation;
    private Vector2 OriginalScale;
    
    public void Click()
    {
        if (InAnimation)
        {
            return;
        }
        
        InAnimation = true;
        CounterOfClicks++;
        OriginalScale = Egg.localScale;
        
        if (CounterOfClicks == NumberOfClick)
        {
            LastAnimation();
            return;
        }
        Animate();
    }

    private void Animate()
    {
        var bigOffset = 1 + (ScaleMultiple + PowerOfImpuls) / 100;
        var offset = 1 + ScaleMultiple / 100;
        var bigScale = new Vector3(OriginalScale.x * bigOffset, OriginalScale.y * bigOffset);
        var targetScale = new Vector3(OriginalScale.x * offset, OriginalScale.y * offset);
        var sequence = DOTween.Sequence().Append(Egg.DOScale(bigScale, AnimationSpeed))
            .Append(Egg.DOScale(targetScale, AnimationSpeed));
        sequence.OnComplete(() => InAnimation = false);
    }

    private void LastAnimation()
    {
        DOTween.Sequence().Append(Egg.DOScale(FinalScale, AnimationSpeed))
            .Join(BlackScreen.DOFade(1, .8f))
            .OnComplete(SwitchToMainMenu);
    }

    private void SwitchToMainMenu()
    {
        MainMenu.gameObject.SetActive(true);
        MainMenu.ShowMainMenu();
        Destroy(transform.parent);
    }
}
