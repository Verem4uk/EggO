using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class FadeScreen : MonoBehaviour
{
    private void Start()
    {
        DOTween.Sequence().Append(gameObject.GetComponent<Image>().DOFade(0, .8f))
            .OnComplete(() => Destroy(gameObject));
    }
}
