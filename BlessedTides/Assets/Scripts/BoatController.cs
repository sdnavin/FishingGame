using UnityEngine;

public class BoatController : MonoBehaviour
{
    public float speed = 5f;            // Movement speed
    public float turnSpeed = 180f;     // Turning speed
    public float swayAmount = 2f;      // The amount the boat sways left/right
    public float swaySpeed = 2f;       // Speed of the swaying motion

    private Rigidbody rb;              // Reference to the Rigidbody component
    private float swayTimer = 0f;      // Timer for the swaying motion
    private Transform childTransform;  // Reference to the first child object
    private Vector2 joystickInput = Vector2.zero; // Joystick input values

    public int slotID;
    public int slotoffset=1;
    private void Start()
    {
        rb = GetComponent<Rigidbody>(); // Get the Rigidbody component
        if (rb != null)
        {
            rb.useGravity = false;      // Disable gravity to prevent Y-axis movement
            rb.constraints = RigidbodyConstraints.FreezeRotationX |
                              RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionY; // Freeze X/Z rotations
        }

        // Get the first child object of the boat (adjust this if needed)
        childTransform = transform.GetChild(0);
    }

    private void FixedUpdate()
    {
        HandleMovementJoystick();
        // Boat movement controls (using arrow keys or joystick)
        float moveForward = (Input.GetAxis("Vertical")) * speed;
        float turn = (Input.GetAxis("Horizontal")) * turnSpeed * Time.fixedDeltaTime;
       
        // Calculate the forward movement direction
        Vector3 forwardMovement = transform.forward * moveForward * Time.fixedDeltaTime;

        // Move the boat using Rigidbody
        rb.MovePosition(rb.position + forwardMovement);

        // Rotate the boat using Rigidbody
        Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);
        rb.MoveRotation(rb.rotation * turnRotation);

        // Keep the boat at a constant height (no Y-axis movement)
        Vector3 position = rb.position;
        position.y = 0f; // Keep Y position at 0 (or adjust to the desired height)
        rb.position = position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Portal")
        {
            other.GetComponent<PortalReveal>().OnReveal();
        }
    }

    private void HandleMovementJoystick()
    {
        if (WebSocketClient.dataIn[slotID] == null)
            return;
        //if (slotID != WebSocketClient.dataIn[slotID].data.slotId)
        //{
        //    return;
        //}
        if (WebSocketClient.dataIn != null)
            joystickInput = Vector2.Lerp(joystickInput, new Vector2((float)WebSocketClient.dataIn[slotID].data.x * slotoffset, (float)WebSocketClient.dataIn[slotID].data.y* slotoffset),speed*Time.deltaTime);
        else
        {
            joystickInput = Vector2.zero;
        }
        // Get joystick input
        // If there's no input, don't move the boat
        //if (joystickInput == Vector2.zero)
        //    return;

        // Normalize the input for consistent movement
        Vector2 normalizedInput = joystickInput;

        // Calculate the movement direction based on joystick input
        Vector3 direction = new Vector3(normalizedInput.x, 0, normalizedInput.y);

        // Move the boat in the direction of the joystick input
        transform.Translate(direction * speed * Time.deltaTime, Space.World);

        // Rotate the boat to face the movement direction
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * speed);
        }
      
    }

    private void Update()
    {
        // Add slight swaying effect (rotate the first child object left/right)
        SwayEffect();
    }

    // Sway effect simulating boat rocking, applied to the first child object
    void SwayEffect()
    {
        swayTimer += Time.deltaTime * swaySpeed;
        float sway = Mathf.Sin(swayTimer) * swayAmount; // Sinusoidal movement for smooth swaying

        // Apply sway only to the first child's rotation (without affecting the parent)
        if (childTransform != null)
        {
            Vector3 currentRotation = childTransform.localEulerAngles;
            childTransform.localRotation = Quaternion.Euler(currentRotation.x, currentRotation.y, sway);
        }
    }

    // Optionally add an up/down wave effect for a more dynamic boat motion
    void SwayUpDownEffect()
    {
        float waveHeight = Mathf.Sin(Time.time * swaySpeed) * 0.1f; // Up/down motion
        Vector3 position = rb.position;
        position.y += waveHeight;
        rb.position = position;
    }
}
