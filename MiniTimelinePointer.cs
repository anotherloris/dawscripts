using UnityEngine;

public class MiniTimelinePointer : MonoBehaviour
{
    // These should be assigned in the Inspector.
    // These transforms represent the start and end positions of your mini timeline.
    public Transform miniTimelineStart;
    public Transform miniTimelineEnd;

    // Optionally, you can cache a reference to the AudioManager if needed.
    private AudioManager audioManager;
    private PlaybackManager playbackManager;

    void Start()
    {
        audioManager = AudioManager.Instance;
        playbackManager = PlaybackManager.Instance;
        if (miniTimelineStart == null || miniTimelineEnd == null)
        {
            Debug.LogError("MiniTimelinePointer: Please assign both miniTimelineStart and miniTimelineEnd in the Inspector.");
        }
    }

    void Update()
    {
        // Ensure both managers are available.
        if (audioManager == null || playbackManager == null)
            return;

        // Get total track duration in seconds from AudioManager.
        float totalDuration = audioManager.trackLengthSeconds;
        float currentTime = playbackManager.currentTime;

        // Safeguard against division by zero.
        float t = totalDuration > 0f ? Mathf.Clamp01(currentTime / totalDuration) : 0f;

        // Interpolate between the start and end positions.
        transform.position = Vector3.Lerp(miniTimelineStart.position, miniTimelineEnd.position, t);
    }
}