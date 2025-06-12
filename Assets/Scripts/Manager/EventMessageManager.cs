using System;
using UnityEngine;

public class EventMessageManager : MonoBehaviour
{
    public static event Action<IndicationOnScreenEnum> OnMessageSend;
    public static event Action OnDeleteMessage;

    //Metodi statici da chiamare nelle classi in cui voglio triggerare l'evento
    public static void SendTextMessage(IndicationOnScreenEnum indication){
        OnMessageSend?.Invoke(indication);
    }

    public static void DeleteMessage(){
        OnDeleteMessage?.Invoke();
    }
}
