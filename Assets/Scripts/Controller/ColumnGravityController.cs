using System.Collections;
using Dreamteck.Splines;
using UnityEngine;

public class ColumnGravityController : MonoBehaviour
{
    [SerializeField] CommonValues commonValues;
    [SerializeField] Transform target;
    Rigidbody rb;
    Collider col;

    ColumnDirection _columnDirection;

    WaitUntil waitUntil;
    WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();

    void Awake() {
        rb = GetComponent<Rigidbody>();
        waitUntil = new WaitUntil(() => rb.useGravity);

        col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void Start() {
        GetComponent<SplineProjector>().spline = commonValues.currentSpline;
        col.excludeLayers = 1 << gameObject.layer;

        StartCoroutine(EnableGravity());
    }

    IEnumerator EnableGravity(){
        yield return waitForFixedUpdate;
        col.isTrigger = false;

        // yield return waitUntil;
        while(true){
            target.SetPositionAndRotation(transform.position, transform.rotation);
            yield return waitForFixedUpdate;
        }
    }

    public void ReInitialize(ColumnDirection columnDirection){
        StopAllCoroutines();
        col.isTrigger = true;
        rb.useGravity = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        _columnDirection = columnDirection;

        GetComponent<SplineProjector>().spline = commonValues.currentSpline;

        // if(columnDirection == ColumnDirection.Up && !shouldFall) col.excludeLayers = (1 << LayerMask.NameToLayer("Ground")) | (1 << gameObject.layer);
        // else col.excludeLayers = 1 << gameObject.layer;

        StartCoroutine(EnableGravity());
    }

    void OnTriggerStay(Collider other)
    {
        if(_columnDirection == ColumnDirection.Up && other.gameObject.layer == LayerMask.NameToLayer("Ground")) return;
        // Debug.Log($"sono {gameObject.layer} e ho triggerato {other.gameObject.layer}");
        target.GetComponent<ColumnController>().Reset();
    }
}
