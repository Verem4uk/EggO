using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CoffeeView : MonoBehaviour
{
    [SerializeField]
    private Text CoffeeQuestion;

    [SerializeField]
    private Controller Controller;

    [SerializeField]
    private Image Image;

    [SerializeField]
    private Sprite StartSprite;

    [SerializeField]
    private Sprite SadSprite;

    [SerializeField]
    private TMP_InputField AnswerField;

    [SerializeField]
    private GameObject Buttons;

    [SerializeField]
    private Question Ask;

    [SerializeField]
    private Question WhyNot;

    private static string StripePaymentLink = "https://donate.stripe.com/dRm9AT4uffKkfu83EB6g800";

    private void OnEnable()
    {
        CoffeeQuestion.text = Ask.GetText();
        Buttons.SetActive(true);
        AnswerField.gameObject.SetActive(false);
        Image.sprite = StartSprite;
    }

    public void Buy()
    {
        Analytics.StartPurchase(Saver.LastPastLevel);

#if UNITY_WEBGL && !UNITY_EDITOR
        Application.ExternalEval($"window.open('{StripePaymentLink}', '_self');");
#else
        Application.OpenURL(StripePaymentLink);
#endif    

        Close();             
    }

    public void NoBuy()
    {
        CoffeeQuestion.text = WhyNot.GetText();
        Image.sprite = SadSprite;
        Buttons.SetActive(false);
        AnswerField.gameObject.SetActive(true);
    }

    public void SendMessage()
    {
        Analytics.SendSuccessFeedback(AnswerField.text);
        Close();
    }

    public void Close()
    {
        Controller.BackToMenu();
        gameObject.SetActive(false);
    }
}
