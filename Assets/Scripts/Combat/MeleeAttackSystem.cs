using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ElderCloak.Interfaces;

namespace ElderCloak.Combat
{
    /// <summary>
    /// Comprehensive melee attack system for 2D platformer combat.
    /// Supports multiple attack types, combos, and hitbox detection.
    /// Designed for extensibility and integration with animation systems.
    /// </summary>
    [System.Serializable]
    public class MeleeAttackSystem : IAttackable
    {
        [Header("Attack Configuration")]
        [SerializeField] private float attackDamage = 25f;
        [SerializeField] private float attackRange = 2f;
        [SerializeField] private float attackDuration = 0.3f;
        [SerializeField] private float attackCooldown = 0.5f;
        [SerializeField] private LayerMask targetLayerMask = -1;

        [Header("Attack Types")]
        [SerializeField] private AttackData[] attackVariations;
        [SerializeField] private bool enableComboSystem = true;
        [SerializeField] private float comboWindow = 1.5f;
        [SerializeField] private int maxComboCount = 3;

        [Header("Knockback Settings")]
        [SerializeField] private float knockbackForce = 10f;
        [SerializeField] private float knockbackDuration = 0.2f;
        [SerializeField] private AnimationCurve knockbackCurve = AnimationCurve.EaseOut(0, 1, 1, 0);

        [Header("Hit Effects")]
        [SerializeField] private GameObject hitEffectPrefab;
        [SerializeField] private AudioClip[] hitSounds;
        [SerializeField] private AudioClip[] attackSounds;
        [SerializeField] private float hitStopDuration = 0.1f;

        // Internal state
        private MonoBehaviour owner;
        private Transform ownerTransform;
        private bool isAttacking;
        private float lastAttackTime;
        private int currentCombo;
        private float comboTimer;
        private List<Collider2D> hitTargets = new List<Collider2D>();

        // Events
        public System.Action<Vector2> OnAttackStarted;
        public System.Action OnAttackEnded;
        public System.Action<GameObject> OnTargetHit;
        public System.Action<int> OnComboIncreased;

        /// <summary>
        /// Initialize the attack system with an owner.
        /// </summary>
        /// <param name="owner">The MonoBehaviour that owns this attack system</param>
        public void Initialize(MonoBehaviour owner)
        {
            this.owner = owner;
            this.ownerTransform = owner.transform;
            
            // Initialize default attack if no variations are set
            if (attackVariations == null || attackVariations.Length == 0)
            {
                attackVariations = new AttackData[] { CreateDefaultAttack() };
            }
        }

        /// <summary>
        /// Update method to be called from the owner's Update method.
        /// </summary>
        public void Update()
        {
            UpdateComboTimer();
        }

        #region IAttackable Implementation

        public void Attack(Vector2 direction)
        {
            if (!CanAttack()) return;

            AttackData currentAttackData = GetCurrentAttackData();
            if (currentAttackData == null) return;

            StartAttack(direction, currentAttackData);
        }

        public bool CanAttack()
        {
            return !isAttacking && Time.time >= lastAttackTime + attackCooldown;
        }

        public float GetAttackDamage()
        {
            AttackData currentAttackData = GetCurrentAttackData();
            return currentAttackData?.damage ?? attackDamage;
        }

        public float GetAttackRange()
        {
            AttackData currentAttackData = GetCurrentAttackData();
            return currentAttackData?.range ?? attackRange;
        }

        #endregion

        #region Attack Execution

        /// <summary>
        /// Start an attack with the given direction and attack data.
        /// </summary>
        private void StartAttack(Vector2 direction, AttackData attackData)
        {
            isAttacking = true;
            lastAttackTime = Time.time;
            hitTargets.Clear();

            // Update combo system
            if (enableComboSystem)
            {
                if (Time.time <= comboTimer + comboWindow && currentCombo < maxComboCount)
                {
                    currentCombo++;
                }
                else
                {
                    currentCombo = 1;
                }
                
                comboTimer = Time.time;
                OnComboIncreased?.Invoke(currentCombo);
            }

            // Play attack sound
            PlayAttackSound(attackData);

            // Trigger attack events
            OnAttackStarted?.Invoke(direction);

            // Start the attack coroutine
            if (owner != null)
            {
                owner.StartCoroutine(ExecuteAttack(direction, attackData));
            }
        }

