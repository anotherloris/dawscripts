using UnityEngine;
using TMPro;

public class BPMDisplay : MonoBehaviour
{
    // Cached reference to the TMP_Text component on this GameObject.
    private TMP_Text bpmText;

    private void Awake()
    {
        bpmText = GetComponent<TMP_Text>();
        if (bpmText == null)
        {
            Debug.LogError("BPMDisplay: No TMP_Text component found on " + gameObject.name);
        }
    }

    private void Start()
    {
        UpdateDisplay();
    }

    // Update is called every frame.
    private void Update()
    {
        UpdateDisplay();
    }

    // Updates the text to display the current BPM.
    public void UpdateDisplay()
    {
        if (bpmText != null && AudioManager.Instance != null)
        {
            bpmText.text = AudioManager.Instance.currentBPM.ToString() + " BPM";
        }
    }
}