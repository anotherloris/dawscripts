using System.Collections;
using UnityEngine;

public class MasterRecordController : MonoBehaviour
{
    [Header("Timing Settings")]
    public float bpm = 120f;
    public int numberOfBars = 4;
    public int countInBeats = 4;
    [Header("Audio Settings")]
    public AudioClip metronomeLoop;
    [Range(0f, 1f)]
    public float metronomeVolume = 0.5f;

    [Header("Visual Settings")]
    public Sprite defaultSprite;
    public Sprite recordingSprite;
    public Sprite playingSprite;
    public Meter meter;

    private AudioSource audioSource;
    private SpriteRenderer buttonRenderer;
    private bool isRecording = false;
    private bool isPlaying = false;
    private bool isCountingIn = false;
    private float beatDuration;

    private void Start()
    {
        buttonRenderer = GetComponent<SpriteRenderer>();
        audioSource = gameObject.AddComponent<AudioSource>();
        beatDuration = 60f / bpm;

        // Set up audio source for metronome
        audioSource.clip = metronomeLoop;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.volume = metronomeVolume;

        // Adjust pitch based on BPM (assuming metronome is recorded at 120 BPM)
        audioSource.pitch = bpm / 120f;

        UpdateVisualState();
    }

    private void OnMouseDown()
    {
        Debug.Log("Master record button pressed");
        OnMasterButtonPressed();
    }

    public void OnMasterButtonPressed()
    {
        if (isRecording || isCountingIn)
            return;

        // If already playing, stop playback
        if (isPlaying)
        {
            StopAllPlayback();
            return;
        }

        // Check if any tracks are armed for recording or playback
        DrumController[] drumControllers = FindObjectsOfType<DrumController>();
        bool anyTracksArmedForRecording = false;
        bool anyTracksArmedForPlayback = false;

        foreach (DrumController controller in drumControllers)
        {
            if (controller.HasTracksArmedForRecording())
            {
                anyTracksArmedForRecording = true;
                break;
            }
            if (controller.HasTracksArmedForPlayback())
            {
                anyTracksArmedForPlayback = true;
            }
        }

        if (anyTracksArmedForRecording)
        {
            StartCountIn();
        }
        else if (anyTracksArmedForPlayback)
        {
            StartPlayback();
        }
    }

    private void StartCountIn()
    {
        isCountingIn = true;
        StartCoroutine(CountInSequence());
    }

    private IEnumerator CountInSequence()
    {
        // Start metronome
        audioSource.Play();

        // Wait for count-in beats
        yield return new WaitForSeconds(beatDuration * countInBeats);

        // Stop metronome
        audioSource.Stop();

        isCountingIn = false;
        StartRecording();
    }

    private void StartRecording()
    {
        isRecording = true;
        isPlaying = true;
        UpdateVisualState();

        if (meter != null)
        {
            meter.StartMeter();
        }

        // Notify all DrumControllers to start recording
        DrumController[] drumControllers = FindObjectsOfType<DrumController>();
        foreach (DrumController controller in drumControllers)
        {
            controller.OnRecordingStarted();
        }

        StartCoroutine(RecordingDurationSequence());
    }

    private void StartPlayback()
    {
        isPlaying = true;
        UpdateVisualState();

        DrumController[] drumControllers = FindObjectsOfType<DrumController>();
        foreach (DrumController controller in drumControllers)
        {
            controller.OnPlaybackStarted();
        }
    }

    private void StopAllPlayback()
    {
        isPlaying = false;
        UpdateVisualState();

        DrumController[] drumControllers = FindObjectsOfType<DrumController>();
        foreach (DrumController controller in drumControllers)
        {
            controller.OnPlaybackStopped();
        }
    }

    private IEnumerator RecordingDurationSequence()
    {
        float totalDuration = numberOfBars * 4 * beatDuration;
        float elapsedTime = 0f;

        while (elapsedTime < totalDuration)
        {
            elapsedTime += Time.deltaTime;

            if (meter != null)
            {
                float progress = elapsedTime / totalDuration;
                meter.UpdateFrame(progress);
            }

            yield return null;
        }

        StopRecording();
    }

    private void StopRecording()
    {
        isRecording = false;
        isPlaying = false;
        UpdateVisualState();

        if (meter != null)
        {
            meter.StopMeter();
        }

        DrumController[] drumControllers = FindObjectsOfType<DrumController>();
        foreach (DrumController controller in drumControllers)
        {
            controller.OnRecordingStopped();
        }
    }

    private void UpdateVisualState()
    {
        if (buttonRenderer != null)
        {
            if (isRecording)
            {
                buttonRenderer.sprite = recordingSprite;
            }
            else if (isPlaying)
            {
                buttonRenderer.sprite = playingSprite;
            }
            else
            {
                buttonRenderer.sprite = defaultSprite;
            }
        }
    }

    public bool IsRecording()
    {
        return isRecording;
    }

    public float GetBeatDuration()
    {
        return beatDuration;
    }

    public float GetTotalDuration()
    {
        return numberOfBars * 4 * beatDuration;
    }
}