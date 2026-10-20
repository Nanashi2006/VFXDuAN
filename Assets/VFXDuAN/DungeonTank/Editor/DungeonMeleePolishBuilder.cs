#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace VFXDuAN.DungeonTank.Editor
{
    public static class DungeonMeleePolishBuilder
    {
        private const string RootFolder = "Assets/VFXDuAN/DungeonTank";
        private const string GeneratedFolder = RootFolder + "/Generated";
        private const string ControllerFolder = GeneratedFolder + "/Controllers";
        private const string ScenePath = RootFolder + "/Scenes/DungeonTank_Showcase.unity";

        private const string CrusaderPrefab = "Assets/Crusader tank/prefab/Crusader tank_prefab.prefab";
        private const string CrusaderIdle = "Assets/Crusader tank/animation/crusadershield@battle poze.fbx";
        private const string CrusaderWalk = "Assets/Crusader tank/animation/crusadershield@walk.fbx";
        private const string CrusaderAttack = "Assets/Crusader tank/animation/crusadershield@atack1.fbx";
        private const string CrusaderHit = "Assets/Crusader tank/animation/crusadershield@gethit.fbx";
        private const string CrusaderDeath = "Assets/Crusader tank/animation/crusadershield@death.fbx";

        private const string SkeletonIdle = "Assets/DungeonCharacters/Skeletons_demo/animation/DS_onehand_idle_A.FBX";
        private const string SkeletonWalk = "Assets/DungeonCharacters/Skeletons_demo/animation/DS_onehand_walk.FBX";
        private const string SkeletonAttack = "Assets/DungeonCharacters/Skeletons_demo/animation/DS_onehand_attack_A.FBX";

        private const string FloorPrefab = "Assets/StylizedHandPaintedDungeon(Free)/Prefabs/Floor.prefab";
        private const string WallPrefab = "Assets/StylizedHandPaintedDungeon(Free)/Prefabs/Wall.prefab";
        private const string EntrancePrefab = "Assets/StylizedHandPaintedDungeon(Free)/Prefabs/Wall_Entrance.prefab";
        private const string DoorPrefab = "Assets/StylizedHandPaintedDungeon(Free)/Prefabs/Door.prefab";
        private const string PillarPrefab = "Assets/StylizedHandPaintedDungeon(Free)/Prefabs/Pillar.prefab";
        private const string Lamp1Prefab = "Assets/StylizedHandPaintedDungeon(Free)/Prefabs/Lamp_01.prefab";
        private const string Lamp2Prefab = "Assets/StylizedHandPaintedDungeon(Free)/Prefabs/Lamp_02.prefab";

        private const string CheckpointVFX = "Assets/Particlecollection_Free samples/Prefab/Magic_ring/magic_ring_03.prefab";
        private const string SlashVFX = "Assets/Particlecollection_Free samples/Prefab/Cleave/power_03.prefab";
        private const string HitVFX = "Assets/Particlecollection_Free samples/Prefab/Hit/Hit_03.prefab";
        private const string DeathVFX = "Assets/Particlecollection_Free samples/Prefab/Sprays/Sprays_04.prefab";

        [MenuItem("Tools/VFXDuAN/Build POLISHED Melee Dungeon Showcase", priority = 1)]
        public static void BuildPolished()
        {
            DungeonTankShowcaseBuilder.Build();
            PolishCurrentScene();
        }

        [MenuItem("Tools/VFXDuAN/Polish Current Dungeon Scene", priority = 2)]
        public static void PolishCurrentScene()
        {
            EnsureFolders();

            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid())
            {
                Debug.LogError("[VFXDuAN] No active scene to polish.");
                return;
            }

            GameObject player = GameObject.Find("PLAYER_CRUSADER_TANK__CONTROLLABLE");
            if (player == null)
                player = Object.FindFirstObjectByType<CrusaderTankController>()?.gameObject;

            if (player == null)
            {
                Debug.LogError("[VFXDuAN] Player was not found. Run Build Dungeon Tank Showcase first.");
                return;
            }

            AnimatorController crusaderController = BuildCrusaderAnimatorController();
            AnimatorController skeletonController = BuildSkeletonAnimatorController();

            PolishHierarchy();
            PolishPlayer(player, crusaderController);
            PolishEnemies(skeletonController);
            PolishCheckpoints();
            RebuildDungeonVisuals();
            PolishLighting();
            AddTeacherChecklist();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeGameObject = player;
            Debug.Log("[VFXDuAN] POLISHED MELEE DUNGEON COMPLETE. LMB/Space attack, E save checkpoint, R reset save.");
        }

        private static void PolishHierarchy()
        {
            Rename("02_PLAYER__CRUSADER_TANK", "02_PLAYER__CRUSADER_MELEE");
            Rename("06_VFX__PARTICLE_PACKAGE_INTEGRATION", "07_VFX__HIT_SLASH_CHECKPOINT_DEATH");
            Rename("07_LIGHTING_CAMERA__CINEMATIC", "08_LIGHTING_CAMERA__DUNGEON_MOOD");
            Rename("08_FINAL_PORTAL__BLACK_HOLE_VFX", "09_FINAL_GOAL__BLACK_HOLE_PORTAL");

            if (GameObject.Find("06_UI__HP_AND_GAMEPLAY") == null)
            {
                GameObject ui = new GameObject("06_UI__HP_AND_GAMEPLAY");
                Note(ui.transform, "PLAYER_HP__Runtime_bar_bottom_left");
                Note(ui.transform, "ENEMY_HP__Runtime_world_space_bars");
                Note(ui.transform, "CHECKPOINT_PROMPT__Press_E_to_Save");
            }
        }

        private static void PolishPlayer(GameObject player, RuntimeAnimatorController controllerAsset)
        {
            player.name = "PLAYER_CRUSADER__MELEE_CONTROLLABLE";
            player.transform.position = new Vector3(0f, 0.1f, -30f);

            BoxCollider oldBox = player.GetComponent<BoxCollider>();
            if (oldBox != null) Object.DestroyImmediate(oldBox);

            CapsuleCollider capsule = player.GetComponent<CapsuleCollider>();
            if (capsule == null) capsule = player.AddComponent<CapsuleCollider>();
            capsule.center = new Vector3(0f, 1.0f, 0f);
            capsule.height = 2.0f;
            capsule.radius = 0.45f;

            Rigidbody body = player.GetComponent<Rigidbody>();
            if (body != null)
            {
                body.mass = 75f;
                body.useGravity = true;
                body.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            }

            Transform attackPoint = FindChildRecursive(player.transform, "02_MUZZLE__PROJECTILE_AND_VFX_ORIGIN");
            if (attackPoint == null)
            {
                attackPoint = new GameObject("02_ATTACK_POINT__MELEE").transform;
                attackPoint.SetParent(player.transform, false);
            }
            attackPoint.name = "02_ATTACK_POINT__MELEE";
            attackPoint.localPosition = new Vector3(0f, 1.05f, 1.25f);
            attackPoint.localRotation = Quaternion.identity;

            Animator animator = player.GetComponentInChildren<Animator>(true);
            if (animator != null)
            {
                animator.runtimeAnimatorController = controllerAsset;
                animator.applyRootMotion = false;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            }

            CrusaderTankController controller = player.GetComponent<CrusaderTankController>();
            controller.animator = animator;
            controller.attackPoint = attackPoint;
            controller.moveSpeed = 4.2f;
            controller.turnSpeed = 125f;
            controller.attackRadius = 1.65f;
            controller.attackDamage = 25f;
            controller.attackCooldown = 0.72f;
            controller.damageDelay = 0.22f;
            controller.slashVFXPrefab = LoadPrefab(SlashVFX);
            controller.hitVFXPrefab = LoadPrefab(HitVFX);

            CrusaderTankHealth health = player.GetComponent<CrusaderTankHealth>();
            health.animator = animator;
            health.maxHealth = 100f;
            health.hitVFXPrefab = LoadPrefab(HitVFX);
            health.deathVFXPrefab = LoadPrefab(DeathVFX);
            health.showHealthHUD = true;

            Transform visualRoot = FindChildRecursive(player.transform, "Crusader_Tank__Imported");
            if (visualRoot != null)
                FitToHeightAndGround(visualRoot.gameObject, 2.15f, player.transform.position.y);

            Transform oldNotes = FindChildRecursive(player.transform, "CONTROLS__W_S_MOVE__A_D_TURN__LMB_OR_SPACE_FIRE");
            if (oldNotes != null) oldNotes.name = "CONTROLS__WS_MOVE__AD_TURN__LMB_OR_SPACE_MELEE_ATTACK";

            Note(player.transform, "MELEE__AttackPoint_1_65m__Damage_25__OneHitPerSwing");
            Note(player.transform, "ANIMATION__Idle_Walk_Attack_Hit_Death__CrusaderPackage");
        }

        private static void PolishEnemies(RuntimeAnimatorController skeletonController)
        {
            SkeletonEnemyAI[] enemies = Object.FindObjectsByType<SkeletonEnemyAI>(FindObjectsSortMode.None);
            GameObject hit = LoadPrefab(HitVFX);
            GameObject death = LoadPrefab(DeathVFX);

            for (int i = 0; i < enemies.Length; i++)
            {
                SkeletonEnemyAI ai = enemies[i];
                if (ai == null) continue;

                ai.gameObject.name = $"SKELETON_{i + 1:00}__CHASE_MELEE_NO_PUSH";
                ai.moveSpeed = 2.35f;
                ai.detectionRange = 13f;
                ai.attackRange = 1.75f;
                ai.stopDistance = 1.55f;
                ai.attackDamage = 14f;
                ai.attackCooldown = 1.25f;
                ai.damageDelay = 0.32f;

                Animator animator = ai.GetComponentInChildren<Animator>(true);
                if (animator != null)
                {
                    animator.runtimeAnimatorController = skeletonController;
                    animator.applyRootMotion = false;
                    ai.animator = animator;
                }

                SkeletonEnemyHealth health = ai.GetComponent<SkeletonEnemyHealth>();
                if (health != null)
                {
                    health.maxHealth = 60f;
                    health.animator = animator;
                    health.hitVFXPrefab = hit;
                    health.deathVFXPrefab = death;
                    health.showWorldHealthBar = true;
                    health.healthBarHeight = 2.2f;
                }

                Collider[] colliders = ai.GetComponentsInChildren<Collider>(true);
                foreach (Collider c in colliders)
                    if (c != null) c.isTrigger = true;
            }
        }

        private static void PolishCheckpoints()
        {
            DungeonCheckpoint[] checkpoints = Object.FindObjectsByType<DungeonCheckpoint>(FindObjectsSortMode.None);
            System.Array.Sort(checkpoints, (a, b) => a.checkpointIndex.CompareTo(b.checkpointIndex));
            GameObject checkpointFx = LoadPrefab(CheckpointVFX);
            Vector3[] positions =
            {
                new Vector3(0f, 0.25f, -7f),
                new Vector3(0f, 0.25f, 21f),
                new Vector3(0f, 0.25f, 47f)
            };

            for (int i = 0; i < checkpoints.Length; i++)
            {
                DungeonCheckpoint checkpoint = checkpoints[i];
                if (checkpoint == null) continue;

                int index = Mathf.Clamp(checkpoint.checkpointIndex, 1, 3);
                checkpoint.gameObject.name = $"CHECKPOINT_{index:00}__PRESS_E_TO_SAVE";
                checkpoint.saveVFXPrefab = checkpointFx;
                if (index <= positions.Length)
                    checkpoint.transform.position = positions[index - 1];

                Note(checkpoint.transform, "SAVE__Updates_PlayerPrefs_and_respawn_position");
            }
        }

        private static void RebuildDungeonVisuals()
        {
            GameObject map = GameObject.Find("03_DUNGEON_MAP__STYLIZED_PACKAGE");
            if (map == null) return;

            Transform collision = FindChildRecursive(map.transform, "01_GAMEPLAY_COLLISION_FOUNDATION");
            if (collision != null)
            {
                Transform floor = FindChildRecursive(collision, "Floor_Main");
                if (floor != null)
                {
                    floor.position = new Vector3(0f, -0.35f, 15f);
                    floor.localScale = new Vector3(24f, 0.7f, 92f);
                }

                Transform left = FindChildRecursive(collision, "Wall_Left");
                if (left != null)
                {
                    left.position = new Vector3(-12.25f, 2.5f, 15f);
                    left.localScale = new Vector3(0.7f, 5.5f, 92f);
                }

                Transform right = FindChildRecursive(collision, "Wall_Right");
                if (right != null)
                {
                    right.position = new Vector3(12.25f, 2.5f, 15f);
                    right.localScale = new Vector3(0.7f, 5.5f, 92f);
                }

                List<GameObject> oldGates = new List<GameObject>();
                foreach (Transform child in collision.GetComponentsInChildren<Transform>(true))
                {
                    if (child != collision && child.name.StartsWith("Gate_"))
                        oldGates.Add(child.gameObject);
                }
                foreach (GameObject gate in oldGates)
                    gate.SetActive(false);
            }

            Transform oldImported = FindChildRecursive(map.transform, "02_IMPORTED_DUNGEON_PACKAGE_VISUALS");
            if (oldImported != null) oldImported.gameObject.SetActive(false);

            Transform oldPolished = FindChildRecursive(map.transform, "00_POLISHED_DUNGEON_PACKAGE_VISUALS");
            if (oldPolished != null) Object.DestroyImmediate(oldPolished.gameObject);

            Transform polished = new GameObject("00_POLISHED_DUNGEON_PACKAGE_VISUALS").transform;
            polished.SetParent(map.transform, false);

            Transform architecture = Group(polished, "01_ARCHITECTURE__FLOOR_WALL_WIDE_GATES");
            Transform props = Group(polished, "02_PROPS__PILLARS_DOORS_LAMPS");
            Transform mood = Group(polished, "03_DUNGEON_MOOD__WARM_LIGHTS");

            BuildFloorTiles(architecture);
            BuildSideWalls(architecture);
            BuildWideGate(architecture, -14f, "ENTRY_GATE");
            BuildWideGate(architecture, 10f, "ROOM_GATE_A");
            BuildWideGate(architecture, 34f, "ROOM_GATE_B");
            BuildWideGate(architecture, 58f, "FINAL_GATE");
            BuildRoomProps(props, mood);

            Note(polished, "MAP_WIDTH__24m__DoorOpening_5_6m__CrusaderFriendly");
            Note(polished, "PACKAGE__StylizedHandPaintedDungeon_Free__ExactPrefabs");
        }

        private static void BuildFloorTiles(Transform parent)
        {
            for (int z = -28; z <= 58; z += 8)
            {
                for (int x = -8; x <= 8; x += 8)
                {
                    GameObject tile = SpawnPrefab(FloorPrefab, parent, $"Floor_{x}_{z}", new Vector3(x, 0f, z), Quaternion.identity);
                    if (tile != null) FitLargest(tile, 7.9f);
                }
            }
        }

        private static void BuildSideWalls(Transform parent)
        {
            for (int z = -26; z <= 56; z += 6)
            {
                GameObject left = SpawnPrefab(WallPrefab, parent, $"Wall_L_{z}", new Vector3(-11.75f, 0f, z), Quaternion.Euler(0f, 90f, 0f));
                GameObject right = SpawnPrefab(WallPrefab, parent, $"Wall_R_{z}", new Vector3(11.75f, 0f, z), Quaternion.Euler(0f, -90f, 0f));
                if (left != null) FitToHeightAndGround(left, 4.5f, 0f);
                if (right != null) FitToHeightAndGround(right, 4.5f, 0f);
            }
        }

        private static void BuildWideGate(Transform parent, float z, string label)
        {
            GameObject entrance = SpawnPrefab(EntrancePrefab, parent, $"{label}__PACKAGE_ARCH", new Vector3(0f, 0f, z), Quaternion.identity);
            if (entrance != null) FitLargest(entrance, 7.0f);

            GameObject door = SpawnPrefab(DoorPrefab, parent, $"{label}__DOOR_VISUAL", new Vector3(0f, 0f, z + 0.15f), Quaternion.identity);
            if (door != null)
            {
                FitToHeightAndGround(door, 3.8f, 0f);
                Collider[] doorColliders = door.GetComponentsInChildren<Collider>(true);
                foreach (Collider c in doorColliders) c.enabled = false;
            }

            Material collisionMat = CreateGeneratedMaterial("MAT_WideGateCollision", new Color(0.07f, 0.065f, 0.075f, 1f));
            Primitive($"{label}__Collision_Left", parent, new Vector3(-7.4f, 2.35f, z), new Vector3(9.2f, 4.7f, 0.55f), collisionMat);
            Primitive($"{label}__Collision_Right", parent, new Vector3(7.4f, 2.35f, z), new Vector3(9.2f, 4.7f, 0.55f), collisionMat);
            Primitive($"{label}__Collision_Top", parent, new Vector3(0f, 4.35f, z), new Vector3(5.6f, 0.7f, 0.55f), collisionMat);
        }

        private static void BuildRoomProps(Transform props, Transform mood)
        {
            float[] rooms = { -2f, 22f, 48f };
            for (int r = 0; r < rooms.Length; r++)
            {
                float z = rooms[r];
                for (int side = -1; side <= 1; side += 2)
                {
                    Vector3 p1 = new Vector3(side * 8.7f, 0f, z - 5.3f);
                    Vector3 p2 = new Vector3(side * 8.7f, 0f, z + 5.3f);

                    GameObject pillar1 = SpawnPrefab(PillarPrefab, props, $"Room{r + 1}_Pillar_{side}_A", p1, Quaternion.identity);
                    GameObject pillar2 = SpawnPrefab(PillarPrefab, props, $"Room{r + 1}_Pillar_{side}_B", p2, Quaternion.identity);
                    if (pillar1 != null) FitToHeightAndGround(pillar1, 3.8f, 0f);
                    if (pillar2 != null) FitToHeightAndGround(pillar2, 3.8f, 0f);

                    string lampPath = r % 2 == 0 ? Lamp1Prefab : Lamp2Prefab;
                    Vector3 lampPos = new Vector3(side * 10.8f, 1.7f, z);
                    GameObject lamp = SpawnPrefab(lampPath, props, $"Room{r + 1}_Lamp_{side}", lampPos, Quaternion.Euler(0f, side < 0 ? 90f : -90f, 0f));
                    if (lamp != null) FitLargest(lamp, 1.7f);

                    GameObject lightGo = new GameObject($"Room{r + 1}_TorchLight_{side}");
                    lightGo.transform.SetParent(mood, false);
                    lightGo.transform.position = lampPos + Vector3.up * 0.3f;
                    Light light = lightGo.AddComponent<Light>();
                    light.type = LightType.Point;
                    light.color = new Color(1f, 0.42f, 0.12f, 1f);
                    light.intensity = 5.2f;
                    light.range = 9f;
                    light.shadows = LightShadows.Soft;
                }
            }
        }

        private static void PolishLighting()
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.025f, 0.035f, 0.055f, 1f);
            RenderSettings.fogDensity = 0.0135f;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.075f, 0.085f, 0.12f, 1f);

            Light[] lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
            foreach (Light light in lights)
            {
                if (light.type == LightType.Directional)
                {
                    light.color = new Color(0.34f, 0.41f, 0.62f, 1f);
                    light.intensity = 0.38f;
                }
            }
        }

        private static void AddTeacherChecklist()
        {
            GameObject guide = GameObject.Find("00_TEACHER_GUIDE__OPEN_ME");
            if (guide == null) return;

            Note(guide.transform, "POLISHED_BUILD__MELEE_DUNGEON_VERSION");
            Note(guide.transform, "PLAYER__Crusader_exact_prefab__Melee__Animation__HP");
            Note(guide.transform, "ENEMY__Skeleton_exact_prefab__Chase__Melee__NoPush__WorldHP");
            Note(guide.transform, "MAP__24m_wide__5_6m_doors__Floor_Wall_Pillar_Door_Lamp_package_assets");
            Note(guide.transform, "CHECKPOINT__3_points__E_Save__Death_Respawn");
            Note(guide.transform, "VFX__Cleave_attack__Hit__MagicRing_checkpoint__Death_spray");
            Note(guide.transform, "MOOD__Warm_torches__Blue_ambient__Dungeon_fog");
        }

        private static AnimatorController BuildCrusaderAnimatorController()
        {
            string path = ControllerFolder + "/Crusader_Melee.controller";
            AssetDatabase.DeleteAsset(path);
            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(path);
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Hit", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Die", AnimatorControllerParameterType.Trigger);

            AnimatorStateMachine sm = controller.layers[0].stateMachine;
            AnimatorState idle = sm.AddState("Idle_Battle");
            AnimatorState walk = sm.AddState("Walk");
            AnimatorState attack = sm.AddState("Attack_Melee");
            AnimatorState hit = sm.AddState("GetHit");
            AnimatorState die = sm.AddState("Death");

            idle.motion = LoadClip(CrusaderIdle);
            walk.motion = LoadClip(CrusaderWalk);
            attack.motion = LoadClip(CrusaderAttack);
            hit.motion = LoadClip(CrusaderHit);
            die.motion = LoadClip(CrusaderDeath);
            sm.defaultState = idle;

            AddFloatTransition(idle, walk, "Speed", AnimatorConditionMode.Greater, 0.1f);
            AddFloatTransition(walk, idle, "Speed", AnimatorConditionMode.Less, 0.1f);
            AddTriggerTransition(sm, attack, "Attack");
            AddTriggerTransition(sm, hit, "Hit");
            AddTriggerTransition(sm, die, "Die");
            AddExitToIdle(attack, idle, 0.82f);
            AddExitToIdle(hit, idle, 0.82f);

            EditorUtility.SetDirty(controller);
            return controller;
        }

        private static AnimatorController BuildSkeletonAnimatorController()
        {
            string path = ControllerFolder + "/Skeleton_Melee.controller";
            AssetDatabase.DeleteAsset(path);
            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(path);
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);

            AnimatorStateMachine sm = controller.layers[0].stateMachine;
            AnimatorState idle = sm.AddState("Idle");
            AnimatorState walk = sm.AddState("Walk");
            AnimatorState attack = sm.AddState("Attack");
            idle.motion = LoadClip(SkeletonIdle);
            walk.motion = LoadClip(SkeletonWalk);
            attack.motion = LoadClip(SkeletonAttack);
            sm.defaultState = idle;

            AddFloatTransition(idle, walk, "Speed", AnimatorConditionMode.Greater, 0.1f);
            AddFloatTransition(walk, idle, "Speed", AnimatorConditionMode.Less, 0.1f);
            AddTriggerTransition(sm, attack, "Attack");
            AddExitToIdle(attack, idle, 0.88f);

            EditorUtility.SetDirty(controller);
            return controller;
        }

        private static void AddFloatTransition(AnimatorState from, AnimatorState to, string parameter, AnimatorConditionMode mode, float threshold)
        {
            AnimatorStateTransition transition = from.AddTransition(to);
            transition.hasExitTime = false;
            transition.duration = 0.12f;
            transition.AddCondition(mode, threshold, parameter);
        }

        private static void AddTriggerTransition(AnimatorStateMachine sm, AnimatorState to, string parameter)
        {
            AnimatorStateTransition transition = sm.AddAnyStateTransition(to);
            transition.hasExitTime = false;
            transition.duration = 0.06f;
            transition.canTransitionToSelf = false;
            transition.AddCondition(AnimatorConditionMode.If, 0f, parameter);
        }

        private static void AddExitToIdle(AnimatorState from, AnimatorState idle, float exitTime)
        {
            AnimatorStateTransition transition = from.AddTransition(idle);
            transition.hasExitTime = true;
            transition.exitTime = exitTime;
            transition.duration = 0.1f;
        }

        private static AnimationClip LoadClip(string path)
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (Object asset in assets)
            {
                if (asset is AnimationClip clip && !clip.legacy && !clip.name.StartsWith("__preview__"))
                    return clip;
            }
            Debug.LogWarning("[VFXDuAN] Animation clip not found: " + path);
            return null;
        }

        private static GameObject LoadPrefab(string path)
        {
            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        private static GameObject SpawnPrefab(string path, Transform parent, string name, Vector3 position, Quaternion rotation)
        {
            GameObject prefab = LoadPrefab(path);
            if (prefab == null)
            {
                Debug.LogWarning("[VFXDuAN] Prefab not found: " + path);
                return null;
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null) return null;
            instance.name = name;
            instance.transform.SetParent(parent, true);
            instance.transform.SetPositionAndRotation(position, rotation);
            return instance;
        }

        private static void FitLargest(GameObject go, float desiredLargest)
        {
            if (!TryGetBounds(go, out Bounds bounds)) return;
            float largest = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
            if (largest <= 0.0001f) return;
            go.transform.localScale *= desiredLargest / largest;
        }

        private static void FitToHeightAndGround(GameObject go, float desiredHeight, float groundY)
        {
            if (!TryGetBounds(go, out Bounds before) || before.size.y <= 0.0001f) return;
            go.transform.localScale *= desiredHeight / before.size.y;
            if (!TryGetBounds(go, out Bounds after)) return;
            go.transform.position += Vector3.up * (groundY - after.min.y);
        }

        private static bool TryGetBounds(GameObject go, out Bounds bounds)
        {
            Renderer[] renderers = go.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                bounds = default;
                return false;
            }

            bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);
            return true;
        }

        private static GameObject Primitive(string name, Transform parent, Vector3 position, Vector3 scale, Material material)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, true);
            go.transform.position = position;
            go.transform.localScale = scale;
            Renderer renderer = go.GetComponent<Renderer>();
            if (renderer != null && material != null) renderer.sharedMaterial = material;
            return go;
        }

        private static Material CreateGeneratedMaterial(string name, Color color)
        {
            string materialFolder = GeneratedFolder + "/Materials";
            EnsureFolder(GeneratedFolder, "Materials");
            string path = materialFolder + "/" + name + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null) shader = Shader.Find("Standard");
                material = new Material(shader) { name = name };
                AssetDatabase.CreateAsset(material, path);
            }
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Transform Group(Transform parent, string name)
        {
            Transform t = new GameObject(name).transform;
            t.SetParent(parent, false);
            return t;
        }

        private static void Note(Transform parent, string name)
        {
            if (FindChildRecursive(parent, name) != null) return;
            Transform note = new GameObject(name).transform;
            note.SetParent(parent, false);
        }

        private static Transform FindChildRecursive(Transform root, string name)
        {
            if (root == null) return null;
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
                if (t.name == name) return t;
            return null;
        }

        private static void Rename(string oldName, string newName)
        {
            GameObject go = GameObject.Find(oldName);
            if (go != null) go.name = newName;
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets", "VFXDuAN");
            EnsureFolder("Assets/VFXDuAN", "DungeonTank");
            EnsureFolder(RootFolder, "Generated");
            EnsureFolder(GeneratedFolder, "Controllers");
            EnsureFolder(GeneratedFolder, "Materials");
            EnsureFolder(RootFolder, "Scenes");
        }

        private static void EnsureFolder(string parent, string name)
        {
            string path = parent + "/" + name;
            if (!AssetDatabase.IsValidFolder(path))
                AssetDatabase.CreateFolder(parent, name);
        }
    }
}
#endif
