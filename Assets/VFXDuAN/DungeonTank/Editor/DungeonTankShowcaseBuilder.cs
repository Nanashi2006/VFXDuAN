#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using VFXDuAN.BlackHole;

namespace VFXDuAN.DungeonTank.Editor
{
    public static class DungeonTankShowcaseBuilder
    {
        private const string RootFolder = "Assets/VFXDuAN/DungeonTank";
        private const string SceneFolder = RootFolder + "/Scenes";
        private const string GeneratedFolder = RootFolder + "/Generated";
        private const string MaterialFolder = GeneratedFolder + "/Materials";
        private const string ScenePath = SceneFolder + "/DungeonTank_Showcase.unity";

        private const string DungeonPackage = "Assets/StylizedHandPaintedDungeon(Free)";
        private const string TankPackage = "Assets/Crusader tank";
        private const string CharacterPackage = "Assets/DungeonCharacters";
        private const string ParticlePackage = "Assets/Particlecollection_Free samples";

        [MenuItem("Tools/VFXDuAN/Build Dungeon Tank Showcase")]
        public static void Build()
        {
            EnsureFolders();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "DungeonTank_Showcase";

            GameObject teacher = Group("00_TEACHER_GUIDE__OPEN_ME");
            GameObject systems = Group("01_SYSTEMS__SAVE_RESPAWN_GAME_MANAGER");
            GameObject playerGroup = Group("02_PLAYER__CRUSADER_TANK");
            GameObject mapGroup = Group("03_DUNGEON_MAP__STYLIZED_PACKAGE");
            GameObject enemyGroup = Group("04_ENEMIES__SKELETON_DUNGEON");
            GameObject checkpointGroup = Group("05_CHECKPOINTS__3_SAVE_RESPAWN_POINTS");
            GameObject vfxGroup = Group("06_VFX__PARTICLE_PACKAGE_INTEGRATION");
            GameObject cameraGroup = Group("07_LIGHTING_CAMERA__CINEMATIC");
            GameObject portalGroup = Group("08_FINAL_PORTAL__BLACK_HOLE_VFX");

            Material floorMat = GetOrCreateMaterial("MAT_DungeonFloor", new Color(0.055f, 0.065f, 0.09f, 1f), false);
            Material wallMat = GetOrCreateMaterial("MAT_DungeonWall", new Color(0.11f, 0.10f, 0.13f, 1f), false);
            Material trimMat = GetOrCreateMaterial("MAT_DungeonTrim", new Color(0.24f, 0.16f, 0.08f, 1f), false);
            Material checkpointMat = GetOrCreateMaterial("MAT_CheckpointGlow", new Color(0.08f, 0.42f, 1.0f, 1f), true);
            Material enemyPadMat = GetOrCreateMaterial("MAT_EnemyPad", new Color(0.45f, 0.035f, 0.035f, 1f), true);

            GameObject dungeonFloorPrefab = FindBestGameObject(DungeonPackage, "floor", "tile", "ground", "stone");
            GameObject dungeonWallPrefab = FindBestGameObject(DungeonPackage, "wall", "arch", "gate");
            GameObject dungeonPropPrefab = FindBestGameObject(DungeonPackage, "torch", "pillar", "column", "brazier", "crate");
            GameObject tankPrefab = FindBestGameObject(TankPackage, "crusader", "tank");
            GameObject skeletonPrefab = FindBestGameObject(CharacterPackage, "skeleton", "skel", "undead");

            GameObject checkpointVFX = FindBestGameObject(ParticlePackage, "magic", "portal", "aura", "spark", "energy");
            GameObject muzzleVFX = FindBestGameObject(ParticlePackage, "muzzle", "flash", "fire", "spark");
            GameObject impactVFX = FindBestGameObject(ParticlePackage, "impact", "hit", "burst", "explosion");
            GameObject deathVFX = FindBestGameObject(ParticlePackage, "explosion", "smoke", "fire", "burst");

            BuildTeacherGuide(teacher.transform, dungeonFloorPrefab, dungeonWallPrefab, dungeonPropPrefab, tankPrefab, skeletonPrefab, checkpointVFX, muzzleVFX, impactVFX);
            BuildDungeon(mapGroup.transform, floorMat, wallMat, trimMat, dungeonFloorPrefab, dungeonWallPrefab, dungeonPropPrefab);

            GameObject player = BuildPlayer(playerGroup.transform, tankPrefab, muzzleVFX, impactVFX, deathVFX);
            CrusaderTankHealth playerHealth = player.GetComponent<CrusaderTankHealth>();

            DungeonGameManager manager = systems.AddComponent<DungeonGameManager>();
            manager.player = playerHealth;
            Note(systems.transform, "SYSTEM__PlayerPrefs_checkpoint_save_persists_between_play_sessions");
            Note(systems.transform, "SYSTEM__Death_waits_then_respawns_at_last_saved_checkpoint");
            Note(systems.transform, "SYSTEM__Press_R_to_reset_saved_checkpoint");

            BuildCheckpoints(checkpointGroup.transform, checkpointMat, checkpointVFX);
            BuildEnemies(enemyGroup.transform, enemyPadMat, skeletonPrefab, impactVFX, deathVFX, player.transform);
            BuildVFXGallery(vfxGroup.transform, checkpointVFX, muzzleVFX, impactVFX, deathVFX);
            BuildLightingAndCamera(cameraGroup.transform, player.transform);
            BuildFinalPortal(portalGroup.transform);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeGameObject = teacher;
            Debug.Log("[VFXDuAN] DUNGEON TANK SHOWCASE BUILT. Scene: " + ScenePath);
            Debug.Log("[VFXDuAN] Controls: W/S move, A/D turn, LMB/Space fire, E save checkpoint, R reset save.");
        }

