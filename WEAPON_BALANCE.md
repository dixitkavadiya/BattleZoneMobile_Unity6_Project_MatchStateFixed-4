# BattleZone Mobile - Weapon Balance

Milestone 24B adds the first original weapon roster using `AdvancedWeaponData` ScriptableObjects under `Assets/BattleZoneMobile/Resources/WeaponData`.

All names, values, silhouettes, and hooks are original placeholders for BattleZone Mobile. They are not copied from BGMI/PUBG weapons, skins, sounds, recoil tables, UI, or branding.

## Milestone 24D Attachment Balance

The live-match `WeaponController` now accepts data-driven `WeaponAttachmentData` pickups through the inventory. Attachment compatibility is declared per attachment using compatible `WeaponSlot` arrays and weapon support flags; pickup logic does not hardcode individual attachment behavior.

| Attachment | Slot | Rarity | Compatible Weapons | Primary Modifier |
| --- | --- | --- | --- | --- |
| Red Dot | Optic | Common | All firearms | Slight spread reduction |
| Holo Sight | Optic | Common | All firearms | Slight ADS stability |
| 2x Scope | Optic | Uncommon | Rifle, SMG, Sniper | Mid zoom accuracy helper |
| 4x Scope | Optic | Rare | Rifle, Sniper | Long-range spread reduction |
| 6x Scope | Optic | Epic | Rifle, Sniper | Strong long-range spread reduction |
| 8x Scope | Optic | Epic | Sniper | Dedicated sniper optic |
| Compensator | Muzzle | Rare | Rifle, SMG | Strong recoil reduction |
| Flash Hider | Muzzle | Uncommon | All firearms | Moderate recoil/spread cleanup |
| Suppressor | Muzzle | Epic | All firearms | Suppressed-fire audio with small handling tradeoff |
| Extended Mag | Magazine | Rare | All firearms | Larger magazine |
| Quickdraw Mag | Magazine | Uncommon | All firearms | Faster reload |
| Extended Quickdraw | Magazine | Epic | All firearms | Larger magazine and faster reload |
| Vertical Grip | Grip | Uncommon | Rifle, SMG | Vertical recoil control |
| Angled Grip | Grip | Rare | Rifle, SMG | Spread and handling control |
| Lightweight Grip | Grip | Rare | Rifle, SMG, Sniper | ADS spread stability |
| Tactical Stock | Stock | Rare | Rifle, SMG | Recoil/reload smoothing |
| Compact Laser | Laser | Uncommon | All firearms | Hip-fire spread reduction |

Attachment effects currently modify the existing runtime values:

- `recoilMultiplier`
- `spreadMultiplier`
- `reloadMultiplier`
- `magazineMultiplier`
- `fireRateMultiplier`
- `suppressesFireAudio`

Attachment replacement returns the old attachment to backpack storage when capacity allows; otherwise the equip action is rejected so attachments are not silently lost. Magazine replacement clamps overflow safely back into reserve ammo, and extended magazines can top up from reserve if the weapon was already full before attachment.

## Milestone 24C Runtime Gun Feel

The current live-match `WeaponController` now exposes and uses per-weapon recoil and feedback values on `WeaponDefinition`:

| Runtime Weapon | Camera Recoil | Weapon Kick | Crosshair Behavior | ADS Feel |
| --- | --- | --- | --- | --- |
| Pistol | Medium vertical, light horizontal | Short snap kick, fast return | Moderate bloom, fast recovery | Light sway |
| Assault Rifle | Controlled vertical climb, mild horizontal drift | Moderate repeated kick | Sustained bloom while firing/moving | Stable mid-range ADS |
| SMG | Low vertical, snappier horizontal drift | Small fast kick | Higher movement bloom, quick return | Fast close-range ADS |
| Sniper | Heavy single-shot vertical kick | Strong visual kick, slower return | Tight ADS, large shot bloom | Scoped breathing sway |
| Shotgun | Heavy kick with wider horizontal variance | Strong pump-style visual kick | Large shot bloom, slower recovery | Close-range ADS tightening |

24C also adds optional `WeaponDefinition` audio override fields for fire, suppressed fire, reload, dry fire, hit confirm, headshot confirm, and kill confirm. When no authored clip is assigned, the project uses generated original temporary tones from `RuntimeAudioBank`.

