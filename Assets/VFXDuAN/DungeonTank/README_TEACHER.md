# VFXDuAN Polished Melee Dungeon Showcase

This folder contains the teacher-review build for a stylized dungeon melee game made from the four imported packages.

## One-click build
Use Unity menu:

`Tools > VFXDuAN > Build POLISHED Melee Dungeon Showcase`

This first creates the base showcase, then applies the polished melee setup, exact package prefabs, animation controllers, wider dungeon layout, HP bars, VFX, lighting and checkpoint presentation.

## Imported package usage
- `Assets/StylizedHandPaintedDungeon(Free)` -> Floor, Wall, Wall_Entrance, Door, Pillar, Lamp_01, Lamp_02
- `Assets/Crusader tank` -> Crusader player model + Idle/Walk/Attack/Hit/Death animations
- `Assets/DungeonCharacters` -> Skeleton model + Idle/Walk/Attack animations
- `Assets/Particlecollection_Free samples` -> Cleave attack VFX, Hit VFX, Magic Ring checkpoint VFX, death VFX

The four Asset Store packages remain local and are not required to be redistributed through GitHub.

## Gameplay
- W/S: move forward/back
- A/D: rotate
- Left Mouse or Space: melee attack
- E inside checkpoint: save respawn checkpoint
- R: reset saved checkpoint

Player melee attack uses an `AttackPoint`, short melee radius and one damage application per swing. There is no projectile gameplay in the polished build.

Skeleton enemies chase the player, stop before contact, attack at melee range and use trigger-only colliders so they do not physically push the player.

## Health
- Player: 100 HP with runtime HUD bar
- Skeleton: 60 HP with world-space runtime HP bar
- Player death -> respawn at last saved checkpoint
- Skeleton death -> VFX then remove enemy

## Checkpoints
Three checkpoints are placed through the dungeon. Walk into one and press E to save it. The latest saved checkpoint becomes the respawn point.

## Dungeon presentation
The polished layout uses a 24m-wide gameplay corridor/room structure with approximately 5.6m-wide gate openings, exact dungeon package floor/wall/pillar/door/lamp assets, warm torch lights, cool ambient lighting and dungeon fog.

## Teacher-facing hierarchy
- `00_TEACHER_GUIDE__OPEN_ME`
- `01_SYSTEMS__SAVE_RESPAWN_GAME_MANAGER`
- `02_PLAYER__CRUSADER_MELEE`
- `03_DUNGEON_MAP__STYLIZED_PACKAGE`
- `04_ENEMIES__SKELETON_DUNGEON`
- `05_CHECKPOINTS__3_SAVE_RESPAWN_POINTS`
- `06_UI__HP_AND_GAMEPLAY`
- `07_VFX__HIT_SLASH_CHECKPOINT_DEATH`
- `08_LIGHTING_CAMERA__DUNGEON_MOOD`
- `09_FINAL_GOAL__BLACK_HOLE_PORTAL`

The `00_TEACHER_GUIDE__OPEN_ME` group also receives extra note objects describing the implemented features for quick grading.
