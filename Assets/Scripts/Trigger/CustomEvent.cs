using System;
using Dreamteck.Splines.Primitives;
using UnityEngine;

public class CustomEvent : MonoBehaviour
{
    public event Action<Transform> EnterTrigger;
    public event Action ExitTrigger;
    private BoxCollider boxCollider;

    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
    }


    /// <summary>
    /// OnTriggerEnter is called when the Collider other enters the trigger.
    /// </summary>
    /// <param name="other">The other Collider involved in this collision.</param>
    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
            EnterTrigger?.Invoke(transform);
    }

    /// <summary>
    /// OnTriggerExit is called when the Collider other has stopped touching the trigger.
    /// </summary>
    /// <param name="other">The other Collider involved in this collision.</param>
    void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
            ExitTrigger?.Invoke();
    }
}
