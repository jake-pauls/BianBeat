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
    private float m_Velocity = 5f;

    [SerializeField]
    [Tooltip("Time a target can be on top of the player before it is destroyed.")]
    private float m_MissTimeout = 0.5f;

    [SerializeField]
    [Tooltip("Animation curve for scaling. X-axis is progress (0=spawn, 1=center), Y-axis is scale multiplier. Default: scales from 1x at spawn to 7x at center.")]
    // private AnimationCurve m_ScaleCurve = AnimationCurve.EaseOut(0f, 1f, 1f, 7f);
    private AnimationCurve m_ScaleCurve = new AnimationCurve(
    new Keyframe(0f, 1f, 0f, 5f),
    new Keyframe(0.1f, 3f, 2.5f, 0.5f),
    new Keyframe(1f, 7f, 0f, 0f)
);

    public UnityEvent OnTargetMissed;

    private Vector3 m_TargetPosition;
    private Vector3 m_InitialPosition;
    private Vector3 m_InitialScale;
    private float m_TotalDistance;
    private PlayerController m_PlayerController;
    private CircleCollider2D m_PlayerCollider;
    private bool m_IsOverlapping = false;
    private Coroutine m_MissTimeoutCoroutine;
    private bool m_IsDestroyed = false;

    private void Start()
    {
        // Store initial position and scale
        m_InitialPosition = transform.position;
        m_InitialScale = transform.localScale;

        // Find PlayerController
        PlayerController playerController = FindFirstObjectByType<PlayerController>();

        if (playerController != null)
        {
            m_TargetPosition = playerController.transform.position;
            m_PlayerCollider = playerController.GetComponent<CircleCollider2D>();
            
            // Calculate total distance from spawn to target
            m_TotalDistance = Vector3.Distance(m_InitialPosition, m_TargetPosition);
        }
    }

    private void Update()
    {
        if (m_TargetPosition != null && m_TotalDistance > 0f)
        {
            // //Calculate the direction to the target
            // Vector3 direction = (m_TargetPosition - transform.position).normalized;

            // // Move towards target at constant velocity
            // transform.position += direction * m_Velocity * Time.deltaTime;

            float distanceToTarget = Vector3.Distance(transform.position, m_TargetPosition);

            // Stop moving if we are close to the target
            float minDistance = 0.01f;
            if (distanceToTarget > minDistance)
            {
                // Using Vector3.MoveTowards to move towards the target
                float moveDistance = m_Velocity * Time.deltaTime;
                transform.position = Vector3.MoveTowards(transform.position, m_TargetPosition, moveDistance);
            }
            else
            {
                // Snap to target position when very close to prevent jittering
                transform.position = m_TargetPosition;
            }

            // Calculate progress (0 = at spawn, 1 = at center)
            float currentDistance = Vector3.Distance(transform.position, m_TargetPosition);
            float progress = 1f - (currentDistance / m_TotalDistance);
            progress = Mathf.Clamp01(progress);

            // Scale based on progress using the animation curve
            float scaleMultiplier = m_ScaleCurve.Evaluate(progress);
            transform.localScale = m_InitialScale * scaleMultiplier;
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
            AudioManager.PlaySound(AudioType.WRONGMASK);
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
