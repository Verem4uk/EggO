using UnityEngine;
using UnityEngine.UI;

public class ScaledImage : MonoBehaviour
{
    [SerializeField]
    private Image Image;

    public void Show(Image image)
    {
        Image.sprite = image.sprite;
        gameObject.SetActive(true);
    }
}
