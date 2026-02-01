using FaceDetection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MaskFillButton : MonoBehaviour
{
    /// <summary>
    /// Invoked when the mask fill button is filled to 100%.
    /// </summary>
    public UnityEvent OnButtonFilled;
    
    [Header("References")]
    [SerializeField] 
    [Tooltip("The mask image to control the fill along with the player's expression being held.")]
    private Image m_MaskImage;
    
    [SerializeField] 
    [Tooltip("Used to get the current expression from the player to use the button.")]
    private PlayerController m_PlayerController;

    [SerializeField] 
    [Tooltip("The expression the player has to hold to trigger the button.")]
    private Expression m_ExpressionToPressButton;

    [Header("Timing")]
    [SerializeField] 
    [Tooltip("The duration of time the player has to hold the expression matched with the button.")]
    private float m_ExpressionHoldDuration;

    private float m_CurrentExpressionHoldDuration;

    private void Awake()
    {
        if (m_PlayerController is null)
            Debug.LogWarning("No reference to the player controller is set, using buttons in the UI will fail!");
        
        if (m_MaskImage is null)
            Debug.LogWarning("No reference to a mask image is set, the mask fill button will fail!");
    }

    private void Update()
    {
        if (m_PlayerController.CurrentExpression == m_ExpressionToPressButton)
        {
            m_CurrentExpressionHoldDuration += Time.deltaTime;
            m_MaskImage.fillAmount = Mathf.Clamp01(m_CurrentExpressionHoldDuration / m_ExpressionHoldDuration);
        }

        if (m_CurrentExpressionHoldDuration >= m_ExpressionHoldDuration)
        {
            OnButtonFilled.Invoke();
        }
    }
}
