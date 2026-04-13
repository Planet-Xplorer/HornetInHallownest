# Hornet In Hallownest - Controls Guide

## Mod Controls

### Character Toggle
- **F5** - Toggle between Hornet and Knight sprites
  - Switches character appearance and mechanics
  - Enables Hornet-specific movement abilities

### Hornet Movement Mechanics
- **Hold Dash (while grounded)** - Sprint at 2× speed
  - Uses unique sprint animation from Knight folder
  - Speed multiplier: 2.0f base run speed
  - Smooth buffer system for transitions

- **Double Jump** - Always available (no charms needed)
  - Hornet can double jump regardless of equipment
  - Uses dedicated double jump animation frames

- **Crystal Dash (Super Dash input)** - Clawline attack
  - Replaces crystal dash with needle throw and zip
  - Uses harpoon animations: Harpoon Throw, Harpoon Needle, Harpoon Needle Return
  - Max range: 8 units, 0.16s cooldown
  - Creates visual needle projectile at impact point

### Crest Switching
- **G** - Cycle through crests in sequential order
  - Hunter (default) -> Wanderer -> Warrior -> Reaper -> Spinner -> Toolmaster -> Witch -> Hunter
  - Triggers disappear/appear animation sequence
  - Changes both HUD display and attack animations

### Crest Sequence Order
1. **Hunter** - Balanced dagger attacks
2. **Wanderer** - Fast dagger attacks  
3. **Warrior** (Beast) - Heavy attacks
4. **Reaper** - Scythe attacks
5. **Spinner** (Shaman) - Spinning attacks
6. **Toolmaster** (Architect) - Drill lance attacks
7. **Witch** - Whip attacks

## Animation Systems

### HUD Animations
- Crest switching uses HUD frame animations from `HUD Anim` folder
- Each crest has unique visual display
- Disappear/appear transitions when switching

### Attack Animations  
- Combat uses weapon-specific animations from `CrestWeapon` folders
- Each crest type has distinct attack patterns
- Animations match weapon type (dagger, scythe, whip, etc.)

## Visual Feedback

### Crest Switch Animation
- **Phase 1**: Current crest fades out and shrinks (0.3s)
- **Phase 2**: Crest data switches to next in sequence  
- **Phase 3**: New crest fades in with elastic bounce (0.4s)

### HUD Display
- Crest icons appear in top-left position
- Silk spool display updates based on crest type
- Mask displays show current mask count

## Notes
- Cloakless and Cursed crests are skipped in the cycling sequence
- All controls work only when Hornet sprite is enabled
- Animation system prevents sprite conflicts between HUD and combat
