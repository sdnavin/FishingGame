using UnityEngine;
using UnityEngine.UI;

public class BaseDirectionIndicator : MonoBehaviour
{
    public Transform baseTransform;         // The base's Transform
    public Transform boat;         // The base's Transform
    public RectTransform arrowUI;          // The UI arrow to indicate the base
    public Camera mainCamera;              // The main camera
    public Canvas canvas;                  // The canvas containing the arrow
    public float edgeOffset = 50f;         // Offset from the edge of the screen for the arrow

    private RectTransform canvasRect;      // Reference to the Canvas RectTransform

    void Start()
    {
        if (canvas != null)
            canvasRect = canvas.GetComponent<RectTransform>();
    }

    void Update()
    {
        if (baseTransform == null || mainCamera == null || arrowUI == null || canvas == null)
            return;

        Vector3 screenPosition = mainCamera.WorldToScreenPoint(baseTransform.position);

        // Check if the base is within the camera's view
        bool isOffScreen = screenPosition.z < 0 ||
                           screenPosition.x < 0 || screenPosition.x > Screen.width ||
                           screenPosition.y < 0 || screenPosition.y > Screen.height;

        if (isOffScreen)
        {
            arrowUI.gameObject.SetActive(true);

            // Clamp the arrow to stay on the canvas edge
            Vector2 clampedPosition = ClampToCanvasEdge(screenPosition);

            // Position the arrow in the UI
            arrowUI.anchoredPosition = clampedPosition;

            // Rotate the arrow to point towards the base
            Vector3 direction = baseTransform.position - boat.transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            arrowUI.rotation = Quaternion.Euler(0, 0, angle);
        }
        else
        {
            arrowUI.gameObject.SetActive(false); // Hide the arrow if the base is visible
        }
    }

    private Vector2 ClampToCanvasEdge(Vector3 screenPosition)
    {
        Vector2 viewportPosition = mainCamera.ScreenToViewportPoint(screenPosition);
        Vector2 canvasPosition = new Vector2(
            viewportPosition.x * canvasRect.sizeDelta.x - canvasRect.sizeDelta.x * 0.5f,
            viewportPosition.y * canvasRect.sizeDelta.y - canvasRect.sizeDelta.y * 0.5f
        );

        // Clamp the arrow position within canvas bounds
        canvasPosition.x = Mathf.Clamp(canvasPosition.x, -canvasRect.sizeDelta.x / 2 + edgeOffset, canvasRect.sizeDelta.x / 2 - edgeOffset);
        canvasPosition.y = Mathf.Clamp(canvasPosition.y, -canvasRect.sizeDelta.y / 2 + edgeOffset, canvasRect.sizeDelta.y / 2 - edgeOffset);

        return canvasPosition;
    }
}
