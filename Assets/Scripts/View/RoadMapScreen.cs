using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class RoadMapScreen : MonoBehaviour
{  
    public Image[] imageBlocks;                  
    public GameObject imageWithTextBlock;      
       
    public float fadeDuration = 0.5f;
    public float delayBeforeStart = 0.5f;
    public float floatAmplitude = 5f;
    public float floatSpeed = 1f;

    private Vector3 floatStartPos;

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

        yield return new WaitForSeconds(0.1f);
        
        yield return StartCoroutine(FadeInGroup(imageWithTextBlock));
        
        floatStartPos = imageWithTextBlock.transform.localPosition;
        StartCoroutine(FloatObject(imageWithTextBlock.transform));
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

    private IEnumerator FadeInGroup(GameObject obj)
    {
        obj.SetActive(true);
                
        Image[] images = obj.GetComponentsInChildren<Image>(true);
        TextMeshProUGUI[] texts = obj.GetComponentsInChildren<TextMeshProUGUI>(true);

        foreach (var img in images)
        {
            var c = img.color;
            c.a = 0f;
            img.color = c;
        }
        foreach (var txt in texts)
        {
            var c = txt.color;
            c.a = 0f;
            txt.color = c;
        }

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);

            foreach (var img in images)
            {
                var c = img.color;
                c.a = alpha;
                img.color = c;
            }

            foreach (var txt in texts)
            {
                var c = txt.color;
                c.a = alpha;
                txt.color = c;
            }

            yield return null;
        }
    }

    private IEnumerator FloatObject(Transform objTransform)
    {
        while (true)
        {
            float offsetY = Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
            objTransform.localPosition = floatStartPos + new Vector3(0f, offsetY, 0f);
            yield return null;
        }
    }
}
