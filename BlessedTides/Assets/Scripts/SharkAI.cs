using UnityEngine;

public class SharkAI : MonoBehaviour
{
    public float moveSpeed = 3f;               // Shark's movement speed
    public float rotationSpeed = 5f;          // Speed at which the shark rotates smoothly
    public float detectionRadius = 10f;       // Radius within which the shark detects the boat
    public float attackDistance = 1f;         // Distance at which the shark stops near the boat
    public Transform boat;                    // Reference to the boat transform
    private Vector3 roamTarget;               // Target position for free roaming
    private bool isAttacking = false;         // Is the shark currently attacking?
    public float fixedYPosition;              // Fixed Y position for the shark
    BoatHealth boatHealth;
    public int damageMake;

    public Animator animator;
    public float damageCooldown = 0.2f;      // Time between each damage instance
    private float lastDamageTime = -999f;     // Tracks the last time damage was dealt

    private void Start()
    {
        boatHealth = boat.GetComponent<BoatHealth>();
        GenerateNewRoamTarget();
    }

    private void Update()
    {
        if (Vector3.Distance(transform.position, boat.position) <= detectionRadius)
        {
            // Move towards the boat if within detection radius
            isAttacking = true;
            MoveTowardsBoat();
        }
        else
        {
            // Free roam if boat is not detected
            isAttacking = false;
            FreeRoam();
        }
    }

    private void MoveTowardsBoat()
    {
        Vector3 directionToBoat = (boat.position - transform.position).normalized;

        // Stop at the attack distance
        if (Vector3.Distance(transform.position, boat.position) > attackDistance)
        {
            // Smoothly rotate towards the boat
            Quaternion targetRotation = Quaternion.LookRotation(new Vector3(directionToBoat.x, 0f, directionToBoat.z)); // Ignore Y-axis for rotation
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            // Move forward on X and Z, keep Y static
            Vector3 movement = transform.forward * moveSpeed * 1.5f * Time.deltaTime;
            transform.position = new Vector3(transform.position.x + movement.x, fixedYPosition, transform.position.z + movement.z);
        }
        else
        {
            DealDamageToBoat();
        }
    }

    private void FreeRoam()
    {
        // Move towards roam target
        Vector3 directionToTarget = (roamTarget - transform.position).normalized;

        if (Vector3.Distance(transform.position, roamTarget) > 1.5f)
        {
            // Smoothly rotate towards roam target
            Quaternion targetRotation = Quaternion.LookRotation(new Vector3(directionToTarget.x, 0f, directionToTarget.z)); // Ignore Y-axis for rotation
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            // Move forward on X and Z, keep Y static
            Vector3 movement = transform.forward * moveSpeed * Time.deltaTime;
            transform.position = new Vector3(transform.position.x + movement.x, fixedYPosition, transform.position.z + movement.z);
        }
        else
        {
            // Generate a new roam target when close to the current one
            GenerateNewRoamTarget();
        }
    }

    private void DealDamageToBoat()
    {
        if (Time.time >= lastDamageTime + damageCooldown)
        {
            // Trigger damage and animation
            boatHealth.TakeDamage(damageMake);
            animator.SetTrigger("eat");

            // Update last damage time
            lastDamageTime = Time.time;
        }
    }

    private void GenerateNewRoamTarget()
    {
        float roamRadius = 10f;
        Vector3 randomDirection = Random.insideUnitSphere * roamRadius;
        randomDirection.y = 0; // Keep the roam target on the same horizontal plane
        roamTarget = new Vector3(transform.position.x + randomDirection.x, fixedYPosition, transform.position.z + randomDirection.z);
    }

    private void OnDrawGizmos()
    {
        // Draw detection radius for visualization
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
