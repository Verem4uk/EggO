using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class CanvasGroupAutoFadeWithImages : MonoBehaviour
{
    [Header("CanvasGroup Timings")]
    [SerializeField] private float fadeInDuration = 0.35f;
    [SerializeField] private float fadeOutDuration = 0.35f;
    [Header("Images Timings")]
    [SerializeField] private float imagesFadeInDuration = 0.25f;
    [SerializeField] private float imagesFadeOutDuration = 0.25f;
    [SerializeField] private bool useUnscaledTime = false;
    [Header("Local images to fade after show / before hide")]
    [SerializeField] private Image[] images;

    private CanvasGroup Group;
    private Tween AutomaticFade;
    private bool ManagedExternally;
    private readonly HashSet<Transform> ExcludedSubtrees = new HashSet<Transform>();

    public void ExcludeImagesUnder(Transform root) => ExcludedSubtrees.Add(root);

    private void Awake()
    {
        EnsureGroup();
        SetVisibleImmediate(false);
    }

    private void OnEnable()
    {
        if (!ManagedExternally) Show();
    }

    public void SetManagedExternally()
    {
        ManagedExternally = true;
        CancelAutomaticFade();
    }

    private bool EnsureGroup()
    {
        // A Unity wrapper can survive destruction of its native component.
        if (this == null) return false;
        if (Group == null) Group = GetComponent<CanvasGroup>();
        return Group != null;
    }

    public void SetInput(bool enabled)
    {
        if (!EnsureGroup()) return;
        Group.interactable = enabled;
        Group.blocksRaycasts = enabled;
    }

    public void SetVisibleImmediate(bool visible, bool input = true)
    {
        CancelAutomaticFade();
        if (!EnsureGroup()) return;
        Group.alpha = visible ? 1f : 0f;
        SetInput(visible && input);
        foreach (var image in images ?? System.Array.Empty<Image>())
            if (IsLocal(image)) SetAlpha(image, Group.alpha);
    }

    // The caller owns these sequences, including cancellation and deactivation.
    public Sequence ShowSequence(bool enableInput = true)
    {
        CancelAutomaticFade();
        SetInput(false);
        var sequence = DOTween.Sequence().Pause().SetUpdate(useUnscaledTime);
        sequence.Append(Group.DOFade(1f, Mathf.Max(0, fadeInDuration)).SetEase(Ease.Linear));
        AppendImages(sequence, 1f, imagesFadeInDuration);
        sequence.AppendCallback(() => SetInput(enableInput));
        return sequence;
    }

    public Sequence HideSequence()
    {
        CancelAutomaticFade();
        SetInput(false);
        var sequence = DOTween.Sequence().Pause().SetUpdate(useUnscaledTime);
        AppendImages(sequence, 0f, imagesFadeOutDuration);
        sequence.Append(Group.DOFade(0f, Mathf.Max(0, fadeOutDuration)).SetEase(Ease.Linear));
        return sequence;
    }

    public void Show(bool immediate = false)
    {
        if (immediate) { SetVisibleImmediate(true); return; }
        AutomaticFade = ShowSequence().Play();
    }

    public void HideAndDeactivate(bool immediate = false)
    {
        if (immediate)
        {
            SetVisibleImmediate(false);
            gameObject.SetActive(false);
            return;
        }
        if (!gameObject.activeInHierarchy) return;
        AutomaticFade = HideSequence().OnComplete(() => gameObject.SetActive(false)).Play();
    }

    private void AppendImages(Sequence sequence, float alpha, float duration)
    {
        float start = sequence.Duration();
        foreach (var image in images ?? System.Array.Empty<Image>())
            if (IsLocal(image))
                sequence.Insert(start, image.DOFade(alpha, Mathf.Max(0, duration)).SetEase(Ease.Linear));
    }

    private bool IsLocal(Image image)
    {
        if (image == null || !image.transform.IsChildOf(transform)) return false;
        foreach (var root in ExcludedSubtrees)
            if (root != null && image.transform.IsChildOf(root)) return false;
        return true;
    }

    private static void SetAlpha(Graphic image, float alpha)
    {
        var color = image.color;
        color.a = alpha;
        image.color = color;
    }

    private void CancelAutomaticFade()
    {
        AutomaticFade?.Kill();
        AutomaticFade = null;
    }

    private void OnDisable()
    {
        SetVisibleImmediate(false);
    }
}
