using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class SearchForContact : MonoBehaviour
{

    public event Action EnterTrigger;
    public event Action EnableGravity;
    public event Action ExitTrigger;
    public event Action ResetColumn;
    [SerializeField] public BoxCollider boxCollider;

    void Start()
    {
        switch (name)
        {
            case "UpperTrigger":
                gameObject.layer = LayerMask.NameToLayer("UpperTrigger");
                break;

            case "LowerTrigger":
                gameObject.layer = LayerMask.NameToLayer("LowerTrigger");
                break;

            case "CheckReset":
                gameObject.layer = LayerMask.NameToLayer("CheckReset");
                break;

            default:
                Debug.LogWarning($"Nessun layer assegnato per il nome: {name}");
                break;
        }
    }

    public void Reset(bool value = false)
    {
        if (gameObject.name == "CheckReset")
            boxCollider.enabled = value;
    }

    void OnTriggerEnter(Collider other)
    {
        //L'upperTrigger deve fare contatto solo con il LowerTrigger e disattivarsi quando finisce di salire.
        if (gameObject.layer == LayerMask.NameToLayer("CheckReset"))
        {
            ResetColumn?.Invoke();
            return;
        }

        //Parte per il lowerTrigger che deve anche vedere quando attivare la gravità

        /* 
                if (gameObject.layer == LayerMask.NameToLayer("UpperTrigger"))
                {
                    Debug.Log($"{LayerMask.LayerToName(gameObject.layer)} e {LayerMask.LayerToName(other.gameObject.layer)} si sono toccati");
                    Debug.Log($"Precisamente {LayerMask.LayerToName(gameObject.transform.parent.gameObject.layer)} tocca {LayerMask.LayerToName(other.gameObject.transform.parent.gameObject.layer)}");
                } */

        //Se il downtrigger tocca il ground
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground") && gameObject.layer == LayerMask.NameToLayer("LowerTrigger"))
        {
            EnableGravity?.Invoke();
        }




        //Se ti scontri con l'upperTrigger (e quindi sei il LowerTrigger) inizia a salire!
        if (other.gameObject.layer == LayerMask.NameToLayer("UpperTrigger"))
        {
            EnterTrigger?.Invoke();
            return;
        }
    }

    //Quando il lower non trova più il contatto con l'upper, interrompi l'azione.
    void OnTriggerExit(Collider other)
    {
        // Se sei il LowerTrigger ed esci dall'upper trigger, chiama funzione per interrompere l'azione di salire
        if (other.gameObject.layer == LayerMask.NameToLayer("UpperTrigger") && gameObject.layer == LayerMask.NameToLayer("LowerTrigger"))
        {

            // MANDA EVENTO PER SMETTERE DI SALIRE
            ExitTrigger?.Invoke();
        }



    }


}
