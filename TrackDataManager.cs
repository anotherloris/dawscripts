// TrackDataManager.cs
using System.Collections.Generic;
using UnityEngine;

public static class TrackDataManager
{
    private static Dictionary<string, DrumTrack.SavedTrackData> savedTracks =
        new Dictionary<string, DrumTrack.SavedTrackData>();

    public static void SaveTrackData(string trackId, DrumTrack.SavedTrackData data)
    {
        savedTracks[trackId] = data;
        Debug.Log($"Saved track data for ID: {trackId}");
    }

    public static DrumTrack.SavedTrackData LoadTrackData(string trackId)
    {
        if (savedTracks.ContainsKey(trackId))
        {
            Debug.Log($"Loaded track data for ID: {trackId}");
            return savedTracks[trackId];
        }
        return null;
    }

    public static bool HasTrackData(string trackId)
    {
        return savedTracks.ContainsKey(trackId);
    }
}