        private static void BuildTeacherGuide(
            Transform parent,
            GameObject floor,
            GameObject wall,
            GameObject prop,
            GameObject tank,
            GameObject skeleton,
            GameObject checkpointVfx,
            GameObject muzzleVfx,
            GameObject impactVfx)
        {
            Note(parent, "README__DUNGEON_TANK_VFX_SHOWCASE__TEACHER_CHECKLIST");
            Note(parent, "01__MAP__StylizedHandPaintedDungeon_Free_package_integrated");
            Note(parent, "02__PLAYER__Crusader_Tank_package_visual_plus_runtime_controller");
            Note(parent, "03__ENEMY__DungeonCharacters_Skeleton_chase_attack_health_death");
            Note(parent, "04__CHECKPOINTS__THREE_points__enter_plus_E_saves_respawn");
            Note(parent, "05__DEATH__tank_returns_to_last_saved_checkpoint");
            Note(parent, "06__VFX__Particlecollection_Free_samples_used_for_save_shot_hit_death");
            Note(parent, "07__BLACK_HOLE__custom_shader_VFX_final_portal");
            Note(parent, "08__CONTROLS__WS_move__AD_turn__LMB_or_SPACE_fire__E_save__R_reset");
            Note(parent, "09__SCENE__Assets_VFXDuAN_DungeonTank_Scenes_DungeonTank_Showcase");
            Note(parent, "10__SAFE_FALLBACKS__scene_still_builds_if_an_imported_prefab_name_changes");

            Note(parent, "FOUND_DUNGEON_FLOOR__" + AssetLabel(floor));
            Note(parent, "FOUND_DUNGEON_WALL__" + AssetLabel(wall));
            Note(parent, "FOUND_DUNGEON_PROP__" + AssetLabel(prop));
            Note(parent, "FOUND_PLAYER_TANK__" + AssetLabel(tank));
            Note(parent, "FOUND_ENEMY_SKELETON__" + AssetLabel(skeleton));
            Note(parent, "FOUND_VFX_CHECKPOINT__" + AssetLabel(checkpointVfx));
            Note(parent, "FOUND_VFX_MUZZLE__" + AssetLabel(muzzleVfx));
            Note(parent, "FOUND_VFX_IMPACT__" + AssetLabel(impactVfx));
        }

