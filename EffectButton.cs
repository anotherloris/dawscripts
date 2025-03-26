// EffectButton.cs
using UnityEngine;

public class EffectButton : MonoBehaviour
{
    [Header("Effect Settings")]
    public EffectController.EffectType effectType;

    [Header("Sprite Settings")]
    public Sprite defaultSprite;
    public Sprite activeSprite;
    public Sprite pressedSprite;

    private SpriteRenderer spriteRenderer;
    private EffectController effectController;
    private bool isPressed = false;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        effectController = FindObjectOfType<EffectController>();

        if (effectController == null)
        {
            Debug.LogError("No EffectController found in scene!");
            return;
        }

        effectController.onEffectChanged.AddListener(UpdateButtonSprite);
        UpdateButtonSprite();
    }

    private void UpdateButtonSprite()
    {
        if (spriteRenderer != null)
        {
            if (isPressed && pressedSprite != null)
            {
                spriteRenderer.sprite = pressedSprite;
            }
            else if (effectController.GetCurrentEffect() == effectType)
            {
                spriteRenderer.sprite = activeSprite;
            }
            else
            {
                spriteRenderer.sprite = defaultSprite;
            }
        }
    }

    private void OnMouseDown()
    {
        if (effectController != null)
        {
            isPressed = true;
            if (pressedSprite != null)
            {
                spriteRenderer.sprite = pressedSprite;
            }
            effectController.ToggleEffect(effectType);
        }
    }

    private void OnMouseUp()
    {
        isPressed = false;
        UpdateButtonSprite();
    }

    private void OnDestroy()
    {
        if (effectController != null)
        {
            effectController.onEffectChanged.RemoveListener(UpdateButtonSprite);
        }
    }
}