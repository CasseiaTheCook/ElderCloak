using UnityEngine;

// This class acts as a bridge for Animation Events from a child Animator
// to call methods on scripts on the parent GameObject.
public class ChildAnimatorNotifier : MonoBehaviour
{
    private PlayerAttack attackScript;

    private void Awake()
    {
        attackScript = GetComponentInParent<PlayerAttack>();
    }

    // Called by Animation Event at the end of the attack animation.
    public void NotifyEndAttack()
    {
        if (attackScript != null)
        {
            attackScript.EndAttack();
        }
    }
}