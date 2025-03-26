using UnityEngine;

public class AddRowButton : MonoBehaviour
{
    // Reference to the DynamicRowManager that controls row instantiation.
    // Assign this in the Inspector.
    public DynamicRowManager rowManager;

    // If you're using 2D colliders (triggers) for touch input:
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (rowManager != null)
        {
            rowManager.AddNewRow();
        }
        else
        {
            Debug.LogWarning("DynamicRowManager reference is not set on AddRowButton.");
        }
    }

    // Alternatively, if you want to use OnMouseDown (works both in Editor and on devices with a click):
    private void OnMouseDown()
    {
        if (rowManager != null)
        {
            rowManager.AddNewRow();
        }
        else
        {
            Debug.LogWarning("DynamicRowManager reference is not set on AddRowButton.");
        }
    }
}