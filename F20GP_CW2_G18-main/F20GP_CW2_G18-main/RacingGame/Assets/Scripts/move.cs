using UnityEngine;

public class move : MonoBehaviour
{
    // Rigidbody for physics
    public Rigidbody rb;
    // Base speed and steering sensitivity
    public float speed = 20f;
    public float steering = 3f;
    // Maximum speed in km/h (used to adjust turning at high speeds)
    public float maxSpeed = 150f;

    // How fast the car accelerates and slows down
    public float acceleration = 5f;
    public float deceleration = 3f;
    private float currentSpeed = 0f;

    // For tilting the car body during turns
    public Transform carBody;
    public float bodyTiltAmount = 5f;

    // Wheels and their properties
    public Transform[] wheels;
    public float wheelRadius = 0.4f;
    public float steeringAngle = 30f;
    public bool[] isSteeringWheel; // Marks which wheels steer
    public bool isInvertedControls = false; // Option to invert controls

    // Off-track behavior
    public bool isOffTrack = false;
    public float offTrackDecelaration = 3.5f;

    // Jumping
    public float jumpForce = 5f;
    public bool isGrounded = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        // Freeze unwanted rotations
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        // Initialize steering wheel flags if needed
        if (wheels != null && (isSteeringWheel == null || isSteeringWheel.Length != wheels.Length))
        {
            isSteeringWheel = new bool[wheels.Length];
            if (wheels.Length >= 2)
            {
                isSteeringWheel[0] = true;
                isSteeringWheel[1] = true;
            }
        }
    }

    void Update()
    {
        // Tilt the car body based on horizontal input
        if (carBody != null)
        {
            float tiltAmount = -Input.GetAxis("Horizontal") * bodyTiltAmount;
            Quaternion targetRotation = Quaternion.Euler(0, 0, tiltAmount);
            carBody.localRotation = Quaternion.Slerp(carBody.localRotation, targetRotation, Time.deltaTime * 5f);
        }

        // Jump if Space is pressed and car is grounded
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Jump();
        }
    }

    void FixedUpdate()
    {
        // Reset rotation if R is pressed
        if (Input.GetKeyDown(KeyCode.R))
        {
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
            rb.angularVelocity = Vector3.zero;
        }

        float verticalInput = Input.GetAxis("Vertical");
        float horizontalInput = Input.GetAxis("Horizontal");

        // Invert controls if needed
        if (isInvertedControls)
        {
            verticalInput *= -1;
            horizontalInput *= -1;
        }

        // Set target speed from vertical input
        float targetSpeed = speed * verticalInput;
        float effectDeceleration = isOffTrack ? deceleration * offTrackDecelaration : deceleration;

        // Accelerate or decelerate
        if (Mathf.Abs(verticalInput) > 0.01f)
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.deltaTime);
        else
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0, effectDeceleration * Time.deltaTime);

        // Move the car forward
        rb.MovePosition(rb.position + transform.forward * currentSpeed * Time.deltaTime);

        // Turn the car based on horizontal input
        if (Mathf.Abs(horizontalInput) > 0.01f)
        {
            float turnStrength = horizontalInput * steering;
            float speedFactor = Mathf.Clamp01(Mathf.Abs(currentSpeed) / (maxSpeed / 3.6f));
            turnStrength *= Mathf.Lerp(1f, 0.5f, speedFactor);
            transform.Rotate(0, turnStrength * Time.deltaTime * 50f, 0);
        }

        UpdateWheels(horizontalInput);
    }

    // Update wheel rotation and steering visuals
    void UpdateWheels(float steerInput)
    {
        float wheelRPM = Mathf.Abs(currentSpeed) / (2f * Mathf.PI * wheelRadius) * 60f;
        float rotationSpeed = (wheelRPM / 60f) * 360f;
        float rotationDirection = Mathf.Sign(currentSpeed);

        for (int i = 0; i < wheels.Length; i++)
        {
            if (wheels[i] != null)
            {
                bool isBackWheel = (isSteeringWheel != null && i < isSteeringWheel.Length && !isSteeringWheel[i]);
                float wheelRotationModifier = isBackWheel ? -1f : 1f;
                wheels[i].Rotate(rotationSpeed * rotationDirection * wheelRotationModifier * Time.deltaTime, 0, 0, Space.Self);

                // Adjust steering for wheels that can steer
                if (isSteeringWheel != null && i < isSteeringWheel.Length && isSteeringWheel[i])
                {
                    float steerAngle = steerInput * steeringAngle;
                    wheels[i].localRotation = Quaternion.Euler(
                        wheels[i].localRotation.eulerAngles.x,
                        steerAngle,
                        wheels[i].localRotation.eulerAngles.z
                    );
                }
            }
        }
    }

    public void ResetMovement()
    {
        currentSpeed = 0f;           // Reset the internal speed tracker
        rb.linearVelocity = Vector3.zero;  // Clear any existing movement
    }


    // Make the car jump
    void Jump()
    {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        isGrounded = false;
    }

    // Set isGrounded based on collisions with "Track" or "Floor"
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Track") || collision.gameObject.CompareTag("Floor"))
            isGrounded = true;
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Track") || collision.gameObject.CompareTag("Floor"))
            isGrounded = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Track") || collision.gameObject.CompareTag("Floor"))
            isGrounded = false;
    }
}