        private static void BuildDungeon(
            Transform parent,
            Material floorMat,
            Material wallMat,
            Material trimMat,
            GameObject importedFloor,
            GameObject importedWall,
            GameObject importedProp)
        {
            Transform collision = NewGroup(parent, "01_GAMEPLAY_COLLISION_FOUNDATION");
            Transform imported = NewGroup(parent, "02_IMPORTED_DUNGEON_PACKAGE_VISUALS");
            Transform rooms = NewGroup(parent, "03_THREE_ROOMS_AND_CORRIDORS");
            Transform props = NewGroup(parent, "04_DUNGEON_PROPS_AND_SET_DRESSING");

            Primitive("Floor_Main", PrimitiveType.Cube, collision, new Vector3(0f, -0.35f, 15f), new Vector3(18f, 0.7f, 92f), floorMat);
            Primitive("Wall_Left", PrimitiveType.Cube, collision, new Vector3(-9.2f, 2.5f, 15f), new Vector3(0.7f, 5.5f, 92f), wallMat);
            Primitive("Wall_Right", PrimitiveType.Cube, collision, new Vector3(9.2f, 2.5f, 15f), new Vector3(0.7f, 5.5f, 92f), wallMat);

            BuildGateWall(collision, -14f, wallMat, "Gate_Entrance");
            BuildGateWall(collision, 10f, wallMat, "Gate_Room1_to_Room2");
            BuildGateWall(collision, 34f, wallMat, "Gate_Room2_to_Room3");
            BuildGateWall(collision, 58f, wallMat, "Gate_FinalPortal");

            float[] roomCenters = { -2f, 22f, 46f };
            for (int i = 0; i < roomCenters.Length; i++)
            {
                Transform room = NewGroup(rooms, $"ROOM_{i + 1:00}__CHECKPOINT_ZONE");
                Primitive($"Room{i + 1}_CenterPlate", PrimitiveType.Cube, room, new Vector3(0f, -0.02f, roomCenters[i]), new Vector3(13f, 0.08f, 13f), trimMat);

                for (int side = -1; side <= 1; side += 2)
                {
                    Primitive($"Room{i + 1}_Pillar_{side}_A", PrimitiveType.Cube, room, new Vector3(side * 6.8f, 1.7f, roomCenters[i] - 5f), new Vector3(1.0f, 3.4f, 1.0f), wallMat);
                    Primitive($"Room{i + 1}_Pillar_{side}_B", PrimitiveType.Cube, room, new Vector3(side * 6.8f, 1.7f, roomCenters[i] + 5f), new Vector3(1.0f, 3.4f, 1.0f), wallMat);
                }
            }

            if (importedFloor != null)
            {
                for (int z = -26; z <= 56; z += 8)
                    SpawnImported(importedFloor, imported, $"PKG_Floor__Z{z}", new Vector3(0f, 0.02f, z), Quaternion.identity, 7.2f, true);
            }
            else
            {
                Note(imported, "PACKAGE_FLOOR_NOT_FOUND__Using_collision_floor_fallback");
            }

            if (importedWall != null)
            {
                for (int z = -24; z <= 56; z += 10)
                {
                    SpawnImported(importedWall, imported, $"PKG_Wall_L__Z{z}", new Vector3(-8.45f, 1.8f, z), Quaternion.Euler(0f, 90f, 0f), 4.3f, true);
                    SpawnImported(importedWall, imported, $"PKG_Wall_R__Z{z}", new Vector3(8.45f, 1.8f, z), Quaternion.Euler(0f, -90f, 0f), 4.3f, true);
                }
            }
            else
            {
                Note(imported, "PACKAGE_WALL_NOT_FOUND__Using_collision_wall_fallback");
            }

            if (importedProp != null)
            {
                Vector3[] positions =
                {
                    new Vector3(-6.8f, 0f, -8f), new Vector3(6.8f, 0f, -8f),
                    new Vector3(-6.8f, 0f, 16f), new Vector3(6.8f, 0f, 16f),
                    new Vector3(-6.8f, 0f, 40f), new Vector3(6.8f, 0f, 40f),
                    new Vector3(-5.8f, 0f, 54f), new Vector3(5.8f, 0f, 54f)
                };

                for (int i = 0; i < positions.Length; i++)
                    SpawnImported(importedProp, props, $"PKG_Prop_{i + 1:00}", positions[i], Quaternion.Euler(0f, (i * 47f) % 360f, 0f), 2.6f, true);
            }
            else
            {
                Note(props, "PACKAGE_PROP_NOT_FOUND__Fallback_set_dressing_only");
            }
        }

