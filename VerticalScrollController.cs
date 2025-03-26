using UnityEngine;
using System.Collections;

public class VerticalScrollController : MonoBehaviour
{
    // The container holding all row GameObjects.
    public Transform contentContainer;

    // Border colliders defining the allowed vertical boundaries.
    public Collider2D topStopper;
    public Collider2D bottomStopper;

    // Smoothing factor for dragging.
    public float smoothFactor = 10f;
    // Duration for snapping back into place.
    public float snapDuration = 0.3f;

    // Internal variables for drag handling.
    private Vector3 initialMouseWorldPos;
    private Vector3 initialContentPos;
    private bool isDragging = false;
    private Vector3 targetPos;

    // Row height to use for snapping; assumes rows are evenly spaced.
    private float rowHeight = 1f;

    private void Start()
    {
        // If there is at least one row, determine rowHeight from the first row's collider.
        if (contentContainer.childCount > 0)
        {
            Collider2D firstRowCol = contentContainer.GetChild(0).GetComponent<Collider2D>();
            if (firstRowCol != null)
            {
                rowHeight = firstRowCol.bounds.size.y;
            }
        }
        targetPos = contentContainer.position;
    }

    private void OnMouseDown()
    {
        isDragging = true;
        // Record the initial mouse world position.
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;
        initialMouseWorldPos = mouseWorldPos;

        // Record the current container position.
        initialContentPos = contentContainer.position;
    }

    private void OnMouseDrag()
    {
        if (!isDragging)
            return;

        // Convert the current mouse position to world coordinates.
        Vector3 currentMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        currentMousePos.z = 0f;

        // Calculate the desired new container position based on drag.
        Vector3 desiredPos = initialContentPos + (currentMousePos - initialMouseWorldPos);
        targetPos = ClampVerticalPosition(desiredPos);

        // Smoothly interpolate the content container's position toward targetPos.
        contentContainer.position = Vector3.Lerp(contentContainer.position,
                                                   new Vector3(contentContainer.position.x, targetPos.y, contentContainer.position.z),
                                                   Time.deltaTime * smoothFactor);
    }

    private void OnMouseUp()
    {
        isDragging = false;
        // Snap the content container to the grid in the direction of the drag.
        StartCoroutine(SnapToGrid());
    }

    // Clamps pos.y so that:
    // - The first row’s bottom edge does not fall below topStopper’s top edge.
    // - The last row’s top edge does not go above bottomStopper’s bottom edge.
    private Vector3 ClampVerticalPosition(Vector3 pos)
    {
        if (contentContainer.childCount == 0 || topStopper == null || bottomStopper == null)
            return pos;

        // Assume the first child is the top row and the last child is the bottom row.
        Transform firstRow = contentContainer.GetChild(0);
        Transform lastRow = contentContainer.GetChild(contentContainer.childCount - 1);

        Collider2D firstRowCol = firstRow.GetComponent<Collider2D>();
        Collider2D lastRowCol = lastRow.GetComponent<Collider2D>();
        if (firstRowCol == null || lastRowCol == null)
            return pos;

        float offsetY = pos.y - initialContentPos.y;

        // Predicted positions:
        float predictedFirstRowBottom = firstRowCol.bounds.min.y + offsetY;
        float predictedLastRowTop = lastRowCol.bounds.max.y + offsetY;

        // Allowed boundaries:
        // The first row's bottom must not be lower than the topStopper's top edge.
        float allowedFirstRowBottom = topStopper.bounds.max.y;
        // The last row's top must not be higher than the bottomStopper's bottom edge.
        float allowedLastRowTop = bottomStopper.bounds.min.y;

        float adjustedOffset = offsetY;

        if (predictedFirstRowBottom < allowedFirstRowBottom)
        {
            adjustedOffset += (allowedFirstRowBottom - predictedFirstRowBottom);
            Debug.Log("Adjusting upward by: " + (allowedFirstRowBottom - predictedFirstRowBottom));
        }
        if (predictedLastRowTop > allowedLastRowTop)
        {
            adjustedOffset -= (predictedLastRowTop - allowedLastRowTop);
            Debug.Log("Adjusting downward by: " + (predictedLastRowTop - allowedLastRowTop));
        }

        pos.y = initialContentPos.y + adjustedOffset;
        return pos;
    }

    // Snaps the content container to the nearest grid line in the direction of the drag.
    private IEnumerator SnapToGrid()
    {
        // Determine current offset relative to where the drag started.
        float currentOffsetY = contentContainer.position.y - initialContentPos.y;
        float snapOffset = 0f;

        // Snap in the direction of movement:
        // If drag is upward (offset > 0), use Ceil to snap upward.
        // If drag is downward (offset < 0), use Floor to snap downward.
        if (currentOffsetY > 0)
            snapOffset = Mathf.Ceil(currentOffsetY / rowHeight) * rowHeight;
        else if (currentOffsetY < 0)
            snapOffset = Mathf.Floor(currentOffsetY / rowHeight) * rowHeight;
        else
            snapOffset = 0f;

        Vector3 snapTargetPos = new Vector3(contentContainer.position.x, initialContentPos.y + snapOffset, contentContainer.position.z);

        float elapsed = 0f;
        Vector3 startPos = contentContainer.position;
        while (elapsed < snapDuration)
        {
            contentContainer.position = Vector3.Lerp(startPos, snapTargetPos, elapsed / snapDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        contentContainer.position = snapTargetPos;
    }
}