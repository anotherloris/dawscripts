// EffectController.cs
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Audio;

public class EffectController : MonoBehaviour
{
    public enum EffectType
    {
        None,
        Reverb,
        Delay
    }

    [Header("Mixer Groups")]
    public AudioMixerGroup cleanGroup;
    public AudioMixerGroup reverbGroup;
    public AudioMixerGroup delayGroup;

    [Header("Octave Settings")]
    public int defaultOctave = 2;
    public int minOctave = 0;
    public int maxOctave = 7;

    [HideInInspector]
    public int currentOctave;
    public EffectType currentEffect = EffectType.None;

    public UnityEvent onOctaveChanged;
    public UnityEvent onEffectChanged;

    private void Start()
    {
        currentOctave = Mathf.Clamp(defaultOctave, minOctave, maxOctave);
        onOctaveChanged?.Invoke();
    }

    public void IncreaseOctave()
    {
        if (currentOctave < maxOctave)
        {
            currentOctave++;
            onOctaveChanged?.Invoke();
        }
    }

    public void DecreaseOctave()
    {
        if (currentOctave > minOctave)
        {
            currentOctave--;
            onOctaveChanged?.Invoke();
        }
    }

    public int GetCurrentOctave()
    {
        return currentOctave;
    }

    public void ToggleEffect(EffectType effectType)
    {
        if (currentEffect == effectType)
        {
            currentEffect = EffectType.None;
        }
        else
        {
            currentEffect = effectType;
        }

        UpdateAllPianoKeyMixerGroups();
        onEffectChanged?.Invoke();
    }

    private void UpdateAllPianoKeyMixerGroups()
    {
        PianoKey[] allKeys = FindObjectsOfType<PianoKey>();

        AudioMixerGroup targetGroup = cleanGroup;

        switch (currentEffect)
        {
            case EffectType.Reverb:
                targetGroup = reverbGroup;
                break;
            case EffectType.Delay:
                targetGroup = delayGroup;
                break;
        }

        foreach (PianoKey key in allKeys)
        {
            key.UpdateMixerGroup(targetGroup);
        }
    }

    public EffectType GetCurrentEffect()
    {
        return currentEffect;
    }
}