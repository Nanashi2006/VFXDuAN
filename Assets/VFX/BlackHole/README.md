# VFXDuAN - Black Hole Effect

Black hole effect inspired by the linked Unity VFX/Shader Graph tutorial, implemented as original URP shaders + a procedural Unity setup script so it can be merged without copying the tutorial's project assets.

## What is included

- Event horizon sphere with emissive Fresnel rim
- Animated accretion disk with procedural spiral/turbulence
- Screen-space gravitational lens distortion
- Orbiting glowing particles
- Editor menu that creates the whole effect automatically

## Requirements

- Unity 6 recommended
- Universal Render Pipeline (URP)
- Camera Opaque Texture enabled for the lens distortion
- Bloom recommended for the best glow

## Create the effect

After Unity recompiles:

1. Open any scene.
2. In the top menu choose **GameObject > VFX > VFXDuAN Black Hole**.
3. A `VFX_BlackHole` object is created in the Hierarchy.
4. Press Play to see the disk and particles rotate.

You can also add the `BlackHoleEffect` component to an empty GameObject and use the component context menu **Build Black Hole**.

## Enable lens distortion in URP

The distortion shader samples `_CameraOpaqueTexture`.

Depending on your URP setup, enable **Opaque Texture** on your URP Pipeline Asset or Renderer settings. If it is disabled, the event horizon and disk still render, but the lens may appear incorrect or black.

## Recommended scene look

- Dark skybox or space background
- Global Volume with Bloom enabled
- Bloom Intensity around 1-3 is a good starting point
- Put the black hole several units in front of the camera
- Try disk tilt between 8 and 20 degrees

## Main controls

On `BlackHoleEffect`:

- `Core Radius` - size of the event horizon
- `Lens Radius` - size of the lensing plane
- `Disk Radius` - size of the accretion disk
- `Disk Tilt` - viewing angle of the disk
- `Disk Spin Speed` - disk rotation speed
- `Particle Count` - density of small orbiting particles
- `Particle Spin Speed` - particle-ring rotation speed
- `Distortion` - strength of gravitational lensing

## Files

- `Shaders/BlackHoleCore.shader`
- `Shaders/BlackHoleDisk.shader`
- `Shaders/BlackHoleLens.shader`
- `Shaders/BlackHoleParticle.shader`
- `Scripts/BlackHoleEffect.cs`
- `Editor/CreateBlackHoleMenu.cs`

## Notes

This package uses procedural shaders rather than copied textures or files from the referenced tutorial. It is intended to be easy to inspect in the Hierarchy for assignment/demo grading.
