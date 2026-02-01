using System;

namespace FaceDetection
{
    /// <summary>
    /// Enumeration of expressions the face detection tracks.
    /// Idea: We could use 'Flags' because a player may match multiple emotions depending on our heuristic.
    /// </summary>
    [Serializable]
    public enum Expression
    {
        Neutral,
        Happy,
        Sad,
        Angry,
        Shocked,
        // This should always be at the end of the enumeration.
        // Note that BLEM does not consider this a valid output label.
        Max
    }
}