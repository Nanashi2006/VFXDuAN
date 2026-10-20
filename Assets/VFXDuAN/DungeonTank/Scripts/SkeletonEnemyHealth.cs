using UnityEngine;

namespace VFXDuAN.DungeonTank
{
    [DisallowMultipleComponent]
    public class SkeletonEnemyHealth : MonoBehaviour
    {
        [Header("Health")]
        [Min(1f)] public float maxHealth = 60f;
        public Animator animator;

        [Header("VFX")]
        public GameObject hitVFXPrefab;
        public GameObject deathVFXPrefab;

        [Header("World HP Bar")]
        public bool showWorldHealthBar = true;
        [Min(0.5f)] public float healthBarHeight = 2.15f;

        private float currentHealth;
        private bool dead;
        private SkeletonEnemyAI ai;
        private Collider[] colliders;

        private Transform healthBarRoot;
        private Transform healthFill;
        private Material healthFillMaterial;
        private const float HealthBarWidth = 1.55f;

        public bool IsDead => dead;
        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;

        private void Awake()
        {
            currentHealth = maxHealth;
            ai = GetComponent<SkeletonEnemyAI>();
            colliders = GetComponentsInChildren<Collider>(true);
            if (animator == null) animator = GetComponentInChildren<Animator>();

            if (showWorldHealthBar)
                CreateWorldHealthBar();
        }

        private void LateUpdate()
        {
            if (healthBarRoot == null) return;

            Camera cam = Camera.main;
            if (cam != null)
                healthBarRoot.rotation = cam.transform.rotation;
        }

        public void TakeDamage(float amount)
        {
            if (dead || amount <= 0f) return;

            currentHealth = Mathf.Max(0f, currentHealth - amount);
            UpdateHealthBar();

            if (hitVFXPrefab != null)
            {
                GameObject fx = Instantiate(hitVFXPrefab, transform.position + Vector3.up * 1.05f, Quaternion.identity);
                fx.name = "VFX_Skeleton_Hit__Runtime";
                Destroy(fx, 5f);
            }

            TryTrigger("Hit");

            if (currentHealth <= 0f)
                Die();
        }

        private void Die()
        {
            if (dead) return;
            dead = true;

            if (ai != null) ai.enabled = false;

            foreach (Collider c in colliders)
                if (c != null) c.enabled = false;

            TryTrigger("Die");
            TryTrigger("Death");

            if (healthBarRoot != null)
                healthBarRoot.gameObject.SetActive(false);

            if (deathVFXPrefab != null)
            {
                GameObject fx = Instantiate(deathVFXPrefab, transform.position + Vector3.up * 0.75f, Quaternion.identity);
                fx.name = "VFX_Skeleton_Death__Runtime";
                Destroy(fx, 7f);
            }

            Destroy(gameObject, 2.4f);
        }

        private void CreateWorldHealthBar()
        {
            GameObject root = new GameObject("UI__ENEMY_HP_BAR");
            healthBarRoot = root.transform;
            healthBarRoot.SetParent(transform, false);
            healthBarRoot.localPosition = new Vector3(0f, healthBarHeight, 0f);

            GameObject bg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bg.name = "HP_Background";
            bg.transform.SetParent(healthBarRoot, false);
            bg.transform.localScale = new Vector3(HealthBarWidth + 0.12f, 0.16f, 0.045f);
            RemoveCollider(bg);
            Renderer bgRenderer = bg.GetComponent<Renderer>();
            if (bgRenderer != null)
                bgRenderer.sharedMaterial = CreateColorMaterial(new Color(0.035f, 0.035f, 0.04f, 1f), "EnemyHP_Background");

            GameObject fill = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fill.name = "HP_Fill";
            healthFill = fill.transform;
            healthFill.SetParent(healthBarRoot, false);
            healthFill.localPosition = new Vector3(0f, 0f, -0.055f);
            healthFill.localScale = new Vector3(HealthBarWidth, 0.105f, 0.035f);
            RemoveCollider(fill);

            Renderer fillRenderer = fill.GetComponent<Renderer>();
            if (fillRenderer != null)
            {
                healthFillMaterial = CreateColorMaterial(new Color(0.22f, 0.95f, 0.24f, 1f), "EnemyHP_Fill");
                fillRenderer.sharedMaterial = healthFillMaterial;
            }

            UpdateHealthBar();
        }

        private void UpdateHealthBar()
        {
            if (healthFill == null) return;

            float ratio = maxHealth > 0f ? Mathf.Clamp01(currentHealth / maxHealth) : 0f;
            float width = HealthBarWidth * ratio;
            healthFill.localScale = new Vector3(Mathf.Max(0.001f, width), 0.105f, 0.035f);
            healthFill.localPosition = new Vector3(-(HealthBarWidth - width) * 0.5f, 0f, -0.055f);

            if (healthFillMaterial != null)
            {
                Color color = ratio > 0.5f
                    ? new Color(0.18f, 0.95f, 0.26f, 1f)
                    : ratio > 0.25f
                        ? new Color(1f, 0.55f, 0.05f, 1f)
                        : new Color(0.95f, 0.08f, 0.08f, 1f);
                SetMaterialColor(healthFillMaterial, color);
            }
        }

        private static void RemoveCollider(GameObject go)
        {
            Collider col = go.GetComponent<Collider>();
            if (col != null) Destroy(col);
        }

        private static Material CreateColorMaterial(Color color, string materialName)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) return null;

            Material mat = new Material(shader) { name = materialName };
            SetMaterialColor(mat, color);
            return mat;
        }

        private static void SetMaterialColor(Material material, Color color)
        {
            if (material == null) return;
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
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
