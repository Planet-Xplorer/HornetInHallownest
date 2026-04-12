using UnityEngine;
using System;
using System.Reflection;
using GlobalEnums;

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
    // IMPLEMENTED (Hornet-specific):
    //   Sprint          — hold [dash key] on ground → 2× run speed
    //   Clawline        — throw needle, zip to impact point (replaces crystal dash)
    //   Silk Soar       — consume silk, upward air-dash (TODO)
    //
    // FSM Integration:
    //   Sprint uses "TRY SPRINT" event from hornet_sprint FSM
    //   Clawline uses "DO MOVE" event from hornet_harpoon_dash FSM
    //   Double jump tracking matches Silksong behavior
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

        // Hornet movement parameters from Silksong FSM analysis
        private float _originalRunSpeed;
        private float _originalWalkSpeed;
        private float _originalJumpSpeed;
        private bool _speedsModified;

        private object _originalDJumpWingsPrefab;
        private bool _visualsModified;

        // Sprint system (from hornet_sprint FSM)
        private bool _isSprinting;
        private bool _sprintBufferActive;
        private float _sprintBufferTimer;
        private const float SPRINT_BUFFER_TIME = 0.2f;
        private const float SPRINT_SPEED_MULTIPLIER = 2.0f;

        // Clawline system (from hornet_harpoon_dash FSM)
        private GameObject _clawlineProjectile;
        private Vector2 _clawlineDirection;
        private bool _isClawlining;
        private float _harpoonDashCooldown;
        private const float HARPOON_COOLDOWN = 0.16f;
        private const float ClawlineMaxDistance = 8f;
        private bool _harpoonQueuing;
        private int _harpoonQueueSteps;
        private const int HARPOON_QUEUE_STEPS = 8;

        // Double jump tracking (matches Silksong behavior)
        private bool _hasDoubleJumped;
        private bool _doubleJumpQueuing;

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
            On.HeroController.Update += OnHeroUpdate;
            On.HeroController.FixedUpdate += OnHeroFixedUpdate;

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
            On.HeroController.Update -= OnHeroUpdate;
            On.HeroController.FixedUpdate -= OnHeroFixedUpdate;

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
                _hasDoubleJumped = false;
                _isSprinting = false;
                _sprintBufferActive = false;
                // Reset harpoon queuing when Hornet state changes
                _harpoonQueuing = false;
                _harpoonQueueSteps = 0;
            }

            if (!isHornet)
            {
                if (_isClawlining) EndClawline();
                if (_isSprinting) EndSprint();
                // Reset harpoon queuing when Hornet is disabled
                _harpoonQueuing = false;
                _harpoonQueueSteps = 0;
            }
        }

        void FixedUpdate()
        {
            var hc = GetComponent<HeroController>();
            if (hc == null) return;

            bool isHornet = HornetSpriteDriver.IsEnabled;
            if (!isHornet) return;

            // Update harpoon cooldown
            if (_harpoonDashCooldown > 0f)
            {
                if (hc.cState.onGround)
                {
                    _harpoonDashCooldown = 0f;
                }
                else
                {
                    _harpoonDashCooldown -= Time.deltaTime;
                }
            }

            // Update clawline projectile with null check
            if (_isClawlining && _clawlineProjectile != null)
            {
                // Additional safety check in case projectile was destroyed externally
                if (_clawlineProjectile != null)
                {
                    _clawlineProjectile.transform.position = transform.position + (Vector3)(_clawlineDirection * 0.75f);
                }
                else
                {
                    _isClawlining = false;
                }
            }

            // Reset double jump when grounded
            if (hc.cState.onGround)
            {
                _hasDoubleJumped = false;
                _doubleJumpQueuing = false;
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

        // Apply Hornet movement speeds from Silksong parameters
        private void ApplyHornetSpeeds(bool hornetActive)
        {
            var hc = GetComponent<HeroController>();
            if (hc == null) return;

            if (hornetActive && !_speedsModified)
            {
                _originalRunSpeed = hc.RUN_SPEED;
                _originalWalkSpeed = hc.WALK_SPEED;
                _originalJumpSpeed = hc.JUMP_SPEED;
                // Silksong base speeds (before sprint multiplier)
                hc.RUN_SPEED = 8.5f; // Hornet's base run speed
                hc.WALK_SPEED = 5.5f; // Hornet's base walk speed  
                hc.JUMP_SPEED = 22f; // Hornet's jump height
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
        // Matches Silksong behavior with proper queuing.
        private bool OnCanDoubleJump(On.HeroController.orig_CanDoubleJump orig, HeroController self)
        {
            if (!HornetSpriteDriver.IsEnabled) return orig(self);
            
            // Allow double jump if not used yet
            if (!_hasDoubleJumped)
            {
                _hasDoubleJumped = true;
                return true;
            }
            
            return false;
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

            // Handle harpoon queuing (from FSM logic)
            if (InputHandler.Instance.inputActions.SuperDash.IsPressed)
            {
                if (CanHarpoonDash())
                {
                    PerformClawline(self);
                    _harpoonQueuing = false;
                    _harpoonQueueSteps = 0;
                }
                else if (_harpoonQueueSteps <= HARPOON_QUEUE_STEPS)
                {
                    _harpoonQueuing = true;
                    _harpoonQueueSteps++;
                }
            }
            else
            {
                _harpoonQueuing = false;
                _harpoonQueueSteps = 0;
            }
        }

        private void PerformClawline(HeroController self)
        {
            if (self == null || !CanHarpoonDash()) return;

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
            StartHarpoonDashCooldown();

            // Play throw animation (from FSM "Throw" state)
            var animCtrlField = typeof(HeroController).GetField("animCtrl", BindingFlags.Instance | BindingFlags.NonPublic);
            HeroAnimationController animCtrl = null;
            if (animCtrlField != null)
            {
                animCtrl = (HeroAnimationController)animCtrlField.GetValue(self);
                if (animCtrl != null)
                {
                    animCtrl.PlayClip("Harpoon Throw");
                }
            }

            // Teleport to hit point (FSM "To Needle" logic)
            self.transform.position = hit.point;
            rb.velocity = Vector2.zero;

            // Play return animation (FSM retract logic)
            if (animCtrl != null)
            {
                animCtrl.PlayClip("Harpoon Needle Return");
            }

            // TODO: Implement enemy hit detection from FSM "Hit Enemy?" state
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

        // ── Hornet-specific Update/FixedUpdate hooks ──────────────────────────────────
        private void OnHeroUpdate(On.HeroController.orig_Update orig, HeroController self)
        {
            orig(self);
            
            if (!HornetSpriteDriver.IsEnabled) return;
            
            HandleSprintInput(self);
        }

        private void OnHeroFixedUpdate(On.HeroController.orig_FixedUpdate orig, HeroController self)
        {
            orig(self);
            
            if (!HornetSpriteDriver.IsEnabled) return;
            
            UpdateSprintState(self);
        }

        // ── Sprint Implementation (from hornet_sprint FSM) ────────────────────────
        private void HandleSprintInput(HeroController self)
        {
            if (self.cState == null) return;
            
            // Sprint input: hold dash while grounded
            if (InputHandler.Instance.inputActions.Dash.IsPressed && self.cState.onGround && !self.cState.dashing)
            {
                if (!_isSprinting)
                {
                    StartSprint();
                }
                _sprintBufferActive = true;
                _sprintBufferTimer = SPRINT_BUFFER_TIME;
            }
            else if (!InputHandler.Instance.inputActions.Dash.IsPressed)
            {
                if (_isSprinting)
                {
                    EndSprint();
                }
            }
        }

        private void UpdateSprintState(HeroController self)
        {
            // Update sprint buffer
            if (_sprintBufferActive)
            {
                _sprintBufferTimer -= Time.deltaTime;
                if (_sprintBufferTimer <= 0f)
                {
                    _sprintBufferActive = false;
                }
            }
        }

        private void StartSprint()
        {
            if (_isSprinting) return;
            
            var hc = GetComponent<HeroController>();
            if (hc == null) return;
            
            _isSprinting = true;
            hc.RUN_SPEED = _originalRunSpeed * SPRINT_SPEED_MULTIPLIER;
            
            // Play sprint animation
            var animCtrlField = typeof(HeroController).GetField("animCtrl", BindingFlags.Instance | BindingFlags.NonPublic);
            if (animCtrlField != null)
            {
                var animCtrl = (HeroAnimationController)animCtrlField.GetValue(hc);
                animCtrl?.PlayClip("Sprint");
            }
        }

        private void EndSprint()
        {
            if (!_isSprinting) return;
            
            var hc = GetComponent<HeroController>();
            if (hc == null) return;
            
            _isSprinting = false;
            hc.RUN_SPEED = _originalRunSpeed;
        }

        // ── Harpoon/Clawline Helper Methods ─────────────────────────────────────
        private bool CanHarpoonDash()
        {
            var hc = GetComponent<HeroController>();
            if (hc == null) 
            {
                // Reset queue if HeroController is missing
                _harpoonQueuing = false;
                _harpoonQueueSteps = 0;
                return false;
            }
            
            bool canDash = hc.hero_state != ActorStates.hard_landing &&
                          hc.hero_state != ActorStates.dash_landing &&
                          _harpoonDashCooldown <= 0f &&
                          (!hc.cState.dashing || hc.dash_timer <= 0f) &&
                          (!hc.cState.attacking || hc.attack_time >= hc.Config.AttackRecoveryTime) &&
                          !hc.cState.dead && !hc.cState.hazardDeath;
            
            // Reset queue if conditions change to prevent stuck queuing
            if (!canDash && _harpoonQueuing)
            {
                _harpoonQueuing = false;
                _harpoonQueueSteps = 0;
            }
            
            return canDash;
        }

        private void StartHarpoonDashCooldown()
        {
            _harpoonDashCooldown = HARPOON_COOLDOWN;
        }

        // ── Future Implementation ─────────────────────────────────────────────────
        // TODO Silk Soar:  dedicated input + SilkAmount > 0 → consume silk, upward air-dash
        // TODO Tool system integration with crest abilities
    }
}
