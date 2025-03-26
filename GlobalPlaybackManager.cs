// GlobalPlaybackManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalPlaybackManager : MonoBehaviour
{
    [Header("Visual Settings")]
    public Sprite enabledSprite;
    public Sprite disabledSprite;

    private SpriteRenderer buttonRenderer;
    private bool isEnabled = false;

    private void Start()
    {
        buttonRenderer = GetComponent<SpriteRenderer>();
        UpdateVisual();
    }

    private void OnMouseDown()
    {
        isEnabled = !isEnabled;
        UpdateVisual();
        Debug.Log($"Global playback {(isEnabled ? "enabled" : "disabled")}");
    }

    private void UpdateVisual()
    {
        if (buttonRenderer != null)
        {
            buttonRenderer.sprite = isEnabled ? enabledSprite : disabledSprite;
        }
    }

    public bool IsEnabled()
    {
        return isEnabled;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateVisual();
    }
}