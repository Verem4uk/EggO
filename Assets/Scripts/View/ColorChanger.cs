using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ColorChanger : MonoBehaviour
{
    public Image targetImage;
    public float duration = 0.5f;
    public Color customColor;
    private bool isBlack = true;

    public void ToggleColor()
    {
        Debug.Log("Change color to " + customColor);
        ToggleColor(customColor);        
    }

    public void ToggleColor(Color color)
    {
        StopAllCoroutines();        
        StartCoroutine(SmoothColorChange(isBlack ? Color.black : color, isBlack ? color : Color.black));
        isBlack = !isBlack;
    }

    private IEnumerator SmoothColorChange(Color from, Color to)
    {
        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            targetImage.color = Color.Lerp(from, to, time / duration);
            yield return null;
        }
        targetImage.color = to;
    }
}
