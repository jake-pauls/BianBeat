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

    private void Awake()
    {
        // TODO: Something should probably set the expression value on masks that spawn.
        ExpressionValue = Expression.Sad;
    }

    private void Update()
    {
        // Move across the track.
        Vector3 pos = transform.position;
        pos.x -= m_Velocity * Time.deltaTime;
        transform.position = pos;
    }
}
