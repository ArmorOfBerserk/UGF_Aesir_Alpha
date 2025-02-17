using System.Collections;
using System.Threading.Tasks;
using Dreamteck.Splines;
using UnityEngine;

public enum ColumnDirection{
    Up,
    Down,
    Left,
    Right
}

public class ColumnController : MonoBehaviour
{
    [SerializeField] private CommonValues commonValues;
    [SerializeField] private float _generationSpeed;
    [SerializeField] private float _lenght;
    [SerializeField] private Rigidbody _gravityController;
    [SerializeField] SplineProjector _splineProjector;

    [SerializeField] private LayerMask _innerCheckLayer;
    [SerializeField] private LayerMask _innerCheckLayerGrounded;
    public ColumnDirection ColumnDirection { get; private set; }
    [HideInInspector] public bool isGenerated = false;

    public bool IsDestroyed { get; private set; }
    int visiblePartitions;
    int startY;


    void Awake()
    {
        IsDestroyed = true;
        visiblePartitions = 0;
    }

    void Start()
    {
        _innerCheckLayer = ~(1 << gameObject.layer);
        _innerCheckLayerGrounded = _innerCheckLayer & ~(1 << LayerMask.NameToLayer("Ground"));
        startY = -50 * gameObject.layer - 28;
    }

    public IEnumerator GenerateColumn(Vector3 position, Quaternion rotation, ColumnDirection columnDirection){
        ColumnDirection = columnDirection;
        bool isGrounded = PlayerMovement.Instance.IsGrounded;

        Reset();
        IsDestroyed = false;

        transform.parent.SetPositionAndRotation(position, rotation);
        transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        _gravityController.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

        while (true){
            yield return new WaitForFixedUpdate();

            if (IsDestroyed) yield break;

            transform.localScale += new Vector3(0, _generationSpeed * Time.deltaTime, 0);
            
            if(transform.localScale.y >= _lenght){
                transform.localScale = new Vector3(transform.localScale.x, _lenght, transform.localScale.z);
                isGenerated = true;
                StartCoroutine(EnableGravity(isGrounded));
                // StartCoroutine(UnexpectedItemInBaggingArea_RemoveThisItemBeforeContinuing());
                break;
            }
        }
    }

    IEnumerator EnableGravity(bool wasGrounded){
        if(!(ColumnDirection == ColumnDirection.Up && wasGrounded)){
            yield return new WaitForSeconds(1);
            _gravityController.useGravity = true;
        }
    }

    IEnumerator UnexpectedItemInBaggingArea_RemoveThisItemBeforeContinuing(){
        float tmp = 0;
        for(int i=0; i<transform.childCount; i++){
            tmp += transform.GetChild(i).localScale.y;
        }

        Vector3 dimension = new Vector3(transform.localScale.x, tmp, transform.localScale.z) / 3;
        LayerMask selected;

        while (true){
            yield return new WaitForFixedUpdate();

            if (IsDestroyed) yield break;

            // if(PlayerMovement.Instance.IsGrounded) selected = _innerCheckLayerGrounded;
            // else selected = _innerCheckLayer;

            selected = _innerCheckLayerGrounded;

            if(Physics.CheckBox(transform.position, dimension, transform.rotation, selected)) {
                var hits = Physics.OverlapBox(transform.position, dimension, transform.rotation, _innerCheckLayer);
                foreach (var hit in hits){
                    Debug.Log($"Hit: {hit.name} - {hit.gameObject.layer}");
                }

                Reset();
                break;
            }
        }
    }

    public void OnColumnHit(ColumnPartition columnType, Collision other){
        switch(ColumnDirection){
            case ColumnDirection.Up:
                other.gameObject.GetComponent<Rigidbody>().AddForce(_splineProjector.result.up * 25, ForceMode.Impulse);
                break;
            case ColumnDirection.Down:
                other.gameObject.GetComponent<Rigidbody>().AddForce(-_splineProjector.result.up * 25, ForceMode.Impulse);
                break;
            case ColumnDirection.Left:
                other.gameObject.GetComponent<Rigidbody>().AddForce(-_splineProjector.result.forward * 50, ForceMode.Impulse);
                break;
            case ColumnDirection.Right:
                other.gameObject.GetComponent<Rigidbody>().AddForce(_splineProjector.result.forward * 50, ForceMode.Impulse);
                break;
        }
    }

    public void Reset(){
        StopAllCoroutines();
        isGenerated = false;
        IsDestroyed = true;
        visiblePartitions = 0;

        transform.parent.position = new Vector3(0, startY, 0);
        
        _gravityController.gameObject.GetComponent<ColumnGravityController>().ReInitialize(ColumnDirection);

        transform.localScale = new Vector3(transform.localScale.x, 0.1f, transform.localScale.z);
        _splineProjector.spline = commonValues.currentSpline;
    }

    public void BecameInvisible(){
        visiblePartitions--;
        if(visiblePartitions <= 0){
            Reset();
        }
    }

    public void BecameVisible(){
        visiblePartitions++;
    }

    private void OnDestroy() {
        IsDestroyed = true;
        StopAllCoroutines();
    }
}