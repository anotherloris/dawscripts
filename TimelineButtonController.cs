using System.Collections.Generic;
using UnityEngine;

public class TimelineButtonController : MonoBehaviour
{
    [Header("State Sprites")]
    [Tooltip("Sprites for button states: index 0 = Off; index 1 = Track1 active; index 2 = Track2 active; index 3 = Track3 active; index 4 = Track4 active.")]
    public Sprite[] stateSprites;  // Length should be at least 5

    [Header("Empty Track Sprites")]
    [Tooltip("Sprites for empty track states for tracks 1-4 (will be used when a track is not recorded).")]
    public Sprite[] emptyTrackSprites;  // Length should be 4

    [Header("Settings")]
    [Tooltip("Total number of states: 5 (0 = off, 1-4 for tracks).")]
    public int totalStates = 5;

    // This represents the current state: 0 = off; 1,2,3,4 correspond to tracks 1-4.
    private int currentState = 0;

    private SpriteRenderer spriteRenderer;

    [Header("Row Reference")]
    [Tooltip("Reference to the RowController that contains trackActive info.")]
    public RowController rowController;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            Debug.LogError("TimelineButtonController: No SpriteRenderer found on " + gameObject.name);

        // If not manually assigned, try to auto-find a RowController in a parent.
        if (rowController == null)
        {
            rowController = GetComponentInParent<RowController>();
            if (rowController == null)
                Debug.LogError("TimelineButtonController: No RowController found in parent for " + gameObject.name);
        }
        // Start with off.
        currentState = 0;
        UpdateVisual();
    }

    // Use OnMouseDown for testing; on touch devices, switch to OnTriggerEnter2D.
    private void OnMouseDown()
    {
        CycleState();
    }

    /// <summary>
    /// Cycles through the fixed five states: 0 (off), then 1, 2, 3, 4,
    /// so that each button press increments the state. (No state is skipped.)
    /// </summary>
    private void CycleState()
    {
        currentState = (currentState + 1) % totalStates;  // Always cycles: 0,1,2,3,4, then back to 0.
        Debug.Log("[TimelineButtonController] " + gameObject.name + " new state: " + currentState);
        UpdateVisual();
    }

    /// <summary>
    /// Updates the SpriteRenderer's sprite based on the current state.
    /// For state 0, uses stateSprites[0] as off.
    /// For states 1-4, checks rowController.trackActive. If the corresponding track is recorded,
    /// uses stateSprites[currentState]; otherwise, uses emptyTrackSprites[currentState-1].
    /// </summary>
    private void UpdateVisual()
    {
        if (currentState == 0)
        {
            if (stateSprites != null && stateSprites.Length > 0)
                spriteRenderer.sprite = stateSprites[0];
            else
                Debug.LogWarning("TimelineButtonController: Off sprite (stateSprites[0]) missing on " + gameObject.name);
            return;
        }

        int trackIndex = currentState - 1;  // Map state 1-> index0, etc.
        bool recorded = false;
        if (rowController != null && rowController.trackActive != null && rowController.trackActive.Length > trackIndex)
        {
            recorded = rowController.trackActive[trackIndex];
        }
        else
        {
            Debug.LogWarning("TimelineButtonController: rowController.trackActive not set up correctly on " + gameObject.name);
        }

        if (recorded)
        {
            if (stateSprites != null && stateSprites.Length > currentState)
                spriteRenderer.sprite = stateSprites[currentState];
            else
                Debug.LogWarning("TimelineButtonController: Active sprite for state " + currentState + " missing on " + gameObject.name);
        }
        else
        {
            if (emptyTrackSprites != null && emptyTrackSprites.Length > trackIndex)
                spriteRenderer.sprite = emptyTrackSprites[trackIndex];
            else
                Debug.LogWarning("TimelineButtonController: Empty sprite for track " + (trackIndex + 1) + " missing on " + gameObject.name);
        }
    }
}