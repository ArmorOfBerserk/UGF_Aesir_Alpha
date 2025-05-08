using UnityEngine;
using UnityEngine.UIElements;

public class PassthroughFloor : MonoBehaviour
{
    public bool isOneWay = false;

    BoxCollider boxCollider;

    void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            boxCollider.excludeLayers = LayerMask.GetMask("Player");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            boxCollider.excludeLayers = 0;
        }
    }
}
