using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CoffeeView : MonoBehaviour
{
    [SerializeField]
    private Text CoffeeQuestion;

    [SerializeField]
    private Image Image;

    [SerializeField]
    private Sprite[] SpritesStack;

    [SerializeField]
    private Sprite SadSprite;

    [SerializeField]
    private TMP_InputField AnswerField;

    [SerializeField]
    private GameObject Buttons;

    [SerializeField]
    private Question Ask;

    [SerializeField]
    private Question Thanks;

    [SerializeField]
    private Question WhyNot;

    private void OnEnable()
    {
        CoffeeQuestion.text = Ask.GetText();
        Buttons.SetActive(true);
        AnswerField.gameObject.SetActive(false);
        Image.sprite = SpritesStack[0];
    }

    public void Buy()
    {
        CoffeeQuestion.text = Thanks.GetText();
        StartCoroutine(SuccessAnimation());
        Buttons.SetActive(false);
        AnswerField.gameObject.SetActive(true);
        Analytics.StartPurchase(Saver.LastPastLevel);
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
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    public IEnumerator SuccessAnimation()
    {
        for(int i  = 1; i < SpritesStack.Length; i++)
        {
            Image.sprite = SpritesStack[i];
            yield return new WaitForSeconds(.5f);
        }        
    }
}
