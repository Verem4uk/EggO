using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PointMap : MonoBehaviour
{
    [SerializeField] 
    private Image eggo; 
    
    [SerializeField] 
    private Image Eggo; 
    
    [SerializeField] 
    private TextMeshProUGUI Text;    
    
    [SerializeField] 
    private Pulsation Pulsation;        

    public void Activate()
    {
        StartCoroutine(ShowSequence());
    }

    public void Deactivate()
    {
        StartCoroutine(HideSequence());
    }

    private void OnDisable()
    {
        Pulsation.enabled = false;
        Eggo.gameObject.SetActive(false);
        Text.gameObject.SetActive(false);
    }

    private IEnumerator ShowSequence()
    {        
        yield return StartCoroutine(FadeOutImage(eggo, 0.5f));
                        
        Eggo.gameObject.SetActive(true);
        Text.gameObject.SetActive(true);
                
        SetAlpha(Eggo, 0f);
        SetAlpha(Text, 0f);

        yield return StartCoroutine(FadeInImage(Eggo.GetComponent<Image>(), 0.5f));
        yield return StartCoroutine(FadeText(Text, 0.5f, true));

        Pulsation.enabled = true;
    }

    private IEnumerator HideSequence()
    {
        Pulsation.enabled = false;
        yield return StartCoroutine(FadeOutImage(Eggo.GetComponent<Image>(), 0.5f));
        yield return StartCoroutine(FadeText(Text, 0.5f, false));
    }

    private IEnumerator FadeOutImage(Image img, float duration)
    {
        float t = 0f;
        Color original = img.color;

        while (t < duration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(original.a, 0f, t / duration);
            img.color = new Color(original.r, original.g, original.b, alpha);
            yield return null;
        }

        img.color = new Color(original.r, original.g, original.b, 0f);
    }

    private IEnumerator FadeInImage(Image img, float duration)
    {
        float t = 0f;
        Color original = img.color;

        while (t < duration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, t / duration);
            img.color = new Color(original.r, original.g, original.b, alpha);
            yield return null;
        }

        img.color = new Color(original.r, original.g, original.b, 1f);
    }

    private IEnumerator FadeText(TextMeshProUGUI text, float duration, bool fadeIn)
    {
        float t = 0f;
        Color original = text.color;

        float startAlpha = fadeIn ? 0f : 1f;
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

    private void SetAlpha(Graphic graphic, float alpha)
    {
        if (graphic == null) return;
        var c = graphic.color;
        graphic.color = new Color(c.r, c.g, c.b, alpha);
    }
    
}
