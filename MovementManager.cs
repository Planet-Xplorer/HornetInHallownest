using UnityEngine;
using System;
using System.Reflection;

namespace HornetInHallownest
{
    // Manages all Hornet-specific movement changes when Hornet is active (F5 toggle).
    //
    // LOCKED OUT (Knight-only moves):
    //   Shadow Dash     — PlayerData.hasShadowDash suppressed while Hornet active;
    //                     HK naturally falls back to normal Dash without it
    //   Crystal Dash    — same input as SuperDash, replaced with Hornet clawline when enabled
    //   Dreamnail       — blocked via CanDreamNail while Hornet active
    //   Dash Slash      — TODO: detect charge+dash input combo and suppress
    //   Spinning Slash  — TODO: detect charge+up/down combo and suppress
    //
    // GRANTED (Hornet has these regardless of PlayerData state):
    //   Double Jump     — CanDoubleJump always returns true
    //
    // STUBBED (Hornet-new, Phase 1+):
    //   Sprint          — hold [dash key] on ground → 2× run speed
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

        // Hornet movement parameters inspired by Silksong
        private float _originalRunSpeed;
        private float _originalWalkSpeed;
        private float _originalJumpSpeed;
        private bool _speedsModified;

        private object _originalDJumpWingsPrefab;
        private bool _visualsModified;

        private GameObject _clawlineProjectile;
        private Vector2 _clawlineDirection;
        private bool _isClawlining;
        private const float ClawlineMaxDistance = 8f;
        private const float ClawlineSpeed = 50f;

        void Awake()
        {
            Instance = this;
        }

        void OnEnable()
        {
            On.HeroController.CanDoubleJump += OnCanDoubleJump;
            On.HeroController.CanInfiniteAirJump += OnCanInfiniteAirJump;
            On.HeroController.CanSuperDash += OnCanSuperDash;
            On.HeroController.SuperDash += OnSuperDash;
            On.HeroController.CanDreamNail += OnCanDreamNail;

            _prevIsHornet = HornetSpriteDriver.IsEnabled;
            ApplyShadowDashSuppression(_prevIsHornet);
            ApplyHornetSpeeds(_prevIsHornet);
            ApplyHornetVisualOverrides(_prevIsHornet);
        }

        void OnDisable()
        {
            On.HeroController.CanDoubleJump -= OnCanDoubleJump;
            On.HeroController.CanInfiniteAirJump -= OnCanInfiniteAirJump;
            On.HeroController.CanSuperDash -= OnCanSuperDash;
            On.HeroController.SuperDash -= OnSuperDash;
            On.HeroController.CanDreamNail -= OnCanDreamNail;

            // Always restore PlayerData when this component is torn down
            ApplyShadowDashSuppression(false);
            ApplyHornetSpeeds(false);
            ApplyHornetVisualOverrides(false);
        }

        void Update()
        {
            bool isHornet = HornetSpriteDriver.IsEnabled;
            if (isHornet != _prevIsHornet)
            {
                _prevIsHornet = isHornet;
                ApplyShadowDashSuppression(isHornet);
                ApplyHornetSpeeds(isHornet);
                ApplyHornetVisualOverrides(isHornet);
            }

            if (!isHornet && _isClawlining)
            {
                EndClawline();
            }
        }

