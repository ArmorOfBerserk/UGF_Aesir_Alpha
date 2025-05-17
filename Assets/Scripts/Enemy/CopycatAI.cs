using System.Collections;
using UnityEngine;

// Checks (preso spunto dal PlayerMovement)
[System.Serializable]
public struct CopycatChecks
{
    public LayerMask groundLayer;      
    public Transform groundCheck;     
    public Vector3 groundCheckSize;    
    public Transform frontCheck;       
    public Vector3 frontCheckSize;   
    public Transform backCheck;        
    public Vector3 backCheckSize;    

    public void CorrectHalfSize()
    {
        groundCheckSize /= 2f;
        frontCheckSize  /= 2f;
        backCheckSize   /= 2f;
    }
}

[RequireComponent(typeof(CharacterController))]
public class CopycatAI : EnemyBase
{
    // stati logici dell'AI (utile per il settaggio dell'Animator)
    public enum EnemyState
    {
        Calm,               // player troppo lontano, non triggerato 
        Walk,               // cammina tra i patrol points fissati 
        TriggeredWalk,      // il nemico ha visto il player ma non vuole attaccare 
        TriggeredRunAttack, // corsa pre attacco 
        BladeTransition,    // apre le lame 
        BladeAttack         // 
    }

    [Header("Movement Param")]
    [SerializeField] private float walkSpeed = 1f;  
    [SerializeField] private float runSpeed = 5f; 
    [SerializeField] private float accelSmooth = 5f;

    [Header("Detect Player")]
    public float chaseRange = 10f;
    public float eyeHeight = 1.5f;


    [Header("Checks & Gravity")]
    [SerializeField] private CopycatChecks checks; 
    [SerializeField] private float gravity = -9.81f; // Accelerazione di gravità

    [Header("Combat Settings")]
    [SerializeField] private float contactDamageWalk = 10f; // danno da contatto in TriggeredWalk
    [SerializeField] private float contactDamageRun = 20f; // danno da contatto in TriggeredRunAttack
    [SerializeField] private float contactDamageBlade = 30f; // danno da BladeAttack
    [SerializeField] private float attackRange = 1.5f; // raggio per attacco a distanza
    [SerializeField] private float attackCooldown = 2f;   // cooldown tra un attacco e l'altro (evita spam)

    [Header("Damage Multipliers")] // danno inflitto dal player 
    [Range(0f,1f)] [SerializeField] private float calmDamageMul = 1f;
    [Range(0f,1f)] [SerializeField] private float walkDamageMul = 1f;
    [Range(0f,1f)] [SerializeField] private float runDamageMul = 0.8f;
    [Range(0f,1f)] [SerializeField] private float bladeAttackDamageMul = 0f;

    [Header("Animator")]
    [SerializeField] private Animator animator;  

