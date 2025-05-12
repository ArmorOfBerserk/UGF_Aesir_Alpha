using UnityEngine;

public class TriggerCaller : MonoBehaviour
{
    [Tooltip("Riferimento all'Animator della Camera")]
    public Animator cameraAnimator;

    private void OnTriggerEnter(Collider other)
    {
        //cameraAnimator.SetTrigger("doMove");
        if (other.CompareTag("Player"))
        {
            cameraAnimator.SetTrigger("doMove");
        }
    }
}