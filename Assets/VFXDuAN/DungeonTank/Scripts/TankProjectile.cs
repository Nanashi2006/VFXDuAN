using UnityEngine;

namespace VFXDuAN.DungeonTank
{
    [DisallowMultipleComponent]
    public class TankProjectile : MonoBehaviour
    {
        private float speed;
        private float damage;
        private float life = 4f;
        private GameObject impactVFXPrefab;
        private GameObject owner;

        public static TankProjectile Create(
            Vector3 position,
            Quaternion rotation,
            float projectileSpeed,
            float projectileDamage,
            GameObject impactVFX,
            GameObject projectileOwner)
        {
            GameObject shell = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            shell.transform.SetPositionAndRotation(position, rotation);
            shell.transform.localScale = new Vector3(0.18f, 0.18f, 0.42f);

            SphereCollider collider = shell.GetComponent<SphereCollider>();
            collider.isTrigger = true;

            Rigidbody rb = shell.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = true;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

            Renderer renderer = shell.GetComponent<Renderer>();
            if (renderer != null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null) shader = Shader.Find("Standard");
                if (shader != null)
                {
                    Material material = new Material(shader);
                    Color glow = new Color(1f, 0.36f, 0.04f, 1f);
                    if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", glow);
                    if (material.HasProperty("_Color")) material.SetColor("_Color", glow);
                    if (material.HasProperty("_EmissionColor"))
                    {
                        material.EnableKeyword("_EMISSION");
                        material.SetColor("_EmissionColor", glow * 4f);
                    }
                    renderer.material = material;
                }
            }

            TankProjectile projectile = shell.AddComponent<TankProjectile>();
            projectile.speed = projectileSpeed;
            projectile.damage = projectileDamage;
            projectile.impactVFXPrefab = impactVFX;
            projectile.owner = projectileOwner;
            return projectile;
        }

        private void Update()
        {
            transform.position += transform.forward * (speed * Time.deltaTime);
            life -= Time.deltaTime;
            if (life <= 0f) Destroy(gameObject);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other == null) return;
            if (owner != null && other.transform.IsChildOf(owner.transform)) return;

            SkeletonEnemyHealth enemy = other.GetComponentInParent<SkeletonEnemyHealth>();
            if (enemy != null)
                enemy.TakeDamage(damage);

            if (impactVFXPrefab != null)
            {
                GameObject fx = Instantiate(impactVFXPrefab, transform.position, Quaternion.identity);
                fx.name = "VFX_Projectile_Impact__Runtime";
                Destroy(fx, 6f);
            }

            Destroy(gameObject);
        }
    }
}
