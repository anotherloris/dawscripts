// NoteEvent.cs
[System.Serializable]
public class NoteEvent
{
    public int keyIndex;
    public float timeStamp;
    public bool isNoteOn;
    public string trackID;
    public int octave;
    public EffectController.EffectType effect;
}