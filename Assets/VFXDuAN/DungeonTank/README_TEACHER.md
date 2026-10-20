# VFXDuAN Dungeon Tank Showcase

This folder contains the one-click dungeon/tank/checkpoint gameplay showcase built for teacher review.

Use Unity menu: `Tools > VFXDuAN > Build Dungeon Tank Showcase`.

The builder searches the imported packages at runtime in the Unity Editor:
- `Assets/StylizedHandPaintedDungeon(Free)` for dungeon environment pieces
- `Assets/Crusader tank` for the player tank visual
- `Assets/DungeonCharacters` for Skeleton enemy visuals
- `Assets/Particlecollection_Free samples` for checkpoint, muzzle, hit and death VFX

If a package prefab cannot be found, the builder creates a safe primitive fallback so the scene still builds instead of throwing an error.

Gameplay controls:
- W/S: move tank
- A/D: rotate tank
- Left Mouse or Space: fire
- E while inside a checkpoint: SAVE respawn checkpoint
- R: reset saved checkpoint

Teacher-facing hierarchy created by the builder:
- `00_TEACHER_GUIDE__OPEN_ME`
- `01_SYSTEMS__SAVE_RESPAWN_GAME_MANAGER`
- `02_PLAYER__CRUSADER_TANK`
- `03_DUNGEON_MAP__STYLIZED_PACKAGE`
- `04_ENEMIES__SKELETON_DUNGEON`
- `05_CHECKPOINTS__3_SAVE_RESPAWN_POINTS`
- `06_VFX__PARTICLE_PACKAGE_INTEGRATION`
- `07_LIGHTING_CAMERA__CINEMATIC`
- `08_FINAL_PORTAL__BLACK_HOLE_VFX`

Important: the four Asset Store packages themselves are not committed by this code. They must remain imported in the local Unity project. The builder discovers and uses them from their package folders.
