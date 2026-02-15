using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ColorChanger : MonoBehaviour
{
    public Image targetImage;
    public float duration = 0.5f;
    public Color customColor;
    
    public void ToggleColor()
    {        
        ToggleColor(customColor);        
    }

    public void ToggleColor(Color color)
    {
        StopAllCoroutines();     
        if(color == null)
        {
            color = customColor; 
        }
        StartCoroutine(SmoothColorChange(color));
    }

    private IEnumerator SmoothColorChange(Color to)
    {
        float time = 0f;
        var from = targetImage.color;

        while (time < duration)
        {
            time += Time.deltaTime;
            targetImage.color = Color.Lerp(from, to, time / duration);
            yield return null;
        }
        targetImage.color = to;
    }
}
