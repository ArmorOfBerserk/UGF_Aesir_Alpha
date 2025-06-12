using System;
using Dreamteck.Splines.Primitives;
using UnityEngine;

public class CustomEvent : MonoBehaviour
{
    public event Action<Transform> EnterTrigger;
    public event Action ExitTrigger;

    /// <summary>
    /// OnTriggerEnter is called when the Collider other enters the trigger.
    /// </summary>
    /// <param name="other">The other Collider involved in this collision.</param>
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log(other.name);
            EnterTrigger?.Invoke(transform);
        }
    }

    /// <summary>
    /// OnTriggerExit is called when the Collider other has stopped touching the trigger.
    /// </summary>
    /// <param name="other">The other Collider involved in this collision.</param>
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            ExitTrigger?.Invoke();
    }
}
