using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class FirstPersonController : MonoBehaviour
{
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float runSpeed = 7f;
    [SerializeField] private float jumpSpeed = 5f;
    [SerializeField] private float lookSpeed = 2f;

    private Rigidbody rb;
    private CapsuleCollider capsule;
    private float pitch;
    private bool jumpRequested;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();
        rb.freezeRotation = true;
    }

    void Start()
    {
        LockCursor(true);
    }

    void Update()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            transform.Rotate(0f, Input.GetAxis("Mouse X") * lookSpeed, 0f);
            pitch = Mathf.Clamp(pitch - Input.GetAxis("Mouse Y") * lookSpeed, -85f, 85f);
            if (cameraPivot != null)
                cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        if (Input.GetKeyDown(KeyCode.Space))
            jumpRequested = true;

        if (Input.GetKeyDown(KeyCode.Escape))
            LockCursor(false);
        else if (Input.GetMouseButtonDown(0))
            LockCursor(true);
    }

    void FixedUpdate()
    {
        var input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
        input = Vector3.ClampMagnitude(input, 1f);
        float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

        Vector3 velocity = transform.TransformDirection(input) * speed;
        velocity.y = rb.linearVelocity.y;

        if (jumpRequested && IsGrounded())
            velocity.y = jumpSpeed;
        jumpRequested = false;

        rb.linearVelocity = velocity;
    }

    private bool IsGrounded()
    {
        float distance = capsule.height / 2f + 0.1f;
        return Physics.Raycast(transform.position, Vector3.down, distance, ~0, QueryTriggerInteraction.Ignore);
    }

    private static void LockCursor(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}
