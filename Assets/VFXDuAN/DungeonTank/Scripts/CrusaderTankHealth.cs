using UnityEngine;

namespace VFXDuAN.DungeonTank
{
    [DisallowMultipleComponent]
    public class CrusaderTankHealth : MonoBehaviour
    {
        [Header("Health")]
        [Min(1f)] public float maxHealth = 100f;
        public Animator animator;

        [Header("VFX")]
        public GameObject hitVFXPrefab;
        public GameObject deathVFXPrefab;

        [Header("HUD")]
        public bool showHealthHUD = true;

        private float currentHealth;
        private bool dead;
        private CrusaderTankController controller;
        private Rigidbody body;
        private Collider[] colliders;
        private Renderer[] renderers;

        private static readonly int HitHash = Animator.StringToHash("Hit");
        private static readonly int DieHash = Animator.StringToHash("Die");

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
            if (animator == null) animator = GetComponentInChildren<Animator>();

            InitialSpawnPosition = transform.position;
            InitialSpawnRotation = transform.rotation;
        }

        public void TakeDamage(float amount)
        {
            if (dead || amount <= 0f) return;

            currentHealth = Mathf.Max(0f, currentHealth - amount);

            if (currentHealth > 0f)
            {
                TryTrigger(HitHash, "Hit");
                SpawnVFX(hitVFXPrefab, transform.position + Vector3.up * 1.1f, "VFX_Crusader_Hit__Runtime", 5f);
            }
            else
            {
                Die();
            }
        }

        public void Heal(float amount)
        {
            if (dead || amount <= 0f) return;
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        }

        public void HealFull()
        {
            if (!dead) currentHealth = maxHealth;
        }

        private void Die()
        {
            if (dead) return;
            dead = true;
            currentHealth = 0f;

            if (controller != null)
                controller.SetGameplayEnabled(false);

            TryTrigger(DieHash, "Die");
            SpawnVFX(deathVFXPrefab, transform.position + Vector3.up * 0.8f, "VFX_Crusader_Death__Runtime", 7f);

            foreach (Collider c in colliders)
            {
                if (c != null && !c.isTrigger)
                    c.enabled = false;
            }

            if (body != null)
            {
#if UNITY_6000_0_OR_NEWER
                body.linearVelocity = Vector3.zero;
#else
                body.velocity = Vector3.zero;
#endif
                body.angularVelocity = Vector3.zero;
            }

            DungeonGameManager manager = DungeonGameManager.Instance;
            if (manager != null)
                manager.RequestRespawn(this);
            else
                Invoke(nameof(RespawnAtInitialPoint), 1.5f);
        }

        private void RespawnAtInitialPoint()
        {
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

            if (animator != null)
            {
                animator.Rebind();
                animator.Update(0f);
            }

            if (controller != null)
                controller.SetGameplayEnabled(true);
        }

        private void SpawnVFX(GameObject prefab, Vector3 position, string runtimeName, float lifetime)
        {
            if (prefab == null) return;
            GameObject fx = Instantiate(prefab, position, Quaternion.identity);
            fx.name = runtimeName;
            Destroy(fx, lifetime);
        }

        private void TryTrigger(int hash, string parameterName)
        {
            if (animator == null) return;
            foreach (AnimatorControllerParameter parameter in animator.parameters)
            {
                if (parameter.name == parameterName && parameter.type == AnimatorControllerParameterType.Trigger)
                {
                    animator.SetTrigger(hash);
                    return;
                }
            }
        }

        private void OnGUI()
        {
            if (!showHealthHUD) return;

            const float x = 20f;
            const float y = 132f;
            const float width = 320f;
            const float height = 24f;

            float ratio = maxHealth > 0f ? Mathf.Clamp01(currentHealth / maxHealth) : 0f;

            GUIStyle title = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
            GUI.Label(new Rect(x, y - 24f, width, 22f), $"CRUSADER HP  {Mathf.CeilToInt(currentHealth)} / {Mathf.CeilToInt(maxHealth)}", title);

            Color previous = GUI.color;
            GUI.color = new Color(0.08f, 0.08f, 0.08f, 0.92f);
            GUI.DrawTexture(new Rect(x, y, width, height), Texture2D.whiteTexture);

            GUI.color = ratio > 0.55f
                ? new Color(0.18f, 0.85f, 0.28f, 1f)
                : ratio > 0.25f
                    ? new Color(1f, 0.58f, 0.08f, 1f)
                    : new Color(0.92f, 0.12f, 0.12f, 1f);
            GUI.DrawTexture(new Rect(x + 3f, y + 3f, (width - 6f) * ratio, height - 6f), Texture2D.whiteTexture);
            GUI.color = previous;
        }
    }
}
