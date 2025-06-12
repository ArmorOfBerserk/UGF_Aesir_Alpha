using System;
using TMPro;
using UnityEngine;

public enum IndicationOnScreenEnum
{
    UP_DIRECTION = 0,
    DOWN_DIRECTION,
    DIALOGUE_INTERACTION
}

public class MessageDisplay : MonoBehaviour
{
    private CanvasGroup _canvasGroup;
    [SerializeField] GameObject[] panels;
    void Start()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0;
    }

    //Ci andrà un enum
    private void ShowMessage(IndicationOnScreenEnum indication)
    {
        _canvasGroup.alpha = 1;
        panels[(int)indication].SetActive(true);
    }

    private void HideMessage()
    {
        /* _canvasGroup.alpha = 0; */
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
