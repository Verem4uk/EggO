using UnityEngine;
using UnityEngine.UI;

public class CoffeeView : MonoBehaviour
{
    [SerializeField]
    private Text CoffeeQuestion;

    [SerializeField]
    private Controller Controller;

    [SerializeField]
    private Question Ask;

    private static string StripePaymentLink = "https://donate.stripe.com/dRm9AT4uffKkfu83EB6g800";
    private static string NoCoffeeLink = "https://verlema.life/EggO/NoVirtualCoffee/";

    private void OnEnable()
    {
        CoffeeQuestion.text = Ask.GetText();        
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
        Analytics.RefusePurchase(Saver.LastPastLevel);

#if UNITY_WEBGL && !UNITY_EDITOR
        Application.ExternalEval($"window.open('{NoCoffeeLink}', '_self');");
#else

        Application.OpenURL(NoCoffeeLink);
#endif    

        Close();        
    }

    public void Close()
    {
        Controller.BackToMenu();
        gameObject.SetActive(false);
    }
}
