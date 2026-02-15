using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PointMap : MonoBehaviour
{
    [SerializeField]
    private int ID;
        
    [SerializeField] 
    private Button ActiveEggO;

    [SerializeField]
    private Image PastEggO;

    [SerializeField] 
    private TextMeshProUGUI Text;    
    
    [SerializeField] 
    private Pulsation Pulsation;        

    public IEnumerator Show()
    {
        var maxPastLevel = Saver.GetLevel();

        if (ID <= maxPastLevel)
        {
            PastEggO.gameObject.SetActive(true);
            
            Text.gameObject.SetActive(true);

            yield return StartCoroutine(FadeInImage(PastEggO, 0.5f));
            yield return StartCoroutine(FadeText(Text, 0.5f, true));
        }

        if (ID > maxPastLevel)
        {
            ActiveEggO.interactable = false;
            yield return StartCoroutine(FadeInImage(ActiveEggO.image, 0.5f));
        }        
    }

    public IEnumerator UnlockIfAvailable()
    {
        if (Saver.GetLevel() + 1 == ID)
        {            
            ActiveEggO.gameObject.SetActive(true);
            ActiveEggO.interactable = true;
            Text.gameObject.SetActive(true);
            yield return StartCoroutine(FadeText(Text, 0.5f, true));
            Pulsation.enabled = true;
        }
    }

    public void Disable()
    {
        StartCoroutine(Hide());
    }

    public IEnumerator Hide()
    {
        if (ActiveEggO.gameObject.activeInHierarchy)
        {
            Pulsation.enabled = false;
            yield return StartCoroutine(FadeOutImage(ActiveEggO.image, 0.5f));            
            yield return StartCoroutine(FadeText(Text, 0.5f, false));
        }
        
        if (PastEggO.gameObject.activeInHierarchy)
        {            
            yield return StartCoroutine(FadeOutImage(PastEggO, 0.5f));
            yield return StartCoroutine(FadeText(Text, 0.5f, false));
        }              
    }

    private IEnumerator FadeInImage(Image img, float duration) //appear
    {
        float t = 0f;
        Color original = img.color;
        img.color = new Color(original.r, original.g, original.b, 0);

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

    private void SetAlpha(Graphic graphic, float alpha)
    {
        if (graphic == null) return;
        var c = graphic.color;
        graphic.color = new Color(c.r, c.g, c.b, alpha);
    }

    public void ResetEggO()
    {
        Pulsation.enabled = false;        
        Text.gameObject.SetActive(false);
        SetAlpha(ActiveEggO.image, 0f);        
        SetAlpha(Text, 0f);
        ActiveEggO.interactable = false;
    }

    private void OnDisable()
    {
        ResetEggO();
    }
}
