using UnityEngine;

public class Levitation : MonoBehaviour
{
    public float floatAmplitude = 5f;   
    public float floatSpeed = 1f;       

    private Vector3 startPos;

    private void OnEnable()
    {
        startPos = transform.localPosition;
    }

    private void Update()
    {
        float offsetY = Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.localPosition = startPos + new Vector3(0f, offsetY, 0f);
    }
}
