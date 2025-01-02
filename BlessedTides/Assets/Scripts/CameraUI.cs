using UnityEngine;

public class CameraUI : MonoBehaviour
{
    public Camera targetCamera; // Assign the camera in the Inspector

    void Start()
    {
        // If no camera is assigned, default to the main camera
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    void Update()
    {
        // Make the canvas face the camera
        transform.LookAt(targetCamera.transform);

        // Optionally, invert the rotation to prevent flipping
        transform.rotation = Quaternion.LookRotation(transform.position - targetCamera.transform.position);
    }
}
