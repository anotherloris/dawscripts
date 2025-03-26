using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    // Global BPM; the default is 120.
    public int currentBPM = 120;

    // Reference to one row (e.g., the first row) used for column counting.
    public RowController rowController;

    // Each column represents 16 beats.
    private const int beatsPerColumn = 16;

    // Track length properties.
    public int trackLengthBeats { get; private set; } = 0;
    public float trackLengthSeconds { get; private set; } = 0f;
    public string formattedTrackLength { get; private set; } = "";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist between scenes if desired.
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UpdateRowsBPM();
        UpdateTrackLength();
    }

    public void SetBPM(int newBPM)
    {
        currentBPM = newBPM;
        Debug.Log("AudioManager: BPM updated to " + currentBPM);
        UpdateRowsBPM();
        UpdateTrackLength();
    }

    private void UpdateRowsBPM()
    {
        RowController[] rows = FindObjectsOfType<RowController>();
        foreach (RowController row in rows)
        {
            row.UpdateBPM(currentBPM);
        }
    }

    // Calculates the track length based on the number of columns (from DynamicRowManager)
    // and then updates the display.
    public void UpdateTrackLength()
    {
        if (rowController == null)
        {
            Debug.LogError("AudioManager: rowController is not assigned!");
            return;
        }

        // Get the column count from DynamicRowManager.
        int numColumns = DynamicRowManager.Instance.GetColumnCount(rowController);
        trackLengthBeats = numColumns * beatsPerColumn;

        // Calculate time in seconds: each beat lasts 60 / BPM seconds.
        if (currentBPM > 0)
            trackLengthSeconds = trackLengthBeats * (60f / currentBPM);
        else
            trackLengthSeconds = 0f;

        int minutes = Mathf.FloorToInt(trackLengthSeconds / 60f);
        int seconds = Mathf.FloorToInt(trackLengthSeconds % 60f);
        int milliseconds = Mathf.FloorToInt((trackLengthSeconds * 1000f) % 1000f);
        // Convert milliseconds to hundredths (two digits).
        int hundredths = milliseconds / 10;

        // Format the string as "MM:SS:HH".
        formattedTrackLength = string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, hundredths);
        Debug.Log("AudioManager: Track length is " + formattedTrackLength);

        // Update the display immediately.
        TrackLengthDisplay display = FindObjectOfType<TrackLengthDisplay>();
        if (display != null)
            display.UpdateDisplay(formattedTrackLength);
        else
            Debug.LogWarning("AudioManager: No TrackLengthDisplay found in the scene.");
    }
}