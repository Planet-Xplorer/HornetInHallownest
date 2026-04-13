using GlobalEnums;
using Modding;
using System.Collections.Generic;
using UnityEngine;

namespace HornetInHallownest
{
    // Mirrors GlobalEnums.CrestType from Silksong's Assembly-CSharp.dll exactly,
    // plus Cursed which is a custom addition based on the cursed-attack sprites.
    //
    // Internal code name  →  Display name in game
    // ─────────────────────────────────────────────
    // Hunter              →  Hunter's Crest       (default starting crest)
    // Wanderer            →  Wanderer's Crest
    // Warrior             →  Beast Crest          (bundle: crestbeast)
    // Reaper              →  Reaper Crest
    // Spinner             →  Shaman Crest         (bundle: crestshaman)
    // Toolmaster          →  Architect Crest      (bundle: crestarchitect)
    // Cloakless           →  Cloakless Crest      (special, active only in prison segment normally: can be chosen to be a choice along with other crests in config menu)
    // Cursed              →  Cursed Crest         (special, active only during cursed quest normally: just like cloakless, can be chosen to be a choice along with other crests in config menu)
    // Witch               →  Witch Crest
    public enum CrestType
    {
        Hunter,      // Hunter's Crest — default starting crest
        Reaper,      // Reaper Crest
        Wanderer,    // Wanderer's Crest
        Warrior,     // Beast Crest          (internal: Warrior, bundle: crestbeast)
        Witch,       // Witch Crest
        Toolmaster,  // Architect Crest      (internal: Toolmaster, bundle: crestarchitect)
        Spinner,     // Shaman Crest         (internal: Spinner, bundle: crestshaman)
        Cloakless,   // Cloakless Crest      (prison segment only)
        Cursed,      // Cursed Crest         (cursed quest only — Hornet_Cursed_attack_tendril sprites)
    }

    public class CrestManager : MonoBehaviour
    {
        public static CrestManager Instance { get; private set; }

        // Hunter is the default starting crest in Silksong.
        public static CrestType CurrentCrest = CrestType.Hunter;

        // Sequential crest order (skipping Cloakless and Cursed)
        private static readonly CrestType[] CrestSequence = new[]
        {
            CrestType.Hunter,
            CrestType.Wanderer,
            CrestType.Warrior,
            CrestType.Reaper,
            CrestType.Spinner,
            CrestType.Toolmaster,
            CrestType.Witch
        };

        // HUD animation mappings - using HUD Anim folder for display only
        private static readonly Dictionary<CrestType, CrestAnimationData> CrestHUDAnimations = new()
        {
            { CrestType.Hunter, new CrestAnimationData { SpriteKey = "HUD Frame Hunter v3", AnimationFolder = "HUD Anim", AnimationName = "HUD Frame Hunter v3" } },
            { CrestType.Wanderer, new CrestAnimationData { SpriteKey = "HUD Frame Wanderer", AnimationFolder = "HUD Anim", AnimationName = "HUD Frame Wanderer" } },
            { CrestType.Warrior, new CrestAnimationData { SpriteKey = "HUD Frame Warrior", AnimationFolder = "HUD Anim", AnimationName = "HUD Frame Warrior" } },
            { CrestType.Reaper, new CrestAnimationData { SpriteKey = "HUD Frame Reaper", AnimationFolder = "HUD Anim", AnimationName = "HUD Frame Reaper" } },
            { CrestType.Spinner, new CrestAnimationData { SpriteKey = "HUD Frame Shaman", AnimationFolder = "HUD Anim", AnimationName = "HUD Frame Shaman" } },
            { CrestType.Toolmaster, new CrestAnimationData { SpriteKey = "HUD Frame Toolmaster", AnimationFolder = "HUD Anim", AnimationName = "HUD Frame Toolmaster" } },
            { CrestType.Witch, new CrestAnimationData { SpriteKey = "HUD Frame FWitch", AnimationFolder = "HUD Anim", AnimationName = "HUD Frame FWitch" } }
        };

