using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] float speed = 5f;
    [SerializeField] float jumpForce = 6f;
    
    [Header("Jump Feel Settings")]
    [SerializeField] float fallMultiplier = 2.5f;
    [SerializeField] float lowJumpMultiplier = 2f;

    Rigidbody rb;
    Transform mainCam;
    
    bool isGrounded, jumpQueued, isJumpHolding;
    Vector3 groundNormal = Vector3.up;
    float lastJumpTime = -10f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        mainCam = Camera.main.transform; 
    }

    void Update()
    {
        // 1. Read holding inputs in Update for frame-perfect accuracy
        isJumpHolding = Input.GetKey(KeyCode.Space);
        
        // 2. Add a tiny cooldown so we don't double-queue jumps
        if (isGrounded && Input.GetKeyDown(KeyCode.Space) && Time.time > lastJumpTime + 0.2f)
        {
            jumpQueued = true;
        }
    }

    void FixedUpdate()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 camForward = mainCam.forward;
        Vector3 camRight = mainCam.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 move = (camForward * v) + (camRight * h);
        move = Vector3.ClampMagnitude(move, 1f) * speed;
        Vector3 slopeMove = Vector3.ProjectOnPlane(move, groundNormal).normalized * move.magnitude;

        // 3. Check if we JUST jumped in the last 0.1 seconds
        bool recentlyJumped = Time.time < lastJumpTime + 0.1f;

        // 4. Only snap to the ground if we didn't just launch into the air
        if (isGrounded && !jumpQueued && !recentlyJumped)
        {
            rb.useGravity = false;
            rb.linearVelocity = slopeMove;
        }
        else
        {
            rb.useGravity = true;
            rb.linearVelocity = new Vector3(move.x, rb.linearVelocity.y, move.z);

            if (rb.linearVelocity.y < 0)
            {
                rb.linearVelocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
            }
            // 5. Use the safe 'isJumpHolding' bool from Update() to check for short hops
            else if (rb.linearVelocity.y > 0 && !isJumpHolding)
            {
                rb.linearVelocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
            }
        }

        if (jumpQueued)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            lastJumpTime = Time.time;
            jumpQueued = false;
        }

        isGrounded = false;
        groundNormal = Vector3.up;
    }

    void OnCollisionStay(Collision c)
    {
        foreach (ContactPoint contact in c.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                isGrounded = true;
                groundNormal = contact.normal;
                break;
            }
        }
    }
}