// SimpleButton.cs
using UnityEngine;

public class SimpleButton : MonoBehaviour
{
    public enum ButtonType
    {
        Piano,
        Drums
    }

    public ButtonType buttonType;
    public Sprite defaultSprite;
    public Sprite pressedSprite;

    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = defaultSprite;
    }

    private void OnMouseDown()
    {
        spriteRenderer.sprite = pressedSprite;
    }

    private void OnMouseUp()
    {
        spriteRenderer.sprite = defaultSprite;

        if (buttonType == ButtonType.Piano)
        {
            PersistentManager.Instance.LoadPianoScene();
        }
        else
        {
            PersistentManager.Instance.LoadDrumScene();
        }
    }
}