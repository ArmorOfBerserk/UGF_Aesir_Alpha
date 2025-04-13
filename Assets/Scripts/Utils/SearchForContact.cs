using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class SearchForContact : MonoBehaviour
{

    public event Action EnterTrigger;
    public event Action EnableGravity;
    public event Action ExitTrigger;
    [SerializeField] private BoxCollider boxCollider;

    void Start()
    {
        gameObject.layer = name == "UpperTrigger" ? LayerMask.NameToLayer("UpperTrigger") : LayerMask.NameToLayer("LowerTrigger");
    }




    void OnTriggerEnter(Collider other)
    {
        /*         //L'upperTrigger deve fare contatto solo con il LowerTrigger e disattivarsi quando finisce di salire.
                if (gameObject.layer == LayerMask.NameToLayer("UpperTrigger"))
                {

                    return;
                } */

        //Parte per il lowerTrigger che deve anche vedere quando attivare la gravità

/* 
        if (gameObject.layer == LayerMask.NameToLayer("UpperTrigger"))
        {
            Debug.Log($"{LayerMask.LayerToName(gameObject.layer)} e {LayerMask.LayerToName(other.gameObject.layer)} si sono toccati");
            Debug.Log($"Precisamente {LayerMask.LayerToName(gameObject.transform.parent.gameObject.layer)} tocca {LayerMask.LayerToName(other.gameObject.transform.parent.gameObject.layer)}");
        } */

        //Se il downtrigger tocca il ground
        /* if (other.gameObject.layer == LayerMask.NameToLayer("Ground") && gameObject.layer == LayerMask.NameToLayer("LowerTrigger"))
        {
        
            Debug.Log("A morte i ....");
            EnableGravity?.Invoke();
        } */

        if (other.gameObject.layer == LayerMask.NameToLayer("UpperTrigger") && gameObject.layer == LayerMask.NameToLayer("LowerTrigger"))
        {
            Debug.Log("ON TRIGGER EVENT UPPER INTO LOWER");
            // TO-DO MANDA EVENTO PER SMETTERE DI SALIRE
            ExitTrigger?.Invoke();
        }


        //Parte per il lowerTrigger che deve dare l'input di salire quando rileva sotto l'upper trigger
        if (other.gameObject.layer == LayerMask.NameToLayer("UpperTrigger"))
        {
            EnterTrigger?.Invoke();
            return;
            // TO-DO MANDA EVENTO PER SALIRE
        }
    }

    //Quando il lower non trova più il contatto con l'upper, interrompi l'azione.
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("UpperTrigger") && gameObject.layer == LayerMask.NameToLayer("LowerTrigger"))
        {
            Debug.Log("ON TRIGGER EXIT UPPER INTO LOWER");
            // TO-DO MANDA EVENTO PER SMETTERE DI SALIRE
            ExitTrigger?.Invoke();
        }

    }


}
