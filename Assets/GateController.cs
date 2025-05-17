using System.Collections;
using UnityEngine;

public class GateController : Triggerable
{
    [SerializeField] private Vector3 openPosition;
    [SerializeField] private Vector3 closedPosition;
    [SerializeField] private float speed = 2f;
    [SerializeField] private int triggerRequired = 1;

    private bool isOpen = false;

    public override void Trigger()
    {
        triggerRequired--;
        if (triggerRequired > 0) return;
        
        isOpen = !isOpen;

        StopAllCoroutines();
        StartCoroutine(TriggerGate());
    }

    IEnumerator TriggerGate()
    {
        Vector3 targetPosition = isOpen ? openPosition : closedPosition;

        while (Vector3.Distance(transform.localPosition, targetPosition) > 0.01f)
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetPosition, speed * Time.deltaTime);
            yield return null;
        }

        transform.localPosition = targetPosition;
    }
}