    [Header("References")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private Transform player;     

    private EnemyState currentState = EnemyState.Walk; // stato iniziale
    private int patrolIndex = 0;     

    private CharacterController controller; 
    private Vector3 velocity; 
    private Vector3 smoothVel; 
    private bool isGrounded;
    private bool edgeAhead;  
    private float attackTimer = 0f; 


    protected override void Awake()
    {
        base.Awake();
        controller = GetComponent<CharacterController>();
        checks.CorrectHalfSize();
    }

    void Start()
    {
        TransitionToState(EnemyState.Walk);
    }

    void Update()
    {
        // se vedo il player mentre sto in Calm o Walk, passo a TriggeredWalk
        if ((currentState == EnemyState.Calm || currentState == EnemyState.Walk) && player != null && SeePlayer())
        {
            TransitionToState(EnemyState.TriggeredWalk);
            return;
        }


        if (knockbackVelocity.sqrMagnitude > 0.01f)
        {
            velocity.y = 0;
            controller.Move(knockbackVelocity * Time.deltaTime);

            knockbackVelocity = Vector3.MoveTowards(knockbackVelocity, Vector3.zero, knockbackDecay * Time.deltaTime);

            return; // evitiamo che il nemico cada dalla spline nel frame della collisione con il proiettile 
        }

        attackTimer += Time.deltaTime;

        isGrounded = Physics.CheckBox(
            checks.groundCheck.position,
            checks.groundCheckSize,
            Quaternion.identity,
            checks.groundLayer);
        edgeAhead = !Physics.CheckBox(
            checks.frontCheck.position,
            checks.frontCheckSize,
            Quaternion.identity,
            checks.groundLayer);

        if (isGrounded && velocity.y < 0f)
            velocity.y = -2f;
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // il nemico esegue un comportamento in base allo stato corrente nell'animator 
        switch (currentState)
        {
            case EnemyState.Calm:
            case EnemyState.Walk:
                Patrol(); // si muove tra i patrol points 
                break;

            case EnemyState.TriggeredWalk:
                Chase(player.position, walkSpeed);        // inseguimento lento
                TryAttack(contactDamageWalk);              // tentativo di attacco
                break;

            case EnemyState.TriggeredRunAttack:
                Chase(player.position, runSpeed);         // inseguimento veloce
                TryAttack(contactDamageRun);               // tentativo di attacco
                break;

            case EnemyState.BladeTransition:
                Chase(player.position, runSpeed);
                break;

            case EnemyState.BladeAttack:
                TryAttack(contactDamageBlade);             // attacco lama invincibile per loky 
                break;
        }
    }

    // metodo per la camminata tra i patrol points 
    private void Patrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;
        Vector3 target = patrolPoints[patrolIndex].position;
        target.y = transform.position.y; // aggiunto perchè il nemico volava (da fixare)
        Chase(target, walkSpeed);

        if (Vector3.Distance(transform.position, target) < 0.2f)
            patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
    }

    // metodo di inseguimento 
    private void Chase(Vector3 target, float speed)
    {
        Vector3 dir = target - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.01f) return;

        Vector3 desired = dir.normalized * speed;
        smoothVel = Vector3.Lerp(smoothVel, desired, accelSmooth * Time.deltaTime);
        controller.Move(smoothVel * Time.deltaTime);