        private static GameObject BuildPlayer(
            Transform parent,
            GameObject tankPrefab,
            GameObject muzzleVfx,
            GameObject impactVfx,
            GameObject deathVfx)
        {
            GameObject root = new GameObject("PLAYER_CRUSADER_TANK__CONTROLLABLE");
            root.transform.SetParent(parent, false);
            root.transform.position = new Vector3(0f, 0.85f, -30f);

            Rigidbody body = root.AddComponent<Rigidbody>();
            body.mass = 7f;
            body.useGravity = true;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

            BoxCollider collider = root.AddComponent<BoxCollider>();
            collider.center = new Vector3(0f, 0.65f, 0f);
            collider.size = new Vector3(2.8f, 1.5f, 4.2f);

            Transform visual = NewGroup(root.transform, "01_VISUAL__CRUSADER_TANK_PACKAGE");
            if (tankPrefab != null)
                SpawnImported(tankPrefab, visual, "Crusader_Tank__Imported", Vector3.zero, Quaternion.identity, 4.6f, true, true);
            else
                BuildFallbackTankVisual(visual);

            Transform muzzle = new GameObject("02_MUZZLE__PROJECTILE_AND_VFX_ORIGIN").transform;
            muzzle.SetParent(root.transform, false);
            muzzle.localPosition = new Vector3(0f, 1.25f, 2.35f);

            CrusaderTankController controller = root.AddComponent<CrusaderTankController>();
            controller.muzzle = muzzle;
            controller.muzzleVFXPrefab = muzzleVfx;
            controller.impactVFXPrefab = impactVfx;

            CrusaderTankHealth health = root.AddComponent<CrusaderTankHealth>();
            health.maxHealth = 100f;
            health.deathVFXPrefab = deathVfx != null ? deathVfx : impactVfx;

            Note(root.transform, "CONTROLS__W_S_MOVE__A_D_TURN__LMB_OR_SPACE_FIRE");
            Note(root.transform, "HEALTH__100__Skeleton_attacks_damage_tank__death_respawns");
            return root;
        }

        private static void BuildEnemies(
            Transform parent,
            Material padMat,
            GameObject skeletonPrefab,
            GameObject hitVfx,
            GameObject deathVfx,
            Transform player)
        {
            Note(parent, "ENEMY_AI__Detect_14m__Chase__Attack_2_25m__Damage_16");
            Note(parent, "ENEMY_HP__60__Tank_shell_damage_25__Death_VFX_then_destroy");

            Vector3[] positions =
            {
                new Vector3(-4.0f, 1f, -4f), new Vector3(4.2f, 1f, 2f),
                new Vector3(-5.0f, 1f, 18f), new Vector3(4.8f, 1f, 25f), new Vector3(0f, 1f, 29f),
                new Vector3(-4.5f, 1f, 43f), new Vector3(4.5f, 1f, 49f), new Vector3(0f, 1f, 53f)
            };

            for (int i = 0; i < positions.Length; i++)
            {
                GameObject enemy = new GameObject($"SKELETON_{i + 1:00}__DUNGEON_ENEMY");
                enemy.transform.SetParent(parent, false);
                enemy.transform.position = positions[i];

                CapsuleCollider collider = enemy.AddComponent<CapsuleCollider>();
                collider.center = new Vector3(0f, 1.0f, 0f);
                collider.height = 2.1f;
                collider.radius = 0.42f;

                Primitive($"EnemyPad_{i + 1:00}", PrimitiveType.Cylinder, enemy.transform, new Vector3(0f, -0.85f, 0f), new Vector3(1.3f, 0.04f, 1.3f), padMat, true);

                Transform visual = NewGroup(enemy.transform, "VISUAL__SKELETON_PACKAGE");
                Animator animator = null;
                if (skeletonPrefab != null)
                {
                    GameObject instance = SpawnImported(skeletonPrefab, visual, "Skeleton__Imported", Vector3.zero, Quaternion.identity, 2.2f, true, true);
                    if (instance != null) animator = instance.GetComponentInChildren<Animator>();
                }
                else
                {
                    BuildFallbackSkeletonVisual(visual);
                }

                SkeletonEnemyHealth health = enemy.AddComponent<SkeletonEnemyHealth>();
                health.maxHealth = 60f;
                health.hitVFXPrefab = hitVfx;
                health.deathVFXPrefab = deathVfx != null ? deathVfx : hitVfx;
                health.animator = animator;

                SkeletonEnemyAI ai = enemy.AddComponent<SkeletonEnemyAI>();
                ai.target = player;
                ai.animator = animator;
                ai.moveSpeed = 2.2f + (i % 3) * 0.15f;
            }
        }

