using UnityEngine;

public class PlaySFX : MonoBehaviour
{
    public AudioClip Attack;
    public AudioClip Jump;
    public AudioClip Land;
    public AudioClip Hit;
    public AudioClip Hurt;
    public AudioClip Dash;

    public void PlayAttack()
        {
            AudioSource.PlayClipAtPoint(Attack, transform.position);
        }

        public void PlayJump()
        {
            AudioSource.PlayClipAtPoint(Jump, transform.position);
        }

        public void PlayLand()
        {
            AudioSource.PlayClipAtPoint(Land, transform.position);
        }

        public void PlayHit()
        {
            AudioSource.PlayClipAtPoint(Hit, transform.position);
        }

        public void PlayHurt()
        {

            AudioSource.PlayClipAtPoint(Hurt, transform.position);
        }

        public void PlayDash()
        {
            AudioSource.PlayClipAtPoint(Dash, transform.position);
        }
    }