        Vector3 s = transform.localScale;
        s.x = Mathf.Sign(smoothVel.x) * Mathf.Abs(s.x);
        transform.localScale = s;
    }

    // Tenta un attacco se nel range e il cooldown è scaduto
    private void TryAttack(float damage)
    {
        if (attackTimer < attackCooldown) return;
        if (Vector3.Distance(transform.position, player.position) <= attackRange)
        {
            animator.SetTrigger("Attack"); // animazione colpo (da capire)
            var ps = player.GetComponent<PlayerCombatStats>();
            if (ps != null)
            {
                ps.TakeDamage(damage);
                Debug.Log($"[CopycatAI] l'attacco ha colpito il player con danno ={damage}");
            }
            attackTimer = 0f;
        }
    }

    // trigger con proiettile e contatto player
    void OnTriggerEnter(Collider other)
    {
        // se a collidere è il proiettile cambia stato a TriggeredWalk 
        if (other.GetComponentInParent<Attack>() is Attack proj && currentState < EnemyState.TriggeredWalk)
        {
            proj = other.GetComponentInParent<Attack>();
            TakeDamage(proj.GetDamage());

            StartCoroutine(DelayedTransition(EnemyState.TriggeredWalk, 1f));
            return;
        }

        // se è player, applica danno da contatto
        if (other.GetComponentInParent<PlayerCombatStats>() is PlayerCombatStats ps)
        {
            float damage = 0f;
            switch (currentState)
            {
                case EnemyState.TriggeredWalk:      damage = contactDamageWalk;  break;
                case EnemyState.TriggeredRunAttack: damage = contactDamageRun;   break;
                case EnemyState.BladeAttack:        damage = contactDamageBlade; break;
            }
            if (damage > 0f)
            {
                ps.TakeDamage(damage);
                Debug.Log($"[CopycatAI] Player subisce danno contatto: {damage}");
            }
        }
    }

    // transizioni di stato dell'animator e reset parametri
    private void TransitionToState(EnemyState newState)
    {
        currentState = newState;
        attackTimer = 0f;               // reset del cooldown di attacco

        // reset dei trigger e booleani dell'animator
        animator.ResetTrigger("EnterBladeTransition");
        animator.SetBool("IsCalm", false);
        animator.SetBool("IsWalking", false);
        animator.SetBool("IsTriggeredWalk", false);
        animator.SetBool("IsTriggeredRunAttack", false);
        animator.SetBool("IsBladeAttack", false);

        switch (newState)
        {
            case EnemyState.Calm:
                animator.SetBool("IsCalm", true);
                break;
            case EnemyState.Walk:
                animator.SetBool("IsWalking", true);
                break;
            case EnemyState.TriggeredWalk:
                animator.SetBool("IsTriggeredWalk", true);
                StartCoroutine(TimerThen(Random.Range(2f,5f),
                    () => TransitionToState(EnemyState.TriggeredRunAttack)));
                break;
            case EnemyState.TriggeredRunAttack:
                animator.SetBool("IsTriggeredRunAttack", true);
                StartCoroutine(RandomChoice(Random.Range(3f,6f),
                    EnemyState.TriggeredWalk,
                    EnemyState.BladeTransition));
                break;
            case EnemyState.BladeTransition:
                animator.SetTrigger("EnterBladeTransition");
                StartCoroutine(BladeTransitionRoutine());
                break;
            case EnemyState.BladeAttack:
                animator.SetBool("IsBladeAttack", true);
                StartCoroutine(TimerThen(Random.Range(4f,7f),
                    () => TransitionToState(EnemyState.TriggeredRunAttack)));
                break;
        }
    }

    private bool SeePlayer()
    {
        // raycast altezza degli occhi 
        Vector3 origin = transform.position + Vector3.up * eyeHeight;
        Vector3 target = player.position + Vector3.up * eyeHeight;
        
        float dist = Vector3.Distance(origin, target);
        if (dist > chaseRange) return false;

        Ray ray = new Ray(origin, (target - origin));
        if (Physics.Raycast(ray, out RaycastHit hit, chaseRange))
        {
            if (hit.transform == player)
                return true;
        }
        return false;
    }

    private IEnumerator TimerThen(float delay, System.Action callback)
    {
        yield return new WaitForSeconds(delay);
        callback?.Invoke();
    }

    // coroutine per scelta randomica di stato dopo delay
    private IEnumerator RandomChoice(float delay, EnemyState a, EnemyState b)
    {
        yield return new WaitForSeconds(delay);
        TransitionToState(Random.value < 0.5f ? a : b);
    }

    // coroutine per BladeTransition (4s di apertura lame)
    private IEnumerator BladeTransitionRoutine()
    {
        float t = 4f;
        bool allHit = false;
        while (t > 0f)
        {
            yield return null;
            t -= Time.deltaTime;
        }
        TransitionToState(allHit ? EnemyState.TriggeredRunAttack : EnemyState.BladeAttack);
    }

    private IEnumerator DelayedTransition(EnemyState newState, float delay)
    {
        yield return new WaitForSeconds(delay);
        TransitionToState(newState);
    }

    // override di TakeDamage (di enemy base) per applicare moltiplicatori di stato
    public override void TakeDamage(float damage)
    {
        float mul = 1f;
        switch (currentState)
        {
            case EnemyState.Calm: mul = calmDamageMul; break;
            case EnemyState.Walk: mul = walkDamageMul; break;
            case EnemyState.TriggeredRunAttack: mul = runDamageMul; break;
            case EnemyState.BladeAttack: mul = bladeAttackDamageMul; break;
        }
        base.TakeDamage(damage * mul);
    }

    protected override void Die()
    {
        // eventalmente aggiungere qui qualche comportamento diverso da EnemyBase::Die
        base.Die();
    }
}