// OctaveButton.cs
using UnityEngine;

public class OctaveButton : MonoBehaviour
{
    public enum ButtonType
    {
        Increase,
        Decrease
    }

    [Header("Button Settings")]
    public ButtonType buttonType;

    [Header("Sprite Settings")]
    public Sprite[] octaveSprites; // Array of sprites for each octave
    public Sprite pressedSprite;   // Optional pressed state sprite

    private SpriteRenderer spriteRenderer;
    private EffectController effectController;
    private Sprite currentDefaultSprite; // Store the current default sprite

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        effectController = FindObjectOfType<EffectController>();

        if (effectController == null)
        {
            Debug.LogError("No EffectController found in scene!");
            return;
        }

        // Subscribe to the octave changed event
        effectController.onOctaveChanged.AddListener(UpdateButtonSprite);

        // Initial sprite update
        UpdateButtonSprite();
    }

    private void UpdateButtonSprite()
    {
        if (spriteRenderer != null && octaveSprites != null && octaveSprites.Length > 0)
        {
            int currentOctave = effectController.GetCurrentOctave();

            // Make sure we have a valid index
            if (currentOctave < octaveSprites.Length)
            {
                currentDefaultSprite = octaveSprites[currentOctave];
                spriteRenderer.sprite = currentDefaultSprite;
            }
        }
    }

    private void OnMouseDown()
    {
        if (effectController != null)
        {
            // Change to pressed sprite if available
            if (spriteRenderer != null && pressedSprite != null)
            {
                spriteRenderer.sprite = pressedSprite;
            }

            // Trigger the appropriate action based on button type
            if (buttonType == ButtonType.Increase)
            {
                effectController.IncreaseOctave();
            }
            else
            {
                effectController.DecreaseOctave();
            }
        }
    }

    private void OnMouseUp()
    {
        // Restore current default sprite
        if (spriteRenderer != null && currentDefaultSprite != null)
        {
            spriteRenderer.sprite = currentDefaultSprite;
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from the event when the object is destroyed
        if (effectController != null)
        {
            effectController.onOctaveChanged.RemoveListener(UpdateButtonSprite);
        }
    }
}