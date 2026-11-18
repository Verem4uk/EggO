using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ColorChanger : MonoBehaviour
{
    public Image targetImage;
    public float duration = 0.5f;
    public Color customColor;
    public Color startColor;
    private Tween ColorTween;

    public void ToggleColor() => ToggleColor(customColor);

    public void ToggleColor(Color color)
    {
        ColorTween?.Kill();
        ColorTween = targetImage.DOColor(color, Mathf.Max(0f, duration))
            .SetEase(Ease.Linear).SetUpdate(true);
    }

    private void OnDisable()
    {
        ColorTween?.Kill();
        ColorTween = null;
    }
}
