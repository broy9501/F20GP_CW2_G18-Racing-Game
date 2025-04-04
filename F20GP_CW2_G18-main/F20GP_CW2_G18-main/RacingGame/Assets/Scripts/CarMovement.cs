using UnityEngine;

public class CarController : MonoBehaviour
{
    public float maxSpeed = 150f;

    public float acceleration = 30f;

    public float deceleration = 10f;

    public float braking = 60f;

    public float steering = 3.5f;

    [Range(0f, 1f)]
    public float driftFactor = 0.7f;

    public float driftRecoveryFactor = 5f;

    public float bodyTiltAmount = 5f;

    public Transform carBody; 

    private Rigidbody rb;
    private float currentSpeed;
    private float speedInput;
    private float turnInput;
    private bool isBraking;
    private Vector3 moveDirection;
    private float lateralVelocity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0, -0.5f, 0);

        rb.constraints = RigidbodyConstraints.FreezeRotationY;
    }

    private void Update()
    {
        speedInput = 0;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            speedInput = 1;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            speedInput = -1;

        turnInput = Input.GetAxis("Horizontal");
        isBraking = Input.GetKey(KeyCode.Space);

        HandleCarTilt();
    }

    private void FixedUpdate()
    {
        currentSpeed = Vector3.Dot(rb.linearVelocity, transform.forward);

        lateralVelocity = Vector3.Dot(rb.linearVelocity, transform.right);

        if (speedInput != 0)
        {
            rb.AddForce(transform.forward * speedInput * acceleration, ForceMode.Acceleration);
        }
        else
        {
            rb.AddForce(-rb.linearVelocity.normalized * deceleration, ForceMode.Acceleration);
        }

        if (isBraking)
        {
            rb.AddForce(-rb.linearVelocity.normalized * braking, ForceMode.Acceleration);
        }

        float turnStrength = turnInput * steering;

        float speedFactor = Mathf.Clamp01(currentSpeed / (maxSpeed / 3.6f));
        turnStrength *= Mathf.Lerp(1f, 0.5f, speedFactor);

        if (Mathf.Abs(currentSpeed) > 0.1f) // Only steer when moving
        {
            float turnAngle = turnInput * steering * Time.fixedDeltaTime;
            transform.Rotate(0, turnAngle, 0);
        }
        ApplyDrift();

        if (rb.linearVelocity.magnitude > maxSpeed / 3.6f)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed / 3.6f;
        }
    }

    private void ApplyDrift()
    {
        Vector3 forwardVelocity = transform.forward * Vector3.Dot(rb.linearVelocity, transform.forward);
        Vector3 rightVelocity = transform.right * Vector3.Dot(rb.linearVelocity, transform.right);

        float driftReduction = isBraking ? driftFactor * 1.2f : driftFactor;

        rb.linearVelocity = forwardVelocity + rightVelocity * driftReduction;

        if (Mathf.Abs(lateralVelocity) > 3f && Mathf.Abs(currentSpeed) > 10f)
        {
            float driftTorque = lateralVelocity * 0.05f;
            rb.AddTorque(transform.up * driftTorque, ForceMode.Acceleration);
        }
    }

    private void HandleCarTilt()
    {
        if (carBody != null)
        {
            Quaternion targetRotation = Quaternion.Euler(
                0,
                0,
                -turnInput * bodyTiltAmount
            );

            carBody.localRotation = Quaternion.Slerp(
                carBody.localRotation,
                targetRotation,
                Time.deltaTime * 5f
            );
        }
    }
}