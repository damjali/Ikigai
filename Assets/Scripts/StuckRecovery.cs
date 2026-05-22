using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class StuckRecovery : MonoBehaviour
{
    [Header("Settings")]
    public float checkInterval = 0.5f;
    public float movementThreshold = 0.05f;
    public float recoveryDuration = 1f;
    public float recoverySpeedMultiplier = 1.2f;

    private Rigidbody2D rb;
    private Vector2 lastPosition;
    private float checkTimer;
    private float recoveryTimer;
    private bool isRecovering;
    private Vector2 recoveryDirection;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        lastPosition = transform.position;
    }

    void Update()
    {
        if (isRecovering)
        {
            recoveryTimer -= Time.deltaTime;
            if (recoveryTimer <= 0)
            {
                isRecovering = false;
            }
            return;
        }

        checkTimer += Time.deltaTime;
        if (checkTimer >= checkInterval)
        {
            float distanceMoved = Vector2.Distance(transform.position, lastPosition);
            
            // If we are trying to move (velocity > 0) but haven't moved much
            if (rb.linearVelocity.sqrMagnitude > 0.1f && distanceMoved < movementThreshold)
            {
                TriggerRecovery();
            }

            lastPosition = transform.position;
            checkTimer = 0;
        }
    }

    void FixedUpdate()
    {
        if (isRecovering)
        {
            rb.linearVelocity = recoveryDirection * (rb.linearVelocity.magnitude > 0 ? rb.linearVelocity.magnitude * recoverySpeedMultiplier : 2f);
        }
    }

    public void TriggerRecovery()
    {
        isRecovering = true;
        recoveryTimer = recoveryDuration;

        // Try to move in a random perpendicular direction or backwards
        Vector2 currentVelocityDir = rb.linearVelocity.normalized;
        float angle = Random.Range(90f, 270f); // Move away from the current direction
        recoveryDirection = Quaternion.Euler(0, 0, angle) * currentVelocityDir;

        Debug.Log(gameObject.name + " is stuck! Attempting recovery...");
    }

    public bool IsRecovering => isRecovering;
}