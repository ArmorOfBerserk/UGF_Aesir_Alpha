using System.Collections;
using UnityEngine;
using Dreamteck.Splines;

public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CommonValues commonValues;
    [SerializeField] private PlayerMovement player;
    [SerializeField] private SpriteRenderer spriteRenderer;
    private SplineProjector _splineProjector;
    private Rigidbody rb;
    private Transform model;
    private Animator anim;

    [Header("Movement")] 
    [SerializeField] private float speed = 3f;
    [SerializeField] private float acceleration = 2f;
    [SerializeField] private float followRange = 5f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float obstacleDetectionRange = 2f;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Patrol")] 
    [SerializeField] private Transform[] patrolPoints;
    private int currentPatrolIndex = 0;

    private bool isFollowingPlayer = false;
    private Vector3 _targetVelocity;
    private Vector3 _velocityChange;
    private Vector3 _zeroVelocity = Vector3.zero;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        _splineProjector = GetComponent<SplineProjector>();
        model = transform.GetChild(0).GetChild(0);
        anim = GetComponentInChildren<Animator>();
    }

    private void FixedUpdate()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        isFollowingPlayer = distanceToPlayer <= followRange;

        Vector3 forwardDirection = _splineProjector.result.forward;
        float velocityAlongSpline = Vector3.Dot(rb.linearVelocity, forwardDirection);

        if (velocityAlongSpline < 0) model.localScale = new Vector3(-0.5f, 0.5f, 0.5f);
        else if (velocityAlongSpline > 0) model.localScale = new Vector3(0.5f, 0.5f, 0.5f);

        if (isFollowingPlayer)
        {
            FollowPlayer(forwardDirection);
        }
        else
        {
            Patrol(forwardDirection);
        }

        _velocityChange = acceleration * (_targetVelocity - rb.linearVelocity);
        _velocityChange.y = 0;
        rb.AddForce(_velocityChange, ForceMode.Acceleration);
        anim.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.magnitude));
    }

    private void FollowPlayer(Vector3 forwardDirection)
    {
        Vector3 directionToPlayer = (player.transform.position - transform.position).normalized;
        float dotProduct = Vector3.Dot(forwardDirection, directionToPlayer);
        _targetVelocity = forwardDirection * speed * Mathf.Sign(dotProduct);

        Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;
        Vector3 rayDirection = _targetVelocity.normalized;
        Debug.DrawRay(rayOrigin, rayDirection * obstacleDetectionRange, Color.green);
        if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, obstacleDetectionRange, obstacleLayer))
        {
            Debug.Log("Ostacolo rilevato: " + hit.collider.name);
            Jump();
        }
    }

    private void Patrol(Vector3 forwardDirection)
    {
        if (patrolPoints.Length == 0) return;

        Transform targetPatrolPoint = patrolPoints[currentPatrolIndex];
        Vector3 directionToPatrolPoint = (targetPatrolPoint.position - transform.position).normalized;
        float dotProduct = Vector3.Dot(forwardDirection, directionToPatrolPoint);
        _targetVelocity = forwardDirection * speed * Mathf.Sign(dotProduct);

        if (Vector3.Distance(transform.position, targetPatrolPoint.position) < 0.5f)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        }

        Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;
        Vector3 rayDirection = _targetVelocity.normalized;
        Debug.DrawRay(rayOrigin, rayDirection * obstacleDetectionRange, Color.green);
        if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, obstacleDetectionRange, obstacleLayer))
        {
            Debug.Log("Ostacolo rilevato: " + hit.collider.name);
            Jump();
        }
    }

    private void Jump()
    {
        if (Mathf.Abs(rb.linearVelocity.y) < 0.1f)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, followRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position + Vector3.up * 0.5f, transform.position + Vector3.up * 0.5f + _targetVelocity.normalized * obstacleDetectionRange);
    }
}