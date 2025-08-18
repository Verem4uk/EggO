using UnityEngine;
using System.Collections;
using UnityEngine.UI;


public class RoadMapScreen : MonoBehaviour
{
    public Image[] imageBlocks;
    public PointMap pointMap;

    public float fadeDuration = 0.2f;
    public float delayBeforeStart = 0.5f;

    private void OnEnable()
    {
        StartCoroutine(ShowSequence());
    }

    private IEnumerator ShowSequence()
    {
        yield return new WaitForSeconds(delayBeforeStart);

        foreach (var img in imageBlocks)
        {
            yield return StartCoroutine(FadeInImage(img));
        }

        pointMap.Activate();
    }
       
    private IEnumerator FadeInImage(Image img)
    {
        img.gameObject.SetActive(true);
        Color originalColor = img.color;
        Color color = originalColor;
        color.a = 0f;
        img.color = color;

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            color.a = Mathf.Lerp(0f, originalColor.a, t / fadeDuration);
            img.color = color;
            yield return null;
        }

        img.color = originalColor;
    }

    private IEnumerator FadeOutImage(Image img)
    {
        Color originalColor = img.color;
        float startAlpha = originalColor.a;
        float t = 0f;

        while (t < 0.5f)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, 0f, t / 0.5f);
            img.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        img.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
        img.gameObject.SetActive(false);
    }

    public void HideAll()
    {
        StartCoroutine(HideSequence());        
    }

    public IEnumerator HideSequence()
    {
        pointMap.Deactivate();
        Coroutine[] fades = new Coroutine[imageBlocks.Length];
        for (int i = 0; i < imageBlocks.Length; i++)
        {
            fades[i] = StartCoroutine(FadeOutImage(imageBlocks[i]));
        }

        foreach (var fade in fades)
        {
            yield return fade;
        }        
    }

    private void OnDisable()
    {
        foreach(var img in imageBlocks)
        {
            var originalColor = img.color;
            img.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1);
        }
    }
}
