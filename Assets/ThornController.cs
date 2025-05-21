using UnityEngine;

public class ThornController : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Thorn hit");
            other.transform.position = CheckpointController.LastCheckpointPosition;
            other.GetComponentInChildren<HealthBarController>().CurrentHealth -= 1;
        }
    }
}
