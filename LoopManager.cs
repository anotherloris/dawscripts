// LoopManager.cs
using UnityEngine;

public class LoopManager : MonoBehaviour
{
    [Header("Global Settings")]
    public float bpm = 120f;

    [HideInInspector]
    public float loopDurationInSeconds;

    public float GetFrameTime()
    {
        return 60f / (bpm * 4);
    }

    public float GetBarDuration()
    {
        return (60f / bpm) * 4;
    }
}