        // Attack animation mappings - using CrestWeapon folders for combat only
        private static readonly Dictionary<CrestType, CrestAnimationData> CrestAttackAnimations = new()
        {
            { CrestType.Hunter, new CrestAnimationData { SpriteKey = "Hunter Crest Icon v3", AnimationFolder = "Knight", AnimationName = "Hunter" } },
            { CrestType.Wanderer, new CrestAnimationData { SpriteKey = "Wanderer Crest Icon", AnimationFolder = "Hornet CrestWeapon Dagger Anim", AnimationName = "Dagger" } },
            { CrestType.Warrior, new CrestAnimationData { SpriteKey = "Warrior Crest Icon", AnimationFolder = "Hornet CrestWeapon Warrior Anim", AnimationName = "Warrior" } },
            { CrestType.Reaper, new CrestAnimationData { SpriteKey = "Reaper Crest Icon", AnimationFolder = "Hornet CrestWeapon Scythe Anim", AnimationName = "Scythe" } },
            { CrestType.Spinner, new CrestAnimationData { SpriteKey = "Spinner Crest Icon", AnimationFolder = "Hornet CrestWeapon Shaman Anim", AnimationName = "Shaman" } },
            { CrestType.Toolmaster, new CrestAnimationData { SpriteKey = "Toolmaster Crest Icon", AnimationFolder = "Hornet CrestWeapon Drill Lance Anim", AnimationName = "Drill Lance" } },
            { CrestType.Witch, new CrestAnimationData { SpriteKey = "Witch Crest Icon", AnimationFolder = "Hornet CrestWeapon Whip Anim", AnimationName = "Whip" } }
        };

        // Slash effect mappings - each crest has unique visual effects
        private static readonly Dictionary<CrestType, CrestEffectData> CrestSlashEffects = new()
        {
            { CrestType.Hunter, new CrestEffectData { 
                SlashEffect = "Knight/007.SlashEffect", 
                SlashEffectAlt = "Knight/008.SlashEffectAlt", 
                UpSlashEffect = "Knight/015.UpSlashEffect", 
                DownSlashEffect = "Knight/016.DownSlashEffect" 
            }},
            { CrestType.Wanderer, new CrestEffectData { 
                SlashEffect = "Hornet CrestWeapon Dagger Anim/SlashEffect", 
                SlashEffectAlt = "Hornet CrestWeapon Dagger Anim/SlashEffectAlt", 
                UpSlashEffect = "Hornet CrestWeapon Dagger Anim/UpSlashEffect", 
                DownSlashEffect = "Hornet CrestWeapon Dagger Anim/DownSlashEffect" 
            }},
            { CrestType.Warrior, new CrestEffectData { 
                SlashEffect = "Hornet CrestWeapon Warrior Anim/003.SlashEffect", 
                SlashEffectAlt = "Hornet CrestWeapon Warrior Anim/002.SlashEffectAlt", 
                UpSlashEffect = "Hornet CrestWeapon Warrior Anim/006.UpSlashEffect", 
                DownSlashEffect = "Hornet CrestWeapon Warrior Anim/001.DownSlashEffect" 
            }},
            { CrestType.Reaper, new CrestEffectData { 
                SlashEffect = "Hornet CrestWeapon Scythe Anim/SlashEffect", 
                SlashEffectAlt = "Hornet CrestWeapon Scythe Anim/SlashEffectAlt", 
                UpSlashEffect = "Hornet CrestWeapon Scythe Anim/UpSlashEffect", 
                DownSlashEffect = "Hornet CrestWeapon Scythe Anim/DownSlashEffect" 
            }},
            { CrestType.Spinner, new CrestEffectData { 
                SlashEffect = "Hornet CrestWeapon Shaman Anim/SlashEffect", 
                SlashEffectAlt = "Hornet CrestWeapon Shaman Anim/SlashEffectAlt", 
                UpSlashEffect = "Hornet CrestWeapon Shaman Anim/UpSlashEffect", 
                DownSlashEffect = "Hornet CrestWeapon Shaman Anim/DownSlashEffect" 
            }},
            { CrestType.Toolmaster, new CrestEffectData { 
                SlashEffect = "Hornet CrestWeapon Drill Lance Anim/SlashEffect", 
                SlashEffectAlt = "Hornet CrestWeapon Drill Lance Anim/SlashEffectAlt", 
                UpSlashEffect = "Hornet CrestWeapon Drill Lance Anim/UpSlashEffect", 
                DownSlashEffect = "Hornet CrestWeapon Drill Lance Anim/DownSlashEffect" 
            }},
            { CrestType.Witch, new CrestEffectData { 
                SlashEffect = "Hornet CrestWeapon Whip Anim/SlashEffect", 
                SlashEffectAlt = "Hornet CrestWeapon Whip Anim/SlashEffectAlt", 
                UpSlashEffect = "Hornet CrestWeapon Whip Anim/UpSlashEffect", 
                DownSlashEffect = "Hornet CrestWeapon Whip Anim/DownSlashEffect" 
            }}
        };

