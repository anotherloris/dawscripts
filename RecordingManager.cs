// RecordingManager.cs
using UnityEngine;
using System.Collections.Generic;

public class RecordingManager : MonoBehaviour
{
    private static RecordingManager instance;
    public static RecordingManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<RecordingManager>();
            }
            return instance;
        }
    }

    public GlobalPlaybackManager globalPlayback;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void OnRecordingStarted()
    {
        if (!globalPlayback.IsEnabled()) return;

        // Find all tracks armed for playback
        DrumTrack[] drumTracks = FindObjectsOfType<DrumTrack>();
        foreach (DrumTrack track in drumTracks)
        {
            if (track.IsArmedForPlayback())
            {
                track.StartPlayback();
                Debug.Log($"Started playback on track: {track.gameObject.name}");
            }
        }
    }

    public void OnRecordingStopped()
    {
        // Stop all playing tracks
        DrumTrack[] drumTracks = FindObjectsOfType<DrumTrack>();
        foreach (DrumTrack track in drumTracks)
        {
            if (track.IsPlaying())
            {
                track.StopPlayback();
            }
        }
    }
}