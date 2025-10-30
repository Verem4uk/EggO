using UnityEngine;
using System.Collections;
using UnityEngine.UI;


public class RoadMapScreen : MonoBehaviour
{    
    public PointMap[] pointsMap;

    //public float delayBetweenPoints = 0.2f;
    public float delayBeforeStart = 0.5f;

    private void OnEnable()
    {
        StartCoroutine(ShowSequence());
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
        yield return new WaitForSeconds(delayBeforeStart);

        foreach (var point in pointsMap)
        {
            point.Disable();
        }
    }   
}
