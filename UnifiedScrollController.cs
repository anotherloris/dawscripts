using UnityEngine;
using System.Collections;

public class UnifiedScrollController : MonoBehaviour
{
    // Enum to determine the detected drag mode.
    private enum DragMode { None, Vertical, Horizontal }
    private DragMode currentDragMode = DragMode.None;

    // Containers for vertical (rows) and horizontal (columns) scrolling.
    public Transform verticalContainer;   // For vertical scrolling (rows).
    public Transform horizontalContainer; // For horizontal scrolling (columns).

    // Flags to enable scrolling in each direction.
    public bool allowVerticalScroll = true;
    public bool allowHorizontalScroll = true;

    // Border colliders for vertical snapping.
    public Collider2D topStopper;
    public Collider2D bottomStopper;

    // Border colliders for horizontal snapping.
    public Collider2D leftStopper;
    public Collider2D rightStopper;

    // Grid cell sizes for snapping.
    // These are automatically calculated in Start(), but they can be overridden if needed.
    public float rowHeight = 1f;
    public float columnWidth = 1f;

    // Smoothing factors and snap duration.
    public float smoothFactor = 10f;
    public float snapDuration = 0.3f;

    // Internal drag tracking variables.
    private Vector3 initialMouseWorldPos;
    private Vector3 initialVerticalPos;
    private Vector3 initialHorizontalPos;
    private bool isDragging = false;

    private void Start()
    {
        // Automatically calculate rowHeight using verticalContainer first child's collider.
        if (verticalContainer != null && verticalContainer.childCount > 0)
        {
            Collider2D firstRowCol = verticalContainer.GetChild(0).GetComponent<Collider2D>();
            if (firstRowCol != null)
                rowHeight = firstRowCol.bounds.size.y;
        }

        // Automatically calculate columnWidth using horizontalContainer first child's collider.
        if (horizontalContainer != null && horizontalContainer.childCount > 0)
        {
            Collider2D firstColCol = horizontalContainer.GetChild(0).GetComponent<Collider2D>();
            if (firstColCol != null)
                columnWidth = firstColCol.bounds.size.x;
        }

        // Optionally disable colliders in the middle columns for horizontalContainer.
        DisableInternalColumnColliders();
    }

    private void OnMouseDown()
    {
        isDragging = true;
        currentDragMode = DragMode.None; // Reset dragging mode.

        // Record the mouse world position.
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;
        initialMouseWorldPos = mouseWorldPos;

        // Record starting positions for both containers.
        if (verticalContainer != null)
            initialVerticalPos = verticalContainer.position;
        if (horizontalContainer != null)
            initialHorizontalPos = horizontalContainer.position;
    }

    private void OnMouseDrag()
    {
        if (!isDragging)
            return;

        // Get current mouse world position.
        Vector3 currentMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        currentMousePos.z = 0f;
        Vector3 dragDelta = currentMousePos - initialMouseWorldPos;

        // Determine initial drag mode if not already set.
        if (currentDragMode == DragMode.None)
        {
            if (Mathf.Abs(dragDelta.x) > Mathf.Abs(dragDelta.y))
                currentDragMode = allowHorizontalScroll ? DragMode.Horizontal : DragMode.Vertical;
            else
                currentDragMode = allowVerticalScroll ? DragMode.Vertical : DragMode.Horizontal;
        }

        // Update only the relevant container.
        if (currentDragMode == DragMode.Vertical && verticalContainer != null)
        {
            Vector3 desiredPos = initialVerticalPos + new Vector3(0, dragDelta.y, 0);
            Vector3 clampedPos = ClampVerticalPosition(desiredPos);
            verticalContainer.position = Vector3.Lerp(verticalContainer.position,
                                                      new Vector3(verticalContainer.position.x, clampedPos.y, verticalContainer.position.z),
                                                      Time.deltaTime * smoothFactor);
        }
        else if (currentDragMode == DragMode.Horizontal && horizontalContainer != null)
        {
            Vector3 desiredPos = initialHorizontalPos + new Vector3(dragDelta.x, 0, 0);
            Vector3 clampedPos = ClampHorizontalPosition(desiredPos);
            horizontalContainer.position = Vector3.Lerp(horizontalContainer.position,
                                                        new Vector3(clampedPos.x, horizontalContainer.position.y, horizontalContainer.position.z),
                                                        Time.deltaTime * smoothFactor);
        }
    }

    private void OnMouseUp()
    {
        isDragging = false;
        if (currentDragMode == DragMode.Vertical && verticalContainer != null)
            StartCoroutine(SnapVertical());
        else if (currentDragMode == DragMode.Horizontal && horizontalContainer != null)
            StartCoroutine(SnapHorizontal());
    }

