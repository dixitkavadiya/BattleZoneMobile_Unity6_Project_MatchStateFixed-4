# BattleZone Mobile - Production Art Specification

This specification defines original art targets for a premium mobile battle royale visual direction. It does not claim the current runtime-generated prototype art is final. Current primitive/runtime proxy assets remain placeholders until replaced by authored meshes, textures, rigs, animation clips, and audio made specifically for BattleZone Mobile.

## Character Targets

Player and bot characters must be original low-poly tactical humans with readable silhouettes at mobile distance.

- LOD0: 6,000-8,000 triangles for the hero player, 4,000-6,000 for common bots.
- LOD1: 45-55 percent of LOD0 triangles.
- LOD2: 15-25 percent of LOD0 triangles, preserving head, torso, backpack, and weapon readability.
- Texture budget: one 1024x1024 albedo/ORM/normal set for player LOD0; 512x512 for bot variants; 256x256 for LOD2 where practical.
- Rig: Humanoid-compatible skeleton, 65 bones or fewer, no cloth simulation required for gameplay-critical pieces.
- Required sockets: `Head`, `Spine`, `RightHand`, `LeftHand`, `Backpack`, `MuzzleAnchor`, `Foot_L`, `Foot_R`.
- Animation clips: idle, walk, run, sprint, jump, crouch, prone, aim, fire, reload, hit, death, weapon switch.
- Animation requirements: feet must stay planted during locomotion loops, root motion disabled unless gameplay code explicitly consumes it, and reload/fire clips must expose event timing notes for muzzle flash, shell eject, and ammo commit.

## Weapon Targets

All weapons must use original silhouettes and names. Do not trace or approximate recognizable BGMI/PUBG weapons or skins.

- Assault rifle LOD0: 2,200-3,000 triangles; LOD1: 1,000-1,500; LOD2: 350-650.
- Pistol LOD0: 1,000-1,600 triangles; LOD1: 500-850; LOD2: 180-350.
- Texture budget: 1024x512 for assault rifle, 512x512 for pistol, packed ORM map preferred.
- Required sockets: `Muzzle`, `ShellEject`, `Magazine`; assault rifle also needs `Optic` and `Grip`.
- Materials: one shared gunmetal material family plus small accent material; avoid unique material proliferation.

## Vehicle Targets

- Jeep and pickup LOD0: 7,000-10,000 triangles; buggy: 5,000-8,000; motorcycle: 3,500-5,500.
- LOD1: 45-55 percent of LOD0; LOD2: 15-25 percent.
- Texture budget: 1024x1024 shared vehicle atlas, 512x512 for small vehicle accessories.
- Required sockets: `DriverSeat`, `PassengerSeat`, `ExitLeft`, `ExitRight`, `Wheel_FL`, `Wheel_FR`, `Wheel_RL`, `Wheel_RR`, `FuelCap`, `DamageCenter`.
- Keep suspension pieces separate if they need runtime motion.

## Buildings, Props, Loot, Vegetation

- Small house LOD0: 6,000-9,000 triangles including interior kit; warehouse LOD0: 9,000-12,000.
- Common props: 150-1,200 triangles each; loot crates: 600-1,200.
- Trees: 1,200-1,800 triangles LOD0, 500-900 LOD1, 150-300 LOD2 or billboard/card substitute.
- Texture atlases: use modular 1024x1024 atlases for buildings and city props; 512x512 for loot families; 1024x1024 vegetation atlas with alpha kept modest.
- Building interiors must use separate wall/floor/ceiling modules so occlusion and collision can be authored cleanly.

## Materials And Shaders

- Use Unity 6.5 URP Lit or Simple Lit only unless a specific mobile-compatible shader is approved.
- Prefer one material per asset family and packed texture maps: Albedo, Normal, ORM.
- Avoid transparency except vegetation and UI; keep overdraw low.
- Use baked/fake detail in textures instead of heavy shader features.
- No HDRP, Shader Graph-only features, tessellation, geometry shaders, or screen-space heavy effects.

## Android Texture Compression

- Target ASTC 6x6 for main characters, weapons, buildings, vehicles, and UI.
- Use ASTC 8x8 for distant props and vegetation where quality holds.
- Use ETC2 fallback settings when ASTC is unavailable.
- Disable read/write on imported textures unless runtime access is required.
- Generate mipmaps for world textures; disable mipmaps for crisp UI icons.

## Blender To Unity FBX Settings

- Apply transforms before export: scale 1, rotation 0, location 0.
- Forward: `-Z Forward`; Up: `Y Up`.
- Units: meters.
- Export selected objects only.
- Mesh: apply modifiers, triangulate on export or before final review, include smoothing/normals/tangents.
- Armature: add leaf bones off, bake animation on for clips, simplify curves cautiously.
- Unity import scale: 1.0.
- Rig: Humanoid for characters, Generic for weapons/vehicles/props.
- Materials: import material names for mapping only; final Unity materials should live under `Assets/BattleZoneMobile/ArtPipeline/Materials` or the future production art folder.
