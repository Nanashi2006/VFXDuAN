using System.Collections;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace VFXDuAN.DungeonTank
{
    [DisallowMultipleComponent]
    public class DungeonGameManager : MonoBehaviour
    {
        public static DungeonGameManager Instance { get; private set; }

        private const string CheckpointKey = "VFXDuAN_Dungeon_Checkpoint";

        [Header("Player / Respawn")]
        public CrusaderTankHealth player;
        [Min(0.1f)] public float respawnDelay = 1.8f;

        [Header("Runtime status")]
        [SerializeField] private int activeCheckpointIndex;
        [SerializeField] private Vector3 respawnPosition;
        [SerializeField] private Quaternion respawnRotation = Quaternion.identity;

        public int ActiveCheckpointIndex => activeCheckpointIndex;
        public string StatusMessage { get; private set; } = "Reach a checkpoint and press E to save.";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            if (player == null)
                player = FindAnyObjectByType<CrusaderTankHealth>();

            if (player != null)
            {
                respawnPosition = player.transform.position;
                respawnRotation = player.transform.rotation;
            }

            LoadSavedCheckpoint();
        }

        private void Update()
        {
            if (ResetPressed())
                ResetCheckpointSave();
        }

        public void SaveCheckpoint(DungeonCheckpoint checkpoint)
        {
            if (checkpoint == null) return;

            activeCheckpointIndex = checkpoint.checkpointIndex;
            respawnPosition = checkpoint.RespawnPosition;
            respawnRotation = checkpoint.RespawnRotation;

            PlayerPrefs.SetInt(CheckpointKey, activeCheckpointIndex);
            PlayerPrefs.Save();

            DungeonCheckpoint[] all = FindObjectsByType<DungeonCheckpoint>(FindObjectsSortMode.None);
            foreach (DungeonCheckpoint cp in all)
                if (cp != null) cp.SetActivated(cp.checkpointIndex == activeCheckpointIndex);

            StatusMessage = $"CHECKPOINT {activeCheckpointIndex} SAVED • Respawn point updated";
            Debug.Log($"[DungeonTank] Checkpoint {activeCheckpointIndex} saved at {respawnPosition}.");
        }

        public void RequestRespawn(CrusaderTankHealth tank)
        {
            if (tank == null) return;
            StartCoroutine(RespawnRoutine(tank));
        }

        private IEnumerator RespawnRoutine(CrusaderTankHealth tank)
        {
            StatusMessage = "CRUSADER DEFEATED • Respawning at last checkpoint...";
            yield return new WaitForSeconds(respawnDelay);

            tank.ReviveAt(respawnPosition, respawnRotation);
            StatusMessage = activeCheckpointIndex > 0
                ? $"RESPAWNED AT CHECKPOINT {activeCheckpointIndex}"
                : "RESPAWNED AT START";
        }

        private void LoadSavedCheckpoint()
        {
            int saved = PlayerPrefs.GetInt(CheckpointKey, 0);
            if (saved <= 0) return;

            DungeonCheckpoint[] all = FindObjectsByType<DungeonCheckpoint>(FindObjectsSortMode.None);
            foreach (DungeonCheckpoint checkpoint in all)
            {
                if (checkpoint == null || checkpoint.checkpointIndex != saved) continue;

                activeCheckpointIndex = saved;
                respawnPosition = checkpoint.RespawnPosition;
                respawnRotation = checkpoint.RespawnRotation;
                checkpoint.SetActivated(true);
                StatusMessage = $"SAVE LOADED • CHECKPOINT {saved}";
                return;
            }
        }

        public void ResetCheckpointSave()
        {
            PlayerPrefs.DeleteKey(CheckpointKey);
            PlayerPrefs.Save();
            activeCheckpointIndex = 0;

            if (player != null)
            {
                respawnPosition = player.InitialSpawnPosition;
                respawnRotation = player.InitialSpawnRotation;
            }

            DungeonCheckpoint[] all = FindObjectsByType<DungeonCheckpoint>(FindObjectsSortMode.None);
            foreach (DungeonCheckpoint cp in all)
                if (cp != null) cp.SetActivated(false);

            StatusMessage = "CHECKPOINT SAVE RESET • Start point restored";
            Debug.Log("[DungeonTank] Checkpoint save reset.");
        }

        private void OnGUI()
        {
            GUIStyle panel = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.UpperLeft,
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                padding = new RectOffset(14, 14, 12, 12)
            };

            string checkpoint = activeCheckpointIndex > 0 ? $"Respawn: CHECKPOINT {activeCheckpointIndex}" : "Respawn: START";
            string controls = "W/S Move • A/D Turn • LMB/Space Attack • E Save • R Reset • Hold RMB Orbit Camera • Mouse Wheel Zoom";

            GUI.Box(new Rect(16f, 16f, 850f, 88f),
                $"DUNGEON CRUSADER • THIRD-PERSON MELEE VFX SHOWCASE\n{checkpoint}   |   {StatusMessage}\n{controls}", panel);
        }

        private static bool ResetPressed()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.R);
#endif
        }
    }
}
