using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PointMap : MonoBehaviour
{
    [SerializeField] private int ID;
    [SerializeField] private Button ActiveEggO;
    [SerializeField] private Image PastEggO;
    [SerializeField] private TextMeshProUGUI Text;
    [SerializeField] private Pulsation Pulsation;
    [SerializeField, Min(0)] private float fadeDuration = 0.5f;

    // Some existing prefabs intentionally leave Button.targetGraphic unset.
    private Image ActiveImage => ActiveEggO.image != null
        ? ActiveEggO.image : ActiveEggO.GetComponent<Image>();

    public Sequence ShowSequence(int completedLevel)
    {
        ResetEggO();
        bool completed = ID <= completedLevel;
        ActiveEggO.gameObject.SetActive(true);
        ActiveImage.enabled = !completed || PastEggO == null;
        if (PastEggO != null) PastEggO.gameObject.SetActive(completed);
        Text.gameObject.SetActive(completed);

        var sequence = DOTween.Sequence().Pause();
        sequence.Append((completed && PastEggO != null ? PastEggO : ActiveImage)
            .DOFade(1f, fadeDuration).SetEase(Ease.Linear));
        if (completed) sequence.Append(Text.DOFade(1f, fadeDuration).SetEase(Ease.Linear));
        return sequence;
    }

    public Sequence UnlockSequence(int completedLevel)
    {
        var sequence = DOTween.Sequence().Pause();
        if (ID != completedLevel + 1) return sequence;
        sequence.AppendCallback(() => Text.gameObject.SetActive(true));
        sequence.Append(Text.DOFade(1f, fadeDuration).SetEase(Ease.Linear));
        sequence.AppendCallback(() =>
        {
            ActiveEggO.interactable = true;
            Pulsation.enabled = true;
        });
        return sequence;
    }

    public Sequence HideSequence()
    {
        ActiveEggO.interactable = false;
        Pulsation.enabled = false;
        var sequence = DOTween.Sequence().Pause();
        var image = PastEggO != null && PastEggO.gameObject.activeSelf ? PastEggO : ActiveImage;
        sequence.Append(image.DOFade(0f, fadeDuration).SetEase(Ease.Linear));
        if (Text.gameObject.activeSelf)
            sequence.Append(Text.DOFade(0f, fadeDuration).SetEase(Ease.Linear));
        return sequence;
    }

    public void ResetEggO()
    {
        Pulsation.enabled = false;
        ActiveEggO.interactable = false;
        Text.gameObject.SetActive(false);
        SetAlpha(ActiveImage, 0f);
        SetAlpha(PastEggO, 0f);
        SetAlpha(Text, 0f);
    }

    // Retained for the existing prefab's OnClick binding. The screen owns fading.
    public void Disable()
    {
        ActiveEggO.interactable = false;
        Pulsation.enabled = false;
    }

    private static void SetAlpha(Graphic graphic, float alpha)
    {
        if (graphic == null) return;
        var color = graphic.color;
        color.a = alpha;
        graphic.color = color;
    }

    private void OnDisable() => ResetEggO();
}
