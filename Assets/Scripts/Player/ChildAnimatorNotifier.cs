using UnityEngine;

public class ChildAnimatorNotifier : MonoBehaviour
{
    public void NotifyParent()
    {
        // Find the PlayerController script on the parent object
        PlayerAttack attackScript = GetComponentInParent<PlayerAttack>();

        if (attackScript != null)
        {
            attackScript.EndAttack();
        }
        else
        {
            Debug.LogWarning("Parent object does not have a PlayerController script.");
        }
    }
}