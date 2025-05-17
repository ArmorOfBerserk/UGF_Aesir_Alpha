using UnityEngine;

public class TriggerZone : MonoBehaviour
{
    [SerializeField] private Triggerable[] triggerables;
    [SerializeField] private bool mustReset = false;

    private bool hasTriggered = false;
    private readonly float triggerDelay = 0.5f;
    private float lastTriggerTime = 0f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;

            if (triggerables == null || triggerables.Length == 0)
            {
                Debug.LogWarning("No triggerables assigned.");
                return;
            }

            foreach (var triggerable in triggerables)
            {
                triggerable.Trigger();
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && mustReset)
        {
            if (Time.time - lastTriggerTime < triggerDelay) return;

            hasTriggered = false;

            if (triggerables == null || triggerables.Length == 0)
            {
                Debug.LogWarning("No triggerables assigned.");
                return;
            }

            foreach (var triggerable in triggerables)
            {
                triggerable.Trigger();
            }

            lastTriggerTime = Time.time;
        }
    }
}
