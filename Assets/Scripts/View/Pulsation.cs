using UnityEngine;

public class Pulsation : MonoBehaviour
{
    public float minScale = 0.8f;
    public float maxScale = 1.2f;
    public float speed = 1f;

    private Vector3 BaseScale;
    private bool Initialized;
    private float Phase;

    private void CaptureScale()
    {
        if (Initialized) return;
        BaseScale = transform.localScale;
        Initialized = true;
    }

    private void Awake() => CaptureScale();

    private void OnEnable()
    {
        CaptureScale();
        float range = maxScale - minScale;
        Phase = Mathf.Abs(range) > 0.0001f
            ? Mathf.Asin(Mathf.Clamp((1f - minScale) / range * 2f - 1f, -1f, 1f))
            : 0f;
    }

    private void Update()
    {
        Phase += Time.unscaledDeltaTime * speed;
        float scale = Mathf.Lerp(minScale, maxScale, (Mathf.Sin(Phase) + 1f) / 2f);
        transform.localScale = new Vector3(BaseScale.x * scale, BaseScale.y * scale, BaseScale.z);
    }

    private void OnDisable()
    {
        if (Initialized) transform.localScale = BaseScale;
    }
}
