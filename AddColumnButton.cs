using UnityEngine;

public class AddColumnButton : MonoBehaviour
{
    // Reference to the DynamicRowManager that handles row/column spawning.
    public DynamicRowManager dynamicRowManager;

    // For 2D collision detection
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (dynamicRowManager != null)
        {
            dynamicRowManager.AddColumnToAllRows();
        }
        else
        {
            Debug.LogWarning("DynamicRowManager reference is not set on AddColumnButton.");
        }
    }

    // Alternatively, you can use OnMouseDown for testing
    private void OnMouseDown()
    {
        if (dynamicRowManager != null)
        {
            dynamicRowManager.AddColumnToAllRows();
        }
        else
        {
            Debug.LogWarning("DynamicRowManager reference is not set on AddColumnButton.");
        }
    }
}