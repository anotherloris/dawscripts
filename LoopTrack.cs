// LoopTrack.cs
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LoopTrack : MonoBehaviour
{
    public enum TrackState
    {
        Empty,
        Recording,
        Recorded,
        Playing
    }

    [Header("Timing Settings")]
    public int numberOfBars = 4;
    public float countInBeats = 4;

    [Header("Visual Settings")]
    public Sprite emptySprite;
    public Sprite recordingSprite;
    public Sprite recordedSprite;
    public Sprite playingSprite;
    public Sprite countInSprite;

    [Header("References")]
    public LoopManager loopManager;
    public Meter meter;

    [Header("Input Settings")]
    public float longPressTime = 0.5f;

    private TrackState currentState = TrackState.Empty;
    private float recordStartTime;
    private int currentFrame = 0;
    private float nextFrameTime;
    private List<NoteEvent> recordedNotes = new List<NoteEvent>();
    private SpriteRenderer buttonRenderer;
    private float pressStartTime;
    private bool isPressed = false;
    private Vector3 originalScale;
    private bool isCountingIn = false;
    private int countInStep = 0;
    private float nextCountInTime;

    private void Start()
    {
        buttonRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
        UpdateVisualState();
    }

    private void Update()
    {
        if (currentState == TrackState.Recording)
        {
            UpdateRecording();
        }
        else if (currentState == TrackState.Playing)
        {
            PlaybackLoop();
        }

        HandleInput();
    }

    private void HandleInput()
    {
        if (isCountingIn) return;

#if UNITY_EDITOR
        HandleMouseInput();
#endif
        HandleTouchInput();
    }

    private void HandleTouchInput()
    {
        if (Input.touchCount > 0)
        {
            foreach (Touch touch in Input.touches)
            {
                Vector2 touchPosition = Camera.main.ScreenToWorldPoint(touch.position);
                RaycastHit2D hit = Physics2D.Raycast(touchPosition, Vector2.zero);

                if (hit.collider != null && hit.collider.gameObject == gameObject)
                {
                    switch (touch.phase)
                    {
                        case TouchPhase.Began:
                            OnPressStart();
                            break;

                        case TouchPhase.Ended:
                            OnPressEnd();
                            break;

                        case TouchPhase.Canceled:
                            isPressed = false;
                            break;
                    }
                }
            }
        }
    }

#if UNITY_EDITOR
    private void HandleMouseInput()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

        if (hit.collider != null && hit.collider.gameObject == gameObject)
        {
            if (Input.GetMouseButtonDown(0))
            {
                OnPressStart();
            }
            else if (Input.GetMouseButtonUp(0))
            {
                OnPressEnd();
            }
        }

        if (isPressed && currentState == TrackState.Recorded)
        {
            if (Time.time - pressStartTime >= longPressTime)
            {
                DeleteRecording();
            }
        }
    }
