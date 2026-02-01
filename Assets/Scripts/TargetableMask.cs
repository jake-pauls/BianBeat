using FaceDetection;
using UnityEngine;

/// <summary>
/// An instance of a targetable mask that spawns on the rhythm track.
/// </summary>
public class TargetableMask : MonoBehaviour
{
    [HideInInspector]
    public Expression ExpressionValue;
    
    [SerializeField] 
    [Min(0)]
    [Tooltip("The speed for the mask once it is spawned on the track.")]
    private float m_Velocity;

    [SerializeField]
    [Tooltip("Reference to the PlayerController to move towards.")]
    private PlayerController m_TargetPlayerController;

    private Vector3 m_TargetPosition;

    private void Awake()
    {
        // TODO: Something should probably set the expression value on masks that spawn.
        ExpressionValue = Expression.Shocked;
    }

    private void Start()
    {
        // Find PlayerController
        if (m_TargetPlayerController == null)
        {
            m_TargetPlayerController = FindObjectOfType<PlayerController>();
        }

        // Set target position
        if (m_TargetPlayerController != null)
        {
            m_TargetPosition = m_TargetPlayerController.transform.position;
        }
    }

    private void Update()
    {
        if (m_TargetPosition != null)
        {
            //Calculate the direction to the target
            Vector3 direction = (m_TargetPosition - transform.position).normalized;

            // Move towards target at constant velocity
            transform.position += direction * m_Velocity * Time.deltaTime;
        }
    }
}
