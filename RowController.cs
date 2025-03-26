using UnityEngine;

public class RowController : MonoBehaviour
{
    [Header("Row Spawn Settings")]
    [Tooltip("Child Transform that marks where the next row should spawn.")]
    public Transform rowSpawnPoint;

    [Header("Column Settings")]
    [Tooltip("Container for timeline columns in this row.")]
    public Transform timelineContainer;
    [Tooltip("Transform used as the spawn point for new timeline columns in this row. " +
             "Initially assigned to the spawn point in the first timeline column.")]
    public Transform columnSpawnPoint;

    [Header("Track Recording Status")]
    [Tooltip("Boolean array indicating if each track (of 4) has a recording on it.")]
    public bool[] trackActive = new bool[4];  // Index 0 = track 1, etc.

    [Header("BPM Setting")]
    [Tooltip("The current BPM received from AudioManager.")]
    public int currentBPM = 120;

    // Optional convenience method to get column count from DynamicRowManager.
    public int GetColumnCount()
    {
        return DynamicRowManager.Instance.GetColumnCount(this);
    }

    /// <summary>
    /// Updates the BPM value for this row.
    /// Called by the AudioManager.
    /// </summary>
    public void UpdateBPM(int newBPM)
    {
        currentBPM = newBPM;
        Debug.Log(gameObject.name + ": BPM updated to " + currentBPM);
        // Future: Instruments in the row can reference currentBPM for timing.
    }
}