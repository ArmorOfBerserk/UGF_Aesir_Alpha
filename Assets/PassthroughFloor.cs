using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class PassthroughFloor : MonoBehaviour
{
    public bool isOneWay = false;
    public float timerDuration = 2f;
    public float resetTimer = 2f;
    public float rotationSpeed = 10f;

    BoxCollider boxCollider;
    Transform parent;

    Coroutine timer;

    void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
        parent = transform.parent;
        timer = null;
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

    IEnumerator Timer()
    {
        yield return new WaitForSeconds(timerDuration);

        boxCollider.enabled = false;

        // Prendo l’angolo di partenza sull’asse X (in gradi)
        float angleX = transform.eulerAngles.x;

        while (angleX > -90f)
        {
            // Decremento in modo frame-rate indipendente
            angleX -= rotationSpeed * Time.fixedDeltaTime;

            // Applico il nuovo angolo in gradi
            parent.transform.eulerAngles = new Vector3(angleX, transform.eulerAngles.y, transform.eulerAngles.z);

            yield return null;
        }

        // Assicuro il vincolo esatto a -90°  
        parent.transform.eulerAngles = new Vector3(-90f, transform.eulerAngles.y, transform.eulerAngles.z);

        StartCoroutine(ResetFloor());
    }

    IEnumerator ResetFloor()
    {
        yield return new WaitForSeconds(resetTimer);

        // Riporto l’angolo a 0
        float angleX = -90f;

        while (angleX < 0f)
        {
            // Incremento in modo frame-rate indipendente
            angleX += rotationSpeed * Time.fixedDeltaTime;

            // Applico il nuovo angolo in gradi
            parent.transform.eulerAngles = new Vector3(angleX, transform.eulerAngles.y, transform.eulerAngles.z);

            yield return null;
        }

        // Assicuro il vincolo esatto a 0°  
        parent.transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y, transform.eulerAngles.z);

        boxCollider.enabled = true;
    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            if (timer != null) StopCoroutine(timer);

            timer = StartCoroutine(Timer());
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            if (timer != null) StopCoroutine(timer);
            timer = null;
        }
    }
}
