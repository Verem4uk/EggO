using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class StartAnimation : MonoBehaviour
{
    [SerializeField]
    private RectTransform Egg;

    [SerializeField] 
    private Image BlackScreen;
    
    [SerializeField] 
    private ScreenManager ScreenManager;
    
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

    [SerializeField] 
    private bool AutoClick;

    [SerializeField]
    private float AutoClickDelay = 1f;

    private int CounterOfClicks;
    private float IncrementScale;
    private bool InAnimation;
    private Vector2 OriginalScale;
/*
    private void Start()
    {
        if (AutoClick)
        {
            StartCoroutine(AutoClickCoroutine());
        }
    }

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
/*
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
            .OnComplete(ScreenManager.StartApplication);
    }

    private IEnumerator AutoClickCoroutine()
    {
        while (CounterOfClicks < NumberOfClick)
        {
            yield return new WaitForSeconds(AutoClickDelay);
            Click();
        }
    }
    */
}
