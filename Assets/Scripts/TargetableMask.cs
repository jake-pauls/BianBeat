using FaceDetection;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;

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
    [Tooltip("Time a target can be on top of the player before it is destroyed.")]
    private float m_MissTimeout = 0.5f;

    public UnityEvent OnTargetMissed;

    private Vector3 m_TargetPosition;
    private PlayerController m_PlayerController;
    private CircleCollider2D m_PlayerCollider;
    private bool m_IsOverlapping = false;
    private Coroutine m_MissTimeoutCoroutine;
    private bool m_IsDestroyed = false;

    private void Start()
    {
        // Find PlayerController
        PlayerController playerController = FindFirstObjectByType<PlayerController>();

        if (playerController != null)
        {
            m_TargetPosition = playerController.transform.position;
            m_PlayerCollider = playerController.GetComponent<CircleCollider2D>();
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

        // Check if we are overlapping with the player collider
        if (m_PlayerCollider != null && !m_IsDestroyed)
        {
            bool currentlyOverlapping = m_PlayerCollider.OverlapPoint(transform.position);

            if (currentlyOverlapping && !m_IsOverlapping)
            {
                // Just started overlapping
                m_IsOverlapping = true;
                if (m_MissTimeoutCoroutine == null)
                {
                    m_MissTimeoutCoroutine = StartCoroutine(MissTimeoutCoroutine());
                }
            }
            else if (!currentlyOverlapping && m_IsOverlapping)
            {
                // No longer overlapping / destroyed
                m_IsOverlapping = false;
                if (m_MissTimeoutCoroutine != null)
                {
                    StopCoroutine(m_MissTimeoutCoroutine);
                    m_MissTimeoutCoroutine = null;
                }
            }
        }
    }

    private IEnumerator MissTimeoutCoroutine()
    {
        yield return new WaitForSeconds(m_MissTimeout);

        // If we're still overlapping and target isn't destroyed, we missed the target
        if (m_IsOverlapping && !m_IsDestroyed)
        {
            OnTargetMissed?.Invoke();
            Debug.Log("Target missed!");
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        m_IsDestroyed = true;
        if (m_MissTimeoutCoroutine != null)
        {
            StopCoroutine(m_MissTimeoutCoroutine);
        }
    }
}
