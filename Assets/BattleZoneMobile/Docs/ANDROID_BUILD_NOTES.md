# Android Build Notes

Use Unity 6.5 or newer within the Unity 6 line. The project has been checked with Unity `6000.5.3f1`.

## Required Settings

- Platform: Android
- Product Name: `BattleZone Mobile`
- Orientation: Landscape Left
- Active Input Handling: Both
- Scripting Backend: IL2CPP
- Target Architectures: ARM64
- Target frame rate: 60 FPS at runtime

## Build Steps

1. Open the project folder in Unity Hub.
2. Open `Assets/BattleZoneMobile/Scenes/BZ_Main.unity`.
3. Open `File > Build Profiles` or `File > Build Settings`.
4. Select `Android`.
5. Click `Switch Platform`.
6. Confirm `BZ_Main` is listed in Scenes In Build.
7. Open `Project Settings > Player`.
8. Confirm the required settings above.
9. Connect an Android device with USB debugging enabled.
10. Click `Build And Run`.

## Device Test

- Main menu appears in landscape.
- Touch joystick moves the humanoid player.
- Right-side drag rotates the camera.
- Sprint, crouch, jump, ADS, fire, reload, swap, medkit, inventory, pause, and settings buttons respond.
- Minimap, compass, match timer, kill feed, crosshair bloom, ammo, HP, backpack, and zone status display.
- Loot pickup works for five weapons, typed ammo, medkits, and bandages.
- Procedural audio plays for footsteps, guns, reloads, hits, deaths, pickups, and weapon switching.
- No red Console errors appear during Play or Build And Run.
