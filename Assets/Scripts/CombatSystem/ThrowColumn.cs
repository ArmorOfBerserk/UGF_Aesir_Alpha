using Dreamteck.Splines;
using UnityEngine;

public class ThrowColumn : MonoBehaviour
{
    [SerializeField] int speed = 12;

    SplineProjector sp;
    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        sp = GetComponent<SplineProjector>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Attack")) return;

        Vector3 otherPos = other.transform.position;
        Vector3 myPos = transform.position;
        Vector3 impactDir = (otherPos - myPos).normalized;

        rb.isKinematic = false;

        if (impactDir.x > 0.5f)
        {
            Debug.Log("Colpito da destra, deve andare a sx");
            rb.linearVelocity = -sp.result.forward * speed;
        }
        else if (impactDir.x < -0.5f)
        {

            Debug.Log("Colpito da sinistra, deve andare a dx");
            rb.linearVelocity = sp.result.forward * speed; rb.linearVelocity = sp.result.forward * speed;
        }
    }

    
    

    void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.E))
        {

        }
        else if (Input.GetKey(KeyCode.Q))
        {

        }
    }
}