        void FixedUpdate()
        {
            // No movement logic needed since it's instant teleport
            // Keep checks for cleanup
            if (!_isClawlining) return;

            // Update projectile position if needed
            if (_clawlineProjectile != null)
            {
                _clawlineProjectile.transform.position = transform.position + (Vector3)(_clawlineDirection * 0.75f);
            }
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

        // Apply Hornet movement speeds inspired by Silksong logic
        private void ApplyHornetSpeeds(bool hornetActive)
        {
            var hc = GetComponent<HeroController>();
            if (hc == null) return;

            if (hornetActive && !_speedsModified)
            {
                _originalRunSpeed = hc.RUN_SPEED;
                _originalWalkSpeed = hc.WALK_SPEED;
                _originalJumpSpeed = hc.JUMP_SPEED;
                // Inspired by Silksong: adjust speeds for Hornet's movement feel
                hc.RUN_SPEED = 10f; // Example: faster run speed
                hc.WALK_SPEED = 6f; // Example: faster walk speed
                hc.JUMP_SPEED = 25f; // Example: higher jump
                _speedsModified = true;
            }
            else if (!hornetActive && _speedsModified)
            {
                hc.RUN_SPEED = _originalRunSpeed;
                hc.WALK_SPEED = _originalWalkSpeed;
                hc.JUMP_SPEED = _originalJumpSpeed;
                _speedsModified = false;
            }
        }

        private void ApplyHornetVisualOverrides(bool hornetActive)
        {
            var hc = GetComponent<HeroController>();
            if (hc == null) return;

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            var field = typeof(HeroController).GetField("dJumpWingsPrefab", flags);
            if (hornetActive && !_visualsModified)
            {
                if (field != null)
                {
                    _originalDJumpWingsPrefab = field.GetValue(hc);
                    field.SetValue(hc, null);
                }
                _visualsModified = true;
            }
            else if (!hornetActive && _visualsModified)
            {
                if (field != null)
                {
                    field.SetValue(hc, _originalDJumpWingsPrefab);
                }
                _visualsModified = false;
            }
        }

        // Hornet always has double jump regardless of PlayerData.hasDoubleJump.
        private bool OnCanDoubleJump(On.HeroController.orig_CanDoubleJump orig, HeroController self)
        {
            if (!HornetSpriteDriver.IsEnabled) return orig(self);
            return true;
        }

        private bool OnCanInfiniteAirJump(On.HeroController.orig_CanInfiniteAirJump orig, HeroController self)
        {
            if (!HornetSpriteDriver.IsEnabled) return orig(self);
            return false;
        }

        private bool OnCanDreamNail(On.HeroController.orig_CanDreamNail orig, HeroController self)
        {
            if (!HornetSpriteDriver.IsEnabled) return orig(self);
            return false;
        }

        private bool OnCanSuperDash(On.HeroController.orig_CanSuperDash orig, HeroController self)
        {
            if (!HornetSpriteDriver.IsEnabled) return orig(self);
            return true;
        }

        private void OnSuperDash(On.HeroController.orig_SuperDash orig, HeroController self)
        {
            if (!HornetSpriteDriver.IsEnabled)
            {
                orig(self);
                return;
            }

            try
            {
                PerformClawline(self);
            }
            catch (Exception e)
            {
                Modding.Logger.Log("[HornetInHallownest] Clawline error, falling back to normal SuperDash: " + e.Message);
                Modding.Logger.LogError(e);
                orig(self);
            }
        }

        private void PerformClawline(HeroController self)
        {
            if (self == null) return;

            var rb = self.GetComponent<Rigidbody2D>();
            if (rb == null) return;

            if (_isClawlining)
            {
                EndClawline();
            }

            _clawlineDirection = self.transform.localScale.x >= 0f ? Vector2.right : Vector2.left;
            var origin = (Vector2)self.transform.position;
            var hit = Physics2D.Raycast(origin, _clawlineDirection, ClawlineMaxDistance, ~0);
            if (hit.collider == null || hit.distance <= 0.1f)
            {
                EndClawline();
                return;
            }

            _isClawlining = true;
            _clawlineProjectile = CreateClawlineProjectile(hit.point);

            // Play throw animation to match FSM's "Throw" state
            var animCtrlField = typeof(HeroController).GetField("animCtrl", BindingFlags.Instance | BindingFlags.NonPublic);
            HeroAnimationController animCtrl = null;
            if (animCtrlField != null)
            {
                animCtrl = (HeroAnimationController)animCtrlField.GetValue(self);
                if (animCtrl != null)
                {
                    animCtrl.PlayClip("Slash");
                }
            }

            // Teleport to the hit point, matching the FSM's "To Needle" logic
            self.transform.position = hit.point;
            rb.velocity = Vector2.zero;

            // Play return animation to match FSM's retract/catch logic
            if (animCtrl != null)
            {
                animCtrl.PlayClip("Harpoon Needle Return");
            }

            // TODO: Implement enemy hit detection and damage from FSM's "Hit Enemy?" and "Do Enemy Damage"
            // For now, end immediately
            EndClawline();
        }

        private GameObject CreateClawlineProjectile(Vector2 position)
        {
            if (HornetSpriteDriver.FrameSprites == null) return null;
            if (!HornetSpriteDriver.FrameSprites.TryGetValue("176-00-1374", out var needleSprite)) return null;

            var go = new GameObject("HornetClawlineNeedle");
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = needleSprite;
            renderer.sortingOrder = 100;
            go.transform.position = position;
            return go;
        }

        private void EndClawline()
        {
            _isClawlining = false;
            if (_clawlineProjectile != null)
            {
                UnityEngine.Object.Destroy(_clawlineProjectile);
                _clawlineProjectile = null;
            }
        }

        // ── Phase 1+ stubs ─────────────────────────────────────────────────
        // TODO Sprint:     hold Left Shift while grounded → 2× run speed (see HornetController.FixedUpdate in Silksong DLL)
        // TODO Clawline:   dedicated input → spawn needle projectile, zip to first contact point
        // TODO Silk Soar:  dedicated input + SilkAmount > 0 → consume silk, upward air-dash
    }
}
