using UnityEngine;

public class TrackLightsController : MonoBehaviour
{
    [Header("Light GameObjects")]
    [Tooltip("Assign the specific game objects (with SpriteRenderers) for each track light (order: 0 = Track 1, 1 = Track 2, 2 = Track 3, 3 = Track 4).")]
    public GameObject[] lightObjects;  // Should be length 4

    [Header("Sprites")]
    [Tooltip("Sprites to use for the 'on' state. One for each track—order should match lightObjects.")]
    public Sprite[] onSprites;  // For example, length 4; each track can have its own on color
    [Tooltip("Sprite to use for the 'off' state (applied to all tracks).")]
    public Sprite offSprite;

    // Reference to the parent RowController so we can read the trackActive array.
    [Tooltip("The RowController that holds the track active status.")]
    public RowController rowController;

    private void Start()
    {
        if (rowController == null)
        {
            rowController = GetComponentInParent<RowController>();
            if (rowController == null)
            {
                Debug.LogError("TrackLightsController: No RowController found in parent!");
                return;
            }
        }
        UpdateLights();
    }

    /// <summary>
    /// Updates the track lights based on the rowController's trackActive array.
    /// </summary>
    public void UpdateLights()
    {
        if (rowController == null)
        {
            Debug.LogError("TrackLightsController: RowController is null.");
            return;
        }

        if (lightObjects == null || lightObjects.Length < 4)
        {
            Debug.LogError("TrackLightsController: lightObjects array must have 4 elements.");
            return;
        }

        if (onSprites == null || onSprites.Length < 4)
        {
            Debug.LogError("TrackLightsController: onSprites array must have 4 elements.");
            return;
        }

        for (int i = 0; i < lightObjects.Length; i++)
        {
            if (lightObjects[i] == null)
            {
                Debug.LogWarning("TrackLightsController: Light object at index " + i + " is not assigned.");
                continue;
            }

            SpriteRenderer sr = lightObjects[i].GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                bool isActive = false;
                if (rowController.trackActive != null && rowController.trackActive.Length > i)
                {
                    isActive = rowController.trackActive[i];
                }
                sr.sprite = isActive ? onSprites[i] : offSprite;
                Debug.Log("TrackLightsController: Set light " + i + " to " + (isActive ? "ON" : "OFF"));
            }
            else
            {
                Debug.LogWarning("TrackLightsController: Light object at index " + i + " is missing a SpriteRenderer.");
            }
        }
    }
}