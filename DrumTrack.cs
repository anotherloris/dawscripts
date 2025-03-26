// DrumTrack.cs
using UnityEngine;
using System.Collections.Generic;

public class DrumTrack : MonoBehaviour
{
    [Header("Visual Settings")]
    public Sprite emptySprite;
    public Sprite recordArmedSprite;
    public Sprite recordedSprite;
    public Sprite playbackArmedSprite;

    [System.Serializable]
    public class SavedTrackData
    {
        public List<NoteEvent> notes = new List<NoteEvent>();
        public bool isArmedForPlayback = false;
    }

    private SpriteRenderer buttonRenderer;
    private DrumController controller;
    private List<NoteEvent> recordedNotes = new List<NoteEvent>();
    private float recordStartTime;
    private bool isRecording = false;
    private bool isPlaying = false;
    private bool isArmedForRecording = false;
    private bool isArmedForPlayback = false;
    private string trackId;

    private void Awake()
    {
        trackId = $"{transform.parent.name}_{transform.GetSiblingIndex()}";
        Debug.Log($"Track initialized with ID: {trackId}");
    }

    private void Start()
    {
        buttonRenderer = GetComponent<SpriteRenderer>();
        LoadTrackData();
        UpdateVisualState();
    }

    private void OnEnable()
    {
        LoadTrackData();
    }

    private void OnDisable()
    {
        SaveTrackData();
    }

    private void OnMouseDown()
    {
        OnButtonPressed();
    }

    public void Initialize(DrumController parentController)
    {
        controller = parentController;
    }

    private void Update()
    {
        if (isPlaying)
        {
            PlaybackLoop();
        }
    }

    public void OnButtonPressed()
    {
        if (controller != null)
        {
            controller.OnTrackButtonPressed(this);
        }
    }

    public void ArmForRecording()
    {
        isArmedForRecording = true;
        isArmedForPlayback = false;
        UpdateVisualState();
        SaveTrackData();
        Debug.Log($"Track armed for recording: {trackId}");
    }

    public void ArmForPlayback()
    {
        isArmedForPlayback = true;
        isArmedForRecording = false;
        UpdateVisualState();
        SaveTrackData();
        Debug.Log($"Track armed for playback: {trackId}");
    }

    public void Disarm()
    {
        isArmedForRecording = false;
        isArmedForPlayback = false;
        StopPlayback();
        UpdateVisualState();
        SaveTrackData();
    }

    public void StartRecording()
    {
        if (isArmedForRecording)
        {
            isRecording = true;
            recordStartTime = Time.time;
            recordedNotes.Clear();
            Debug.Log($"Started recording on track: {trackId}");
        }
    }

    public void StopRecording()
    {
        if (isRecording)
        {
            isRecording = false;
            isArmedForRecording = false;
            UpdateVisualState();
            SaveTrackData();
            Debug.Log($"Stopped recording on track: {trackId}");
        }
    }

    public void StartPlayback()
    {
        if (HasRecording() && isArmedForPlayback)
        {
            isPlaying = true;
            recordStartTime = Time.time;
            Debug.Log($"Started playback on track: {trackId}");
        }
    }

    public void StopPlayback()
    {
        isPlaying = false;
    }

    public void AddNote(NoteEvent note)
    {
        if (isRecording && controller != null && controller.associatedDrumPad != null)
        {
            float timeInLoop = (Time.time - recordStartTime) % (controller.masterController.GetTotalDuration());
            note.timeStamp = timeInLoop;
            recordedNotes.Add(note);
            SaveTrackData();
        }
    }

    private void PlaybackLoop()
    {
        if (controller == null || controller.associatedDrumPad == null) return;

        float loopDuration = controller.masterController.GetTotalDuration();
        float currentLoopTime = (Time.time - recordStartTime) % loopDuration;

        foreach (NoteEvent note in recordedNotes)
        {
            if (Mathf.Abs(currentLoopTime - note.timeStamp) < Time.deltaTime)
            {
                if (note.isNoteOn)
                {
                    controller.associatedDrumPad.PlayDrumSound();
                    return;
                }
            }
        }
    }

    private void UpdateVisualState()
    {
        if (buttonRenderer != null)
        {
            if (isArmedForRecording)
            {
                buttonRenderer.sprite = recordArmedSprite;
            }
            else if (isArmedForPlayback)
            {
                buttonRenderer.sprite = playbackArmedSprite;
            }
            else if (HasRecording())
            {
                buttonRenderer.sprite = recordedSprite;
            }
            else
            {
                buttonRenderer.sprite = emptySprite;
            }
        }
    }

    public void SaveTrackData()
    {
        SavedTrackData data = new SavedTrackData
        {
            notes = new List<NoteEvent>(recordedNotes),
            isArmedForPlayback = isArmedForPlayback
        };
        TrackDataManager.SaveTrackData(trackId, data);
    }

    public void LoadTrackData()
    {
        var data = TrackDataManager.LoadTrackData(trackId);
        if (data != null)
        {
            recordedNotes = new List<NoteEvent>(data.notes);
            if (data.isArmedForPlayback)
            {
                ArmForPlayback();
            }
            UpdateVisualState();
        }
    }

    public bool HasRecording()
    {
        return recordedNotes.Count > 0;
    }

    public bool IsArmedForRecording()
    {
        return isArmedForRecording;
    }

    public bool IsArmedForPlayback()
    {
        return isArmedForPlayback;
    }

    public bool IsRecording()
    {
        return isRecording;
    }

    public bool IsPlaying()
    {
        return isPlaying;
    }
}