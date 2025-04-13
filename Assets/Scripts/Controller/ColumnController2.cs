using System;
using System.Collections;
using Dreamteck.Splines;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class ColumnController2 : MonoBehaviour
{

    [SerializeField] private CommonValues commonValues;
    [SerializeField] private float _generationSpeed;
    [SerializeField] private float _lenght;
    [SerializeField] private Rigidbody _rb;
    [SerializeField] SplineProjector _splineProjector;

    //Spazio checkbox
    [SerializeField] private Transform _checkbox;
    [SerializeField] private Vector3 _checkboxSize;

    [SerializeField] private LayerMask _innerCheckLayer;
    [SerializeField] private LayerMask _innerCheckLayerGrounded;
    [HideInInspector] public bool isGenerated = false;

    [SerializeField] private SearchForContact upperTrigger;
    [SerializeField] private SearchForContact lowerTrigger;

    public Transform AttachedPlayer;
    public Vector3 checkBoxCenterOffset;
    public Vector3 checkBoxHalfExtents;

    public bool IsDestroyed { get; private set; }
    private LayerMask myLayerMask;
    private LayerMask myLayerMask1;
    [SerializeField] Vector3 worldCenter;

    private Material material;
    private Coroutine coroutine = null;



    void OnEnable()
    {
        lowerTrigger.EnterTrigger += StartMovement;
        lowerTrigger.EnableGravity += RestoreSituationDefault;
        lowerTrigger.ExitTrigger += FromAirToGround;
    }


    void OnDisable()
    {
        lowerTrigger.EnterTrigger -= StartMovement;
        lowerTrigger.EnableGravity -= RestoreSituationDefault;
        lowerTrigger.ExitTrigger -= FromAirToGround;
    }

    private void StartMovement()
    {

        if (coroutine == null)
            coroutine = StartCoroutine(MoveColumn());
    }

    private void RestoreSituationDefault()
    {
        _rb.isKinematic = true;
        _rb.constraints = RigidbodyConstraints.None;
        _rb.useGravity = false;
    }

    private void FromAirToGround()
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
      /*       _rb.isKinematic = false;
            _rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
            _rb.useGravity = true; */
        }

    }

    IEnumerator MoveColumn()
    {
        float destinazione = transform.position.y + 4.5f;
        while (true)
        {
            yield return new WaitForFixedUpdate();
            if ((transform.position.y > destinazione - 0.24f && transform.position.y <= destinazione) || Mathf.Approximately(transform.position.y, destinazione))
            {
                Debug.Log(new Vector3(transform.position.x, destinazione, transform.position.z));
                transform.position = new Vector3(transform.position.x, destinazione - 0.05f, transform.position.z);
                break;
            }
            transform.position += new Vector3(0, _generationSpeed * Time.fixedDeltaTime, 0);
        }
        // Così le colonne potrebbero essere davvero tanto giuste
        // Però mi torna utile il controllo.
        /* transform.position += new Vector3(0, 0.045f, 0f); */
    }

    void Awake()
    {
        IsDestroyed = true;
        material = GetComponent<Renderer>().material;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GetComponent<SplineProjector>().spline = commonValues.currentSpline;
        int myLayer = gameObject.layer; // Prendi il layer dell'oggetto attuale
        /* myLayerMask = ((1 << 29) | (1 << 30) | (1 << 31)) & ~(1 << myLayer); */
        myLayerMask = 1 << 9;

        //Va tolto un layer delle colonna 3
        myLayerMask1 = ((1 << 29) | (1 << 30) | (1 << 31)) & ~(1 << myLayer);
    }

    private void SetHeight(float height)
    {
        material.SetFloat("_CutoffHeight", height);
        material.SetFloat("_NoiseStrength", 0);
    }

    private void OnDestroy()
    {
        IsDestroyed = true;
        StopAllCoroutines();
    }

    public void AttachPlayer(Transform player)
    {
        AttachedPlayer = player;
        AttachedPlayer.parent = _rb.transform;
    }

    public void DetachPlayer()
    {
        AttachedPlayer.parent = null;
        AttachedPlayer = null;
    }


    public IEnumerator GenerateColumn(Vector3 myPosition, Quaternion rotation, Action onComplete)
    {
        bool isGrounded = PlayerMovement.Instance.IsGrounded;

        Reset();
        IsDestroyed = false;
        transform.SetPositionAndRotation(myPosition, rotation);
        float inizio = -4.51f;

        while (true)
        {
            /* upperTrigger.isEnableTrigger(true); */


            // Verifica se la CheckBox interseca qualche collider

            yield return new WaitForFixedUpdate();

            if (IsDestroyed) yield break;


            transform.position += new Vector3(0, _generationSpeed * Time.fixedDeltaTime, 0);
            /* transform.position += new Vector3(0, _generationSpeed * Time.fixedDeltaTime, 0); */

            // Serve per la texture invisibile
            SetHeight(inizio += _generationSpeed * Time.fixedDeltaTime);

            if (transform.position.y > myPosition.y + 4.90f || Mathf.Approximately(transform.position.y, myPosition.y + 4.90f))
                break;




        }

        GetComponent<BoxCollider>().isTrigger = false;
        onComplete?.Invoke();
    }

    public void Reset()
    {
        StopAllCoroutines();
        isGenerated = false;
        IsDestroyed = true;

        //Non so a cosa servano
        Transform player = transform.Find("Player");
        if (player != null) player.parent = null;
        GetComponent<BoxCollider>().isTrigger = true;
        SetHeight(-4.5f);
        if (!_rb.isKinematic)
        {
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            _rb.isKinematic = true;
        }

        _splineProjector.spline = commonValues.currentSpline;
    }

}
