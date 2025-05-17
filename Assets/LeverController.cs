using UnityEngine;

public class LeverController : MonoBehaviour
{
    [SerializeField] private Triggerable[] triggerables;
    [SerializeField] private bool isOn = false;
    [SerializeField] private bool isOneTime = false;

    bool isPlayerInRange = false;
    bool isTriggered = false;

    void Start()
    {
        if(isOn) transform.rotation = Quaternion.Euler(0, 0, 45);
        else transform.rotation = Quaternion.Euler(0, 0, -45);
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.E) && isPlayerInRange)
        {
            if (isOneTime && isTriggered) return;
            isTriggered = true;

            isOn = !isOn;
            TriggerAll();

            if (isOn)
                transform.rotation = Quaternion.Euler(0, 0, 45);
            else
                transform.rotation = Quaternion.Euler(0, 0, -45);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
        }
    }


    private void TriggerAll()
    {
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