        private static void BuildCheckpoints(Transform parent, Material checkpointMat, GameObject saveVfx)
        {
            Note(parent, "HOW_TO_TEST__Drive_into_checkpoint__PRESS_E__then_die__tank_respawns_here");
            Note(parent, "CHECKPOINT_SAVE__PlayerPrefs_key_VFXDuAN_Dungeon_Checkpoint");

            float[] z = { -8f, 18f, 44f };
            for (int i = 0; i < z.Length; i++)
            {
                int index = i + 1;
                GameObject cp = new GameObject($"CHECKPOINT_{index:00}__PRESS_E_TO_SAVE");
                cp.transform.SetParent(parent, false);
                cp.transform.position = new Vector3(0f, 0f, z[i]);

                BoxCollider trigger = cp.AddComponent<BoxCollider>();
                trigger.isTrigger = true;
                trigger.center = new Vector3(0f, 1.0f, 0f);
                trigger.size = new Vector3(6.5f, 2.5f, 6.5f);

                Transform visual = NewGroup(cp.transform, "01_VISUAL_BEACON");
                List<Renderer> glows = new List<Renderer>();

                GameObject baseRing = Primitive("CheckpointBase", PrimitiveType.Cylinder, visual, new Vector3(0f, 0.08f, 0f), new Vector3(3.0f, 0.12f, 3.0f), checkpointMat, true);
                Renderer baseRenderer = baseRing.GetComponent<Renderer>();
                if (baseRenderer != null) glows.Add(baseRenderer);

                Vector3[] pillarPos =
                {
                    new Vector3(-2.2f, 1.25f, -2.2f), new Vector3(2.2f, 1.25f, -2.2f),
                    new Vector3(-2.2f, 1.25f, 2.2f), new Vector3(2.2f, 1.25f, 2.2f)
                };

                foreach (Vector3 p in pillarPos)
                {
                    GameObject pillar = Primitive("GlowPillar", PrimitiveType.Cylinder, visual, p, new Vector3(0.18f, 1.25f, 0.18f), checkpointMat, true);
                    Renderer r = pillar.GetComponent<Renderer>();
                    if (r != null) glows.Add(r);
                }

                Light light = cp.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = new Color(0.15f, 0.5f, 1f);
                light.intensity = 2.2f;
                light.range = 6f;
                light.shadows = LightShadows.Soft;

                Transform spawn = new GameObject("02_RESPAWN_POINT__SAVED_POSITION").transform;
                spawn.SetParent(cp.transform, false);
                spawn.localPosition = new Vector3(0f, 0.85f, 1.8f);
                spawn.localRotation = Quaternion.identity;

                GameObject labelObj = new GameObject("03_WORLD_LABEL__CHECKPOINT");
                labelObj.transform.SetParent(cp.transform, false);
                labelObj.transform.localPosition = new Vector3(0f, 3.2f, 0f);
                TextMesh text = labelObj.AddComponent<TextMesh>();
                text.text = $"CHECKPOINT {index}\nE = SAVE";
                text.fontSize = 38;
                text.characterSize = 0.08f;
                text.anchor = TextAnchor.MiddleCenter;
                text.alignment = TextAlignment.Center;
                text.color = new Color(0.35f, 0.75f, 1f);

                DungeonCheckpoint checkpoint = cp.AddComponent<DungeonCheckpoint>();
                checkpoint.checkpointIndex = index;
                checkpoint.respawnPoint = spawn;
                checkpoint.saveVFXPrefab = saveVfx;
                checkpoint.checkpointLight = light;
                checkpoint.glowRenderers = glows.ToArray();

                if (saveVfx != null)
                {
                    GameObject preview = SpawnImported(saveVfx, visual, "VFX_Preview__Package_Save_Effect", new Vector3(0f, 0.35f, 0f), Quaternion.identity, 2.2f, true, false);
                    if (preview != null) DisableAutoDestroyComponents(preview);
                }
            }
        }

