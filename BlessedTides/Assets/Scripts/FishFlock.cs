using UnityEngine;
using UnityEngine.Events;

public class FishFlock : MonoBehaviour
{
    public float speed = 2f;                // Speed of the fish
    public float rotationSpeed = 5f;        // Speed of the fish's rotation
    public float radius = 5f;               // Radius within which the fish will move
    public float neighborDistance = 1f;     // Minimum distance from others (not used in this simplified version)

    private Vector3 targetPosition;         // Target position that the fish is moving towards


    void Start()
    {
        // Set initial random target position within the flock area
        targetPosition = GetRandomPositionWithinRadius();
    }

    void Update()
    {
        // Move the fish towards its target
        MoveFish();

        // If the fish is near its target, assign a new random target within the radius
        if (Vector3.Distance(transform.position, targetPosition) < 0.5f)
        {
            targetPosition = GetRandomPositionWithinRadius();
        }

        // Ensure the fish remains at Y = 0
        transform.position = new Vector3(transform.position.x, 0f, transform.position.z);
    }

    private void MoveFish()
    {
        // Move the fish towards the target position
        Vector3 direction = (targetPosition - transform.position).normalized;
        transform.Translate(direction * speed * Time.deltaTime, Space.World);

        // Rotate the fish smoothly towards the target position
        Vector3 targetDirection = targetPosition - transform.position;
        if (targetDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private Vector3 GetRandomPositionWithinRadius()
    {
        // Get a random position within a sphere of the given radius
        Vector3 randomDirection = Random.insideUnitSphere * radius;
        randomDirection += transform.position; // Set the center to the fish's current position
        randomDirection.y = 0; // Keep the fish on a flat plane (Y=0)
        return randomDirection;
    }
}