#endif

    private void UpdateRecording()
    {
        if (Time.time >= nextFrameTime)
        {
            currentFrame++;
            int totalFrames = 16 * numberOfBars;

            if (currentFrame >= totalFrames)
            {
                StopRecording();
                return;
            }

            if (meter != null)
            {
                float progress = (float)currentFrame / totalFrames;
                meter.UpdateFrame(progress);
            }

            nextFrameTime = recordStartTime + ((currentFrame + 1) * loopManager.GetFrameTime());
        }
    }

    private void OnPressStart()
    {
        isPressed = true;
        pressStartTime = Time.time;
    }

    private void OnPressEnd()
    {
        if (!isPressed || isCountingIn) return;

        float pressDuration = Time.time - pressStartTime;
        isPressed = false;

        switch (currentState)
        {
            case TrackState.Empty:
                StartRecording();
                break;

            case TrackState.Recorded:
                if (pressDuration >= longPressTime)
                {
                    return;
                }
                else
                {
                    StartPlayback();
                }
                break;

            case TrackState.Playing:
                StopPlayback();
                break;
        }
    }

    private void StartRecording()
    {
        isCountingIn = true;
        countInStep = 0;
        nextCountInTime = Time.time + (60f / loopManager.bpm);
        StartCoroutine(CountInSequence());
    }

    private IEnumerator CountInSequence()
    {
        while (countInStep < countInBeats)
        {
            buttonRenderer.sprite = (countInStep % 2 == 0) ? countInSprite : emptySprite;

            yield return new WaitForSeconds(60f / loopManager.bpm);
            countInStep++;
        }

        isCountingIn = false;
        currentState = TrackState.Recording;
        recordStartTime = Time.time;
        currentFrame = 0;
        nextFrameTime = Time.time + loopManager.GetFrameTime();
        recordedNotes.Clear();

        if (meter != null)
        {
            meter.StartMeter();
        }

        UpdateVisualState();
    }

    private void StopRecording()
    {
        currentState = TrackState.Recorded;

        if (meter != null)
        {
            meter.StopMeter();
        }

        UpdateVisualState();
    }

    private void StartPlayback()
    {
        currentState = TrackState.Playing;
        recordStartTime = Time.time;
        UpdateVisualState();
        StartCoroutine(ShowPlaybackStartFeedback());
    }

    private void StopPlayback()
    {
        currentState = TrackState.Recorded;
        UpdateVisualState();
        StartCoroutine(ShowPlaybackStopFeedback());
    }

    private void DeleteRecording()
    {
        if (currentState == TrackState.Recorded)
        {
            currentState = TrackState.Empty;
            recordedNotes.Clear();
            UpdateVisualState();
            StartCoroutine(ShowDeletionFeedback());
        }
    }

    private void UpdateVisualState()
    {
        switch (currentState)
        {
            case TrackState.Empty:
                buttonRenderer.sprite = emptySprite;
                break;
            case TrackState.Recording:
                buttonRenderer.sprite = recordingSprite;
                break;
            case TrackState.Recorded:
                buttonRenderer.sprite = recordedSprite;
                break;
            case TrackState.Playing:
                buttonRenderer.sprite = playingSprite;
                break;
        }
    }

    public void AddNote(NoteEvent note)
    {
        if (currentState == TrackState.Recording)
        {
            float timeInLoop = (Time.time - recordStartTime) % (loopManager.GetBarDuration() * numberOfBars);
            note.timeStamp = timeInLoop;
            recordedNotes.Add(note);
        }
    }

    private void PlaybackLoop()
    {
        float loopDuration = loopManager.GetBarDuration() * numberOfBars;
        float currentLoopTime = (Time.time - recordStartTime) % loopDuration;

        foreach (NoteEvent note in recordedNotes)
        {
            if (Mathf.Abs(currentLoopTime - note.timeStamp) < Time.deltaTime)
            {
                if (note.isNoteOn)
                {
                    PlayNote(note);
                }
            }
        }
    }

    private void PlayNote(NoteEvent note)
    {
        PianoKey[] keys = FindObjectsOfType<PianoKey>();
        foreach (PianoKey key in keys)
        {
            if (key.keyIndex == note.keyIndex)
            {
                key.PlayNoteDirectly(note.octave, note.effect);
                break;
            }
        }
    }

    private IEnumerator ShowDeletionFeedback()
    {
        Color originalColor = buttonRenderer.color;
        buttonRenderer.color = Color.red;
        yield return new WaitForSeconds(0.2f);
        buttonRenderer.color = originalColor;
    }

    private IEnumerator ShowRecordingStartFeedback()
    {
        transform.localScale = originalScale * 1.2f;
        yield return new WaitForSeconds(0.1f);
        transform.localScale = originalScale;
    }

    private IEnumerator ShowPlaybackStartFeedback()
    {
        Color originalColor = buttonRenderer.color;
        buttonRenderer.color = Color.green;
        yield return new WaitForSeconds(0.1f);
        buttonRenderer.color = originalColor;
    }

    private IEnumerator ShowPlaybackStopFeedback()
    {
        Color originalColor = buttonRenderer.color;
        buttonRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.5f);
        yield return new WaitForSeconds(0.1f);
        buttonRenderer.color = originalColor;
    }

    public bool IsRecording()
    {
        return currentState == TrackState.Recording;
    }
}