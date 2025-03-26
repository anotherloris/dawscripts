using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class DrumController : MonoBehaviour
{
    [Header("References")]
    public DrumPad associatedDrumPad;
    public MasterRecordController masterController;
    public DrumTrack[] tracks;

    [Header("Category Selection")]
    public SpriteRenderer categoryButton;
    public Sprite[] categorySprites;

    [Header("Sample Selection")]
    public TMP_Dropdown sampleDropdown;
    public GameObject dropdownPositionReference;
    public bool dropdownOpensUpward = false;
    [Range(5, 50)]
    public int maxNameLength = 20;

    private int currentCategoryIndex = 0;
    private DrumTrack currentlyArmedTrack;

    private void Awake()
    {
        // Find and setup all Canvas components
        Canvas[] canvases = GetComponentsInChildren<Canvas>(true);
        foreach (Canvas canvas in canvases)
        {
            if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                canvas.worldCamera = Camera.main;
                Debug.Log($"Assigned main camera to canvas: {canvas.gameObject.name}");
            }
        }
    }

    private void Start()
    {
        // Initialize tracks
        if (tracks == null || tracks.Length != 4)
        {
            Debug.LogError("DrumController must have exactly 4 tracks assigned!");
        }

        foreach (DrumTrack track in tracks)
        {
            track.Initialize(this);
        }

        // Set initial category sprite
        if (categoryButton != null && categorySprites != null && categorySprites.Length > 0)
        {
            categoryButton.sprite = categorySprites[0];
        }

        // Configure dropdown direction
        if (sampleDropdown != null)
        {
            if (dropdownPositionReference != null)
            {
                sampleDropdown.transform.position = dropdownPositionReference.transform.position;
            }

            var dropdownList = sampleDropdown.GetComponent<TMP_Dropdown>().template;
            if (dropdownList != null)
            {
                var rectTransform = dropdownList.GetComponent<RectTransform>();
                if (dropdownOpensUpward)
                {
                    rectTransform.pivot = new Vector2(0.5f, 0f);
                    rectTransform.anchorMin = new Vector2(0f, 1f);
                    rectTransform.anchorMax = new Vector2(1f, 1f);
                }
                else
                {
                    rectTransform.pivot = new Vector2(0.5f, 1f);
                    rectTransform.anchorMin = new Vector2(0f, 0f);
                    rectTransform.anchorMax = new Vector2(1f, 0f);
                }
            }
        }

        // Initialize dropdown with first category's samples
        UpdateSampleDropdown();
    }

    private void UpdateSampleDropdown()
    {
        if (sampleDropdown != null)
        {
            sampleDropdown.ClearOptions();

            var category = SampleManager.Instance.GetCategory(currentCategoryIndex);
            if (category != null)
            {
                List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();

                for (int i = 0; i < category.samples.Length; i++)
                {
                    if (category.samples[i] != null)
                    {
                        string originalName = category.samples[i].name;
                        string displayName = originalName;

                        // Take only the first line if there are line breaks
                        int lineBreakIndex = displayName.IndexOf('\n');
                        if (lineBreakIndex >= 0)
                        {
                            displayName = displayName.Substring(0, lineBreakIndex);
                        }

                        // Truncate if too long
                        if (displayName.Length > maxNameLength)
                        {
                            displayName = displayName.Substring(0, maxNameLength - 3) + "...";
                        }

                        options.Add(new TMP_Dropdown.OptionData(displayName));
                    }
                }

                sampleDropdown.AddOptions(options);
                sampleDropdown.onValueChanged.RemoveAllListeners();
                sampleDropdown.onValueChanged.AddListener(OnSampleSelected);

                if (options.Count > 0)
                {
                    sampleDropdown.value = 0;
                    OnSampleSelected(0);
                }
            }
        }
    }

    private void OnSampleSelected(int index)
    {
        var category = SampleManager.Instance.GetCategory(currentCategoryIndex);
        if (category != null && index < category.samples.Length)
        {
            associatedDrumPad.SetDrumSound(category.samples[index]);
            associatedDrumPad.SetVolume(category.categoryVolume);
        }
    }

    public void OnCategoryButtonPressed()
    {
        // Cycle to next category
        currentCategoryIndex = (currentCategoryIndex + 1) % SampleManager.Instance.GetCategoryCount();

        // Update button sprite
        if (categoryButton != null && categorySprites != null && currentCategoryIndex < categorySprites.Length)
        {
            categoryButton.sprite = categorySprites[currentCategoryIndex];
        }

        // Get new category volume
        var category = SampleManager.Instance.GetCategory(currentCategoryIndex);
        if (category != null)
        {
            associatedDrumPad.SetVolume(category.categoryVolume);
        }

        // Update dropdown with new category's samples
        UpdateSampleDropdown();
    }

    public void OnTrackButtonPressed(DrumTrack pressedTrack)
    {
        if (pressedTrack == currentlyArmedTrack)
        {
            DisarmCurrentTrack();
            return;
        }

        DisarmCurrentTrack();
        currentlyArmedTrack = pressedTrack;

        if (pressedTrack.HasRecording())
        {
            pressedTrack.ArmForPlayback();
        }
        else
        {
            pressedTrack.ArmForRecording();
        }
    }

    private void DisarmCurrentTrack()
    {
        if (currentlyArmedTrack != null)
        {
            currentlyArmedTrack.Disarm();
            currentlyArmedTrack = null;
        }
    }

    public bool HasTracksArmedForRecording()
    {
        return currentlyArmedTrack != null && currentlyArmedTrack.IsArmedForRecording();
    }

    public bool HasTracksArmedForPlayback()
    {
        return currentlyArmedTrack != null && currentlyArmedTrack.IsArmedForPlayback();
    }

    public void OnRecordingStarted()
    {
        if (currentlyArmedTrack != null)
        {
            if (currentlyArmedTrack.IsArmedForRecording())
            {
                currentlyArmedTrack.StartRecording();
            }
            else if (currentlyArmedTrack.IsArmedForPlayback())
            {
                currentlyArmedTrack.StartPlayback();
            }
        }
    }

    public void OnPlaybackStarted()
    {
        if (currentlyArmedTrack != null && currentlyArmedTrack.IsArmedForPlayback())
        {
            currentlyArmedTrack.StartPlayback();
        }
    }

    public void OnPlaybackStopped()
    {
        if (currentlyArmedTrack != null)
        {
            currentlyArmedTrack.StopPlayback();
        }
    }

    public void OnRecordingStopped()
    {
        if (currentlyArmedTrack != null)
        {
            if (currentlyArmedTrack.IsRecording())
            {
                currentlyArmedTrack.StopRecording();
            }
            else if (currentlyArmedTrack.IsArmedForPlayback())
            {
                currentlyArmedTrack.StopPlayback();
            }
        }
    }

    public void AddNoteToRecording(NoteEvent note)
    {
        if (currentlyArmedTrack != null && currentlyArmedTrack.IsRecording())
        {
            currentlyArmedTrack.AddNote(note);
        }
    }
}