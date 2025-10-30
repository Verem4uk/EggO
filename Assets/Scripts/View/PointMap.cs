using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PointMap : MonoBehaviour
{
    [SerializeField]
    private int ID;

    [SerializeField] 
    private Image LockedEggO; 
    
    [SerializeField] 
    private Image ActiveEggO; 
    
    [SerializeField] 
    private TextMeshProUGUI Text;    
    
    [SerializeField] 
    private Pulsation Pulsation;        

    public IEnumerator Show()
    {
        var maxPastLevel = Saver.GetLevel();
        if (ID <= maxPastLevel)
        {
            ActiveEggO.gameObject.SetActive(true);
            Text.gameObject.SetActive(true);

            yield return StartCoroutine(FadeInImage(ActiveEggO, 0.5f));
            yield return StartCoroutine(FadeText(Text, 0.5f, true));
        }
        else
        {
            yield return StartCoroutine(FadeInImage(LockedEggO, 0.5f));
        }        
    }

    public IEnumerator UnlockIfAvailable()
    {
        if (Saver.GetLevel() + 1 == ID)
        {
            yield return StartCoroutine(FadeOutImage(LockedEggO, 0.5f));
            ActiveEggO.gameObject.SetActive(true);            
            yield return StartCoroutine(FadeInImage(ActiveEggO, 0.5f));
            Text.gameObject.SetActive(true);
            yield return StartCoroutine(FadeText(Text, 0.5f, true));
            Pulsation.enabled = true;
        }
    }

    public void Disable()
    {
        StartCoroutine(Hide());
    }

    private IEnumerator Hide()
    {
        if (ActiveEggO.gameObject.activeInHierarchy)
        {
            Pulsation.enabled = false;
            yield return StartCoroutine(FadeOutImage(ActiveEggO, 0.5f));
            yield return StartCoroutine(FadeText(Text, 0.5f, false));
        }
        else
        {
            yield return StartCoroutine(FadeOutImage(LockedEggO, 0.5f));
        }         
    }

    private IEnumerator FadeInImage(Image img, float duration) //appear
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

    private IEnumerator FadeOutImage(Image img, float duration) //disappear
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

    public void ResetEggO()
    {
        Pulsation.enabled = false;
        ActiveEggO.gameObject.SetActive(false);
        Text.gameObject.SetActive(false);
        SetAlpha(ActiveEggO, 0f);
        SetAlpha(LockedEggO, 0f);
        SetAlpha(Text, 0f);
    }

    private void OnDisable()
    {
        ResetEggO();
    }
}
