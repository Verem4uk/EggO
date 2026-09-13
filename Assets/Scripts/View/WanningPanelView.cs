using UnityEngine;
using UnityEngine.UI;

public class WanningPanelView : MonoBehaviour
{
    [SerializeField]
    private Text WarningQuestionText;

    [SerializeField]
    private Question WarningQuestion;

    private void OnEnable()
    {
        WarningQuestionText.text = WarningQuestion.GetText();
    }
}