        /// <summary>
        /// Execute the attack over time.
        /// </summary>
        private IEnumerator ExecuteAttack(Vector2 direction, AttackData attackData)
        {
            float attackTime = 0f;
            float totalDuration = attackData.duration > 0 ? attackData.duration : attackDuration;

            while (attackTime < totalDuration)
            {
                attackTime += Time.deltaTime;
                
                // Check for hits during active frames
                float normalizedTime = attackTime / totalDuration;
                if (normalizedTime >= attackData.hitboxStartTime && normalizedTime <= attackData.hitboxEndTime)
                {
                    CheckForHits(direction, attackData);
                }

                yield return null;
            }

            isAttacking = false;
            OnAttackEnded?.Invoke();
        }

        /// <summary>
        /// Check for hits in the attack area.
        /// </summary>
        private void CheckForHits(Vector2 direction, AttackData attackData)
        {
            Vector2 attackPosition = ownerTransform.position;
            Vector2 attackDirection = direction.normalized;
            
            // Use different hit detection shapes based on attack type
            Collider2D[] hits = null;
            
            switch (attackData.hitboxShape)
            {
                case HitboxShape.Circle:
                    hits = Physics2D.OverlapCircleAll(
                        attackPosition + attackDirection * (attackData.range * 0.5f),
                        attackData.range * 0.5f,
                        targetLayerMask
                    );
                    break;

                case HitboxShape.Box:
                    hits = Physics2D.OverlapBoxAll(
                        attackPosition + attackDirection * (attackData.range * 0.5f),
                        new Vector2(attackData.range, attackData.height),
                        0f,
                        targetLayerMask
                    );
                    break;

                case HitboxShape.Cone:
                    hits = GetConeHits(attackPosition, attackDirection, attackData);
                    break;
            }

            if (hits != null)
            {
                ProcessHits(hits, attackData, attackDirection);
            }
        }

        /// <summary>
        /// Get hits in a cone-shaped area (for directional attacks).
        /// </summary>
        private Collider2D[] GetConeHits(Vector2 position, Vector2 direction, AttackData attackData)
        {
            // Simple cone detection using multiple overlaps
            List<Collider2D> coneHits = new List<Collider2D>();
            int rayCount = 5;
            float coneAngle = attackData.coneAngle;
            
            for (int i = 0; i < rayCount; i++)
            {
                float angle = Mathf.Lerp(-coneAngle * 0.5f, coneAngle * 0.5f, i / (float)(rayCount - 1));
                Vector2 rayDirection = Quaternion.AngleAxis(angle, Vector3.forward) * direction;
                
                RaycastHit2D hit = Physics2D.Raycast(position, rayDirection, attackData.range, targetLayerMask);
                if (hit.collider != null && !coneHits.Contains(hit.collider))
                {
                    coneHits.Add(hit.collider);
                }
            }
            
            return coneHits.ToArray();
        }

        /// <summary>
        /// Process all hit targets.
        /// </summary>
        private void ProcessHits(Collider2D[] hits, AttackData attackData, Vector2 attackDirection)
        {
            foreach (var hit in hits)
            {
                // Skip if already hit during this attack
                if (hitTargets.Contains(hit)) continue;
                
                // Skip if hitting self
                if (hit.transform == ownerTransform) continue;

                ProcessSingleHit(hit, attackData, attackDirection);
                hitTargets.Add(hit);
            }
        }

        /// <summary>
        /// Process a single hit target.
        /// </summary>
        private void ProcessSingleHit(Collider2D target, AttackData attackData, Vector2 attackDirection)
        {
            // Apply damage
            IDamageable damageable = target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                float damage = attackData.damage > 0 ? attackData.damage : attackDamage;
                
                // Apply combo damage multiplier
                if (enableComboSystem && currentCombo > 1)
                {
                    damage *= attackData.comboDamageMultiplier;
                }
                
                damageable.TakeDamage(damage, ownerTransform.gameObject);
            }

