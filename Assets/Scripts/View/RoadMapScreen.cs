using UnityEngine;
using System.Collections;


public class RoadMapScreen : MonoBehaviour
{
    [SerializeField]
    private CanvasGroupAutoFadeWithImages Canvas;

    public PointMap[] pointsMap;

    [SerializeField]
    private TranslatableText[] translatableTexts; 

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
        yield return new WaitForSeconds(delayBeforeStart);

        foreach (var point in pointsMap)
        {
            point.ResetEggO();
        }

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
        //yield return new WaitForSeconds(delayBeforeStart);

        foreach (var point in pointsMap)
        {
            point.Disable();
            yield return new WaitForSeconds(.2f);
        }

        yield return new WaitForSeconds(.5f);

        Canvas.HideAndDeactivate();
    }   
}
