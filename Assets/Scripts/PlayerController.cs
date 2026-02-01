using System;
using System.Collections.Generic;
using System.Linq;
using FaceDetection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

/// <summary>
/// Player controller that attempts to score points from targets on the rhythm track.
/// </summary>
public class PlayerController : MonoBehaviour
{
    // TODO: It's kind of weird that the player tells the target to die and invokes target killed events. We could
    // try to separate these concerns. However, if we do this the other way every target would have to check if the
    // player killed them.
    //
    // (¬_¬")
    public UnityEvent OnTargetHit; 
    
    /// <summary>
    /// The current expression of the player. Updated by the <see cref="BlemBarracudaRunner"/>.
    /// </summary>
    public Expression CurrentExpression;
    
    [SerializeField] 
    private CircleCollider2D m_ControllerCircleCollider;

    private void Update()
    {
        List<Collider2D> colliders = new();
        int hits = m_ControllerCircleCollider.Overlap(colliders);
        if (hits <= 0)
            return;
        
        // Take the first collider since only one target should be intersected at a time. 
        Collider2D intersectedCollider = colliders.First();
        
        // Check if the player's current expression matches the mask.
        GameObject intersectedObject = intersectedCollider.gameObject;
        TargetableMask mask = intersectedObject.GetComponent<TargetableMask>();
        if (mask is not null && mask.ExpressionValue == CurrentExpression)
        {
            Debug.Log("Target hit!");
            // Destroy the intersected target game object and broadcast that a target was hit.
            Destroy(intersectedObject);
            OnTargetHit?.Invoke();
        }
    }
}
