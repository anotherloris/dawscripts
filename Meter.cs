// Meter.cs
using UnityEngine;

public class Meter : MonoBehaviour
{
    public Sprite[] meterFrames;
    private SpriteRenderer spriteRenderer;
    private bool isActive = false;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("No SpriteRenderer found on Meter");
        }
    }

    public void StartMeter()
    {
        isActive = true;
        UpdateFrame(0);
    }

    public void StopMeter()
    {
        isActive = false;
        UpdateFrame(0);
    }

    public void UpdateFrame(float progress)
    {
        if (meterFrames != null && meterFrames.Length > 0)
        {
            int frameIndex = Mathf.FloorToInt(progress * (meterFrames.Length - 1));
            frameIndex = Mathf.Clamp(frameIndex, 0, meterFrames.Length - 1);
            spriteRenderer.sprite = meterFrames[frameIndex];
        }
    }
}