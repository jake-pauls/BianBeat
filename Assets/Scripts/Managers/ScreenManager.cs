using UnityEngine;
using UnityEngine.SceneManagement;

public class ScreenManager : MonoBehaviour
{
    [Header("Management")] 
    [SerializeField]
    private TargetSpawner m_TargetSpawner;
    
    [Header("Screens")]
    [SerializeField] 
    [Tooltip("Root GameObject containing the start game screen.")]
    private GameObject m_StartGameScreenRoot;
    
    [SerializeField] 
    [Tooltip("Root GameObject containing the game screen.")]
    private GameObject m_GameScreenRoot;
    
    [SerializeField] 
    [Tooltip("Root GameObject containing the end game screen.")]
    private GameObject m_EndGameScreenRoot;

    private GameObject m_CurrentScreenGameObject;

    /// <summary>
    /// Always start the game in the start screen after being awakened;
    /// </summary>
    private void Awake()
        => TransitionToStartScreen();

    [ContextMenu("Transition to Start Game Screen")]
    public void TransitionToStartScreen()
    {
        if (m_CurrentScreenGameObject == m_StartGameScreenRoot)
            return;
        
        m_CurrentScreenGameObject?.SetActive(false);
        m_CurrentScreenGameObject = m_StartGameScreenRoot;
        
        m_StartGameScreenRoot?.SetActive(true);
    }

    [ContextMenu("Transition to Game Screen")]
    public void TransitionToGameScreen()
    {
        if (m_CurrentScreenGameObject == m_GameScreenRoot)
            return;
        
        m_CurrentScreenGameObject?.SetActive(false);
        m_CurrentScreenGameObject = m_GameScreenRoot;
        
        m_GameScreenRoot?.SetActive(true);
        
        // When we transition to the game screen start the game!
        m_TargetSpawner.StartGame();
    }

    [ContextMenu("Transition to End Game Screen")]
    public void TransitionToEndGameScreen()
    {
        if (m_CurrentScreenGameObject == m_EndGameScreenRoot)
            return;
        
        m_CurrentScreenGameObject?.SetActive(false);
        m_CurrentScreenGameObject = m_EndGameScreenRoot;
        
        m_EndGameScreenRoot?.SetActive(true);
    }

    /// <summary>
    /// "Restarts" the game that just reloads the scene name.
    /// </summary>
    public void RestartGame()
        => SceneManager.LoadScene(SceneManager.GetActiveScene().name);

    /// <summary>
    /// Exits the current game.
    /// </summary>
    public void ExitGame()
        => Application.Quit();
}
