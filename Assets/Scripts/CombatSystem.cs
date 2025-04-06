using System;
using System.Collections;
using Dreamteck.Splines;
using UnityEngine;
using UnityEngine.InputSystem;
public class CombatSystem : MonoBehaviour
{
    [SerializeField] private CommonValues commonValues;
    [SerializeField] private GameObject _columnPrefab;
    [SerializeField] public GameObject attackPrefab;
    [SerializeField] public Transform attackPoint;
    [SerializeField] private int _maxColumns = 5;

    [SerializeField] private Transform groundCheck;
    [SerializeField] private Vector3 groundCheckSize = new Vector3(0.5f, 0.1f, 0.5f);
    [SerializeField] private LayerMask groundLayer;

    private SplineProjector _splineProjector;

    private Animator anim;

    private Vector2 rightStickInput;
    private bool canAttack = true;

    
    private ColumnController[] _instances;

    private int lastGeneratedColumn = 0;

    // --- Gestione carica attacco ---
    private float chargeTime = 0f;
    private float maxChargeTime = 2f;  
    public float minAttackDamage = 10f;
    public float maxAttackDamage = 200f; 
    private bool isCharging = false;
    
    // Input System
    public InputActions inputActions;
    private InputAction attackAction;

    public bool IsGrounded 
    { 
        get => Physics.CheckBox(groundCheck.position, groundCheckSize, Quaternion.identity, groundLayer);
    }


    private void Awake()
    {
        inputActions = new InputActions();
        attackAction = inputActions.Player.Attack;

        _splineProjector = GetComponent<SplineProjector>();
        _instances = new ColumnController[_maxColumns];
        for (int i = 0; i < _maxColumns; i++)
        {
            _instances[i] = Instantiate(_columnPrefab, new Vector3(0, -100, 0), Quaternion.identity)
                .GetComponentInChildren<ColumnController>();
            SetLayerRecursively(_instances[i].transform.parent.gameObject, LayerMask.NameToLayer("Column_" + (i + 1)));
        }
        commonValues.currentSpline = _splineProjector.spline;
    }

    private void OnEnable()
    {
        attackAction.started += OnAttackStarted;
        attackAction.performed += OnAttackPerformed;
        attackAction.canceled += OnAttackCanceled;
        inputActions.Enable();
    }

    private void OnDisable()
    {
        attackAction.started -= OnAttackStarted;
        attackAction.performed -= OnAttackPerformed;
        attackAction.canceled -= OnAttackCanceled;
        inputActions.Disable();
    }

    private void Start()
    {
        StartCoroutine(GenerateColumn());
        anim = GetComponentInChildren<Animator>();
    }

    // ---------------------------------------------------------------------------------
    // GESTIONE ATTACCO
    // ---------------------------------------------------------------------------------

    // Chiamato quando il tasto inizia a essere premuto
    private void OnAttackStarted(InputAction.CallbackContext obj)
    {
        isCharging = true;
        chargeTime = 0f;
        anim.SetBool("IsAttacking", true);
    }

    // Chiamato mentre il tasto è tenuto premuto (dipende dalle impostazioni dell'InputAction)
    private void OnAttackPerformed(InputAction.CallbackContext obj){}

    // Chiamato quando il tasto viene rilasciato
    private void OnAttackCanceled(InputAction.CallbackContext obj)
    {
        ReleaseAttack();
    }

    // Aggiorna la carica mentre il tasto è premuto
    private void Update()
    {
        if (isCharging)
        {
            // Se non sei a terra, non carichi nulla
            if (IsGrounded)
            {
                chargeTime += Time.deltaTime;
                chargeTime = Mathf.Clamp(chargeTime, 0f, maxChargeTime);
            }
        }
    }

    private void ReleaseAttack()
    {
        isCharging = false;

        if (!IsGrounded) // cambiare qui 
        {
            SpawnAttack(minAttackDamage);
        }
        else
        {
            float attackPower = minAttackDamage + (chargeTime / maxChargeTime) * (maxAttackDamage - minAttackDamage);
            SpawnAttack(attackPower);
        }

        chargeTime = 0f; 
        anim.Play("Attack_test");
        anim.SetBool("IsAttacking", false);
    }

    private void SpawnAttack(float attackPower)
    {
        var attackObj = Instantiate(attackPrefab, attackPoint.position, attackPoint.rotation);
        var attackScript = attackObj.GetComponent<Attack>();

        if (attackScript != null)
        {
            attackScript.SetDamage(attackPower);
        }
        Debug.Log("Attack spawned. Power: " + attackPower);
    }

    IEnumerator GenerateColumn()
    {
        while (true)
        {
            yield return new WaitUntil(() => rightStickInput != Vector2.zero && canAttack);

            bool generated = false;
            for (int i = 0; i < _maxColumns; i++)
            {
                if (!_instances[i].IsDestroyed) continue;
                GenerationLogic(_instances[i]);
                StartCoroutine(AttackTimer());
                lastGeneratedColumn = i % _maxColumns;
                generated = true;
                break;
            }

            if (!generated)
            {
                lastGeneratedColumn = (lastGeneratedColumn + 1) % _maxColumns;
                _instances[lastGeneratedColumn].Reset();
                GenerationLogic(_instances[lastGeneratedColumn]);
                StartCoroutine(AttackTimer());
            }
        }
    }

    IEnumerator AttackTimer()
    {
        canAttack = false;
        yield return new WaitForSeconds(.5f);
        canAttack = true;
    }

    void GenerationLogic(ColumnController column)
    {
        if (rightStickInput.x == 1)
        {
            StartCoroutine(column.GenerateColumn(
                transform.position - _splineProjector.result.forward * 1.5f,
                Quaternion.LookRotation(_splineProjector.result.up, _splineProjector.result.forward),
                ColumnDirection.Right
            ));
        }
        else if (rightStickInput.x == -1)
        {
            StartCoroutine(column.GenerateColumn(
                transform.position + _splineProjector.result.forward * 1.5f,
                Quaternion.LookRotation(_splineProjector.result.up, -_splineProjector.result.forward),
                ColumnDirection.Left
            ));
        }
        else if (rightStickInput.y == 1)
        {
            StartCoroutine(column.GenerateColumn(
                transform.position - _splineProjector.result.up / 2,
                Quaternion.LookRotation(-_splineProjector.result.forward, _splineProjector.result.up),
                ColumnDirection.Up
            ));
        }
        else if (rightStickInput.y == -1)
        {
            StartCoroutine(column.GenerateColumn(
                transform.position + _splineProjector.result.up * 3f,
                Quaternion.LookRotation(_splineProjector.result.forward, -_splineProjector.result.up),
                ColumnDirection.Down
            ));
        }
    }
    
    void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null) return;
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}

//TODO
// ground check 
// la potenza dell'attacco dipende dalla velocità con cui stai camminando (premendo sempre z)
// se sei in aria per più di 1 secondo l'attacco caricato si annulla