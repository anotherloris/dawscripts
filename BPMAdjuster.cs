using UnityEngine;

public class BPMAdjuster : MonoBehaviour
{
    // Whether to show the input window.
    private bool showInputField = false;
    // The text the user inputs.
    private string inputBPM = "";
    // Rect defining the window position and size.
    private Rect windowRect = new Rect(100, 100, 220, 110);

    void Start()
    {
        // Initialize the input field with the current BPM value, if available.
        if (AudioManager.Instance != null)
            inputBPM = AudioManager.Instance.currentBPM.ToString();
    }

    // OnMouseDown is called when the GameObject is clicked/tapped.
    private void OnMouseDown()
    {
        // Open the input window.
        showInputField = true;
    }

    // OnGUI handles drawing the input window when required.
    private void OnGUI()
    {
        if (showInputField)
        {
            // Draw a draggable window for BPM input.
            windowRect = GUI.Window(0, windowRect, BPMWindow, "Set BPM (60-300)");
        }
    }

    // Contents of the BPM window.
    private void BPMWindow(int windowID)
    {
        GUI.Label(new Rect(10, 20, 200, 20), "Enter BPM:");
        // Create a text field for input (max length of 5 characters).
        inputBPM = GUI.TextField(new Rect(10, 45, 200, 25), inputBPM, 5);

        // Submit button: validate and update BPM.
        if (GUI.Button(new Rect(10, 75, 95, 25), "Submit"))
        {
            int newBPM;
            if (int.TryParse(inputBPM, out newBPM))
            {
                if (newBPM >= 60 && newBPM <= 300)
                {
                    AudioManager.Instance.SetBPM(newBPM);
                    Debug.Log("BPMAdjuster: BPM set to " + newBPM);
                }
                else
                {
                    Debug.Log("BPMAdjuster: BPM value out of range (60-300).");
                }
            }
            else
            {
                Debug.Log("BPMAdjuster: Invalid BPM value.");
            }
            // Hide the window after submission.
            showInputField = false;
        }
        // Cancel button: close the window.
        if (GUI.Button(new Rect(115, 75, 95, 25), "Cancel"))
        {
            showInputField = false;
        }
        // Allow the window to be dragged by the top area.
        GUI.DragWindow(new Rect(0, 0, 10000, 20));
    }
}