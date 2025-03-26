// PersistentManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistentManager : MonoBehaviour
{
    public static PersistentManager Instance { get; private set; }
    public GlobalPlaybackManager globalPlayback { get; private set; }
    public RecordingManager recordingManager { get; private set; }

    [Header("Scene Names")]
    public string pianoSceneName = "PianoScene";
    public string drumSceneName = "DrumScene";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            globalPlayback = GetComponentInChildren<GlobalPlaybackManager>();
            recordingManager = GetComponentInChildren<RecordingManager>();

            if (globalPlayback == null)
                Debug.LogError("No GlobalPlaybackManager found in children!");
            if (recordingManager == null)
                Debug.LogError("No RecordingManager found in children!");
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void LoadPianoScene()
    {
        SceneManager.LoadScene(pianoSceneName);
        Debug.Log("Loading Piano Scene");
    }

    public void LoadDrumScene()
    {
        SceneManager.LoadScene(drumSceneName);
        Debug.Log("Loading Drum Scene");
    }
}