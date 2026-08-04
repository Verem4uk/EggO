using UnityEngine;

public class SliderView : MonoBehaviour
{
    [SerializeField]
    private float TimeToHide = 3f;

    private float Timer;

    private void OnEnable()
    {
        Restart();
    }

    private void Update()
    {
        Timer -= Time.deltaTime;

        if (Timer <= 0f)
        {
            gameObject.SetActive(false);
        }
    }

    public void Restart()
    {
        Timer = TimeToHide;
    }
}