            // Apply knockback
            ApplyKnockback(target, attackDirection, attackData);

            // Spawn hit effects
            SpawnHitEffects(target.transform.position);

            // Play hit sound
            PlayHitSound();

            // Apply hit stop effect
            if (hitStopDuration > 0)
            {
                StartHitStop();
            }

            // Trigger hit event
            OnTargetHit?.Invoke(target.gameObject);
        }

        #endregion

        #region Effects and Feedback

        /// <summary>
        /// Apply knockback to a target.
        /// </summary>
        private void ApplyKnockback(Collider2D target, Vector2 direction, AttackData attackData)
        {
            Rigidbody2D targetRb = target.GetComponent<Rigidbody2D>();
            if (targetRb == null) return;

            float kbForce = attackData.knockbackForce > 0 ? attackData.knockbackForce : knockbackForce;
            Vector2 knockbackDir = direction.normalized;
            
            // Add some upward force for more dynamic knockback
            knockbackDir = new Vector2(knockbackDir.x, knockbackDir.y + 0.3f).normalized;
            
            targetRb.AddForce(knockbackDir * kbForce, ForceMode2D.Impulse);
        }

        /// <summary>
        /// Spawn visual hit effects.
        /// </summary>
        private void SpawnHitEffects(Vector3 position)
        {
            if (hitEffectPrefab != null)
            {
                GameObject effect = Object.Instantiate(hitEffectPrefab, position, Quaternion.identity);
                Object.Destroy(effect, 2f); // Auto cleanup
            }
        }

        /// <summary>
        /// Play attack sound effects.
        /// </summary>
        private void PlayAttackSound(AttackData attackData)
        {
            AudioClip[] sounds = attackData.attackSounds?.Length > 0 ? attackData.attackSounds : attackSounds;
            if (sounds != null && sounds.Length > 0)
            {
                AudioClip sound = sounds[Random.Range(0, sounds.Length)];
                if (sound != null)
                {
                    // Play sound (requires AudioSource component on owner)
                    AudioSource audioSource = owner.GetComponent<AudioSource>();
                    if (audioSource != null)
                    {
                        audioSource.PlayOneShot(sound);
                    }
                }
            }
        }

        /// <summary>
        /// Play hit sound effects.
        /// </summary>
        private void PlayHitSound()
        {
            if (hitSounds != null && hitSounds.Length > 0)
            {
                AudioClip sound = hitSounds[Random.Range(0, hitSounds.Length)];
                if (sound != null)
                {
                    AudioSource audioSource = owner.GetComponent<AudioSource>();
                    if (audioSource != null)
                    {
                        audioSource.PlayOneShot(sound);
                    }
                }
            }
        }

        /// <summary>
        /// Start hit stop effect (brief pause for impact feedback).
        /// </summary>
        private void StartHitStop()
        {
            if (owner != null)
            {
                owner.StartCoroutine(HitStopCoroutine());
            }
        }

        /// <summary>
        /// Hit stop coroutine.
        /// </summary>
        private IEnumerator HitStopCoroutine()
        {
            float originalTimeScale = Time.timeScale;
            Time.timeScale = 0.1f;
            yield return new WaitForSecondsRealtime(hitStopDuration);
            Time.timeScale = originalTimeScale;
        }

        #endregion

        #region Combo System

        /// <summary>
        /// Update combo timer and reset combo if window expires.
        /// </summary>
        private void UpdateComboTimer()
        {
            if (enableComboSystem && Time.time > comboTimer + comboWindow)
            {
                currentCombo = 0;
            }
        }

