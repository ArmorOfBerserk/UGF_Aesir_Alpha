using System;
using TMPro;
using UnityEngine;

public class MessageDisplay : MonoBehaviour
{
    private CanvasGroup _canvasGroup;
    [SerializeField] private TextMeshProUGUI _keyText;
    void Start()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0;
    }

    private void ShowMessage(string message)
    {
        //Inserimento messaggio
        _keyText.text = message;
        _canvasGroup.alpha = 1;
    }

    private void HideMessage()
    {
        _canvasGroup.alpha = 0;
        _keyText.text = "";
    }

    /// <summary>
    /// This function is called when the object becomes enabled and active.
    /// </summary>
    void OnEnable()
    {
        EventMessageManager.OnMessageSend += ShowMessage;
        EventMessageManager.OnDeleteMessage += HideMessage;
    }


    /// <summary>
    /// This function is called when the behaviour becomes disabled or inactive.
    /// </summary>
    void OnDisable()
    {
        EventMessageManager.OnMessageSend -= ShowMessage;
        EventMessageManager.OnDeleteMessage -= HideMessage;

    }
}
