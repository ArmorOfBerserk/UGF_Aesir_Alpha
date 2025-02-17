using Dreamteck.Splines;
using UnityEngine;

public class ThrowColumn : MonoBehaviour
{
    SplineProjector sp;
    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        sp = GetComponent<SplineProjector>();
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.E)){
            rb.AddForce(sp.result.forward * 10, ForceMode.Impulse);
        } else if(Input.GetKeyDown(KeyCode.Q)){
            rb.AddForce(-sp.result.forward * 10, ForceMode.Impulse);
        }
    }
}