        /// <summary>
        /// Get the current attack data based on combo state.
        /// </summary>
        private AttackData GetCurrentAttackData()
        {
            if (attackVariations == null || attackVariations.Length == 0)
                return null;

            if (enableComboSystem && currentCombo > 0)
            {
                int index = Mathf.Min(currentCombo - 1, attackVariations.Length - 1);
                return attackVariations[index];
            }

            return attackVariations[0];
        }

        /// <summary>
        /// Get current combo count.
        /// </summary>
        public int GetCurrentCombo() => currentCombo;

        /// <summary>
        /// Reset combo counter manually.
        /// </summary>
        public void ResetCombo()
        {
            currentCombo = 0;
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Create a default attack data configuration.
        /// </summary>
        private AttackData CreateDefaultAttack()
        {
            return new AttackData
            {
                attackName = "Basic Attack",
                damage = attackDamage,
                range = attackRange,
                duration = attackDuration,
                hitboxStartTime = 0.3f,
                hitboxEndTime = 0.7f,
                hitboxShape = HitboxShape.Circle,
                knockbackForce = knockbackForce,
                comboDamageMultiplier = 1.0f
            };
        }

        /// <summary>
        /// Check if currently attacking.
        /// </summary>
        public bool IsAttacking() => isAttacking;

        #endregion

        #region Debug Visualization

        /// <summary>
        /// Draw attack range gizmos for debugging.
        /// </summary>
        public void DrawDebugGizmos(Vector2 direction)
        {
            if (ownerTransform == null) return;

            AttackData currentAttackData = GetCurrentAttackData();
            if (currentAttackData == null) return;

            Vector2 position = ownerTransform.position;
            Vector2 attackDir = direction.normalized;

            Gizmos.color = isAttacking ? Color.red : Color.yellow;

            switch (currentAttackData.hitboxShape)
            {
                case HitboxShape.Circle:
                    Gizmos.DrawWireSphere(position + attackDir * (currentAttackData.range * 0.5f), 
                                         currentAttackData.range * 0.5f);
                    break;

                case HitboxShape.Box:
                    Vector3 boxCenter = position + attackDir * (currentAttackData.range * 0.5f);
                    Vector3 boxSize = new Vector3(currentAttackData.range, currentAttackData.height, 1f);
                    Gizmos.DrawWireCube(boxCenter, boxSize);
                    break;

                case HitboxShape.Cone:
                    // Draw cone representation with lines
                    float halfAngle = currentAttackData.coneAngle * 0.5f;
                    Vector2 leftEdge = Quaternion.AngleAxis(-halfAngle, Vector3.forward) * attackDir;
                    Vector2 rightEdge = Quaternion.AngleAxis(halfAngle, Vector3.forward) * attackDir;
                    
                    Gizmos.DrawLine(position, position + leftEdge * currentAttackData.range);
                    Gizmos.DrawLine(position, position + rightEdge * currentAttackData.range);
                    Gizmos.DrawLine(position + leftEdge * currentAttackData.range, 
                                   position + rightEdge * currentAttackData.range);
                    break;
            }
        }

        #endregion
    }

    #region Data Structures

    /// <summary>
    /// Data structure for different attack variations.
    /// </summary>
    [System.Serializable]
    public class AttackData
    {
        [Header("Basic Settings")]
        public string attackName = "Attack";
        public float damage = 25f;
        public float range = 2f;
        public float duration = 0.3f;
        public float height = 1f; // For box hitboxes

        [Header("Timing")]
        [Range(0f, 1f)] public float hitboxStartTime = 0.3f;
        [Range(0f, 1f)] public float hitboxEndTime = 0.7f;

        [Header("Hitbox")]
        public HitboxShape hitboxShape = HitboxShape.Circle;
        public float coneAngle = 60f; // For cone-shaped attacks

        [Header("Effects")]
        public float knockbackForce = 10f;
        public float comboDamageMultiplier = 1.2f;

        [Header("Audio")]
        public AudioClip[] attackSounds;
    }

    /// <summary>
    /// Enum for different hitbox shapes.
    /// </summary>
    public enum HitboxShape
    {
        Circle,
        Box,
        Cone
    }

    #endregion
}