        public static KeyCode CrestSwitchKey = KeyCode.G;

        void Awake()
        {
            Instance = this;
        }

        void Update()
        {
            if (Input.GetKeyDown(CrestSwitchKey) && HornetSpriteDriver.IsEnabled)
            {
                SwitchToNextCrest();
            }
        }

        public static void SwitchToNextCrest()
        {
            int currentIndex = System.Array.IndexOf(CrestSequence, CurrentCrest);
            int nextIndex = (currentIndex + 1) % CrestSequence.Length;
            CrestType nextCrest = CrestSequence[nextIndex];
            
            // Trigger crest switch animation
            var hudManager = FindObjectOfType<SilksongHUDManager>();
            if (hudManager != null)
            {
                hudManager.SwitchToCrestWithAnimation(nextCrest);
            }
            else
            {
                HornetInHallownest.Instance?.Log("[CrestManager] SilksongHUDManager not found, using direct switch");
                // Fallback to direct switch
                CurrentCrest = nextCrest;
            }
        }

        public static CrestAnimationData GetCrestHUDAnimationData(CrestType crestType)
        {
            return CrestHUDAnimations.TryGetValue(crestType, out var data) ? data : null;
        }

        public static CrestAnimationData GetCrestAttackAnimationData(CrestType crestType)
        {
            return CrestAttackAnimations.TryGetValue(crestType, out var data) ? data : null;
        }

        void OnEnable()
        {
            On.HeroController.Attack += OnAttack;
            On.HealthManager.Hit += OnHealthHit;
        }

        void OnDisable()
        {
            On.HeroController.Attack -= OnAttack;
            On.HealthManager.Hit -= OnHealthHit;
        }

        // Intercept every attack. When Hornet is active, dispatch to the
        // active crest instead of letting the normal nail logic run.
        private void OnAttack(On.HeroController.orig_Attack orig, HeroController self, AttackDirection dir)
        {
            if (!HornetSpriteDriver.IsEnabled)
            {
                orig(self, dir);
                return;
            }

            switch (CurrentCrest)
            {
                case CrestType.Hunter:
                    ActivateHunter(self, dir);
                    break;
                case CrestType.Reaper:
                    ActivateReaper(self, dir);
                    break;
                case CrestType.Wanderer:
                    ActivateWanderer(self, dir);
                    break;
                case CrestType.Warrior:
                    ActivateWarrior(self, dir);
                    break;
                case CrestType.Witch:
                    ActivateWitch(self, dir);
                    break;
                case CrestType.Toolmaster:
                    ActivateToolmaster(self, dir);
                    break;
                case CrestType.Spinner:
                    ActivateSpinner(self, dir);
                    break;
                case CrestType.Cursed:
                    ActivateCursed(self, dir);
                    break;
                case CrestType.Cloakless:
                    orig(self, dir);
                    break;
                default:
                    orig(self, dir);
                    break;
            }
        }

        // ── Crest stubs ────────────────────────────────────────────────────
        // Each method will eventually trigger its visual effect, sound,
        // and gameplay modifier. For now they fall through to the base attack
        // so the game doesn't break.
        //
        // NOTE — Each crest in Silksong carries a rich, individual combat identity
        // that these stubs do not remotely capture. Hunter's Crest alone has timed
        // parry windows, follow-up cancel options, needle-combo extensions, and
        // separate damage tiers for grounded vs. airborne hits. Warrior/Beast,
        // Spinner/Shaman, and Toolmaster/Architect crests each introduce distinct
        // projectile behaviours, silk interactions, and cooldown rhythms defined in
        // Silksong's BasicSpriteAnimator and crest-specific FSMs. Cursed and Cloakless
        // have entirely separate animation rigs (see Hornet_Cursed_attack_tendril sprites).
        // Implementing any crest faithfully requires reading those scripts first —
        // paste the relevant Silksong decompilations into
        // Resources/Reference_Scripts/Silksong_Scripts/ before writing the actual logic.

        private void ActivateHunter(HeroController hc, AttackDirection dir)
        {
            // Hunter crest - balanced dagger attacks
            PlayCrestAttackAnimation(CrestType.Hunter);
            HornetInHallownest.Instance.Log("[CrestManager] Hunter crest triggered");
            
            try
            {
                On.HeroController.Attack -= OnAttack;
                hc.Attack(dir);
            }
            finally
            {
                On.HeroController.Attack += OnAttack;
            }
        }

