using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    [SerializeField] 
    private Image m_ProgressBarImage;

    private bool m_BeatmapMusicStarted;
    private float m_BeatmapMusicDuration;

    public void OnBeatmapMusicStarted()
        => m_BeatmapMusicStarted = true;

    private void Awake()
    {
        if (m_ProgressBarImage is null)
        {
            Debug.LogWarning("The provided progress bar image is null! The progress bar will not deplete properly.");
            return;
        }

        if (m_ProgressBarImage.type != Image.Type.Filled)
        {
            Debug.LogWarning($"The provided progress bar image type is not {Image.Type.Filled}, depletion may not work properly.");
            return;
        }
    }

    private void Start()
    {
        // This has to be done after the AudioManager is awakened.
        // We love lifetimes. (´。＿。｀)
        m_BeatmapMusicDuration = AudioManager.GetSoundDuration(AudioType.BEATMAPMUSIC);   
    }

    private void Update()
    {
        if (!m_BeatmapMusicStarted)
            return;
        
        m_ProgressBarImage.fillAmount += Time.deltaTime / m_BeatmapMusicDuration;
    }
}
