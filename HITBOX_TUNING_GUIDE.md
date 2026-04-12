# Hornet Hitbox Tuning Guide

## Overview
This guide shows you where to hook into the attack system to modify Hornet's hitbox, damage, range, and knockback.

## Attack System Architecture

### 1. **Attack Entry Point** (Already Hooked in CrestManager)
Located in `HeroController.Attack()` via the `On.HeroController.Attack` hook:

```csharp
// CrestManager.cs - OnAttack() method
private void OnAttack(On.HeroController.orig_Attack orig, HeroController self, AttackDirection dir)
{
    if (!HornetSpriteDriver.IsEnabled) { orig(self, dir); return; }
    
    switch (CurrentCrest) 
    {
        case CrestType.Hunter:
            ActivateHunter(self, dir);  // ← You intercept attacks here
            break;
    }
}
```

### 2. **Slash Damager Assignment** (In HeroController.Attack)
When an attack is triggered, HK assigns: `this.currentSlashDamager` (the hitbox component)

```csharp
// Silksong HeroController.Attack() pseudo-code:
switch (attackDir)
{
    case AttackDirection.normal:
        if (this.cState.altAttack)
        {
            this.SlashComponent = this.alternateSlash;
            this.currentSlashDamager = this.alternateSlashDamager;  // ← Alt slash hitbox
        }
        else
        {
            this.SlashComponent = this.normalSlash;
            this.currentSlashDamager = this.normalSlashDamager;     // ← Grounded slash hitbox
        }
        break;
    case AttackDirection.upward:
        this.SlashComponent = this.upSlash;
        this.currentSlashDamager = this.upSlashDamager;             // ← Up slash hitbox
        break;
    // ...
}
```

**Key components:**
- `SlashComponent` (NailSlash type) - controls animation timing and strike duration
- `currentSlashDamager` - controls collision box and damage values
- Both get assigned during `Attack()` call and activated during `SlashComponent.StartSlash()`

---

## HitInstance Structure (Damage Data)

Every hit uses this struct (from `HitInstance.cs`):

```csharp
public struct HitInstance
{
    public GameObject Source;              // Who dealt damage (Knight/Hornet)
    public AttackTypes AttackType;         // Nail, Spell, Acid, Hazard, etc.
    public bool CircleDirection;           // If true: direction from source→target
    public int DamageDealt;                // Base damage (before multiplier)
    public float Direction;                // Knockback angle (0=right, 90=up, 270=down)
    public float MagnitudeMultiplier;      // Knockback force scale
    public float MoveAngle;                // Victim movement direction
    public float Multiplier;               // Damage multiplier (nail upgrades use this)
    public SpecialTypes SpecialType;       // Dream Nail, extra particles, etc.
}
```

---

## Tuning Points (Where You Modify Behavior)

### **Option A: Hook the Attack Before Calling orig()**

```csharp
private void ActivateHunter(HeroController self, AttackDirection dir)
{
    // Before calling the original attack method:
    // You could modify the slash damager here
    
    // Get current slash damager (if accessible via reflection or public property)
    // var damager = self.GetComponent<SomeHitboxComponent>();
    
    // MODIFY HERE:
    // - damager.scale (increases hitbox size)
    // - damager.offset (moves hitbox position)
    // - damager.damage (increases or decreases hit damage)
    
    // Then call original attack
    bool doUnsubscribe = true;
    if (doUnsubscribe) UnsubscribeHook();
    orig(self, dir);
    SubscribeHook();
}
```

### **Option B: Hook HealthManager.Hit() (Universal Damage Control)**

```csharp
// Already implemented in CrestManager.cs:
private void OnHealthHit(On.HealthManager.orig_Hit orig, 
                         HealthManager self, 
                         HitInstance hitInstance)
{
    if (!HornetSpriteDriver.IsEnabled || hitInstance.AttackType != AttackTypes.Nail)
    {
        orig(self, hitInstance);
        return;
    }

    // Apply crest-specific damage modifications
    switch (CurrentCrest)
    {
        case CrestType.Hunter:
            hitInstance.Multiplier *= 1.0f;  // ← MODIFY THIS VALUE
            hitInstance.MagnitudeMultiplier *= 1.0f;  // ← AND THIS
            break;
        // ... other crests
    }

    orig(self, hitInstance);
}
```

This hook intercepts ALL hits, not just from Hero.

---

## Practical Tuning Examples

### **Increase Slash Damage by 25%**
```csharp
case CrestType.Hunter:
    hitInstance.Multiplier *= 1.25f;  // 1.25x damage
    break;
```

### **Decrease Damage to 80%**
```csharp
case CrestType.Hunter:
    hitInstance.Multiplier *= 0.8f;  // 20% reduction
    break;
```

### **Increase Knockback by 50%**
```csharp
case CrestType.Hunter:
    hitInstance.MagnitudeMultiplier *= 1.5f;  // 50% more knockback force
    break;
```

### **Change Knockback Direction (Down instead of forward)**
```csharp
case CrestType.Hunter:
    hitInstance.Direction = 270f;  // 0=right, 90=up, 180=left, 270=down
    break;
```

---

## Implementation Steps

1. **Open CrestManager.cs**

2. **Find the `OnHealthHit()` method** (around line 200)

3. **Modify the Hunter crest case:**
   ```csharp
   case CrestType.Hunter:
       hitInstance.Multiplier *= 1.25f;  // Increase to 1.25 for +25% damage
       hitInstance.MagnitudeMultiplier *= 1.0f;  // Keep 1.0 for normal knockback
       break;
   ```

4. **Rebuild:**
   ```bash
   dotnet build MyHornetMod.csproj -c Debug
   ```

5. **Test in Hollow Knight and iterate** - adjust multiplier values until combat feels right

---

## Common Multiplier Values

| Multiplier | Effect |
|-----------|--------|
| 0.5 | Half damage |
| 0.8 | -20% damage |
| 1.0 | Normal damage (baseline) |
| 1.25 | +25% damage |
| 1.5 | +50% damage |
| 2.0 | Double damage |

---

## Reference Material

- **Attack Methods:** `HeroController.Attack()`, `HealthManager.Hit()`
- **Damage Types:** `AttackTypes.Nail`, `AttackTypes.Spell`, `AttackTypes.Hazard`
- **Special Effects:** `SpecialTypes.DreamNail`, `SpecialTypes.Extra`
- **Direction Values:** 
  - 0° = right
  - 90° = up
  - 180° = left
  - 270° = down

---

## Files to Modify

- **CrestManager.cs** - The `OnHealthHit()` method (already exists, just change multiplier values)
- That's it! No other files need changes.

---

## Questions?

Check the Reference Scripts for full implementations:
- `/Resources/Reference_Scripts/Silksong_Scripts/HeroController.txt`
- `/Resources/Reference_Scripts/HK_Scripts/HitInstance.txt`
