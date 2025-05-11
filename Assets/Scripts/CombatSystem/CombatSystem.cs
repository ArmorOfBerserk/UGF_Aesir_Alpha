using System;
using System.Collections;
using Dreamteck.Splines;
using UnityEngine;
using UnityEngine.InputSystem;

// classe che gestisce il sistema di combattimento del player: spawn di attacchi, animazioni e interazione col PlayerCombatStats (salute/energia).
public class CombatSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] public GameObject attackPrefab;    // prefab dell'oggetto con script attack 
    [SerializeField] public Transform attackPoint;     
    /* [SerializeField] private int _maxColumns = 5; */
    /* private SplineProjector _splineProjector; */

    private Animator anim;         
    private PlayerCombatStats playerStats;  // riferimento allo script salute/energia

    // --- Gestione caricamento e durata dell'animazione di attacco ---
    private float chargeTime = 0f;     // tempo di caricamento attacco (non usato in versione base) NON IN USO 
    private float maxChargeTime = 2f;  // tempo massimo per caricamento NON IN USO 
    public float minAttackDamage = 10f; // danno minimo dell'attacco 
    public float maxAttackDamage = 20f; // danno massimo dell'attacco caricato NON IN USO 

    // Variabili per limitare durata combo / animazione
    private float attackDurationTimer = 0f;
    [SerializeField] private float maxTime = 1f;       // durata massima dell'animazione di attacco
    private bool durationOutOf = false; // Flag: animazione durata conclusa

    // Input System
    public InputActions inputActions; 
    private InputAction attackAction; 

    private void Awake()
    {
        playerStats = GetComponent<PlayerCombatStats>();
        anim = GetComponentInChildren<Animator>();

        inputActions = new InputActions();
        attackAction = inputActions.Player.Attack;
        /* _splineProjector = GetComponent<SplineProjector>(); */
        /* commonValues.currentSpline = _splineProjector.spline; */
    }

    private void OnEnable()
    {
        if (playerStats.GetCurrentHealth() > 0f)
        {
            attackAction.started += OnAttackStarted;
            inputActions.Enable();
        }
    }

    private void OnDisable()
    {
        attackAction.started -= OnAttackStarted;
        inputActions.Disable();
    }
    
    
    
    private void Update()
    {
        // Se il player muore, disabilita eventi e esci
        if (playerStats.GetCurrentHealth() <= 0f)
        {
            OnDisable();
            return;
        }
        /*
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
        */

        if (anim.GetBool("IsAttacking"))
        {
            attackDurationTimer = 0f;
            durationOutOf = false;
        }
        else
        {
            attackDurationTimer += Time.deltaTime;
            if( attackDurationTimer >= maxTime) {
                durationOutOf = true;
                Debug.Log("sono qui");
            }
        }
        anim.SetBool("DurationOutOf", durationOutOf);
    }

    private void OnAttackStarted(InputAction.CallbackContext obj)
    {
        /*
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
            anim.SetBool("IsAttacking", false);
        }
        */

        // Non attaccare se il player è morto
        if (playerStats.GetCurrentHealth() <= 0f) return;

        // Inizia coroutine che mostra animazione e genera attacco
        StartCoroutine(PlayAttackAnimation(minAttackDamage));
    }

    private void ReleaseAttack()
    {
        float damage = minAttackDamage;

        if (PlayerMovement.Instance != null && PlayerMovement.Instance.IsRunning)
        {
            if (!PlayerMovement.Instance.IsGrounded && chargeTime >= 1f)
                damage = minAttackDamage;
            else
                damage = minAttackDamage + (chargeTime / maxChargeTime) * (maxAttackDamage - minAttackDamage);
        }
        SpawnAttack(damage);
        chargeTime = 0f;
        anim.SetBool("IsAttacking", false);
    }


    private void SpawnAttack(float attackPower)
    {
        Quaternion attackRotation = attackPoint.rotation;

        /*

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
        */

        // Inverti rotazione se il player guarda a sinistra
        if (PlayerMovement.Instance != null && !PlayerMovement.Instance.IsFacingRight)
            attackRotation = Quaternion.Euler(
                attackRotation.eulerAngles.x,
                attackRotation.eulerAngles.y + 180f,
                attackRotation.eulerAngles.z);

        GameObject attackObj = Instantiate(attackPrefab, attackPoint.position, attackRotation);
        Attack attackScript = attackObj.GetComponent<Attack>();
        if (attackScript != null)
            attackScript.SetDamage(attackPower);

        Debug.Log($"[CombatSystem] Attack spawned. Power: {attackPower}");
    }

    private IEnumerator PlayAttackAnimation(float damage)
    {
        anim.SetBool("IsAttacking", true); 
        SpawnAttack(damage);          
        yield return new WaitForSeconds(0.2f);
        anim.SetBool("IsAttacking", false);
    }
}