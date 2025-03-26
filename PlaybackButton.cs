using UnityEngine;

public class PlaybackButton : MonoBehaviour
{
    // Define the types of playback actions available.
    public enum ButtonType { Play, Pause, Restart }
    public ButtonType buttonType;

    // This method is called when the collider attached to this GameObject is clicked.
    private void OnMouseDown()
    {
        if (PlaybackManager.Instance == null)
        {
            Debug.LogError("PlaybackButton: PlaybackManager.Instance is null.");
            return;
        }

        // Call the corresponding method on PlaybackManager based on the button type.
        switch (buttonType)
        {
            case ButtonType.Play:
                // If already playing, resume; else, start playback.
                PlaybackManager.Instance.Play(false);
                Debug.Log("PlaybackButton [Play]: Play triggered.");
                break;
            case ButtonType.Pause:
                PlaybackManager.Instance.TogglePause();
                Debug.Log("PlaybackButton [Pause]: Pause triggered.");
                break;
            case ButtonType.Restart:
                PlaybackManager.Instance.Stop();
                PlaybackManager.Instance.Play(true); // Start from beginning.
                Debug.Log("PlaybackButton [Restart]: Restart triggered.");
                break;
            default:
                Debug.LogWarning("PlaybackButton: Undefined button type.");
                break;
        }
    }
}