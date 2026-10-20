using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace VFXDuAN.DungeonTank
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public class DungeonCheckpoint : MonoBehaviour
    {
        [Min(0)] public int checkpointIndex = 1;
        public Transform respawnPoint;
        public GameObject saveVFXPrefab;
        public Light checkpointLight;
        public Renderer[] glowRenderers;

        private bool playerInside;
        private CrusaderTankHealth currentPlayer;
        private bool activated;

        public Vector3 RespawnPosition => respawnPoint != null ? respawnPoint.position : transform.position + Vector3.up * 0.5f;
        public Quaternion RespawnRotation => respawnPoint != null ? respawnPoint.rotation : transform.rotation;

        private void Reset()
        {
            Collider trigger = GetComponent<Collider>();
            trigger.isTrigger = true;
        }

        private void Update()
        {
            if (!playerInside || currentPlayer == null || !InteractPressed()) return;

            DungeonGameManager manager = DungeonGameManager.Instance;
            if (manager != null)
            {
                manager.SaveCheckpoint(this);
                SetActivated(true);
                PlaySaveVFX();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            CrusaderTankHealth player = other.GetComponentInParent<CrusaderTankHealth>();
            if (player == null) return;

            currentPlayer = player;
            playerInside = true;
        }

        private void OnTriggerExit(Collider other)
        {
            CrusaderTankHealth player = other.GetComponentInParent<CrusaderTankHealth>();
            if (player == null || player != currentPlayer) return;

            currentPlayer = null;
            playerInside = false;
        }

        private void OnGUI()
        {
            if (!playerInside || currentPlayer == null) return;

            GUIStyle style = new GUIStyle(GUI.skin.box)
            {
                fontSize = 18,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };

            float width = 390f;
            float height = 54f;
            Rect rect = new Rect((Screen.width - width) * 0.5f, Screen.height - 120f, width, height);
            GUI.Box(rect, $"CHECKPOINT {checkpointIndex}  •  PRESS E TO SAVE", style);
        }

        public void SetActivated(bool value)
        {
            activated = value;

            if (checkpointLight != null)
            {
                checkpointLight.intensity = activated ? 5.5f : 2.2f;
                checkpointLight.range = activated ? 9f : 6f;
            }

            if (glowRenderers == null) return;
            foreach (Renderer r in glowRenderers)
            {
                if (r == null || r.material == null) continue;
                Color c = activated ? new Color(0.15f, 1.0f, 0.45f, 1f) : new Color(0.15f, 0.45f, 1.0f, 1f);
                if (r.material.HasProperty("_BaseColor")) r.material.SetColor("_BaseColor", c);
                if (r.material.HasProperty("_Color")) r.material.SetColor("_Color", c);
                if (r.material.HasProperty("_EmissionColor"))
                {
                    r.material.EnableKeyword("_EMISSION");
                    r.material.SetColor("_EmissionColor", c * 3f);
                }
            }
        }

        private void PlaySaveVFX()
        {
            if (saveVFXPrefab == null) return;
            GameObject fx = Instantiate(saveVFXPrefab, transform.position + Vector3.up * 0.75f, Quaternion.identity);
            fx.name = $"VFX_Checkpoint_{checkpointIndex}_Saved";
            Destroy(fx, 6f);
        }

        private static bool InteractPressed()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.E);
#endif
        }
    }
}
