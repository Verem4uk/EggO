using TMPro;
using UnityEngine;

public class TranslatableText : MonoBehaviour
{
    [SerializeField]
    private Question Translations;

    [SerializeField]
    private TextMeshProUGUI Text;

    public void Actualize()
    {
        Text.text = Translations.GetText();
    }
}
