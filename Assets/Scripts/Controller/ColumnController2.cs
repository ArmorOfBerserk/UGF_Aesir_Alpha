using System;
using System.Collections;
using System.Collections.Generic;
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
    [HideInInspector] public bool isGenerated = false;

    [SerializeField] private SearchForContact upperTrigger;
    [SerializeField] private SearchForContact lowerTrigger;
    [SerializeField] private SearchForContact checkReset;

    public Transform AttachedPlayer;
    public Vector3 checkBoxCenterOffset;
    public Vector3 checkBoxHalfExtents;

    public bool IsDestroyed { get; private set; }

    private Material material;
    private Coroutine coroutine = null;


    //TEST
    private float clearTimer = 0f;
    private float requiredTime = 0.5f;
    private bool activated = false;

    [SerializeField] Transform checkAir;
    [SerializeField] Vector3 checkAirSize;



    void OnEnable()
    {
        lowerTrigger.EnterTrigger += StartMovement;
        checkReset.ResetColumn += Reset;
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
        if (_rb.useGravity && !_rb.isKinematic)
        {
            _rb.isKinematic = true;
            _rb.constraints = RigidbodyConstraints.None | RigidbodyConstraints.FreezeRotation;
            _rb.useGravity = false;
        }
    }

    private void FromAirToGround()
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }
    }

    void ActivatePhysics()
    {
        _rb.isKinematic = false;
        _rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
        _rb.useGravity = true;

        Debug.Log("Attivata fisica dopo 0.5s di clear.");
    }

    bool IsBoxClear()
    {
        int selfLayer = gameObject.layer;
        int otherLayer = lowerTrigger.gameObject.layer;

        int combined = (1 << selfLayer) | (1 << otherLayer);

        LayerMask mask = ~combined;


        Vector3 center = checkAir.position;
        Vector3 halfExtents = checkAirSize * 0.5f;
        Quaternion rotation = checkAir.rotation;

        Collider[] hitColliders = Physics.OverlapBox(center, halfExtents, rotation, mask);

        if (hitColliders.Length > 0)
        {
            List<string> nomiOggetti = new List<string>();
            foreach (var hitCollider in hitColliders)
            {
                nomiOggetti.Add(hitCollider.gameObject.name);
            }

            string logFinale = "Oggetti trovati nell'area: " + string.Join(", ", nomiOggetti);
            /* Debug.Log(logFinale); */
        }


        return !Physics.CheckBox(center, halfExtents, rotation, mask);
    }

    void OnDrawGizmos()
    {
        int selfLayer = gameObject.layer;
        int otherLayer = lowerTrigger.gameObject.layer;

        int combined = (1 << selfLayer) | (1 << otherLayer);

        LayerMask mask = combined;

        Vector3 center = checkAir.position;
        Quaternion rotation = checkAir.rotation;

        Vector3 size = checkAirSize;


        Gizmos.color = new Color(0.6f, 0f, 1f, 0.5f); // Viola trasparente
        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(center, rotation, checkAir.lossyScale);
        Gizmos.DrawCube(Vector3.zero, size);
        Gizmos.matrix = oldMatrix;
    }

    IEnumerator MoveColumn()
    {
        //Quando reset parte, FAI ATTENZIONE: Rende solo invisibile, ma esisterà ancora. Bisogna cambiare posizione.
        checkReset.Reset(true);
        float destinazione = transform.position.y + 4.5f;
        while (true)
        {
            yield return new WaitForFixedUpdate();
            if ((transform.position.y > destinazione - 0.24f && transform.position.y <= destinazione) || Mathf.Approximately(transform.position.y, destinazione))
            {
    
                transform.position = new Vector3(transform.position.x, destinazione - 0.05f, transform.position.z);
                break;
            }
            transform.position += new Vector3(0, _generationSpeed * Time.fixedDeltaTime, 0);
        }

        // Se lascio questa cosa, le colonne si separano il "giusto", non andando ad incastrarsi.
        // Facendo così, quando avrà "isKinematic=false" non farà saltare la seconda.
        transform.position += new Vector3(0, 0.060f, 0f);
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

        }

        checkReset.Reset();

        _rb.isKinematic = true;
        _rb.constraints = RigidbodyConstraints.None | RigidbodyConstraints.FreezeRotation;
        _rb.useGravity = false;

        _splineProjector.spline = commonValues.currentSpline;
    }

    void FixedUpdate()
    {
        
        if (activated) return;

        if (IsBoxClear())
        {
            clearTimer += Time.fixedDeltaTime;

            if (clearTimer >= requiredTime)
            {
                ActivatePhysics();
                activated = true;
            }
        }
        else
        {
            clearTimer = 0f; // Reset se qualcosa è sotto
        }
    }

}
