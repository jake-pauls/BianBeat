using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;


public static class BeatmapLoader
{
    // Load beatmap data from a TextAsset (CSV file).
    public static BeatmapData LoadFromTextAsset (TextAsset csvAsset)
    {
        Debug.Log("Loading beatmap data from TextAsset...");
        BeatmapData beatmap = ScriptableObject.CreateInstance<BeatmapData>();
        beatmap.notes =  new List<BeatmapNote>();

        if (csvAsset == null)
        {
            Debug.LogError("CSV asset is null!");
            return beatmap;
        }

        string[]lines = csvAsset.text.Split('\n');

        //Skip header line
        int startIndex = 0;
        if (lines.Length > 0 && lines[0].StartsWith("timestamp"))
        {
            startIndex = 1;
        }

        for (int i = startIndex; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line))
                continue;

            string[] values = line.Split(',');
            if (values.Length >=1)
            {
                if (float.TryParse(values[0], out float timestamp))
                {
                    Debug.Log($"Adding note with timestamp: {timestamp}");
                    beatmap.notes.Add(new BeatmapNote { timestamp = timestamp });
                }
            }
        }

        Debug.Log($"Loaded {beatmap.notes.Count} notes from {csvAsset.name}");
            
        return beatmap;
    }
}