        private void ActivateReaper(HeroController hc, AttackDirection dir)
        {
            // Reaper crest - scythe attacks
            PlayCrestAttackAnimation(CrestType.Reaper);
            HornetInHallownest.Instance.Log("[CrestManager] Reaper crest triggered");
            
            try
            {
                On.HeroController.Attack -= OnAttack;
                hc.Attack(dir);
            }
            finally
            {
                On.HeroController.Attack += OnAttack;
            }
        }

        private void ActivateWanderer(HeroController hc, AttackDirection dir)
        {
            // Wanderer crest - fast dagger attacks
            PlayCrestAttackAnimation(CrestType.Wanderer);
            HornetInHallownest.Instance.Log("[CrestManager] Wanderer crest triggered");
            
            try
            {
                On.HeroController.Attack -= OnAttack;
                hc.Attack(dir);
            }
            finally
            {
                On.HeroController.Attack += OnAttack;
            }
        }

        private void ActivateWarrior(HeroController hc, AttackDirection dir)
        {
            // Warrior/Beast crest - heavy attacks
            PlayCrestAttackAnimation(CrestType.Warrior);
            HornetInHallownest.Instance.Log("[CrestManager] Warrior crest triggered");
            
            try
            {
                On.HeroController.Attack -= OnAttack;
                hc.Attack(dir);
            }
            finally
            {
                On.HeroController.Attack += OnAttack;
            }
        }

        private void ActivateWitch(HeroController hc, AttackDirection dir)
        {
            // Witch crest - whip attacks
            PlayCrestAttackAnimation(CrestType.Witch);
            HornetInHallownest.Instance.Log("[CrestManager] Witch crest triggered");
            
            try
            {
                On.HeroController.Attack -= OnAttack;
                hc.Attack(dir);
            }
            finally
            {
                On.HeroController.Attack += OnAttack;
            }
        }

        private void ActivateToolmaster(HeroController hc, AttackDirection dir)
        {
            // Toolmaster/Architect crest - drill lance attacks
            PlayCrestAttackAnimation(CrestType.Toolmaster);
            HornetInHallownest.Instance.Log("[CrestManager] Toolmaster crest triggered");
            
            try
            {
                On.HeroController.Attack -= OnAttack;
                hc.Attack(dir);
            }
            finally
            {
                On.HeroController.Attack += OnAttack;
            }
        }

        private void ActivateSpinner(HeroController hc, AttackDirection dir)
        {
            // Spinner/Shaman crest - spinning attacks
            PlayCrestAttackAnimation(CrestType.Spinner);
            HornetInHallownest.Instance.Log("[CrestManager] Spinner crest triggered");
            
            try
            {
                On.HeroController.Attack -= OnAttack;
                hc.Attack(dir);
            }
            finally
            {
                On.HeroController.Attack += OnAttack;
            }
        }

        private void ActivateCursed(HeroController hc, AttackDirection dir)
        {
            // TODO: Cursed crest visual (Hornet_Cursed_attack_tendril sprites) + gameplay effect
            HornetInHallownest.Instance.Log("[CrestManager] Cursed crest triggered");
            
            try
            {
                On.HeroController.Attack -= OnAttack;
                hc.Attack(dir);
            }
            finally
            {
                On.HeroController.Attack += OnAttack;
            }
        }

        // ── Damage Modulation ──────────────────────────────────────────────
        // Intercept every hit on enemies to apply crest-specific damage multipliers,
        // knockback scaling, and special effects.
        //
        // Modify hitInstance fields BEFORE calling orig() to change:
        //   - Multiplier:         1.0 = normal, 1.25 = +25% damage
        //   - MagnitudeMultiplier: knockback force scaling (1.0 = normal)
        //   - Direction:          0 = right, 90 = up, 180 = left, 270 = down
        //   - DamageDealt:        base damage value (usually leaves this alone)
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
                    // Hunter's Crest — balanced baseline damage
                    hitInstance.Multiplier *= 1.0f;  // 100% of normal dagger damage
                    hitInstance.MagnitudeMultiplier *= 1.0f;  // Normal knockback
                    break;

                case CrestType.Reaper:
                    // TODO: Reaper Crest — higher damage, lower speed
                    break;

                case CrestType.Wanderer:
                    // TODO: Wanderer's Crest — pierce through enemies
                    break;

                case CrestType.Warrior:
                    // TODO: Beast Crest — massive damage, slow attacks
                    break;

                case CrestType.Witch:
                    // TODO: Witch Crest — projectile spawning
                    break;

                case CrestType.Toolmaster:
                    // TODO: Architect Crest — tool-based attacks
                    break;

                case CrestType.Spinner:
                    // TODO: Shaman Crest — spinning attack
                    break;

