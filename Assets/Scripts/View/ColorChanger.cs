using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ColorChanger : MonoBehaviour
{
    public Image targetImage;
    public float duration = 0.5f;
    private bool isBlack = true;

    public void ToggleColor()
    {
        StopAllCoroutines(); 
        StartCoroutine(SmoothColorChange(isBlack ? Color.black : Color.white, isBlack ? Color.white : Color.black));
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
