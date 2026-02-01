using UnityEngine;

public enum AudioType
{
    MAINMENU,
    STARTBUTTON,
    COUNTDOWN,
    BEATMAPMUSIC,
    CORRECTMASK,
    WRONGMASK,
    COMBOSTART,
    COMBOMASK,
    FANOPEN,
    FANCLOSE,
    GAMEWIN
}
[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioClip[] m_AudioList;
    private static AudioManager m_Instance;
    private AudioSource m_AudioSource;

    public void Awake()
    {
        m_Instance = this;
    }

    private void Start()
    {
        m_AudioSource = GetComponent<AudioSource>();
    }

    public static void PlaySound(AudioType audio, float volume = 1.0f)
    {
        m_Instance.m_AudioSource.PlayOneShot(m_Instance.m_AudioList[(int)audio], volume);
    }
}
