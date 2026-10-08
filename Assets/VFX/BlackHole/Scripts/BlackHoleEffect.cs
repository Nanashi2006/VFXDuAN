using UnityEngine;

namespace VFXDuAN.BlackHole
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class BlackHoleEffect : MonoBehaviour
    {
        [Header("Build")]
        [SerializeField] private bool autoBuild = true;

        [Header("Core")]
        [Min(0.1f)] public float coreRadius = 1.15f;
        [Min(0.1f)] public float lensRadius = 1.75f;

        [Header("Accretion Disk")]
        [Min(0.5f)] public float diskRadius = 4.6f;
        [Range(-45f, 45f)] public float diskTilt = 12f;
        public float diskSpinSpeed = 18f;

        [Header("Particles")]
        [Range(0, 500)] public int particleCount = 180;
        public float particleSpinSpeed = -26f;

        [Header("Lens")]
        [Range(0f, 0.2f)] public float distortion = 0.055f;

        private const string GeneratedRootName = "__BlackHoleGenerated";
        private Transform generatedRoot;
        private Transform diskRoot;
        private Transform particleRoot;
        private Transform lensQuad;

        private Material coreMaterial;
        private Material diskMaterial;
        private Material lensMaterial;
        private Material particleMaterial;

        private void OnEnable()
        {
            if (autoBuild && transform.Find(GeneratedRootName) == null)
                Build();
            else
                CacheGeneratedChildren();
        }

        private void Update()
        {
            if (diskRoot != null)
                diskRoot.Rotate(Vector3.up, diskSpinSpeed * Time.deltaTime, Space.Self);

            if (particleRoot != null)
                particleRoot.Rotate(Vector3.up, particleSpinSpeed * Time.deltaTime, Space.Self);

            if (lensQuad != null)
            {
                Camera cam = Camera.main;
                if (cam != null)
                {
                    Vector3 forward = cam.transform.position - lensQuad.position;
                    if (forward.sqrMagnitude > 0.0001f)
                        lensQuad.rotation = Quaternion.LookRotation(-forward.normalized, cam.transform.up);
                }
            }

            if (lensMaterial != null)
                lensMaterial.SetFloat("_Distortion", distortion);
        }

        [ContextMenu("Build Black Hole")]
        public void Build()
        {
            ClearGenerated();

            GameObject root = new GameObject(GeneratedRootName);
            generatedRoot = root.transform;
            generatedRoot.SetParent(transform, false);

            CreateMaterials();
            CreateCore();
            CreateAccretionDisk();
            CreateLens();
            CreateParticles();
        }

        [ContextMenu("Clear Generated Black Hole")]
        public void ClearGenerated()
        {
            Transform old = transform.Find(GeneratedRootName);
            if (old != null)
            {
                if (Application.isPlaying)
                    Destroy(old.gameObject);
                else
                    DestroyImmediate(old.gameObject);
            }

            DestroyRuntimeMaterial(ref coreMaterial);
            DestroyRuntimeMaterial(ref diskMaterial);
            DestroyRuntimeMaterial(ref lensMaterial);
            DestroyRuntimeMaterial(ref particleMaterial);

            generatedRoot = null;
            diskRoot = null;
            particleRoot = null;
            lensQuad = null;
        }

        private void CacheGeneratedChildren()
        {
            generatedRoot = transform.Find(GeneratedRootName);
            if (generatedRoot == null) return;

            diskRoot = generatedRoot.Find("AccretionDisk");
            particleRoot = generatedRoot.Find("OrbitingParticles");
            lensQuad = generatedRoot.Find("Lens");
        }

        private void CreateMaterials()
        {
            coreMaterial = CreateMaterial("VFXDuAN/BlackHole/Core", "BH_Core_Mat");
            diskMaterial = CreateMaterial("VFXDuAN/BlackHole/Disk", "BH_Disk_Mat");
            lensMaterial = CreateMaterial("VFXDuAN/BlackHole/Lens", "BH_Lens_Mat");
            particleMaterial = CreateMaterial("VFXDuAN/BlackHole/Particle", "BH_Particle_Mat");

            if (coreMaterial != null)
            {
                coreMaterial.SetColor("_CoreColor", Color.black);
                coreMaterial.SetColor("_EdgeColor", new Color(0.28f, 0.03f, 1.8f, 1f));
                coreMaterial.SetFloat("_FresnelPower", 3.2f);
                coreMaterial.SetFloat("_EdgeIntensity", 2.8f);
            }

            if (diskMaterial != null)
            {
                diskMaterial.SetColor("_InnerColor", new Color(4.5f, 0.75f, 0.08f, 1f));
                diskMaterial.SetColor("_OuterColor", new Color(0.22f, 0.015f, 2.8f, 1f));
                diskMaterial.SetFloat("_InnerRadius", 0.26f);
                diskMaterial.SetFloat("_OuterRadius", 0.98f);
                diskMaterial.SetFloat("_SpiralArms", 8f);
                diskMaterial.SetFloat("_Twist", 26f);
                diskMaterial.SetFloat("_Speed", 0.8f);
                diskMaterial.SetFloat("_Sharpness", 3.4f);
                diskMaterial.SetFloat("_Intensity", 2.8f);
            }

            if (lensMaterial != null)
            {
                lensMaterial.SetFloat("_Distortion", distortion);
                lensMaterial.SetFloat("_LensRadius", 0.46f);
                lensMaterial.SetFloat("_Falloff", 3.2f);
                lensMaterial.SetFloat("_Darkness", 0.6f);
            }

            if (particleMaterial != null)
            {
                particleMaterial.SetColor("_Tint", new Color(2.2f, 0.25f, 0.045f, 1f));
                particleMaterial.SetFloat("_Softness", 2.4f);
                particleMaterial.SetFloat("_Intensity", 2.4f);
            }
        }

        private void CreateCore()
        {
            GameObject core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            core.name = "EventHorizon";
            core.transform.SetParent(generatedRoot, false);
            core.transform.localScale = Vector3.one * (coreRadius * 2f);

            Collider c = core.GetComponent<Collider>();
            if (c != null)
            {
                if (Application.isPlaying) Destroy(c);
                else DestroyImmediate(c);
            }

            Renderer renderer = core.GetComponent<Renderer>();
            if (renderer != null && coreMaterial != null)
                renderer.sharedMaterial = coreMaterial;
        }

        private void CreateAccretionDisk()
        {
            GameObject holder = new GameObject("AccretionDisk");
            diskRoot = holder.transform;
            diskRoot.SetParent(generatedRoot, false);
            diskRoot.localRotation = Quaternion.Euler(diskTilt, 0f, 0f);

            CreateDiskLayer("Disk_Main", diskRadius, 0f, 0f);
            CreateDiskLayer("Disk_InnerGlow", diskRadius * 0.86f, 0.02f, 33f);
            CreateDiskLayer("Disk_OuterWisps", diskRadius * 1.08f, -0.02f, -19f);
        }

        private void CreateDiskLayer(string objectName, float radius, float yOffset, float yRotation)
        {
            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = objectName;
            quad.transform.SetParent(diskRoot, false);
            quad.transform.localPosition = new Vector3(0f, yOffset, 0f);
            quad.transform.localRotation = Quaternion.Euler(90f, yRotation, 0f);
            quad.transform.localScale = Vector3.one * (radius * 2f);

            Collider c = quad.GetComponent<Collider>();
            if (c != null)
            {
                if (Application.isPlaying) Destroy(c);
                else DestroyImmediate(c);
            }

            Renderer renderer = quad.GetComponent<Renderer>();
            if (renderer != null && diskMaterial != null)
                renderer.sharedMaterial = diskMaterial;
        }

        private void CreateLens()
        {
            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = "Lens";
            lensQuad = quad.transform;
            lensQuad.SetParent(generatedRoot, false);
            lensQuad.localScale = Vector3.one * (lensRadius * 2f);

            Collider c = quad.GetComponent<Collider>();
            if (c != null)
            {
                if (Application.isPlaying) Destroy(c);
                else DestroyImmediate(c);
            }

            Renderer renderer = quad.GetComponent<Renderer>();
            if (renderer != null && lensMaterial != null)
                renderer.sharedMaterial = lensMaterial;
        }

        private void CreateParticles()
        {
            GameObject holder = new GameObject("OrbitingParticles");
            particleRoot = holder.transform;
            particleRoot.SetParent(generatedRoot, false);
            particleRoot.localRotation = Quaternion.Euler(diskTilt, 0f, 0f);

            ParticleSystem ps = holder.AddComponent<ParticleSystem>();
            ParticleSystemRenderer psRenderer = holder.GetComponent<ParticleSystemRenderer>();
            if (particleMaterial != null)
                psRenderer.sharedMaterial = particleMaterial;
            psRenderer.renderMode = ParticleSystemRenderMode.Billboard;

            var main = ps.main;
            main.loop = true;
            main.playOnAwake = true;
            main.maxParticles = Mathf.Max(64, particleCount * 2);
            main.startLifetime = new ParticleSystem.MinMaxCurve(2.8f, 5.5f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.02f, 0.12f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.025f, 0.11f);
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.startColor = new ParticleSystem.MinMaxGradient(
                new Color(1f, 0.18f, 0.03f, 0.8f),
                new Color(0.45f, 0.04f, 1f, 0.65f));

            var emission = ps.emission;
            emission.rateOverTime = Mathf.Max(1f, particleCount / 4f);

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = diskRadius * 0.92f;
            shape.radiusThickness = 0.58f;
            shape.rotation = new Vector3(90f, 0f, 0f);

            var noise = ps.noise;
            noise.enabled = true;
            noise.strength = new ParticleSystem.MinMaxCurve(0.05f, 0.18f);
            noise.frequency = 0.45f;
            noise.scrollSpeed = 0.2f;
            noise.damping = true;

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(new Color(1f, 0.4f, 0.05f), 0f),
                    new GradientColorKey(new Color(0.55f, 0.05f, 1f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(1f, 0.18f),
                    new GradientAlphaKey(0f, 1f)
                });
            colorOverLifetime.color = gradient;

            ps.Play();
        }

        private static Material CreateMaterial(string shaderName, string materialName)
        {
            Shader shader = Shader.Find(shaderName);
            if (shader == null)
            {
                Debug.LogWarning($"Black Hole shader not found: {shaderName}");
                return null;
            }

            Material material = new Material(shader)
            {
                name = materialName,
                hideFlags = HideFlags.HideAndDontSave
            };
            return material;
        }

        private static void DestroyRuntimeMaterial(ref Material material)
        {
            if (material == null) return;

            if (Application.isPlaying)
                Destroy(material);
            else
                DestroyImmediate(material);

            material = null;
        }
    }
}
