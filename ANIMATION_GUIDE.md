# Hornet Animation System Guide

## Animation Storage Locations

### Primary Animation Storage
**File:** `SpriteSwapper.cs` - `HornetSpriteDriver` class
- `HornetSpriteDriver.FrameSprites` - Dictionary of individual sprite frames
- `HornetSpriteDriver.FrameAnimSprites` - Dictionary of animation sequences

### HUD Animation Management
**File:** `HudSpriteManager.cs`
- Crest display animations (lines 227-363)
- Silk spool animations (lines 390-401)
- Mask overlay animations (lines 101-116)

### Silksong HUD System
**File:** `SilksongHUDManager.cs`
- Complete HUD replacement system
- Crest switching with animations (lines 294-356)
- Silk level animations (lines 185-207)

## Animation Key Formats

### Sprite Frame Keys
Format: `{anim_number}-{frame_index}-{sprite_id}`
Example: `001-00-873` (Idle animation, frame 0, sprite ID 873)

### Animation Names
- **Idle:** `001-00-873` through `001-05-879`
- **Run:** `005-00-1162` through `005-09-1169`
- **Dash:** `002-00-953` through `002-08-954`
- **Slash:** `006-00-901` through `006-04-901`

### Crest Sprite Keys
- `Hunter Crest Icon v3`
- `Reaper Crest Icon`
- `Wanderer Crest Icon`
- `Warrior Crest Icon`
- `Witch Crest Icon`
- `Toolmaster Crest Icon`
- `Spinner Crest Icon`
- `Cloakless Crest Icon`
- `Cursed Crest Icon`

### Silk Level Keys
- `Spool Empty`
- `Spool Low`
- `Spool Medium`
- `Spool Full`

## Common Animation Tasks

### Adding New Crest Animation
```csharp
// Find the crest display system
var hudManager = FindObjectOfType<SilksongHUDManager>();
if (hudManager != null)
{
    // Switch crest with animation
    hudManager.SwitchToCrest(CrestType.Warrior);
}
```

### Modifying Silk Spool Display
```csharp
// Update silk amount
CrestManager.SilkAmount = 50f;
CrestManager.SilkMax = 100f;

// Refresh display
var hudManager = FindObjectOfType<SilksongHUDManager>();
hudManager?.RefreshSilkDisplay();
```

### Changing Character Animation
```csharp
// Access animation mapping
var animMap = HornetSpriteDriver.AnimMap;

// Find specific animation
if (animMap.TryGetValue("Run", out var frames))
{
    // Use first frame
    var frameKey = frames[0];
    if (HornetSpriteDriver.FrameSprites.TryGetValue(frameKey, out var sprite))
    {
        // Apply sprite to renderer
        var overlay = FindObjectOfType<SpriteRenderer>();
        if (overlay != null) overlay.sprite = sprite;
    }
}
```

## Animation Loading Process

### Resource Loading (MyFirstMod.cs)
1. Scans assembly resources for `.png` files
2. Parses resource names to extract animation info
3. Creates `Sprite` objects with 64 PPU
4. Populates `FrameSprites` and `FrameAnimSprites` dictionaries

### Resource Name Format
`HornetInHallownest.Resources.Every_Hornet_Animation.<Folder>.<AnimName>.<NNN-FF-ID>.png`

## Safety Features Built Into System

### Error Recovery
- Null checks throughout animation system
- Division by zero protection
- Component cleanup on destroy/disable

### Component Cleanup
- Coroutine cleanup on destroy/disable
- Proper component removal on scene changes
- Memory leak prevention

## Animation System Architecture

### Core Components
1. **HornetSpriteDriver** - Main character animation controller
2. **HudSpriteManager** - HUD overlay animations
3. **SilksongHUDManager** - Complete HUD system
4. **CrestManager** - Crest state management

### Data Flow
1. Resources loaded into dictionaries
2. Animation controller maps HK clips to Hornet sprites
3. HUD systems display crest/silk animations

## Troubleshooting

### Missing Sprites
- Check resource names match expected format
- Verify sprites are loaded into `FrameSprites`
- Use `LogMissing` method in `HudSpriteManager`

### Animation Not Playing
- Verify animation exists in `AnimMap`
- Check `HornetSpriteDriver.IsEnabled`
- Ensure sprite renderer is active

### Crest Display Issues
- Check crest sprite keys in `GetCrestSpriteKey`
- Verify `CrestManager.CurrentCrest`
- Use `SwitchToCrest` for forced updates

## Quick Reference Commands

### HUD Commands
```csharp
FindObjectOfType<SilksongHUDManager>()?.SwitchToCrest(CrestType.Reaper);
FindObjectOfType<SilksongHUDManager>()?.RefreshSilkDisplay();
```

### Animation Queries
```csharp
// Check if animation exists
bool hasAnim = HornetSpriteDriver.AnimMap.ContainsKey("Run");

// Get sprite count
int spriteCount = HornetSpriteDriver.FrameSprites?.Count ?? 0;

// Get animation count
int animCount = HornetSpriteDriver.FrameAnimSprites?.Count ?? 0;
```

This guide shows you exactly where animations are stored and how to safely manipulate them.