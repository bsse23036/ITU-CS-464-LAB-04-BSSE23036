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
    bool isGrounded, jumpQueued;
    Vector3 groundNormal = Vector3.up;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        
        // Cache the main camera to use its rotation for movement
        mainCam = Camera.main.transform; 
    }

    void Update()
    {
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
            jumpQueued = true;
    }

    void FixedUpdate()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // --- CAMERA-RELATIVE MOVEMENT MATH ---
        // 1. Get the camera's forward and right directional vectors
        Vector3 camForward = mainCam.forward;
        Vector3 camRight = mainCam.right;

        // 2. Flatten the vectors on the Y axis so looking down doesn't push the player into the floor
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        // 3. Multiply inputs by the camera's directions instead of global world axes
        Vector3 move = (camForward * v) + (camRight * h);
        move = Vector3.ClampMagnitude(move, 1f) * speed;

        // Slope movement calculation
        Vector3 slopeMove = Vector3.ProjectOnPlane(move, groundNormal).normalized * move.magnitude;

        if (isGrounded && !jumpQueued)
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
            else if (rb.linearVelocity.y > 0 && !Input.GetKey(KeyCode.Space))
            {
                rb.linearVelocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
            }
        }

        if (jumpQueued)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
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