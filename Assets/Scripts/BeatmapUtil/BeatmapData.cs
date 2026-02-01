using UnityEngine;
using System;
using System.Collections.Generic;
using FaceDetection;

/// <summary>
/// A scriptable object that contains the data for a beatmap.
/// </summary>

[Serializable]
public class BeatmapNote
{
    public float timestamp;
    public Expression expression;
}

[CreateAssetMenu(fileName = "BeatmapData", menuName = "Beatmap File")]
public class BeatmapData : ScriptableObject
{
    public List<BeatmapNote> notes = new();
    
}
