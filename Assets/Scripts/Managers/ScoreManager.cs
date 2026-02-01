using TMPro;
using UnityEngine;

/// <summary>
/// Manager for the current score. Also updates the score number in the UI.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    [SerializeField] 
    [Tooltip("Reference to the text block containing the score number in the UI.")]
    private TMP_Text m_ScoreNumberText;

    [SerializeField] 
    [Min(0)]
    [Tooltip("The amount of score granted per target hit.")]
    private int m_ScorePerTargetHit;

    private int m_CurrentScore;

    /// <summary>
    /// Currently bound against the player's OnTargetHit event, increments the score and updates the score in the UI.
    /// </summary>
    public void OnTargetHit()
    {
        m_CurrentScore += m_ScorePerTargetHit;
        m_ScoreNumberText.text = m_CurrentScore.ToString();
    }
}
