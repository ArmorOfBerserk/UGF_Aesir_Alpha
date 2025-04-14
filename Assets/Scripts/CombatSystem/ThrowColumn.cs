using Dreamteck.Splines;
using UnityEngine;

public class ThrowColumn : MonoBehaviour
{
    [SerializeField] int speed = 12;

    SplineProjector sp;
    Rigidbody rb;
    ColumnController2 colControl;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        sp = GetComponent<SplineProjector>();
        colControl = GetComponent<ColumnController2>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Attack")) return;

        Vector3 otherPos = other.transform.position;
        Vector3 myPos = transform.position;
        Vector3 impactDir = (otherPos - myPos).normalized;

        rb.isKinematic = false;

        // Ottieni il centro del collider (BoxCollider)
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        Vector3 colliderCenter = boxCollider.bounds.center;

        // Debug per capire dove è avvenuto l'impatto
        Debug.Log($"Centro del Collider: {colliderCenter}");
        Debug.Log($"Posizione dell'impatto: {otherPos}");

        // Determina se il colpo è avvenuto a sinistra o a destra del centro del collider
        if (otherPos.x > colliderCenter.x)  // Colpo a destra
        {
            Debug.Log("Colpo avvenuto a destra del centro");
            // Aggiungi logica per il movimento a destra
            rb.linearVelocity = -sp.result.forward * speed;
        }
        else if (otherPos.x < colliderCenter.x)  // Colpo a sinistra
        {
            Debug.Log("Colpo avvenuto a sinistra del centro");
            // Aggiungi logica per il movimento a sinistra
            rb.linearVelocity = sp.result.forward * speed;
        }
        else
        {
            Debug.Log("Colpo avvenuto esattamente al centro");
            // Gestisci il caso in cui il colpo avviene esattamente al centro
        }
    }

    // Funzione per far esplodere la colonna.
    /* void OnCollisionEnter(Collision collision)
    {
        if(rb.linearVelocity != Vector3.zero && (!collision.gameObject.CompareTag("Player") || !collision.gameObject.CompareTag("Attack"))){
            colControl.Reset();
        }
    }
 */



    void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.E))
        {
            rb.isKinematic = false;
            rb.linearVelocity = -sp.result.forward * speed;
        }
        else if (Input.GetKey(KeyCode.Q))
        {
            rb.isKinematic = false;
            rb.linearVelocity = sp.result.forward * speed;
        }
    }
}
