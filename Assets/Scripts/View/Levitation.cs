using UnityEngine;

public class Levitation : MonoBehaviour
{
    public float floatAmplitude = 5f;
    public float floatSpeed = 1f;
    private Vector3 StartPosition;
    private float Elapsed;
    private bool Initialized;

    private void CapturePosition()
    {
        if (Initialized) return;
        StartPosition = transform.localPosition;
        Initialized = true;
    }

    private void Awake() => CapturePosition();

    private void OnEnable()
    {
        CapturePosition();
        Elapsed = 0f;
    }

    private void Update()
    {
        Elapsed += Time.unscaledDeltaTime;
        transform.localPosition = StartPosition +
            new Vector3(0f, Mathf.Sin(Elapsed * floatSpeed) * floatAmplitude, 0f);
    }

    private void OnDisable()
    {
        if (Initialized) transform.localPosition = StartPosition;
    }
}
