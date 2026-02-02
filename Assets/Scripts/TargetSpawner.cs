using UnityEngine;
using System.Collections.Generic;
using FaceDetection;
using UnityEngine.Events;

/// <summary>
/// Manager class that spawns targets on the rhythm track.
/// </summary>
public class TargetSpawner : MonoBehaviour
{
    /// <summary>
    /// Invoked when the countdown finishes and the beatmap song starts.
    /// </summary>
    public UnityEvent OnBeatmapMusicStarted;
    
    [SerializeField] 
    [Tooltip("The prefab that the manager will spawn when the cooldown elapses.")]
    private GameObject[] m_TargetMaskPrefabs;
    
    [SerializeField] 
    [Tooltip("The position where the manager will spawn new targets.")]
    private Transform[] m_SpawnPoints;

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

    [SerializeField] 
    private ScreenManager m_ScreenManager;
    
    //TODO: Can likely separate out game manager logic instead of having it i the TargetSpawner. Putting it in here for now for convenience.
    private float m_CurrentTime;
    private float m_GameStartTime;
    // private bool m_isCountingDown = true; // TODO: Might need this on game start perhaps? Not doing anything with it for now.
    private bool m_GameStarted = false;
    private bool m_GameOver;
    private int m_CurrentNoteIndex = 0;
    private float m_BeatmapDuration;

    [SerializeField]
    private List<float> m_SpawnTimes = new();

    public void StartGame()
    {
        // Start the countdown.
        m_GameStartTime = Time.time + m_CountdownDuration;
        StartCoroutine(CountdownCoroutine());
    }

    private void Awake()
    {
        if (m_TargetMaskPrefabs is null)
            Debug.LogWarning("No target prefabs have been set for the rhythm manager!");

        if (m_SpawnPoints is null)
            Debug.LogWarning("No spawn points have been set for the rhythm manager!");

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

        m_BeatmapDuration = AudioManager.GetSoundDuration(AudioType.BEATMAPMUSIC);
    }

    private void Update()
    {
        if (!m_GameStarted || m_BeatmapData == null)
            return;

        float currentGameTime = Time.time - m_GameStartTime;

        while (m_CurrentNoteIndex < m_SpawnTimes.Count && currentGameTime >= m_SpawnTimes[m_CurrentNoteIndex])
        {
            SpawnTarget(m_BeatmapData.notes[m_CurrentNoteIndex], m_BeatmapData.notes[m_CurrentNoteIndex].expression);
            m_CurrentNoteIndex++;
        }

        // Transition to the end game screen once the duration is elapsed.
        if (currentGameTime >= m_BeatmapDuration && !m_GameOver)
        {
            m_ScreenManager.TransitionToEndGameScreen();
            m_GameOver = true;
        }
    }

    private void SpawnTarget(BeatmapNote note, Expression expression)
    {
        // Randomly select one of the spawn points
        if (m_SpawnPoints == null || m_SpawnPoints.Length == 0)
        {
            Debug.LogWarning("No spawn points available!");
            return;
        }

        GameObject target = null;
        if (expression == Expression.Happy)
        {
            Transform happySpawnPoint = m_SpawnPoints[0];
            target = Instantiate(m_TargetMaskPrefabs[0], happySpawnPoint.position, happySpawnPoint.rotation);
        }
        else if (expression == Expression.Sad)
        {
            Transform sadSpawnPoint = m_SpawnPoints[1];
            target = Instantiate(m_TargetMaskPrefabs[1], sadSpawnPoint.position, sadSpawnPoint.rotation);
        }
        else if (expression == Expression.Angry)
        {
            Transform angrySpawnPoint = m_SpawnPoints[2];
            target = Instantiate(m_TargetMaskPrefabs[2], angrySpawnPoint.position, angrySpawnPoint.rotation);
        }
        else if (expression == Expression.Shocked)
        {
            Transform shockedSpawnPoint = m_SpawnPoints[3];
            target = Instantiate(m_TargetMaskPrefabs[3], shockedSpawnPoint.position, shockedSpawnPoint.rotation);
        }

        TargetableMask mask = target.GetComponent<TargetableMask>();
        mask.ExpressionValue = expression;
    }

    private void CalculateSpawnTimes()
    {
        Debug.Log("Calculating spawn times...");
        if (m_BeatmapData == null || m_PlayerController == null)
            return;

        if (m_SpawnPoints == null || m_SpawnPoints.Length == 0)
        {
            Debug.LogWarning("No spawn points available for distance calculation!");
            return;
        }

        // Calculate the maximum distance from any spawn point to player to ensure targets always arrive on time
        float maxDistance = 0f;
        foreach (Transform spawnPoint in m_SpawnPoints)
        {
            if (spawnPoint != null)
            {
                float distance = Vector3.Distance(
                    spawnPoint.position,
                    m_PlayerController.transform.position
                );
                if (distance > maxDistance)
                    maxDistance = distance;
            }
        }

        // Calculate the time it takes for target to travel from the farthest spawn point to collider.
        float travelTime = maxDistance / m_TargetVelocity;
        
        // Calculate all spawn times
        foreach (var note in m_BeatmapData.notes)
        {
            float spawnTime = note.timestamp - travelTime + 0.5f;
            m_SpawnTimes.Add(spawnTime);
        }
    }

    private System.Collections.IEnumerator CountdownCoroutine()
    {
        AudioManager.PlaySound(AudioType.COUNTDOWN);

        //Wait for countdown
        float countdown = m_CountdownDuration;
        while (countdown > 0.0f)
        {
            countdown -= Time.deltaTime;
            yield return null;
        }

        // m_isCountingDown = false; // TODO: Might need this on game start perhaps? Not doing anything with it for now.
        m_GameStarted = true;
        m_GameStartTime = Time.time;
        
        AudioManager.PlaySound(AudioType.BEATMAPMUSIC, 0.6f);
        OnBeatmapMusicStarted.Invoke();
    }
}