                case CrestType.Cursed:
                    // TODO: Cursed Crest — special tendril behavior
                    break;

                case CrestType.Cloakless:
                    // Cloakless: Hornet's base attack (no crest passive)
                    break;
            }

            orig(self, hitInstance);
        }

        private static void PlayCrestAttackAnimation(CrestType crestType)
        {
            var animData = GetCrestAttackAnimationData(crestType);
            if (animData == null) return;

            // Update the main sprite driver's animation mappings for this crest
            UpdateCrestAttackMappings(crestType);
            
            HornetInHallownest.Instance?.Log($"[CrestManager] Updated to {crestType} attack animations from {animData.AnimationFolder}");
        }

        private static void UpdateCrestAttackMappings(CrestType crestType)
        {
            var animData = GetCrestAttackAnimationData(crestType);
            if (animData == null) return;

            // Update the main animation map in SpriteSwapper to use crest-specific attacks
            // This ensures all subsequent attacks use the correct crest animations
            switch (crestType)
            {
                case CrestType.Hunter:
                    // Hunter uses standard Knight animations (already mapped in SpriteSwapper)
                    UpdateSlashEffects(CrestType.Hunter);
                    break;
                    
                case CrestType.Wanderer:
                    // Wanderer uses dagger animations - update slash mappings
                    UpdateSlashAnimations("Hornet CrestWeapon Dagger Anim/Dagger");
                    UpdateSlashEffects(CrestType.Wanderer);
                    break;
                    
                case CrestType.Warrior:
                    // Warrior uses heavy animations - update slash mappings
                    UpdateSlashAnimations("Hornet CrestWeapon Warrior Anim/Warrior");
                    UpdateSlashEffects(CrestType.Warrior);
                    break;
                    
                case CrestType.Reaper:
                    // Reaper uses scythe animations - update slash mappings
                    UpdateSlashAnimations("Hornet CrestWeapon Scythe Anim/Scythe");
                    UpdateSlashEffects(CrestType.Reaper);
                    break;
                    
                case CrestType.Spinner:
                    // Spinner uses shaman animations - update slash mappings
                    UpdateSlashAnimations("Hornet CrestWeapon Shaman Anim/Shaman");
                    UpdateSlashEffects(CrestType.Spinner);
                    break;
                    
                case CrestType.Toolmaster:
                    // Toolmaster uses drill lance animations - update slash mappings
                    UpdateSlashAnimations("Hornet CrestWeapon Drill Lance Anim/Drill Lance");
                    UpdateSlashEffects(CrestType.Toolmaster);
                    break;
                    
                case CrestType.Witch:
                    // Witch uses whip animations - update slash mappings
                    UpdateSlashAnimations("Hornet CrestWeapon Whip Anim/Whip");
                    UpdateSlashEffects(CrestType.Witch);
                    break;
            }
        }

        private static void UpdateSlashAnimations(string crestAnimationKey)
        {
            // TODO: map crest-specific FrameAnimSprites keys back to AnimMap string-key arrays
        }

        private static void UpdateSlashEffects(CrestType crestType)
        {
            // TODO: map crest-specific slash effect FrameAnimSprites keys back to AnimMap string-key arrays
        }

        public static void PlayCrestSlashEffect(string attackType)
        {
            var effectData = CrestSlashEffects.TryGetValue(CurrentCrest, out var data) ? data : null;
            if (effectData == null) return;

            string effectKey = attackType switch
            {
                "Slash" => effectData.SlashEffect,
                "SlashAlt" => effectData.SlashEffectAlt,
                "UpSlash" => effectData.UpSlashEffect,
                "DownSlash" => effectData.DownSlashEffect,
                _ => effectData.SlashEffect
            };

            if (HornetSpriteDriver.FrameAnimSprites.TryGetValue(effectKey, out var effectFrames) && effectFrames.Count > 0)
            {
                // Trigger the visual effect at the appropriate position
                // This would be called by the main attack system when hits occur
                HornetInHallownest.Instance?.Log($"[CrestManager] Playing {CurrentCrest} {attackType} effect: {effectKey}");
            }
        }
    }

    public class CrestAnimationData
    {
        public string SpriteKey { get; set; }
        public string AnimationFolder { get; set; }
        public string AnimationName { get; set; }
    }

    public class CrestEffectData
    {
        public string SlashEffect { get; set; }
        public string SlashEffectAlt { get; set; }
        public string UpSlashEffect { get; set; }
        public string DownSlashEffect { get; set; }
    }
}
