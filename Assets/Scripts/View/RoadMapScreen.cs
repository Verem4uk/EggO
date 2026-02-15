using System.Collections;
using TMPro;
using UnityEngine;


public class RoadMapScreen : MonoBehaviour
{
    [SerializeField]
    private CanvasGroupAutoFadeWithImages Canvas;

    public PointMap[] pointsMap;

    [SerializeField]
    private TranslatableText[] translatableTexts;

    [SerializeField]
    private TextMeshProUGUI EggOText;

    public float delayBeforeStart = 0.5f;

    private void OnEnable()
    {
        StartCoroutine(ShowSequence());
        UpdateTexts();        
    }

    public void UpdateTexts()
    {
        foreach (var text in translatableTexts)
        {
            text.Actualize();
        }
    }

    private IEnumerator ShowSequence()
    {
        foreach (var point in pointsMap)
        {
            point.ResetEggO();
        }

        yield return new WaitForSeconds(delayBeforeStart);        

        StartCoroutine(FadeText(EggOText, .5f, true));

        foreach (var point in pointsMap)
        {
            yield return StartCoroutine(point.Show());
        }

        foreach (var point in pointsMap)
        {
            yield return StartCoroutine(point.UnlockIfAvailable());
        }
    }

    public IEnumerator HideSequence()
    {
        StopAllCoroutines();

        foreach (var point in pointsMap)
        {
            point.Disable();
            yield return new WaitForSeconds(.2f);
        }

        yield return new WaitForSeconds(.5f);

        Canvas.HideAndDeactivate();

        StartCoroutine(FadeText(EggOText, .5f, false));

        foreach (var point in pointsMap)
        {
            point.ResetEggO();
        }
    }

    private IEnumerator FadeText(TextMeshProUGUI text, float duration, bool fadeIn)
    {
        float t = 0f;
        Color original = text.color;

        float startAlpha = original.a;
        float endAlpha = fadeIn ? 1f : 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, t / duration);
            text.color = new Color(original.r, original.g, original.b, alpha);
            yield return null;
        }

        text.color = new Color(original.r, original.g, original.b, endAlpha);
    }
}