        private static void BuildVFXGallery(Transform parent, GameObject checkpoint, GameObject muzzle, GameObject impact, GameObject death)
        {
            Note(parent, "SOURCE_PACKAGE__Assets_Particlecollection_Free_samples");
            Note(parent, "VFX_01__Checkpoint_saved_effect__" + AssetLabel(checkpoint));
            Note(parent, "VFX_02__Tank_muzzle_effect__" + AssetLabel(muzzle));
            Note(parent, "VFX_03__Projectile_hit_effect__" + AssetLabel(impact));
            Note(parent, "VFX_04__Death_explosion_effect__" + AssetLabel(death));
            Note(parent, "VFX_RUNTIME__effects_are_instantiated_then_auto_cleaned_to_avoid_scene_leaks");
        }

        private static void BuildLightingAndCamera(Transform parent, Transform player)
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.07f, 0.10f, 0.18f);
            RenderSettings.ambientEquatorColor = new Color(0.035f, 0.045f, 0.07f);
            RenderSettings.ambientGroundColor = new Color(0.015f, 0.012f, 0.02f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.025f, 0.035f, 0.055f);
            RenderSettings.fogDensity = 0.012f;

            GameObject sun = new GameObject("01_MOONLIGHT__DIRECTIONAL");
            sun.transform.SetParent(parent, false);
            sun.transform.rotation = Quaternion.Euler(52f, -32f, 0f);
            Light sunLight = sun.AddComponent<Light>();
            sunLight.type = LightType.Directional;
            sunLight.color = new Color(0.38f, 0.48f, 0.75f);
            sunLight.intensity = 0.75f;
            sunLight.shadows = LightShadows.Soft;

            float[] z = { -20f, -2f, 18f, 38f, 54f };
            for (int i = 0; i < z.Length; i++)
            {
                GameObject torch = new GameObject($"02_DUNGEON_LIGHT_{i + 1:00}");
                torch.transform.SetParent(parent, false);
                torch.transform.position = new Vector3(i % 2 == 0 ? -6.5f : 6.5f, 2.7f, z[i]);
                Light l = torch.AddComponent<Light>();
                l.type = LightType.Point;
                l.color = i % 2 == 0 ? new Color(1f, 0.32f, 0.08f) : new Color(0.12f, 0.35f, 1f);
                l.intensity = 4.0f;
                l.range = 10f;
                l.shadows = LightShadows.Soft;
            }

            GameObject camObj = new GameObject("03_MAIN_CAMERA__CINEMATIC_FOLLOW");
            camObj.tag = "MainCamera";
            camObj.transform.SetParent(parent, false);
            camObj.transform.position = player.position + new Vector3(0f, 8f, -11f);
            Camera cam = camObj.AddComponent<Camera>();
            cam.fieldOfView = 62f;
            cam.nearClipPlane = 0.08f;
            cam.farClipPlane = 250f;
            cam.clearFlags = CameraClearFlags.Skybox;
            camObj.AddComponent<AudioListener>();