    // Clamps the vertical position of the vertical container.
    // The first row's bottom edge must not fall below the topStopper's top edge,
    // and the last row's top edge must not rise above the bottomStopper's bottom edge.
    private Vector3 ClampVerticalPosition(Vector3 pos)
    {
        if (verticalContainer.childCount == 0 || topStopper == null || bottomStopper == null)
            return pos;

        Transform firstRow = verticalContainer.GetChild(0);
        Transform lastRow = verticalContainer.GetChild(verticalContainer.childCount - 1);

        Collider2D firstRowCol = firstRow.GetComponent<Collider2D>();
        Collider2D lastRowCol = lastRow.GetComponent<Collider2D>();
        if (firstRowCol == null || lastRowCol == null)
            return pos;

        float offsetY = pos.y - initialVerticalPos.y;

        float predictedFirstRowBottom = firstRowCol.bounds.min.y + offsetY;
        float predictedLastRowTop = lastRowCol.bounds.max.y + offsetY;

        // For vertical clamping:
        // The first row's bottom must not fall below topStopper's top edge.
        float allowedFirstRowBottom = topStopper.bounds.max.y;
        // The last row's top must not rise above bottomStopper's bottom edge.
        float allowedLastRowTop = bottomStopper.bounds.min.y;

        float adjustedOffset = offsetY;

        if (predictedFirstRowBottom < allowedFirstRowBottom)
        {
            adjustedOffset += (allowedFirstRowBottom - predictedFirstRowBottom);
            Debug.Log("Vertical Adjust Up: " + (allowedFirstRowBottom - predictedFirstRowBottom));
        }
        if (predictedLastRowTop > allowedLastRowTop)
        {
            adjustedOffset -= (predictedLastRowTop - allowedLastRowTop);
            Debug.Log("Vertical Adjust Down: " + (predictedLastRowTop - allowedLastRowTop));
        }

        pos.y = initialVerticalPos.y + adjustedOffset;
        return pos;
    }

    // Clamps the horizontal position of the horizontal container.
    // The left-most column's right edge must not go past leftStopper's right edge,
    // and the right-most column's left edge must not go before rightStopper's left edge.
    private Vector3 ClampHorizontalPosition(Vector3 pos)
    {
        if (horizontalContainer.childCount == 0 || leftStopper == null || rightStopper == null)
            return pos;

        Transform firstCol = horizontalContainer.GetChild(0);
        Transform lastCol = horizontalContainer.GetChild(horizontalContainer.childCount - 1);

        Collider2D firstColCol = firstCol.GetComponent<Collider2D>();
        Collider2D lastColCol = lastCol.GetComponent<Collider2D>();
        if (firstColCol == null || lastColCol == null)
            return pos;

        float offsetX = pos.x - initialHorizontalPos.x;

        float predictedFirstColRight = firstColCol.bounds.max.x + offsetX;
        float predictedLastColLeft = lastColCol.bounds.min.x + offsetX;

        // Allowed boundaries for horizontal clamping:
        float allowedFirstColRight = leftStopper.bounds.max.x;
        float allowedLastColLeft = rightStopper.bounds.min.x;

        float adjustedOffset = offsetX;

        if (predictedFirstColRight < allowedFirstColRight)
        {
            adjustedOffset += (allowedFirstColRight - predictedFirstColRight);
            Debug.Log("Horizontal Adjust Right: " + (allowedFirstColRight - predictedFirstColRight));
        }
        if (predictedLastColLeft > allowedLastColLeft)
        {
            adjustedOffset -= (predictedLastColLeft - allowedLastColLeft);
            Debug.Log("Horizontal Adjust Left: " + (predictedLastColLeft - allowedLastColLeft));
        }

        pos.x = initialHorizontalPos.x + adjustedOffset;
        return pos;
    }

    // Coroutine to snap the vertical container to the nearest row boundary in the direction of the drag.
    private IEnumerator SnapVertical()
    {
        float currentOffsetY = verticalContainer.position.y - initialVerticalPos.y;
        float snapOffset = 0f;
        if (currentOffsetY > 0)
            snapOffset = Mathf.Ceil(currentOffsetY / rowHeight) * rowHeight;
        else if (currentOffsetY < 0)
            snapOffset = Mathf.Floor(currentOffsetY / rowHeight) * rowHeight;
        else
            snapOffset = 0f;

        Vector3 snapTarget = new Vector3(verticalContainer.position.x, initialVerticalPos.y + snapOffset, verticalContainer.position.z);
        float elapsed = 0f;
        Vector3 startPos = verticalContainer.position;
        while (elapsed < snapDuration)
        {
            verticalContainer.position = Vector3.Lerp(startPos, snapTarget, elapsed / snapDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        verticalContainer.position = snapTarget;
    }

    // Coroutine to snap the horizontal container to the nearest column boundary in the direction of the drag.
    private IEnumerator SnapHorizontal()
    {
        float currentOffsetX = horizontalContainer.position.x - initialHorizontalPos.x;
        float snapOffset = 0f;
        if (currentOffsetX > 0)
            snapOffset = Mathf.Ceil(currentOffsetX / columnWidth) * columnWidth;
        else if (currentOffsetX < 0)
            snapOffset = Mathf.Floor(currentOffsetX / columnWidth) * columnWidth;
        else
            snapOffset = 0f;

        Vector3 snapTarget = new Vector3(initialHorizontalPos.x + snapOffset, horizontalContainer.position.y, horizontalContainer.position.z);
        float elapsed = 0f;
        Vector3 startPos = horizontalContainer.position;
        while (elapsed < snapDuration)
        {
            horizontalContainer.position = Vector3.Lerp(startPos, snapTarget, elapsed / snapDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        horizontalContainer.position = snapTarget;
    }

    // Methods to disable internal colliders on the horizontal container except for the first and last columns.
    private void DisableInternalColumnColliders()
    {
        if (horizontalContainer == null) return;

        int childCount = horizontalContainer.childCount;
        if (childCount < 3)
            return; // Nothing to disable if there are only one or two columns.

        for (int i = 1; i < childCount - 1; i++)
        {
            Collider2D col = horizontalContainer.GetChild(i).GetComponent<Collider2D>();
            if (col != null)
            {
                col.enabled = false;
                Debug.Log("Disabled collider on column index: " + i);
            }
        }
    }
}