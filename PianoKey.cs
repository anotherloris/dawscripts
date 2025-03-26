// PianoKey.cs
using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

public class PianoKey : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioClip[] noteOctaves;
    public AudioMixerGroup mixerGroup;
    public int audioSourceCount = 3;
    public static int maxSimultaneousNotes = 5;
    public int keyIndex;

    [SerializeField]
    private int simultaneousNotesLimit = 5;

    private AudioSource[] audioSources;
    private int currentAudioSource = 0;
    private static List<AudioSource> currentlyPlayingNotes = new List<AudioSource>();

    [Header("Sprite Settings")]
    public Sprite defaultSprite;
    public Sprite pressedSprite;
    private SpriteRenderer spriteRenderer;

    private EffectController effectController;
    private LoopManager loopManager;

    private HashSet<int> activeTouchIds = new HashSet<int>();
    private HashSet<int> touchesTriggeredThisKey = new HashSet<int>();
    private List<int> touchesToRemove = new List<int>();
    private bool isPressed = false;

    private void OnValidate()
    {
        maxSimultaneousNotes = simultaneousNotesLimit;
    }

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        effectController = FindObjectOfType<EffectController>();
        if (effectController == null)
        {
            Debug.LogError("No EffectController found in scene!");
        }

        loopManager = FindObjectOfType<LoopManager>();
        if (loopManager == null)
        {
            Debug.LogError("No LoopManager found in scene!");
        }

        audioSources = new AudioSource[audioSourceCount];
        for (int i = 0; i < audioSourceCount; i++)
        {
            audioSources[i] = gameObject.AddComponent<AudioSource>();
            audioSources[i].playOnAwake = false;
            audioSources[i].outputAudioMixerGroup = mixerGroup;
        }

        if (spriteRenderer != null && defaultSprite != null)
            spriteRenderer.sprite = defaultSprite;
    }

    public void UpdateMixerGroup(AudioMixerGroup newGroup)
    {
        mixerGroup = newGroup;
        foreach (AudioSource source in audioSources)
        {
            source.outputAudioMixerGroup = mixerGroup;
        }
    }

    public void PlayNoteDirectly(int octave, EffectController.EffectType effect)
    {
        if (noteOctaves != null && noteOctaves.Length > 0)
        {
            if (octave < noteOctaves.Length)
            {
                AudioClip currentNote = noteOctaves[octave];

                if (currentNote != null)
                {
                    currentlyPlayingNotes.RemoveAll(source => !source.isPlaying);

                    if (currentlyPlayingNotes.Count >= maxSimultaneousNotes)
                    {
                        if (currentlyPlayingNotes.Count > 0)
                        {
                            AudioSource oldestSource = currentlyPlayingNotes[0];
                            oldestSource.Stop();
                            currentlyPlayingNotes.RemoveAt(0);
                        }
                    }

                    AudioSource currentSource = audioSources[currentAudioSource];
                    currentSource.clip = currentNote;
                    currentSource.PlayOneShot(currentNote);
                    currentlyPlayingNotes.Add(currentSource);

                    currentAudioSource = (currentAudioSource + 1) % audioSourceCount;

                    StartCoroutine(ShowKeyPress());
                }
            }
        }
    }

    private void PlayNote()
    {
        if (effectController != null && noteOctaves != null && noteOctaves.Length > 0)
        {
            int currentOctave = effectController.currentOctave;

            if (currentOctave < noteOctaves.Length)
            {
                AudioClip currentNote = noteOctaves[currentOctave];

                if (currentNote != null)
                {
                    currentlyPlayingNotes.RemoveAll(source => !source.isPlaying);

                    if (currentlyPlayingNotes.Count >= maxSimultaneousNotes)
                    {
                        if (currentlyPlayingNotes.Count > 0)
                        {
                            AudioSource oldestSource = currentlyPlayingNotes[0];
                            oldestSource.Stop();
                            currentlyPlayingNotes.RemoveAt(0);
                        }
                    }

                    AudioSource currentSource = audioSources[currentAudioSource];
                    currentSource.clip = currentNote;
                    currentSource.PlayOneShot(currentNote);
                    currentlyPlayingNotes.Add(currentSource);

                    currentAudioSource = (currentAudioSource + 1) % audioSourceCount;

                    NoteEvent noteEvent = new NoteEvent
                    {
                        keyIndex = keyIndex,
                        isNoteOn = true,
                        octave = effectController.currentOctave,
                        effect = effectController.GetCurrentEffect()
                    };

                    LoopTrack[] tracks = FindObjectsOfType<LoopTrack>();
                    foreach (LoopTrack track in tracks)
                    {
                        track.AddNote(noteEvent);
                    }
                }
            }
        }
    }

    private System.Collections.IEnumerator ShowKeyPress()
    {
        spriteRenderer.sprite = pressedSprite;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.sprite = defaultSprite;
    }

    void Update()
    {
        if (Input.touchCount > 0)
        {
            HashSet<int> currentTouches = new HashSet<int>();

            foreach (Touch touch in Input.touches)
            {
                Vector2 touchPosition = Camera.main.ScreenToWorldPoint(touch.position);
                RaycastHit2D hit = Physics2D.Raycast(touchPosition, Vector2.zero);

                if (hit.collider != null && hit.collider.gameObject == gameObject)
                {
                    currentTouches.Add(touch.fingerId);

                    if (!touchesTriggeredThisKey.Contains(touch.fingerId))
                    {
                        touchesTriggeredThisKey.Add(touch.fingerId);
                        activeTouchIds.Add(touch.fingerId);
                        PlayNote();
                        SetPressed(true);
                    }
                }
                else
                {
                    touchesTriggeredThisKey.Remove(touch.fingerId);
                }
            }

            touchesToRemove.Clear();

            foreach (int touchId in activeTouchIds)
            {
                if (!currentTouches.Contains(touchId))
                {
                    touchesToRemove.Add(touchId);
                    touchesTriggeredThisKey.Remove(touchId);
                }
            }

            foreach (int touchId in touchesToRemove)
            {
                activeTouchIds.Remove(touchId);
            }

            if (activeTouchIds.Count == 0 && isPressed)
            {
                SetPressed(false);
            }
        }
        else if (activeTouchIds.Count > 0)
        {
            activeTouchIds.Clear();
            touchesTriggeredThisKey.Clear();
            SetPressed(false);
        }

#if UNITY_EDITOR
        if (Input.GetMouseButton(0))
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

            if (hit.collider != null && hit.collider.gameObject == gameObject && !isPressed)
            {
                PlayNote();
                SetPressed(true);
            }
            else if ((hit.collider == null || hit.collider.gameObject != gameObject) && isPressed)
            {
                SetPressed(false);
            }
        }
        else if (Input.GetMouseButtonUp(0) && isPressed)
        {
            SetPressed(false);
        }
#endif
    }

    private void SetPressed(bool pressed)
    {
        isPressed = pressed;
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = pressed ? pressedSprite : defaultSprite;
        }

        if (!pressed)
        {
            NoteEvent noteEvent = new NoteEvent
            {
                keyIndex = keyIndex,
                isNoteOn = false,
                octave = effectController.currentOctave,
                effect = effectController.GetCurrentEffect()
            };

            LoopTrack[] tracks = FindObjectsOfType<LoopTrack>();
            foreach (LoopTrack track in tracks)
            {
                track.AddNote(noteEvent);
            }
        }
    }

    private void OnDestroy()
    {
        foreach (AudioSource source in audioSources)
        {
            if (currentlyPlayingNotes.Contains(source))
                currentlyPlayingNotes.Remove(source);
        }
    }
}