            DungeonCameraFollow follow = camObj.AddComponent<DungeonCameraFollow>();
            follow.target = player;
        }

        private static void BuildFinalPortal(Transform parent)
        {
            Note(parent, "FINAL_VFX__Custom_BlackHole_shader_showcase_already_in_project");
            Note(parent, "GOAL__Reach_final_chamber_after_checkpoint_3");

            GameObject portal = new GameObject("FINAL_PORTAL__VFX_BLACK_HOLE");
            portal.transform.SetParent(parent, false);
            portal.transform.position = new Vector3(0f, 3.2f, 61f);
            portal.transform.localScale = Vector3.one * 0.7f;

            BlackHoleEffect effect = portal.AddComponent<BlackHoleEffect>();
            effect.coreRadius = 0.9f;
            effect.lensRadius = 1.5f;
            effect.diskRadius = 3.6f;
            effect.diskTilt = 18f;
            effect.particleCount = 220;
            effect.Build();
        }

        private static void BuildGateWall(Transform parent, float z, Material wallMat, string name)
        {
            Transform gate = NewGroup(parent, name);
            Primitive("Left", PrimitiveType.Cube, gate, new Vector3(-5.6f, 2.5f, z), new Vector3(6.6f, 5.5f, 0.65f), wallMat);
            Primitive("Right", PrimitiveType.Cube, gate, new Vector3(5.6f, 2.5f, z), new Vector3(6.6f, 5.5f, 0.65f), wallMat);
            Primitive("Top", PrimitiveType.Cube, gate, new Vector3(0f, 4.8f, z), new Vector3(4.6f, 0.9f, 0.65f), wallMat);
        }

        private static void BuildFallbackTankVisual(Transform parent)
        {
            Material body = GetOrCreateMaterial("MAT_FallbackTank", new Color(0.23f, 0.27f, 0.20f), false);
            Material dark = GetOrCreateMaterial("MAT_FallbackTankDark", new Color(0.07f, 0.075f, 0.06f), false);
            Primitive("Fallback_Chassis", PrimitiveType.Cube, parent, new Vector3(0f, 0.45f, 0f), new Vector3(2.4f, 0.8f, 3.8f), body, true);
            Primitive("Fallback_Turret", PrimitiveType.Cylinder, parent, new Vector3(0f, 1.05f, 0f), new Vector3(1.4f, 0.35f, 1.4f), body, true);
            Primitive("Fallback_Barrel", PrimitiveType.Cube, parent, new Vector3(0f, 1.15f, 1.75f), new Vector3(0.25f, 0.25f, 2.8f), dark, true);
        }

        private static void BuildFallbackSkeletonVisual(Transform parent)
        {
            Material bone = GetOrCreateMaterial("MAT_FallbackSkeleton", new Color(0.78f, 0.74f, 0.62f), false);
            Primitive("Fallback_Body", PrimitiveType.Capsule, parent, new Vector3(0f, 0.95f, 0f), new Vector3(0.7f, 1.0f, 0.7f), bone, true);
            Primitive("Fallback_Head", PrimitiveType.Sphere, parent, new Vector3(0f, 1.85f, 0f), new Vector3(0.55f, 0.55f, 0.55f), bone, true);
        }

        private static GameObject FindBestGameObject(string folder, params string[] keywords)
        {
            if (!AssetDatabase.IsValidFolder(folder)) return null;

            string[] guids = AssetDatabase.FindAssets("t:GameObject", new[] { folder });
            if (guids == null || guids.Length == 0) return null;

            GameObject best = null;
            int bestScore = int.MinValue;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (asset == null) continue;

                string file = System.IO.Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
                int score = 0;
                foreach (string keyword in keywords)
                {
                    if (file.Contains(keyword.ToLowerInvariant())) score += 20;
                }

                if (file.Contains("demo") || file.Contains("scene")) score -= 15;
                if (file.Contains("lod")) score -= 2;
                if (path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase)) score += 4;

                if (score > bestScore)
                {
                    bestScore = score;
                    best = asset;
                }
            }

            return best;
        }

        private static GameObject SpawnImported(
            GameObject asset,
            Transform parent,
            string name,
            Vector3 position,
            Quaternion rotation,
            float targetMaxDimension,
            bool disableColliders,
            bool positionIsLocal = false)
        {
            if (asset == null) return null;

            GameObject instance = PrefabUtility.InstantiatePrefab(asset) as GameObject;
            if (instance == null) instance = UnityEngine.Object.Instantiate(asset);
            if (instance == null) return null;

            instance.name = name;
            instance.transform.SetParent(parent, false);
            if (positionIsLocal)
            {
                instance.transform.localPosition = position;
                instance.transform.localRotation = rotation;
            }
            else
            {
                instance.transform.SetPositionAndRotation(position, rotation);
            }

            NormalizeVisual(instance, targetMaxDimension);

            if (disableColliders)
            {
                Collider[] colliders = instance.GetComponentsInChildren<Collider>(true);
                foreach (Collider c in colliders)
                    if (c != null) c.enabled = false;
            }

            return instance;
        }

        private static void NormalizeVisual(GameObject instance, float targetMaxDimension)
        {
            Renderer[] renderers = instance.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) return;

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);

            float max = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
            if (max <= 0.0001f) return;

            float scale = targetMaxDimension / max;
            instance.transform.localScale *= scale;

            Renderer[] scaled = instance.GetComponentsInChildren<Renderer>(true);
            if (scaled.Length == 0) return;
            Bounds after = scaled[0].bounds;
            for (int i = 1; i < scaled.Length; i++) after.Encapsulate(scaled[i].bounds);

            float groundOffset = after.min.y;
            Vector3 pos = instance.transform.position;
            instance.transform.position = new Vector3(pos.x, pos.y - groundOffset, pos.z);
        }

        private static void DisableAutoDestroyComponents(GameObject root)
        {
            ParticleSystem[] particles = root.GetComponentsInChildren<ParticleSystem>(true);
            foreach (ParticleSystem ps in particles)
            {
                if (ps == null) continue;
                var main = ps.main;
                main.loop = true;
                ps.Play();
            }
        }

        private static GameObject Primitive(
            string name,
            PrimitiveType type,
            Transform parent,
            Vector3 position,
            Vector3 scale,
            Material material,
            bool removeCollider = false)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = scale;

            Renderer r = go.GetComponent<Renderer>();
            if (r != null) r.sharedMaterial = material;

            if (removeCollider)
            {
                Collider c = go.GetComponent<Collider>();
                if (c != null) UnityEngine.Object.DestroyImmediate(c);
            }

            return go;
        }

        private static Material GetOrCreateMaterial(string name, Color color, bool emissive)
        {
            EnsureFolder(GeneratedFolder, "Materials");
            string path = MaterialFolder + "/" + name + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null) shader = Shader.Find("Standard");
                material = new Material(shader != null ? shader : Shader.Find("Hidden/InternalErrorShader"));
                material.name = name;
                AssetDatabase.CreateAsset(material, path);
            }

            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            if (emissive && material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * 3.5f);
            }
            EditorUtility.SetDirty(material);
            return material;
        }

        private static string AssetLabel(GameObject asset)
        {
            if (asset == null) return "NOT_FOUND__SAFE_FALLBACK_USED";
            string path = AssetDatabase.GetAssetPath(asset);
            string file = System.IO.Path.GetFileNameWithoutExtension(path);
            return Sanitize(file);
        }

        private static string Sanitize(string value)
        {
            if (string.IsNullOrEmpty(value)) return "UNKNOWN";
            foreach (char c in System.IO.Path.GetInvalidFileNameChars()) value = value.Replace(c, '_');
            return value.Replace(' ', '_').Replace('(', '_').Replace(')', '_');
        }

        private static GameObject Group(string name)
        {
            return new GameObject(name);
        }

        private static Transform NewGroup(Transform parent, string name)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        private static void Note(Transform parent, string name)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets", "VFXDuAN");
            EnsureFolder("Assets/VFXDuAN", "DungeonTank");
            EnsureFolder(RootFolder, "Scenes");
            EnsureFolder(RootFolder, "Generated");
            EnsureFolder(GeneratedFolder, "Materials");
        }

        private static void EnsureFolder(string parent, string child)
        {
            string path = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(path))
                AssetDatabase.CreateFolder(parent, child);
        }
    }
}
#endif
