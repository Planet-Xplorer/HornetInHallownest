using UnityEngine;

namespace HornetInHallownest
{
    // Manages all Hornet-specific movement changes when Hornet is active (F5 toggle).
    //
    // LOCKED OUT (Knight-only moves):
    //   Shadow Dash     — PlayerData.hasShadowDash suppressed while Hornet active;
    //                     HK naturally falls back to normal Dash without it
    //   Crystal Dash    — CanSuperDash always returns false
    //   Dreamnail       — TODO: requires PlayMaker FSM hook (GameManager.UseAttack FSM)
    //   Dash Slash      — TODO: detect charge+dash input combo and suppress
    //   Spinning Slash  — TODO: detect charge+up/down combo and suppress
    //
    // GRANTED (Hornet has these regardless of PlayerData state):
    //   Double Jump     — CanDoubleJump always returns true
    //
    // STUBBED (Hornet-new, Phase 1+):
    //   Sprint          — hold [shift] on ground → 2× run speed
    //   Clawline        — throw needle, zip to impact point
    //   Silk Soar       — consume silk, upward air-dash
    //
    // NOTE — Hornet's full moveset is far more nuanced than what's modelled here in these stubs.
    // Silksong's HornetController contains dozens of tuned physics parameters: I can't speak on the specifics accurately since I am not team cherry and i can't see their damn comments in the code that i already spend days trying to find 
    // The substitutions above are a minimal scaffold — they preserve functional
    // parity (Hornet can double-jump, can't crystal/shadow-dash) without yet capturing the feel of playing as Hornet. That work belongs in Phase 1+,
    // guided by HornetController.FixedUpdate and HornetController.Update in the
    // Silksong DLL (see Resources/Reference_Scripts/Silksong_Scripts/ when populated). At least I assume these are the proper DLLs, in all honesty I have no goddamn idea if this is what i should be looking at! Maybe it's a different DLL altogether! Hell if I know! It's so hard to talk to someone who knows what they're talking about!
    public class MovementManager : MonoBehaviour
    {
        public static MovementManager Instance { get; private set; }

        private bool _prevIsHornet;
        private bool _savedShadowDash;

        void Awake()
        {
            Instance = this;
        }

        void OnEnable()
        {
            On.HeroController.CanDoubleJump += OnCanDoubleJump;
            On.HeroController.CanSuperDash  += OnCanSuperDash;
            _prevIsHornet = HornetSpriteDriver.IsEnabled;
            ApplyShadowDashSuppression(_prevIsHornet);
        }

        void OnDisable()
        {
            On.HeroController.CanDoubleJump -= OnCanDoubleJump;
            On.HeroController.CanSuperDash  -= OnCanSuperDash;
            // Always restore PlayerData when this component is torn down
            ApplyShadowDashSuppression(false);
        }

        void Update()
        {
            bool isHornet = HornetSpriteDriver.IsEnabled;
            if (isHornet == _prevIsHornet) return;
            _prevIsHornet = isHornet;
            ApplyShadowDashSuppression(isHornet);
        }

        // Shadow Dash is a Knight-only upgrade. While Hornet is active, clear
        // PlayerData.hasShadowDash so HK's FSM falls through to normal Dash naturally.
        // Restore the original value when Hornet is toggled off.
        private void ApplyShadowDashSuppression(bool hornetActive)
        {
            var pd = PlayerData.instance;
            if (pd == null) return;

            if (hornetActive)
            {
                _savedShadowDash = pd.hasShadowDash;
                pd.hasShadowDash = false;
            }
            else
            {
                pd.hasShadowDash = _savedShadowDash;
            }
        }

        // Hornet always has double jump regardless of PlayerData.hasDoubleJump.
        private bool OnCanDoubleJump(On.HeroController.orig_CanDoubleJump orig, HeroController self)
        {
            if (!HornetSpriteDriver.IsEnabled) return orig(self);
            return true;
        }

        // Crystal Dash is a Knight-only ability. Lock it out entirely for Hornet.
        private bool OnCanSuperDash(On.HeroController.orig_CanSuperDash orig, HeroController self)
        {
            if (!HornetSpriteDriver.IsEnabled) return orig(self);
            return false;
        }

        // ── Phase 1+ stubs ─────────────────────────────────────────────────
        // TODO Sprint:     hold Left Shift while grounded → 2× run speed (see HornetController.FixedUpdate in Silksong DLL)
        // TODO Clawline:   dedicated input → spawn needle projectile, zip to first contact point
        // TODO Silk Soar:  dedicated input + SilkAmount > 0 → consume silk, upward air-dash
    }
}
