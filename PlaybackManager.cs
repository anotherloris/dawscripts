using UnityEngine;
using System;

public class PlaybackManager : MonoBehaviour
{
    public static PlaybackManager Instance;

    // Playback state
    public bool isPlaying { get; private set; } = false;
    public bool isPaused { get; private set; } = false;

    // Playhead time in seconds.
    public float currentTime = 0f;

    // Event to notify when the current column changes (if needed).
    public event Action<int> OnColumnChanged;

    // Beats per column (should match the value in AudioManager/DynamicRowManager).
    public int beatsPerColumn = 16;

    // Current column index (based on the grid).
    public int currentColumnIndex { get; private set; } = 0;

    // Cached BPM from AudioManager (updates if BPM changes).
    private float BPM
    {
        get { return AudioManager.Instance != null ? AudioManager.Instance.currentBPM : 120f; }
    }

    private float secondsPerBeat
    {
        get { return 60f / BPM; }
    }

    private void Awake()
    {
        // Singleton pattern.
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Update()
    {
        // Only update the playhead if playback is active.
        if (isPlaying && !isPaused)
        {
            // Increment the playhead time.
            currentTime += Time.deltaTime;
            UpdateColumnIndex();

            // Log the current time and full duration roughly once per second.
            if (Time.frameCount % 60 == 0 && AudioManager.Instance != null)
            {
                string formattedCurrentTime = FormatTime(currentTime);
                string fullDuration = AudioManager.Instance.formattedTrackLength;
                Debug.Log(formattedCurrentTime + " - " + fullDuration);
            }
        }
    }

    // Helper method to format a time value (in seconds) as "MM:SS:HH", where HH is hundredths of a second.
    private string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        int milliseconds = Mathf.FloorToInt((timeInSeconds * 1000f) % 1000f);
        int hundredths = milliseconds / 10;
        return string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, hundredths);
    }

    // Update the current column index based on the elapsed time.
    private void UpdateColumnIndex()
    {
        int totalBeatsElapsed = Mathf.FloorToInt(currentTime / secondsPerBeat);
        int newColumnIndex = totalBeatsElapsed / beatsPerColumn;

        if (newColumnIndex != currentColumnIndex)
        {
            currentColumnIndex = newColumnIndex;
            Debug.Log("PlaybackManager: Current column index is now " + currentColumnIndex);
            OnColumnChanged?.Invoke(currentColumnIndex);
        }
    }

    // Start playback. If 'restart' is true, resets the playhead.
    public void Play(bool restart = false)
    {
        if (restart)
            currentTime = 0f;
        isPlaying = true;
        isPaused = false;
        Debug.Log("PlaybackManager: Playback started.");
    }

    // Pause/resume playback.
    public void TogglePause()
    {
        isPaused = !isPaused;
        Debug.Log("PlaybackManager: Playback " + (isPaused ? "paused." : "resumed."));
    }

    // Stop playback and reset playhead and state.
    public void Stop()
    {
        isPlaying = false;
        isPaused = false;
        currentTime = 0f;
        currentColumnIndex = 0;
        Debug.Log("PlaybackManager: Playback stopped.");
        OnColumnChanged?.Invoke(currentColumnIndex);
    }
}