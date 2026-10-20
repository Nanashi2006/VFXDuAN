using UnityEngine;

namespace VFXDuAN.DungeonTank
{
    [DisallowMultipleComponent]
    public class CrusaderTankHealth : MonoBehaviour
    {
        [Min(1f)] public float maxHealth = 100f;
        public GameObject deathVFXPrefab;

        private float currentHealth;
        private bool dead;
        private CrusaderTankController controller;
        private Rigidbody body;
        private Collider[] colliders;
        private Renderer[] renderers;

        public float MaxHealth => maxHealth;
        public float CurrentHealth => currentHealth;
        public bool IsDead => dead;
        public Vector3 InitialSpawnPosition { get; private set; }
        public Quaternion InitialSpawnRotation { get; private set; }

        private void Awake()
        {
            currentHealth = maxHealth;
            controller = GetComponent<CrusaderTankController>();
            body = GetComponent<Rigidbody>();
            colliders = GetComponentsInChildren<Collider>(true);
            renderers = GetComponentsInChildren<Renderer>(true);
            InitialSpawnPosition = transform.position;
            InitialSpawnRotation = transform.rotation;
        }

        public void TakeDamage(float amount)
        {
            if (dead || amount <= 0f) return;

            currentHealth = Mathf.Max(0f, currentHealth - amount);
            if (currentHealth <= 0f)
                Die();
        }

        private void Die()
        {
            dead = true;
            if (controller != null) controller.SetGameplayEnabled(false);

            if (deathVFXPrefab != null)
            {
                GameObject fx = Instantiate(deathVFXPrefab, transform.position + Vector3.up, Quaternion.identity);
                fx.name = "VFX_Tank_Destroyed__Runtime";
                Destroy(fx, 6f);
            }

            foreach (Renderer r in renderers)
                if (r != null) r.enabled = false;

            foreach (Collider c in colliders)
                if (c != null && !c.isTrigger) c.enabled = false;

            DungeonGameManager manager = DungeonGameManager.Instance;
            if (manager != null)
                manager.RequestRespawn(this);
            else
                ReviveAt(InitialSpawnPosition, InitialSpawnRotation);
        }

        public void ReviveAt(Vector3 position, Quaternion rotation)
        {
            transform.SetPositionAndRotation(position, rotation);

            if (body != null)
            {
                body.position = position;
                body.rotation = rotation;
#if UNITY_6000_0_OR_NEWER
                body.linearVelocity = Vector3.zero;
#else
                body.velocity = Vector3.zero;
#endif
                body.angularVelocity = Vector3.zero;
            }

            currentHealth = maxHealth;
            dead = false;

            foreach (Renderer r in renderers)
                if (r != null) r.enabled = true;

            foreach (Collider c in colliders)
                if (c != null && !c.isTrigger) c.enabled = true;

            if (controller != null) controller.SetGameplayEnabled(true);
        }
    }
}
