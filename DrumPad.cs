using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DrumPad : MonoBehaviour
{
    [Header("References")]
    public DrumController controller;
    public AudioSource audioSource;

    [Header("Visual Settings")]
    public Sprite defaultSprite;
    public Sprite pressedSprite;

    [Header("Audio Settings")]
    [Range(0f, 1f)]
    public float volume = 1f;

    private AudioClip drumSound;
    private SpriteRenderer spriteRenderer;
    private Dictionary<int, bool> touchIds = new Dictionary<int, bool>();
    private float lastInputTime = -1f;
    private const float MINIMUM_TIME_BETWEEN_INPUTS = 0.05f;

    private void Start()
    {
        if (controller == null)
        {
            Debug.LogError($"DrumPad {gameObject.name} has no DrumController assigned!");
            controller = GetComponentInParent<DrumController>();
            if (controller != null)
            {
                Debug.Log($"Found controller in parent for {gameObject.name}");
            }
        }

        if (controller != null && controller.associatedDrumPad != this)
        {
            Debug.LogError($"DrumController's associatedDrumPad doesn't match this DrumPad: {gameObject.name}");
            controller.associatedDrumPad = this;
            Debug.Log($"Fixed controller reference for {gameObject.name}");
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = defaultSprite;
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
    }

    private void Update()
    {
        // Handle touch input
        foreach (Touch touch in Input.touches)
        {
            Vector2 touchPosition = Camera.main.ScreenToWorldPoint(touch.position);
            RaycastHit2D hit = Physics2D.Raycast(touchPosition, Vector2.zero);

            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                if (touch.phase == TouchPhase.Began && !touchIds.ContainsKey(touch.fingerId))
                {
                    if (Time.time - lastInputTime >= MINIMUM_TIME_BETWEEN_INPUTS)
                    {
                        touchIds.Add(touch.fingerId, true);
                        OnPadHit();
                        lastInputTime = Time.time;
                        Debug.Log($"Touch input on pad {gameObject.name} at time {Time.time}");
                    }
                }
            }

            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                touchIds.Remove(touch.fingerId);
            }
        }

#if UNITY_EDITOR
        // Handle mouse input for testing in editor
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                if (Time.time - lastInputTime >= MINIMUM_TIME_BETWEEN_INPUTS)
                {
                    OnPadHit();
                    lastInputTime = Time.time;
                    Debug.Log($"Mouse input on pad {gameObject.name} at time {Time.time}");
                }
            }
        }
#endif
    }

    private void OnPadHit()
    {
        SetPressed(true);
        PlayDrumSound();

        if (controller != null)
        {
            NoteEvent noteEvent = new NoteEvent
            {
                keyIndex = GetInstanceID(),
                isNoteOn = true,
                timeStamp = 0f
            };
            controller.AddNoteToRecording(noteEvent);
            Debug.Log($"Note event created for pad {gameObject.name}");
        }

        StartCoroutine(ResetPressedState());
    }

    public void PlayDrumSound()
    {
        if (audioSource != null && drumSound != null)
        {
            audioSource.PlayOneShot(drumSound, volume);
            Debug.Log($"Playing sound on pad {gameObject.name}");
        }
    }

    public void SetDrumSound(AudioClip newSound)
    {
        drumSound = newSound;
        if (audioSource != null)
        {
            audioSource.clip = drumSound;
        }
    }

    public void SetVolume(float newVolume)
    {
        volume = newVolume;
        if (audioSource != null)
        {
            audioSource.volume = volume;
        }
    }

    private void SetPressed(bool pressed)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = pressed ? pressedSprite : defaultSprite;
        }
    }

    private IEnumerator ResetPressedState()
    {
        yield return new WaitForSeconds(0.1f);
        SetPressed(false);
    }
}