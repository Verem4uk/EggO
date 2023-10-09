using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuView : MonoBehaviour
{
    [SerializeField] 
    private Image BlackScreen;
    
    private void Start() => DOTween.Sequence().Append(BlackScreen.DOFade(0, .8f));
}
