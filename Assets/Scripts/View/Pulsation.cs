using UnityEngine;

public class Pulsation : MonoBehaviour
{
    public float minScale = 0.8f;
    public float maxScale = 1.2f;
    public float speed = 1f;

    private float t;  

    private void Start()
    {        
        float normalizedLerp = (1f - minScale) / (maxScale - minScale);        
        float sinValue = normalizedLerp * 2f - 1f;        
        t = Mathf.Asin(sinValue);
    }

    private void Update()
    {
        t += Time.deltaTime * speed;
        float scale = Mathf.Lerp(minScale, maxScale, (Mathf.Sin(t) + 1f) / 2f);
        transform.localScale = new Vector2(scale, scale);
    }
}
