using UnityEngine;
using TMPro;

public class TrackLengthDisplay : MonoBehaviour
{
    private TMP_Text trackLengthText;

    private void Awake()
    {
        trackLengthText = GetComponent<TMP_Text>();
        if (trackLengthText == null)
        {
            Debug.LogError("TrackLengthDisplay: No TMP_Text component found on " + gameObject.name);
        }
    }

    // Called directly by AudioManager.UpdateTrackLength() after the calculation.
    public void UpdateDisplay(string newFormattedTrackLength)
    {
        if (trackLengthText != null)
        {
            trackLengthText.text = newFormattedTrackLength;
            Debug.Log("TrackLengthDisplay: Updated display to: " + trackLengthText.text);
        }
        else
        {
            Debug.LogError("TrackLengthDisplay: TMP_Text is not assigned.");
        }
    }
}