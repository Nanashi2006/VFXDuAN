using UnityEngine;

namespace VFXDuAN.DungeonTank
{
    [DisallowMultipleComponent]
    public class SkeletonEnemyHealth : MonoBehaviour
    {
        [Min(1f)] public float maxHealth = 60f;
        public GameObject hitVFXPrefab;
        public GameObject deathVFXPrefab;
        public Animator animator;

        private float currentHealth;
        private bool dead;
        private SkeletonEnemyAI ai;
        private Collider[] colliders;

        public bool IsDead => dead;
        public float CurrentHealth => currentHealth;

        private void Awake()
        {
            currentHealth = maxHealth;
            ai = GetComponent<SkeletonEnemyAI>();
            colliders = GetComponentsInChildren<Collider>(true);
            if (animator == null) animator = GetComponentInChildren<Animator>();
        }

        public void TakeDamage(float amount)
        {
            if (dead || amount <= 0f) return;

            currentHealth = Mathf.Max(0f, currentHealth - amount);

            if (hitVFXPrefab != null)
            {
                GameObject fx = Instantiate(hitVFXPrefab, transform.position + Vector3.up * 1.1f, Quaternion.identity);
                fx.name = "VFX_Skeleton_Hit__Runtime";
                Destroy(fx, 5f);
            }

            TryTrigger("Hit");

            if (currentHealth <= 0f)
                Die();
        }

        private void Die()
        {
            dead = true;
            if (ai != null) ai.enabled = false;

            foreach (Collider c in colliders)
                if (c != null) c.enabled = false;

            TryTrigger("Die");
            TryTrigger("Death");

            if (deathVFXPrefab != null)
            {
                GameObject fx = Instantiate(deathVFXPrefab, transform.position + Vector3.up * 0.7f, Quaternion.identity);
                fx.name = "VFX_Skeleton_Death__Runtime";
                Destroy(fx, 7f);
            }

            Destroy(gameObject, 2.8f);
        }

        private void TryTrigger(string parameter)
        {
            if (animator == null) return;
            foreach (AnimatorControllerParameter p in animator.parameters)
            {
                if (p.name == parameter && p.type == AnimatorControllerParameterType.Trigger)
                {
                    animator.SetTrigger(parameter);
                    return;
                }
            }
        }
    }
}
