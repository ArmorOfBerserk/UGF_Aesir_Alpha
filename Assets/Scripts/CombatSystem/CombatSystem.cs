using System;
using System.Collections;
using Dreamteck.Splines;
using UnityEngine;
using UnityEngine.InputSystem;

public class CombatSystem : MonoBehaviour
{
    /* [SerializeField] private CommonValues commonValues; */
    [SerializeField] public GameObject attackPrefab;
    [SerializeField] public Transform attackPoint;
    /* [SerializeField] private int _maxColumns = 5; */

    /* private SplineProjector _splineProjector; */
    private Animator anim;

    // --- Gestione carica attacco ---
    private float chargeTime = 0f;
    private float maxChargeTime = 2f;  
    public float minAttackDamage = 10f;
    public float maxAttackDamage = 200f; 
    private bool isCharging = false;
    
    // Input System - SERVE?
    public InputActions inputActions;
    private InputAction attackAction;

   

    private void Awake()
    {
        inputActions = new InputActions();
        attackAction = inputActions.Player.Attack;

        /* _splineProjector = GetComponent<SplineProjector>(); */
        /* commonValues.currentSpline = _splineProjector.spline; */
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

            if (!PlayerMovement.Instance.IsGrounded && chargeTime >= 1f)
            {
                isCharging = false;
                chargeTime = 1f;
                ReleaseAttack();
            }
            else if (PlayerMovement.Instance.IsGrounded)
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
            if (!PlayerMovement.Instance.IsGrounded && chargeTime >= 1f)
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
        Quaternion attackRotation = attackPoint.rotation;
        
        if (!PlayerMovement.Instance.IsFacingRight)
        {
            attackRotation = Quaternion.Euler(attackRotation.eulerAngles.x,
                                            attackRotation.eulerAngles.y + 180f,
                                            attackRotation.eulerAngles.z);
        }

        Vector2 moveInput = PlayerMovement.Instance.MoveInput;

        float verticalAimAngle = 45f;  

        if (moveInput.y > 0.1f)
        {
            Debug.Log("attacco orientato verso l'alto");
            attackRotation = Quaternion.Euler(attackRotation.eulerAngles.x - verticalAimAngle,
                                            attackRotation.eulerAngles.y,
                                            attackRotation.eulerAngles.z);
        }
        else if (moveInput.y < -0.1f)
        {
            Debug.Log("attacco orientato verso il basso");
            attackRotation = Quaternion.Euler(attackRotation.eulerAngles.x + verticalAimAngle,
                                            attackRotation.eulerAngles.y,
                                            attackRotation.eulerAngles.z);
        }

        GameObject attackObj = Instantiate(attackPrefab, attackPoint.position, attackRotation);
        Attack attackScript = attackObj.GetComponent<Attack>();

        if (attackScript != null)
        {
            attackScript.SetDamage(attackPower);
        }
        Debug.Log("[SpawnAttack] Attack spawned. Power: " + attackPower);
    }
}