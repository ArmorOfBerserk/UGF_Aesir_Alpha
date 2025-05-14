using System.Collections;
using Dreamteck.Splines;
using UnityEngine;

[System.Serializable]
struct Checks
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
        groundCheckSize /= 2;
        frontCheckSize /= 2;
        backCheckSize /= 2;
    }
}

[System.Serializable]
struct PlayerStats
{
    [Header("Movement")]
    public float horizontalAcceleration;
    public float horizontalDeceleration;
    public float horizontalMaxRunningSpeed;
    public float horizontalMaxSpeed;

    public float verticalMaxSpeed;
    public float verticalFastFallingSpeed;

    [Header("Jumping")]
    public float jumpHeight;
    public float timeToJumpApex;

    [HideInInspector] public float jumpStartSpeed;
    [HideInInspector] public float gravity;

    public void Recalculate()
    {
        jumpHeight += 0.15f;
        timeToJumpApex += 0.03f;

        gravity = -2 * jumpHeight / (timeToJumpApex * timeToJumpApex);
        jumpStartSpeed = 2 * jumpHeight / timeToJumpApex;

        // Physics.gravity = new Vector3(0, gravity, 0);
    }
}

public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement Instance { get; private set; }

    #region Required Variables
    // Variabili che devono essere assegnate dall'inspector o che si riferiscono a classi esterne
    [Header("References")]
    [SerializeField] private CommonValues commonValues;
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private SpriteRenderer spriteRenderer;
    private SplineProjector _splineProjector;
    private Rigidbody rb;
    private Animator anim;
    private Transform model;

    [Header("Checks")]
    [SerializeField] private Checks _checks;

    [Header("Assist")]
    [SerializeField] private float _coyoteTime = 0.1f;
    [SerializeField] private float _jumpBufferTime = 0.1f;
    #endregion

    #region Class Variables
    // Variabili necessarie per il corretto funzionamento della classe
    [SerializeField] private float dashTime = 0.1f;

    private Vector2 moveInput;
    private bool wantsToJump;
    private float _lastTimeGrounded;
    private Vector3 originalScale;
    private float originalVelocity;
    private float velocityReduction;
    private Coroutine _WallJumpReduction;
    private Coroutine _JumpTimer;
    private int wallJumping = 0;
    private float dashTimer = 0;
    private bool wasDashing = false;
    #endregion

    #region Animations State variables
    private bool wasJumping = false;
    private bool wasFalling = false;
    private bool wasGrounded = false;
    private bool isTouchingWall;
    private bool isWallLanding;
    #endregion 

    #region Cached Variables
    // Sono variabili che vengono salvate solo per evitare di appesantire il Garbage Collector
    Vector3 _targetVelocity;
    Vector3 _velocityChange;
    Vector3 _zeroVelocity;
    Vector3 _forewardTimesSpeed;
    #endregion

    [SerializeField] private Vector3 velocity;

    public bool IsGrounded { get => Physics.CheckBox(_checks.groundCheck.position, _checks.groundCheckSize, Quaternion.identity, _checks.groundLayer); }
    public bool IsFront { get => Physics.CheckBox(_checks.frontCheck.position, _checks.frontCheckSize, Quaternion.identity, _checks.groundLayer); }
    public bool IsBack { get => Physics.CheckBox(_checks.backCheck.position, _checks.backCheckSize, Quaternion.identity, _checks.groundLayer); }
    public bool IsRunning { get { return Mathf.Abs(moveInput.x) > 0.1f; } }
    public bool IsDashing { get { return dashTimer > 0; } }
    public Vector2 MoveInput { get { return moveInput; } }

    public bool IsFacingRight
    {
        get
        {
            Transform model = transform.Find("adventurer-idle-00/Loky");
            if (model == null)
            {
                return true;
            }
            return model.localScale.x > 0;
        }
    }

    private void OnValidate()
    {
        Transform parent = transform.Find("Checks");
        _checks.groundCheck = parent.Find("Ground");
        _checks.frontCheck = parent.Find("Front");
        _checks.backCheck = parent.Find("Back");
    }

    private void Awake()
    {
        Instance = this;

        rb = GetComponent<Rigidbody>();
        _splineProjector = GetComponent<SplineProjector>();

        model = transform.GetChild(0).GetChild(0);
    }

    private void Start()
    {
        _zeroVelocity = Vector3.zero;
        velocityReduction = 1;

        playerStats.Recalculate();
        _checks.CorrectHalfSize();

        InputManager.Instance.OnMove += (move) => moveInput = move;
        InputManager.Instance.OnJump += () => Jump();
        InputManager.Instance.OnDash += () => Dash();

        anim = GetComponentInChildren<Animator>();

        /* StartCoroutine(CheckIfStuck()); */
        StartCoroutine(ChangeSpline());
    }

    IEnumerator ChangeSpline()
    {
        while (true)
        {
            yield return new WaitUntil(() => _splineProjector.spline == null);
            yield return new WaitUntil(() => _splineProjector.spline != null);
            commonValues.currentSpline = _splineProjector.spline;
        }
    }

    IEnumerator Spam(){
        while (true)
        {
            Jump();
            yield return null;
        }
    }

    void FixedUpdate()
    {
        #region Variables
        if (IsGrounded) _lastTimeGrounded = Time.time;

        _forewardTimesSpeed = _splineProjector.result.forward * playerStats.horizontalMaxRunningSpeed;

        #endregion

        #region Horizontal Movement

        // Ottieni la direzione della spline nella posizione attuale
        Vector3 forwardDirection = _splineProjector.result.forward;

        if (IsDashing)
        {
            wasDashing = true;
            rb.AddForce(.5f * Mathf.Sign(moveInput.x) * playerStats.horizontalMaxRunningSpeed * _splineProjector.result.forward, ForceMode.VelocityChange);
            dashTimer -= Time.fixedDeltaTime;
        }
        else if (wasDashing){
            var colliders = GetComponents<Collider>();
            foreach(var c in colliders){
                c.excludeLayers = 0;
            }

            _targetVelocity = _zeroVelocity;
            rb.linearVelocity = new Vector3(0, 0, 0);
            rb.AddForce(new Vector3(0, 0, 0), ForceMode.VelocityChange);
            wasDashing = false;
        }
        else if (moveInput.x > 0)
        {
            if(wallJumping != -1) _targetVelocity = _forewardTimesSpeed * velocityReduction;
            else _targetVelocity = rb.linearVelocity;

            _splineProjector.direction = Spline.Direction.Forward;
            model.localScale = new Vector3(0.5f, 0.5f, 0.5f); // Gira il modello a destra
        }
        else if (moveInput.x < 0)
        {
            if(wallJumping != 1) _targetVelocity = -_forewardTimesSpeed * velocityReduction;
            else _targetVelocity = rb.linearVelocity;

            _splineProjector.direction = Spline.Direction.Backward;
            model.localScale = new Vector3(-0.5f, 0.5f, 0.5f); // Gira il modello a sinistra
        }
        else {
            

            if(IsGrounded) _targetVelocity = _zeroVelocity;
            else _targetVelocity = rb.linearVelocity;
        }

        _velocityChange = playerStats.horizontalAcceleration * (_targetVelocity - rb.linearVelocity);
        _velocityChange.y = 0;
        rb.AddForce(_velocityChange, ForceMode.Acceleration);

        #endregion


        #region Vertical Movement
        if (wantsToJump && wallJumping == 0)
        {
            //AGGIUNTO IO
            BlockMovement(false);
            if (Time.time - _lastTimeGrounded < _coyoteTime)
            {
                wantsToJump = false;

                if(Input.GetKey(KeyCode.S)){
                    Physics.Raycast(transform.position, -_splineProjector.result.up, out RaycastHit hit, 5, LayerMask.GetMask("Ground"));
                
                    if(hit.collider != null && hit.collider.GetComponent<PassthroughFloor>() != null){
                        if(!hit.collider.GetComponent<PassthroughFloor>().isOneWay){
                            hit.collider.excludeLayers = LayerMask.GetMask("Player");
                            rb.linearVelocity = new Vector3(rb.linearVelocity.x,0,rb.linearVelocity.z);
                            rb.AddForce(-_splineProjector.result.up * (playerStats.jumpStartSpeed - rb.linearVelocity.y) * 0.5f, ForceMode.VelocityChange);
                        }
                    }

                } else rb.AddForce(_splineProjector.result.up * (playerStats.jumpStartSpeed - rb.linearVelocity.y), ForceMode.VelocityChange);
            }
            else if (IsFront)
            {
                wantsToJump = false;
                rb.linearVelocity = new Vector3(0,0,0);
                rb.AddForce((_splineProjector.result.up * (playerStats.jumpStartSpeed - rb.linearVelocity.y)) - _forewardTimesSpeed * .75f, ForceMode.VelocityChange);

                if (_WallJumpReduction != null) StopCoroutine(_WallJumpReduction);
                StartCoroutine(WallJumpReductionTmp(-1));
                // _WallJumpReduction = StartCoroutine(WallJumpReduction());
            }
            else if (IsBack)
            {
                wantsToJump = false;
                rb.linearVelocity = new Vector3(0,0,0);
                rb.AddForce((_splineProjector.result.up * (playerStats.jumpStartSpeed - rb.linearVelocity.y)) + _forewardTimesSpeed * .75f, ForceMode.VelocityChange);

                if (_WallJumpReduction != null) StopCoroutine(_WallJumpReduction);
                StartCoroutine(WallJumpReductionTmp(1));
                // _WallJumpReduction = StartCoroutine(WallJumpReduction());
            }
        }
        else
        {
            _velocityChange.y = 0;
        }

        if (IsGrounded)
        {
            rb.useGravity = true;
        }
        else
        {
            rb.useGravity = false;
            rb.AddForce(playerStats.gravity * _splineProjector.result.up, ForceMode.Acceleration);
        }
        #endregion
        
        #region Animations

        isTouchingWall = IsFront || IsBack;  // Calcola se il personaggio sta toccando un muro
        bool isMoving = rb.linearVelocity.x > 0.1f || rb.linearVelocity.z > 0.1f;   // Flag movimento orizzontale
        bool isFalling = rb.linearVelocity.y < 0 && !IsGrounded;                    // Caduta
        bool isJumping = wantsToJump || (!IsGrounded && rb.linearVelocity.y > 0);  // Salto

        // Aggiorna i parametri dell'Animator per il BlendTree o altre transizioni
        float speedNormalizzata = rb.linearVelocity.magnitude / playerStats.horizontalMaxRunningSpeed;
        anim.SetFloat("Speed",  Mathf.Clamp01(speedNormalizzata));
        anim.SetBool("IsGrounded", IsGrounded);
        anim.SetBool("IsFalling", isFalling);
        anim.SetBool("IsJumping", isJumping);
        anim.SetBool("isTouchingWall", isTouchingWall);

        // Blocco animazioni quando il personaggio è a terra
        if (IsGrounded)
        {
            isWallLanding = false; // Resetta lo stato Wall_Land quando atterra

        }
        else // In aria
        {
            if (isTouchingWall) 
            {
                if(!isWallLanding) {
                    isWallLanding = true;
                }
            }
        }

        // Aggiorna i flag per rilevare i cambi di stato al frame successivo
        wasJumping = isJumping;
        wasFalling = isFalling;
        wasGrounded = IsGrounded;

        #endregion

        velocity = rb.linearVelocity;
    }

    void Jump()
    {
        if(_JumpTimer != null) StopCoroutine(_JumpTimer);

        _JumpTimer = StartCoroutine(JumpTimer());
    }

    IEnumerator JumpTimer()
    {
        wantsToJump = true;
        yield return new WaitForSeconds(_jumpBufferTime);
        wantsToJump = false;
    }

    IEnumerator WallJumpReduction()
    {
        float time = 0;
        while (time < 1f)
        {
            time += Time.fixedDeltaTime;
            velocityReduction = Mathf.Lerp(0, .9f, time);
            yield return new WaitForFixedUpdate();
        }
        yield return new WaitForFixedUpdate();
        velocityReduction = 1;
        _WallJumpReduction = null;
    }

    IEnumerator WallJumpReductionTmp(int direction)
    {
        yield return new WaitForFixedUpdate();
        wallJumping = direction;
        yield return new WaitUntil(() => rb.linearVelocity.y <= 0);
        wallJumping = 0;
    }

    void Dash(){
        dashTimer = dashTime;
        var colliders = GetComponents<Collider>();

        foreach(var c in colliders){
            c.excludeLayers = (1 << 29) | (1 << 30);
        }
    }

    // void OnCollisionStay(Collision collision)
    // {
    //     if(collision.gameObject.layer > 28 && collision.gameObject.layer < 32){
    //         Debug.Log("stay "+collision.gameObject.name);
    //         transform.parent = collision.transform.parent;
    //         collision.transform.parent.GetComponent<ColumnController>().AttachedPlayer = transform;
    //     }
    // }

    // void OnCollisionExit(Collision collision)
    // {
    //     if(collision.gameObject.layer > 28 && collision.gameObject.layer < 32){
    //         Debug.Log("exit "+collision.gameObject.name);
    //         transform.parent = null;
    //         collision.transform.parent.GetComponent<ColumnController>().AttachedPlayer = null;
    //     }
    // }

    /*     void OnTriggerEnter(Collider other)
        {

            if (LayerMask.LayerToName(other.gameObject.layer) == "Column_1" || LayerMask.LayerToName(other.gameObject.layer) == "Column_2")
            {
                other.transform.GetComponent<ColumnController>().AttachPlayer(transform);
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (LayerMask.LayerToName(other.gameObject.layer) == "Column_1" || LayerMask.LayerToName(other.gameObject.layer) == "Column_2")
            {
                other.transform.GetComponent<ColumnController>().DetachPlayer();
            }
        } */

    /*  void OnCollisionEnter(Collision collision)
     {
         if (collision.collider.gameObject.layer == LayerMask.NameToLayer("UpperTrigger") || LayerMask.LayerToName(collision.gameObject.layer) == "Column_1" || LayerMask.LayerToName(collision.gameObject.layer) == "Column_2")
         {
             Debug.Log("Collison name " + collision.gameObject.name);
             // Imposta il parent del player uguale al parent del trigger toccato
             transform.parent = collision.transform.parent;

             Debug.Log($"Player agganciato a {collision.transform.parent.name}");
         }
     } */

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("UpperTrigger"))
        {
            transform.parent = other.transform.parent;
            other.transform.parent.GetComponent<ColumnController2>().AttachedPlayer = transform;
            Debug.Log($"Player agganciato alla colonna!");
            BlockMovement(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("UpperTrigger"))
        {
            transform.parent = null;
            other.transform.parent.GetComponent<ColumnController2>().AttachedPlayer = null;

            BlockMovement(false);
            Debug.Log("Player scollegato dalla colonna");
        }
    }


    void BlockMovement(bool value)
    {
        if (value)
        {
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }
        else
        {
            rb.constraints = RigidbodyConstraints.None;
            rb.constraints = RigidbodyConstraints.FreezeRotation;

        }
    }



    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(_checks.groundCheck.position, _checks.groundCheckSize);
        Gizmos.DrawWireCube(_checks.frontCheck.position, _checks.frontCheckSize);
        Gizmos.DrawWireCube(_checks.backCheck.position, _checks.backCheckSize);

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(0.05f, 0.2f, 0.05f));
    }
}