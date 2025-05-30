using System;
using UnityEngine;

public class EventMessageManager : MonoBehaviour
{
    public static event Action<string> OnMessageSend;
    public static event Action OnDeleteMessage;

    //Metodi statici da chiamare nelle classi in cui voglio triggerare l'evento
    public static void SendTextMessage(string message){
        OnMessageSend?.Invoke(message);
    }

    public static void DeleteMessage(){
        OnDeleteMessage?.Invoke();
    }
}