Surface impacts are currently routed through runtime placeholder particles and generated impact tones for metal, wood, stone, glass, and ground-like materials. Final authored VFX and audio assets are still needed.

| Weapon | Category | Damage | Fire Rate | Magazine | Reload | Effective Range | Max Range | Recoil Profile | Intended Role | Current Limitations |
| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | --- | --- | --- |
| VXR-56 | Assault Rifle | 24 | 9.2 | 30 | 2.05s | 70m | 150m | Moderate vertical, mild horizontal, quick recovery | Default flexible rifle for mid-range fights | Temporary placeholder visual and audio hooks only |
| ARK-74 | Assault Rifle | 26 | 8.0 | 32 | 2.15s | 78m | 160m | Higher climb, stronger horizontal drift | Harder-hitting rifle with full-auto/burst choice | Temporary placeholder visual and audio hooks only |
| Sentinel AR | Assault Rifle | 30 | 6.2 | 25 | 2.00s | 92m | 175m | Controlled burst recoil, accurate ADS | Precision burst rifle for careful mobile aiming | Temporary placeholder visual and audio hooks only |
| Pulse-9 | SMG | 17 | 13.0 | 36 | 1.62s | 42m | 92m | Low kick, high bloom during sustained fire | Fast close-range pressure weapon | Temporary placeholder visual and audio hooks only |
| Raptor-45 | SMG | 21 | 10.5 | 28 | 1.72s | 48m | 105m | Snappier recoil, better burst control | Heavy SMG for close-to-mid fights | Temporary placeholder visual and audio hooks only |
| Longshot DMR | DMR | 48 | 3.2 | 12 | 2.25s | 124m | 215m | Strong single-shot kick, good ADS stability | Semi-auto marksman weapon | Temporary placeholder visual and audio hooks only |
| Falcon SR | Sniper | 88 | 0.75 | 5 | 2.80s | 180m | 300m | Heavy bolt-action kick, slow recovery | High-damage long-range sniper; projectile delivery flagged for testing | Projectile prefab not yet assigned, so runtime fallback remains raycast until art/VFX prefab exists |
| Breacher-12 | Shotgun | 78 | 1.05 | 6 | 2.35s | 24m | 52m | Heavy pump-action recoil and large spread bloom | Close-range room clearing | Temporary placeholder visual and audio hooks only |
| Sidearm P9 | Pistol | 24 | 4.2 | 15 | 1.32s | 35m | 78m | Light sidearm kick with fast recovery | Reliable backup pistol | Temporary placeholder visual and audio hooks only |
| Hammer .50 | Pistol | 44 | 1.6 | 7 | 1.82s | 52m | 96m | Heavy hand-cannon kick and slower recovery | High-damage sidearm with low capacity | Temporary placeholder visual and audio hooks only |

## Fire Modes

| Weapon | Supported Modes |
| --- | --- |
| VXR-56 | Full-auto, Semi-auto |
| ARK-74 | Full-auto, Burst, Semi-auto |
| Sentinel AR | Burst, Semi-auto |
| Pulse-9 | Full-auto, Semi-auto |
| Raptor-45 | Full-auto, Burst, Semi-auto |
| Longshot DMR | Semi-auto |
| Falcon SR | Bolt-action |
| Breacher-12 | Pump-action |
| Sidearm P9 | Semi-auto |
| Hammer .50 | Semi-auto |

## Attachment Slots

All rifles and SMGs support optic, muzzle, magazine, grip, and stock slots as appropriate. DMR/sniper weapons focus on optic, muzzle, magazine, and stock slots. Pistols use optic, muzzle, and magazine slots. Breacher-12 uses optic, muzzle/choke, and stock slots.

## Current Placeholder Assets

- Weapon visuals are temporary original runtime placeholders generated by `WeaponVisualPlaceholderFactory`.
- Audio references are prepared per weapon but not populated with final sound assets.
- VFX references are prepared per weapon but still rely on existing runtime muzzle/tracer/impact systems until final authored prefabs are assigned.
- Milestone 24C generated audio and particles are original temporary placeholders, not final production gun sounds or impact effects.
- The active live match still uses the stable legacy `WeaponController`; the modular 24B loadout is initialized passively to avoid input regressions during this checkpoint.
