using UnityEngine;

public class DraggableContainer : MonoBehaviour
{
    // Set true or false to restrict movement in each axis.
    public bool allowHorizontal = true;
    public bool allowVertical = true;

    // Minimum and maximum allowed local positions (relative to the parent if one exists).
    public Vector2 minLocalPosition;
    public Vector2 maxLocalPosition;

    // Internal variables to record the initial states when a drag starts.
    private Vector3 initialMousePos;
    private Vector3 initialContainerPos;
    private bool isDragging = false;

    // Reference to the BoxCollider2D on this container.
    private BoxCollider2D boxCollider;

    private void Start()
    {
        // Get the BoxCollider2D component
        boxCollider = GetComponent<BoxCollider2D>();
        // Set the collider size based on children (if any)
        UpdateColliderSize();
    }

    private void Update()
    {
        // In case the container grows (children added/removed), update the collider's size.
        UpdateColliderSize();
    }

    // Updates the BoxCollider2D to encompass all children.
    private void UpdateColliderSize()
    {
        if (boxCollider == null)
            return;

        if (transform.childCount > 0)
        {
            // Initialize bounds with the first child's local position.
            Bounds bounds = new Bounds(transform.GetChild(0).localPosition, Vector3.zero);
            foreach (Transform child in transform)
            {
                bounds.Encapsulate(child.localPosition);
            }
            // Optionally add some padding to the bounds if needed.
            boxCollider.offset = bounds.center;
            boxCollider.size = bounds.size;
        }
    }

    private void OnMouseDown()
    {
        // When a drag begins, record the mouse's world position and the container's position.
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = transform.position.z; // Preserve z axis.
        initialMousePos = mouseWorldPos;
        initialContainerPos = transform.position;
        isDragging = true;
    }

    private void OnMouseDrag()
    {
        if (!isDragging)
            return;

        // Get the current mouse position in world coordinates.
        Vector3 currentMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        currentMousePos.z = transform.position.z;

        // Calculate the new target container position by adding the difference to the initial container position.
        Vector3 targetPos = initialContainerPos + (currentMousePos - initialMousePos);

        // If an axis is locked, preserve the original value.
        if (!allowHorizontal)
            targetPos.x = initialContainerPos.x;
        if (!allowVertical)
            targetPos.y = initialContainerPos.y;

        // Convert targetPos to local space (if a parent exists) so we can clamp it.
        Vector3 localTarget = transform.parent != null
            ? transform.parent.InverseTransformPoint(targetPos)
            : targetPos;

        localTarget.x = Mathf.Clamp(localTarget.x, minLocalPosition.x, maxLocalPosition.x);
        localTarget.y = Mathf.Clamp(localTarget.y, minLocalPosition.y, maxLocalPosition.y);

        // Convert the clamped value back to world space.
        Vector3 clampedWorldPos = transform.parent != null
            ? transform.parent.TransformPoint(localTarget)
            : localTarget;

        transform.position = new Vector3(clampedWorldPos.x, clampedWorldPos.y, transform.position.z);
    }

    private void OnMouseUp()
    {
        isDragging = false;
    }
}