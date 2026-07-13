# BattleZone Mobile Inventory System

Milestone 24D adds the first complete local loot, inventory, armor, healing, and attachment layer while preserving the current stable movement and combat systems.

## Runtime Architecture

- `InventoryItemData`: ScriptableObject data reference for ground loot identity, rarity, quantity, backpack cost, tier, and optional attachment reference.
- `WeaponAttachmentData`: ScriptableObject data reference for attachment slot, rarity, compatibility, backpack cost, and weapon modifiers.
- `LootItem`: world pickup component. It now carries `InventoryItemData`, tier, quantity, rarity, and optional `WeaponAttachmentData`.
- `LootPickupInteractor`: focused pickup scanner and desktop/mobile pickup control. It only consumes a pickup after inventory accepts it.
- `PlayerInventory`: owns backpack capacity, ammo stacks, healing stacks, throwables, stored attachments, timed item use, and quick actions.
- `PlayerEquipment`: owns armor/helmet tiers and durability reporting.
- `WeaponController`: keeps the stable fire/reload/recoil path and applies attachment modifiers through `WeaponAttachmentProfile`.
- `InventoryInputBridge`: desktop input layer for `I`, `Escape`, and right click while inventory is open.

## Supported Slots

- Primary weapon
- Secondary weapon
- Pistol
- Throwable
- Backpack
- Helmet
- Vest
- Healing items
- Ammo stacks
- Attachment storage

The active live-match weapon slots are still backed by the existing stable `WeaponController`. The modular weapon loadout remains initialized but passive for this checkpoint.

## Backpack Capacity

Current runtime tiers:

| Tier | Capacity |
| --- | ---: |
| None | 90 |
| Tier 1 | 120 |
| Tier 2 | 180 |
| Tier 3 | 240 |

Items with backpack cost cannot exceed capacity. Rejected pickups remain on the ground.

## Armor and Helmet

Vest tiers map to the existing armor bar:

| Tier | Max Armor |
| --- | ---: |
| 0 | 0 |
| 1 | 50 |
| 2 | 75 |
| 3 | 100 |

Helmet tiers reduce headshot damage and lose durability when headshot mitigation is applied. Lower-tier replacements are rejected so better gear is not accidentally overwritten.

## Healing

Healing items are timed:

| Item | Time | Effect |
| --- | ---: | --- |
| Medkit | 5.8s | Heals up to full health |
| Bandage | 2.8s | Heals up to 75% health |
| Energy Item | 3.5s | Heals up to full health |

Only one healing item can be used at a time. Movement cancellation is configurable in `PlayerInventory`.

## Attachments

Attachment slots:

- Optic: Red Dot, Holo, 2x, 4x, 6x, 8x
- Muzzle: Compensator, Flash Hider, Suppressor
- Magazine: Extended, Quickdraw, Extended Quickdraw
- Grip: Vertical Grip, Angled Grip, Lightweight Grip
- Other: Stock, Laser

Compatibility is data-driven per `WeaponAttachmentData`. Effects are applied through:

- recoil multiplier
- spread multiplier
- reload multiplier
- magazine multiplier
- fire-rate multiplier
- suppressed-fire audio flag

Replacing an attachment returns the old attachment to backpack storage when capacity allows. If there is no room to store the replaced attachment, the equip action is rejected. `WeaponController.TryDetachAttachment` is available for future UI-specific detach buttons.

## Controls

Desktop:

- `E` or `F`: pick up highlighted item
- `I`: open/close inventory
- `Escape`: close inventory
- Right click while inventory is open: equip the first compatible attachment, otherwise use the best healing item

Mobile:

- `PICKUP`: pick up highlighted item
- `PACK`: open inventory
- `USE`: use best healing item
- `EQUIP`: equip first compatible stored attachment
- `DROP`: discard one low-priority item from inventory

## Test Area

`M24D Loot Inventory Test Area` is generated in Editor/development builds. It contains labeled pickups for:

- Backpack tiers 1-3
- Helmet tiers 1-3
- Vest tiers 1-3
- Medkit, bandage, energy item
- Light, medium, heavy, and shell ammo
- Grenade and smoke grenade
- Every Milestone 24D attachment

## Placeholder Assets

The system uses original runtime placeholder meshes and labels. Final authored icons, pickup models, drag-and-drop item tiles, throwable gameplay, healing audio, and attachment visuals still need professional asset passes.
