using UnityEngine;

public class ThornController : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            other.transform.position = CheckpointController.LastCheckpointPosition;
        }
    }
}
