// InstrumentSwitchButton.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class InstrumentSwitchButton : MonoBehaviour
{
    public string pianoSceneName = "PianoScene";
    public string drumSceneName = "DrumScene";

    [Header("Visual Settings")]
    public Sprite pianoIcon;
    public Sprite drumIcon;

    private SpriteRenderer spriteRenderer;
    private bool isShowingPiano;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        isShowingPiano = SceneManager.GetActiveScene().name == pianoSceneName;
        UpdateSprite();
    }

    private void OnMouseDown()
    {
        SwitchInstrument();
    }

    private void SwitchInstrument()
    {
        string nextScene = isShowingPiano ? drumSceneName : pianoSceneName;
        isShowingPiano = !isShowingPiano;

        SceneManager.LoadScene(nextScene, LoadSceneMode.Single);

        UpdateSprite();
        Debug.Log($"Switching to {nextScene}");
    }

    private void UpdateSprite()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = isShowingPiano ? drumIcon : pianoIcon;
        }
    }
}