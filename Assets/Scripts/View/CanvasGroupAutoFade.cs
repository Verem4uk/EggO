using System.Collections;
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

    [Header("Images to fade AFTER show / BEFORE hide")]
    [SerializeField] private Image[] images;

    private CanvasGroup cg;
    private Coroutine currentRoutine;

    private float Delta => useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

    void Awake()
    {
        cg = GetComponent<CanvasGroup>();

        // Стартуем скрытыми
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;

        // Картинки оставляем активными, но прозрачными (чтобы их можно было плавно показать)
        SetImagesAlpha(0f, onlyIfActive: false);
    }

    void OnEnable()
    {
        Show(); // автопоказ
    }

    /// <summary>Плавно показать: CanvasGroup -> Images.</summary>
    public void Show(bool immediate = false)
    {
        if (immediate)
        {
            StopFade();
            cg.alpha = 1f;
            cg.interactable = true;
            cg.blocksRaycasts = true;
            SetImagesAlpha(1f, onlyIfActive: false);
            return;
        }

        StopFade();
        currentRoutine = StartCoroutine(ShowRoutine());
    }

    /// <summary>Спрятать: Images -> CanvasGroup -> SetActive(false).</summary>
    public void HideAndDeactivate(bool immediate = false)
    {
        if (!gameObject.activeInHierarchy) return;

        if (immediate)
        {
            StopFade();
            SetImagesAlpha(0f, onlyIfActive: false);
            cg.alpha = 0f;
            cg.interactable = false;
            cg.blocksRaycasts = false;
            gameObject.SetActive(false);
            return;
        }

        StopFade();
        currentRoutine = StartCoroutine(HideRoutine());
    }

    // ---------- Routines ----------

    private IEnumerator ShowRoutine()
    {
        // CanvasGroup IN
        cg.interactable = false;
        cg.blocksRaycasts = false;
        yield return FadeCanvasGroup(1f, fadeInDuration);

        // Активируем интерактив после полного появления
        cg.interactable = true;
        cg.blocksRaycasts = true;

        // Images IN (после CanvasGroup)
        yield return FadeImages(1f, imagesFadeInDuration);

        currentRoutine = null;
    }

    private IEnumerator HideRoutine()
    {
        // Сначала Images OUT
        yield return FadeImages(0f, imagesFadeOutDuration);

        // Отключаем интерактив сразу
        cg.interactable = false;
        cg.blocksRaycasts = false;

        // Затем CanvasGroup OUT
        yield return FadeCanvasGroup(0f, fadeOutDuration);

        gameObject.SetActive(false);
        currentRoutine = null;
    }

    // ---------- Helpers ----------

    private IEnumerator FadeCanvasGroup(float target, float duration)
    {
        float start = cg.alpha;
        float t = 0f;
        duration = Mathf.Max(0.0001f, duration);

        while (t < duration)
        {
            t += Delta;
            cg.alpha = Mathf.Lerp(start, target, t / duration);
            yield return null;
        }
        cg.alpha = target;
    }

    private IEnumerator FadeImages(float targetAlpha, float duration)
    {
        if (images == null || images.Length == 0) yield break;

        // Снимем начальные альфы
        float[] startAlphas = new float[images.Length];
        for (int i = 0; i < images.Length; i++)
        {
            if (images[i] == null) continue;
            startAlphas[i] = images[i].color.a;
            // гарантируем, что объекты активны для анимации
            if (!images[i].gameObject.activeSelf) images[i].gameObject.SetActive(true);
        }

        float t = 0f;
        duration = Mathf.Max(0.0001f, duration);

        while (t < duration)
        {
            t += Delta;
            float a = Mathf.Lerp(0f, 1f, t / duration); // нормализованный прогресс
            for (int i = 0; i < images.Length; i++)
            {
                var img = images[i];
                if (img == null) continue;
                float from = startAlphas[i];
                float to = targetAlpha;
                float cur = Mathf.Lerp(from, to, a);
                var c = img.color; c.a = cur; img.color = c;
            }
            yield return null;
        }

        // финальные значения
        for (int i = 0; i < images.Length; i++)
        {
            var img = images[i];
            if (img == null) continue;
            var c = img.color; c.a = targetAlpha; img.color = c;

            // Если полностью скрыли — можно выключить объект (не обязательно, но экономит отрисовку)
            if (Mathf.Approximately(targetAlpha, 0f))
                img.gameObject.SetActive(false);
        }
    }

    private void SetImagesAlpha(float alpha, bool onlyIfActive)
    {
        if (images == null) return;
        for (int i = 0; i < images.Length; i++)
        {
            var img = images[i];
            if (img == null) continue;
            if (onlyIfActive && !img.gameObject.activeSelf) continue;

            // Делаем активным при показе, при скрытии можно оставить активным (или выключать в FadeImages)
            if (alpha > 0f && !img.gameObject.activeSelf)
                img.gameObject.SetActive(true);

            var c = img.color; c.a = alpha; img.color = c;

            if (alpha == 0f && img.gameObject.activeSelf)
                img.gameObject.SetActive(false);
        }
    }

    private void StopFade()
    {
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
            currentRoutine = null;
        }
    }
}
