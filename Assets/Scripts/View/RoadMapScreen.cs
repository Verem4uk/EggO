using DG.Tweening;
using TMPro;
using UnityEngine;

public class RoadMapScreen : MonoBehaviour
{
    [SerializeField] private CanvasGroupAutoFadeWithImages Canvas;
    public PointMap[] pointsMap;
    [SerializeField] private TranslatableText[] translatableTexts;
    [SerializeField] private TextMeshProUGUI EggOText;
    public float delayBeforeStart = 0.5f;
    [SerializeField, Min(0)] private float pointHideStagger = 0.2f;

    private void Awake() => ConfigureFade();
    private void OnEnable() => UpdateTexts();

    public void UpdateTexts()
    {
        foreach (var text in translatableTexts) text.Actualize();
    }

    // The Controller owns the entire sequence, including every point's animation.
    public Sequence ShowSequence(int completedLevel)
    {
        ConfigureFade();
        Canvas.SetVisibleImmediate(false);
        var color = EggOText.color;
        color.a = 0f;
        EggOText.color = color;
        var sequence = DOTween.Sequence().Pause();
        sequence.Join(Canvas.ShowSequence(enableInput: false));
        float cursor = Mathf.Max(0f, delayBeforeStart);
        sequence.Insert(cursor, EggOText.DOFade(1f, 0.5f).SetEase(Ease.Linear));
        foreach (var point in pointsMap)
        {
            var show = point.ShowSequence(completedLevel);
            sequence.Insert(cursor, show);
            cursor += show.Duration();
        }
        foreach (var point in pointsMap)
        {
            var unlock = point.UnlockSequence(completedLevel);
            // DOTween does not insert empty sequences.
            float duration = unlock.Duration();
            if (duration > 0f) sequence.Insert(cursor, unlock);
            else unlock.Kill();
            cursor += duration;
        }
        return sequence;
    }

    private void ConfigureFade()
    {
        Canvas.SetManagedExternally();
        foreach (var point in pointsMap) Canvas.ExcludeImagesUnder(point.transform);
    }

    public Sequence HideSequence()
    {
        Canvas.SetInput(false);
        var sequence = DOTween.Sequence().Pause();
        for (int i = 0; i < pointsMap.Length; i++)
            sequence.Insert(i * pointHideStagger, pointsMap[i].HideSequence());
        sequence.Insert(0f, EggOText.DOFade(0f, 0.5f).SetEase(Ease.Linear));
        sequence.Append(Canvas.HideSequence());
        return sequence;
    }

    public void SetInput(bool enabled)
    {
        if (Canvas != null) Canvas.SetInput(enabled);
    }

    public void ResetView()
    {
        foreach (var point in pointsMap)
            if (point != null) point.ResetEggO();
    }

    private void OnDisable() => ResetView();
}
