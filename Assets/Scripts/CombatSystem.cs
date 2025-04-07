using System;
using System.Collections;
using Dreamteck.Splines;
using UnityEngine;
using UnityEngine.InputSystem;

public class CombatSystem : MonoBehaviour
{
    [SerializeField] private CommonValues commonValues;
    [SerializeField] public GameObject attackPrefab;
    [SerializeField] public Transform attackPoint;
    [SerializeField] private int _maxColumns = 5;

    [SerializeField] private Transform groundCheck;
    [SerializeField] private Vector3 groundCheckSize = new Vector3(0.5f, 0.1f, 0.5f);
    [SerializeField] private LayerMask groundLayer;

    private SplineProjector _splineProjector;
    private Animator anim;

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
        anim = GetComponentInChildren<Animator>();
    }

    private void OnAttackStarted(InputAction.CallbackContext obj)
    {
        if (PlayerMovement.Instance != null && PlayerMovement.Instance.IsRunning)
        {
            isCharging = true;
            chargeTime = 0f;
            anim.SetBool("IsAttacking", true);
        }
        else
        {
            anim.SetBool("IsAttacking", true);
            SpawnAttack(minAttackDamage);
            anim.Play("Attack_test");
            anim.SetBool("IsAttacking", false);
        }
    }

    private void OnAttackPerformed(InputAction.CallbackContext obj){} // inutile 

    private void OnAttackCanceled(InputAction.CallbackContext obj)
    {
        if(isCharging)
            ReleaseAttack();
    }

    private void Update()
    {
        if (isCharging)
        {
            chargeTime += Time.deltaTime;

            if (!IsGrounded && chargeTime >= 1f)
            {
                isCharging = false;
                chargeTime = 1f;
                ReleaseAttack();
            }
            else if (IsGrounded)
            {
                chargeTime = Mathf.Clamp(chargeTime, 0f, maxChargeTime);
            }
        }
    }

    private void ReleaseAttack()
    {
        isCharging = false;
        if (PlayerMovement.Instance == null || !PlayerMovement.Instance.IsRunning)
        {
            SpawnAttack(minAttackDamage);
        }
        else
        {
            if (!IsGrounded && chargeTime >= 1f)
            {
                SpawnAttack(minAttackDamage);
            }
            else
            {
                float attackPower = minAttackDamage + (chargeTime / maxChargeTime) * (maxAttackDamage - minAttackDamage);
                SpawnAttack(attackPower);
            }
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
}