using Dreamteck.Splines;
using UnityEngine;

public class ThrowColumn : MonoBehaviour
{
    [SerializeField] int speed = 50;

    SplineProjector sp;
    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        sp = GetComponent<SplineProjector>();
    }

    void FixedUpdate()
    {
        if(Input.GetKey(KeyCode.E)){
            rb.linearVelocity = sp.result.forward * speed;
        } else if(Input.GetKey(KeyCode.Q)){
            rb.linearVelocity = -sp.result.forward * speed;
        }
    }
}
