using UnityEngine;
using UnityEngine.Audio;

public class FishCatcher : MonoBehaviour
{
    public GameObject boat;               // Reference to the boat object
    public GameObject boatCatch;               // Reference to the boat object
    public float catchDistance = 5f;      // The radius within which the boat can catch fish
    public float followTime = 2f;         // Time to follow the fish before catching it
    public LineRenderer lineRenderer;     // Line renderer to visualize the catching process
    public int fishWeight = 5;            // Weight of each fish caught (adjust as needed)

    private GameObject currentFish;       // The fish currently being followed by the boat
    private float followTimer = 0f;       // Timer to track the 2 seconds of following
    private BoatInventory boatInventory;  // Reference to the boat's inventory system

    private bool isCatching = false;      // Is the catching process active
    private Vector3 fishStartPosition;    // Initial position of the fish
    [SerializeField]
    AudioClip[] allClips;
    [SerializeField]
    AudioSource audioSource;
    [SerializeField]
    CircleProgress circleProgress;
    [SerializeField]
    Animator fishingrod;
    private void Start()
    {
        lineRenderer.enabled = false; // Initially disable the line renderer
        boatInventory = GetComponent<BoatInventory>();

        if (boatInventory == null)
        {
            Debug.LogError("BoatInventory script not found on the boat!");
        }
        circleProgress.progress=(0);
    }
    void Update()
    {
        if(currentFish != null)
        {
            circleProgress.transform.position = currentFish.transform.position;
        }
        // If the boat is currently catching a fish
        if (isCatching && currentFish != null)
        {
            followTimer += Time.deltaTime;

            // Animate the line from the boat to the fish
            float t = followTimer / followTime; // Normalized time
            t = Mathf.Clamp01(t);              // Ensure t is between 0 and 1
            // Set the positions of the line
            lineRenderer.SetPosition(0, lineRenderer.transform.position);  // Start position (boat's position)
            lineRenderer.SetPosition(1, Vector3.Lerp(fishStartPosition, lineRenderer.transform.position, t)); // Animate to boat

            // Move the fish along the line
            currentFish.transform.position = Vector3.Lerp(fishStartPosition, lineRenderer.transform.position, t);

            // If the line animation is complete, catch the fish
            if (t >= 1f)
            {
                audioSource.PlayOneShot(allClips[0]);
                CatchFish();
                circleProgress.progress=(0);
            }
        }
        else if (currentFish == null) // Look for fish if no catching is in progress
        {
            CheckForFishInRange();
        }
        else
        {
            if (!isCatching)
            {
                followTimer += Time.deltaTime;

                // Animate the line from the boat to the fish
                float t = followTimer / followTime; // Normalized time
                t = Mathf.Clamp01(t);              // Ensure t is between 0 and 1
                circleProgress.progress = (t);

                // Set the positions of the line
                lineRenderer.SetPosition(0, lineRenderer.transform.position);  // Start position (boat's position)
                lineRenderer.SetPosition(1, Vector3.Lerp(lineRenderer.transform.position, currentFish.transform.position, t)); // Animate to boat
                if (t >= 1f)
                {
                    circleProgress.progress = (0);

                    isCatching = true;
                    fishingrod.SetTrigger("catch");
                    currentFish.GetComponent<FishFlock>().enabled = false;
                    followTimer = 0;
                    fishStartPosition = currentFish.transform.position; // Save initial fish position
                }
            }
        }
    }


    private void CheckForFishInRange()
    {
        if (boatInventory.isFull)
            return;
        // Find all fish within the radius around the boat
        Collider[] fishInRange = Physics.OverlapSphere(boatCatch.transform.position, catchDistance);

        foreach (var fish in fishInRange)
        {
            if (fish.CompareTag("Fish")) // Assuming all fish have the "Fish" tag
            {
                currentFish = fish.gameObject;  // Assign the fish to be caught
                followTimer = 0f;               // Reset the follow timer
                lineRenderer.enabled = true;    // Enable the line renderer to show the connection
                audioSource.PlayOneShot(allClips[1]);
                //isCatching = true;
                break;                          // Exit loop after finding the first fish
            }
        }
    }


    private void CatchFish()
    {
        if (currentFish != null)
        {
            // Attempt to add fish to the boat inventory
            if (boatInventory.AddToBoat(fishWeight))
            {
                // If successful, deactivate the fish (simulate catching it)
                currentFish.SetActive(false);
                Debug.Log("Fish caught and added to inventory.");
            }
            else
            {
                Debug.Log("Boat is full! Cannot catch this fish.");
            }

            // Reset the line renderer and timer
            lineRenderer.enabled = false;
            followTimer = 0f;
            isCatching = false;
            currentFish = null; // No fish is being followed anymore
        }
    }
}
