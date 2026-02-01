using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manager class that spawns targets on the rhythm track.
/// </summary>
public class TargetSpawner : MonoBehaviour
{
    [SerializeField] 
    [Tooltip("The prefab that the manager will spawn when the cooldown elapses.")]
    private GameObject m_TargetPrefab;
    
    [SerializeField] 
    [Tooltip("The position where the manager will spawn new targets.")]
    private Transform m_SpawnPoint;

    [SerializeField]
    [Tooltip("Reference to the player controller.")]
    private PlayerController m_PlayerController;

    [SerializeField]
    [Tooltip("The CSV file containing the beatmap data (TextAsset).")]
    private TextAsset m_CSVFile;

    [SerializeField]
    [Tooltip("The beatmap data that the manager will use to spawn targets.")]
    private BeatmapData m_BeatmapData;

    [SerializeField]
    [Tooltip("The velocity of the spawnedtarget.")]
    private float m_TargetVelocity = 3f;

    [SerializeField]
    [Tooltip("Countdown duration before the song starts.")]
    private float m_CountdownDuration = 3f;
    
    //TODO: Can likely separate out game manager logic instead of having it i the TargetSpawner. Putting it in here for now for convenience.
    private float m_CurrentTime;
    private float m_GameStartTime;
    private bool m_isCountingDown = true;
    private bool m_GameStarted = false;
    private int m_CurrentNoteIndex = 0;
    private List<float> m_SpawnTimes = new();

    private void Awake()
    {
        if (m_TargetPrefab is null)
            Debug.LogWarning("No target prefab has been set for the rhythm manager!");

        if (m_SpawnPoint is null)
            Debug.LogWarning("No spawn point has been set for the rhythm manager!");

        if (m_BeatmapData is null)
            Debug.LogWarning("No beatmap data has been set for the rhythm manager!");
    }

    private void Start()
    {
        // Load beatmap data from CSV file.
        if (m_CSVFile != null && m_BeatmapData == null)
        {
            m_BeatmapData = BeatmapLoader.LoadFromTextAsset(m_CSVFile);
            Debug.Log($"Loaded {m_BeatmapData.notes.Count} notes from {m_CSVFile.name}");
        }

        m_SpawnTimes.Clear(); // Clear the list of spawn times.
        CalculateSpawnTimes(); // Calculate spawn times based on the beatmap data.

        // Start the countdown.
        m_GameStartTime = Time.time + m_CountdownDuration;
        StartCoroutine(CountdownCoroutine());
    }

    private void Update()
    {
        if (!m_GameStarted || m_BeatmapData == null)
            return;

        float currentGameTime = Time.time - m_GameStartTime;

        while (m_CurrentNoteIndex < m_SpawnTimes.Count && currentGameTime >= m_SpawnTimes[m_CurrentNoteIndex])
        {
            SpawnTarget(m_BeatmapData.notes[m_CurrentNoteIndex]);
            m_CurrentNoteIndex++;
        }
    }

    private void SpawnTarget(BeatmapNote note)
    {
        GameObject target = Instantiate(m_TargetPrefab, m_SpawnPoint);
        TargetableMask mask = target.GetComponent<TargetableMask>();
    }

    private void CalculateSpawnTimes()
    {
        Debug.Log("Calculating spawn times...");
        if (m_BeatmapData == null || m_PlayerController == null)
            return;

        // Calculate the distance from the spawn point point to player collider.
        float distance = Vector3.Distance (
            m_SpawnPoint.position,
            m_PlayerController.transform.position
        );

        // Calculate the time it takes for target to travel from spawn to collider.
        float travelTime = distance / m_TargetVelocity;
        
        // Calculate all spawn times
        foreach (var note in m_BeatmapData.notes)
        {
            float spawnTime = note.timestamp - travelTime;
            m_SpawnTimes.Add(spawnTime);
        }
    }

    private System.Collections.IEnumerator CountdownCoroutine()
    {
        //Wait for countdown
        float countdown = m_CountdownDuration;
        while (countdown > 0.0f)
        {
            countdown -= Time.deltaTime;
            yield return null;
        }

        m_isCountingDown = false;
        m_GameStarted = true;
        m_GameStartTime = Time.time;
    }
}