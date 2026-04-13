using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GlobalEnums;

namespace HornetInHallownest
{
    internal class HudSpriteManager : MonoBehaviour
    {
        private const float FrameDuration = 1f / 12f;
        private bool _initialized;
        private readonly List<AnimatedHudOverlay> _overlays = new List<AnimatedHudOverlay>();
        private readonly HashSet<string> _missingLogged = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Crest UI system (from Silksong HUD analysis)
        private CrestType _currentDisplayedCrest = CrestType.Hunter;
        private AnimatedHudOverlay _crestOverlay;
        private GameObject _crestDisplayContainer;
        private SpriteRenderer _crestIconRenderer;
        private tk2dSprite _crestIconTk2d;
        
        // Silk spool integration
        private AnimatedHudOverlay _silkSpoolOverlay;
        private Component _soulOrbTarget;
        
        // HUD frame management
        private Component _hudFrameTarget;
        private AnimatedHudOverlay _hudFrameOverlay;
        
        // Coroutine tracking for safe cleanup
        private Coroutine _crestSwitchCoroutine;

        void Start()
        {
            EnsureHudOverlays();
        }

        void Update()
        {
            if (!_initialized)
            {
                EnsureHudOverlays();
            }

            // Update all overlays
            foreach (var overlay in _overlays)
            {
                overlay.Tick(Time.deltaTime);
                overlay.Enabled = HornetSpriteDriver.IsEnabled;
            }
            
            // Update crest display if changed
            UpdateCrestDisplay();
            
            // Update silk spool display
            UpdateSilkSpoolDisplay();
        }

        private void EnsureHudOverlays()
        {
            if (_initialized) return;
            if (HornetSpriteDriver.FrameSprites == null || HornetSpriteDriver.FrameAnimSprites == null) return;

            // Setup HUD frame (Hunter frame animation)
            var frameAnim = FindAnimation("HUD Frame Hunter v2 Idle", "HUD Frame Hunter v3 Idle", "HUD Frame Hunter v2", "HUD Frame Hunter v3");
            _hudFrameTarget = FindTargetComponent(new[] { "HUD Frame", "HUDFrame", "HUD", "Hud" }, new[] { "frame" });
            
            if (_hudFrameTarget != null && frameAnim != null)
            {
                var overlay = CreateOverlay(_hudFrameTarget, "HornetHudFrameOverlay", frameAnim[0]);
                if (overlay != null)
                {
                    _hudFrameOverlay = new AnimatedHudOverlay(overlay, frameAnim);
                    _overlays.Add(_hudFrameOverlay);
                }
            }
            else
            {
                LogMissing("HUD frame hunter overlay");
            }

            // Setup silk spool (replaces soul orb)
            var spoolAnim = FindAnimation("Spool Appear", "Spool Extender Appear", "Spool Cursed Bob", "Spool Cursed");
            _soulOrbTarget = FindTargetComponent(new[] { "normalSoulOrb", "soulOrbIcon", "hardcoreSoulOrbCg", "boundSoulDisplay", "boundSoulButton" }, new[] { "soul" });
            
            if (_soulOrbTarget != null && spoolAnim != null)
            {
                var overlay = CreateOverlay(_soulOrbTarget, "HornetSilkSpoolOverlay", spoolAnim[0]);
                if (overlay != null)
                {
                    _silkSpoolOverlay = new AnimatedHudOverlay(overlay, spoolAnim);
                    _overlays.Add(_silkSpoolOverlay);
                }
            }
            else
            {
                LogMissing("silk spool overlay");
            }

            // Setup mask overlays (Hornet's mask system)
            var maskAnim = FindAnimation("Fractured Mask Appear", "Fractured Mask Shine", "Fractured Mask Disappear");
            var maskTargets = FindMaskTargets();
            
            if (maskAnim != null)
            {
                foreach (var target in maskTargets)
                {
                    var overlay = CreateOverlay(target, "HornetMaskOverlay", maskAnim[0]);
                    if (overlay != null) _overlays.Add(new AnimatedHudOverlay(overlay, maskAnim));
                }
            }
            else
            {
                LogMissing("mask overlays");
            }

            // Setup crest display system
            SetupCrestDisplay();

            _initialized = true;
        }

        private List<Sprite> FindAnimation(params string[] names)
        {
            foreach (var candidate in names)
            {
                var match = HornetSpriteDriver.FrameAnimSprites.FirstOrDefault(kv =>
                    kv.Key.IndexOf(candidate, StringComparison.OrdinalIgnoreCase) >= 0);
                if (match.Value != null && match.Value.Count > 0)
                {
                    return match.Value;
                }
            }

            return null;
        }

        private Component FindTargetComponent(string[] exactNames, string[] fallbackContains)
        {
            foreach (var name in exactNames)
            {
                var go = GameObject.Find(name);
                if (go == null) continue;
                var component = GetGraphicComponent(go);
                if (component != null) return component;
            }

            foreach (var sprite in FindObjectsOfType<tk2dSprite>())
            {
                var objectName = sprite.gameObject.name;
                if (exactNames.Any(n => objectName.IndexOf(n, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    fallbackContains.Any(c => objectName.IndexOf(c, StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    return sprite;
                }
            }

            foreach (var sr in FindObjectsOfType<SpriteRenderer>())
            {
                var objectName = sr.gameObject.name;
                if (exactNames.Any(n => objectName.IndexOf(n, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    fallbackContains.Any(c => objectName.IndexOf(c, StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    return sr;
                }
            }

            return null;
        }

        private IEnumerable<Component> FindMaskTargets()
        {
            foreach (var sprite in FindObjectsOfType<tk2dSprite>())
            {
                if (sprite.gameObject.name.IndexOf("mask", StringComparison.OrdinalIgnoreCase) >= 0)
                    yield return sprite;
            }

            foreach (var sr in FindObjectsOfType<SpriteRenderer>())
            {
                if (sr.gameObject.name.IndexOf("mask", StringComparison.OrdinalIgnoreCase) >= 0)
                    yield return sr;
            }
        }

        private Component GetGraphicComponent(GameObject go)
        {
            var tk2d = go.GetComponent<tk2dSprite>();
            if (tk2d != null) return tk2d;
            var sr = go.GetComponent<SpriteRenderer>();
            if (sr != null) return sr;
            return null;
        }

        private SpriteRenderer CreateOverlay(Component target, string overlayName, Sprite sprite)
        {
            if (target == null || sprite == null) return null;

            var parent = target.gameObject;
            var go = new GameObject(overlayName);
            go.transform.SetParent(parent.transform, false);
            go.transform.localPosition = Vector3.zero;

            var renderer = go.AddComponent<SpriteRenderer>();
            var targetRenderer = GetTargetRenderer(target);
            if (targetRenderer != null)
            {
                renderer.sortingLayerName = targetRenderer.sortingLayerName;
                renderer.sortingOrder = targetRenderer.sortingOrder + 1;
            }

            renderer.sprite = sprite;
            renderer.color = Color.white;
            return renderer;
        }

        private Renderer GetTargetRenderer(Component target)
        {
            if (target is tk2dSprite tk2d)
                return tk2d.GetComponent<Renderer>();
            if (target is SpriteRenderer sr)
                return sr;
            return target.GetComponent<Renderer>();
        }

        // ── Crest Display System (Silksong HUD integration) ──────────────────────
        private void SetupCrestDisplay()
        {
            // Find or create crest display container
            var crestContainer = GameObject.Find("Crest Display");
            if (crestContainer == null)
            {
                crestContainer = new GameObject("Crest Display");
                // Position it where Silksong's crest UI would be
                crestContainer.transform.position = new Vector3(-4f, 2.5f, 0f);
            }
            
            _crestDisplayContainer = crestContainer;
            
            // Create crest icon renderer
            _crestIconRenderer = _crestDisplayContainer.AddComponent<SpriteRenderer>();
            _crestIconRenderer.sortingLayerName = "UI";
            _crestIconRenderer.sortingOrder = 100;
            
            // Set initial crest
            UpdateCrestIcon();
        }
        
        private void UpdateCrestDisplay()
        {
            if (_crestDisplayContainer == null) return;
            
            // Only update when crest actually changes
            if (_currentDisplayedCrest != CrestManager.CurrentCrest)
            {
                _currentDisplayedCrest = CrestManager.CurrentCrest;
                UpdateCrestIcon();
                PlayCrestSwitchAnimation();
            }
            
            // Enable/disable based on Hornet state
            _crestDisplayContainer.SetActive(HornetSpriteDriver.IsEnabled);
        }
        
        private void UpdateCrestIcon()
        {
            if (_crestIconRenderer == null) return;
            
            var crestSpriteKey = GetCrestSpriteKey(_currentDisplayedCrest);
            if (HornetSpriteDriver.FrameSprites.TryGetValue(crestSpriteKey, out var crestSprite) && crestSprite != null)
            {
                _crestIconRenderer.sprite = crestSprite;
            }
            else
            {
                // Fallback to Hunter crest sprite with null check
                if (HornetSpriteDriver.FrameSprites.TryGetValue("Hunter Crest Icon", out crestSprite) && crestSprite != null)
                {
                    _crestIconRenderer.sprite = crestSprite;
                }
                else
                {
                    // Ultimate fallback - clear sprite if none available
                    _crestIconRenderer.sprite = null;
                }
            }
        }
        
        private string GetCrestSpriteKey(CrestType crestType)
        {
            var animData = CrestManager.GetCrestHUDAnimationData(crestType);
            return animData?.SpriteKey ?? "HUD Frame Hunter v3";
        }
        
        private void PlayCrestSwitchAnimation()
        {
            if (_crestIconRenderer == null) return;
            
            // Stop any existing crest switch animation
            if (_crestSwitchCoroutine != null)
            {
                StopCoroutine(_crestSwitchCoroutine);
                _crestSwitchCoroutine = null;
            }
            
            // Enhanced crest switch animation inspired by Silksong FSM
            _crestSwitchCoroutine = StartCoroutine(CrestSwitchAnimationSequence());
        }
        
        private System.Collections.IEnumerator CrestSwitchAnimationSequence()
        {
            if (_crestIconRenderer == null) yield break;
            
            Vector3 originalScale = _crestIconRenderer.transform.localScale;
            Color originalColor = _crestIconRenderer.color;
            
            // Phase 1: Fade out and scale down
            float elapsed = 0f;
            while (elapsed < 0.2f)
            {
                float t = elapsed / 0.2f;
                _crestIconRenderer.transform.localScale = Vector3.Lerp(originalScale, originalScale * 0.5f, t);
                _crestIconRenderer.color = Color.Lerp(originalColor, new Color(1f, 1f, 1f, 0f), t);
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            // Phase 2: Switch sprite (this happens during the fade)
            UpdateCrestIcon();
            
            // Phase 3: Scale up and fade in with bounce
            elapsed = 0f;
            while (elapsed < 0.3f)
            {
                float t = elapsed / 0.3f;
                
                // Bounce effect using ease-out-back
                float bounce = 1f + Mathf.Sin(t * Mathf.PI) * 0.3f * (1f - t);
                _crestIconRenderer.transform.localScale = originalScale * bounce;
                
                // Fade in
                _crestIconRenderer.color = Color.Lerp(new Color(1f, 1f, 1f, 0f), originalColor, t);
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            // Reset to final state
            _crestIconRenderer.transform.localScale = originalScale;
            _crestIconRenderer.color = originalColor;
        }
        
        // Legacy method kept for compatibility, but not used in new animation system
        private System.Collections.IEnumerator ScalePulse(Transform target, float duration)
        {
            Vector3 originalScale = target.localScale;
            Vector3 targetScale = originalScale * 1.2f;
            
            float elapsed = 0f;
            while (elapsed < duration)
            {
                target.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            elapsed = 0f;
            while (elapsed < duration)
            {
                target.localScale = Vector3.Lerp(targetScale, originalScale, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            target.localScale = originalScale;
        }
        
        // ── Silk Spool Integration ───────────────────────────────────────────────
        private void UpdateSilkSpoolDisplay()
        {
            if (_silkSpoolOverlay == null || _silkSpoolOverlay.Renderer == null) return;
            
            // Update silk spool visual based on current silk amount with division by zero protection
            var silkPercentage = SilkManager.MaxSilk > 0 ? (float)SilkManager.CurrentSilk / SilkManager.MaxSilk : 0f;
            
            // TODO: Update spool animation based on silk amount
            // This would involve switching between different animation states
            // like "Spool Empty", "Spool Low", "Spool Medium", "Spool Full"
        }
        
        // ── Public API for Crest Switching ───────────────────────────────────────────
        public void SwitchToCrest(CrestType newCrest)
        {
            if (newCrest == _currentDisplayedCrest) return;
            
            _currentDisplayedCrest = newCrest;
            CrestManager.CurrentCrest = newCrest;
            
            // Force immediate update
            UpdateCrestIcon();
            PlayCrestSwitchAnimation();
        }
        
        public CrestType GetCurrentDisplayedCrest() => _currentDisplayedCrest;
        
        // ── Helper Methods ───────────────────────────────────────────────────────
        private void LogMissing(string item)
        {
            if (_missingLogged.Contains(item)) return;
            _missingLogged.Add(item);
            if (HornetInHallownest.Instance != null)
                HornetInHallownest.Instance.Log($"[HudSpriteManager] no target found for {item}");
        }
        
        void OnDestroy()
        {
            // Clean up coroutines to prevent post-destruction execution
            if (_crestSwitchCoroutine != null)
            {
                StopCoroutine(_crestSwitchCoroutine);
                _crestSwitchCoroutine = null;
            }
            
            // Clean up overlay references
            if (_crestDisplayContainer != null)
            {
                Destroy(_crestDisplayContainer);
                _crestDisplayContainer = null;
            }
        }
        
        void OnDisable()
        {
            // Stop coroutines when disabled to prevent issues
            if (_crestSwitchCoroutine != null)
            {
                StopCoroutine(_crestSwitchCoroutine);
                _crestSwitchCoroutine = null;
            }
        }

        private class AnimatedHudOverlay
        {
            private List<Sprite> _frames;
            private float _timer;
            private int _index;
            private bool _enabled = true;
            private bool _loop = true;
            private bool _finished;

            public SpriteRenderer Renderer { get; }
            public bool Enabled
            {
                get => _enabled;
                set
                {
                    _enabled = value;
                    if (Renderer != null)
                        Renderer.enabled = value;
                }
            }
            
            public bool IsPlaying => !_finished && _enabled;
            public bool Loop { get => _loop; set => _loop = value; }

            public AnimatedHudOverlay(SpriteRenderer renderer, List<Sprite> frames, bool loop = true)
            {
                Renderer = renderer;
                _frames = frames ?? new List<Sprite>();
                _loop = loop;
                
                // Validate animation state
                if (_frames.Count == 0)
                {
                    // Mark as finished if no frames available
                    _finished = true;
                    if (Renderer != null)
                        Renderer.sprite = null;
                }
                else if (Renderer != null)
                {
                    // Set initial frame only if valid frames exist
                    Renderer.sprite = _frames[0];
                }
            }

            public void Tick(float deltaTime)
            {
                if (_frames.Count <= 1 || Renderer == null || _finished) return;
                
                _timer += deltaTime;
                if (_timer < FrameDuration) return;
                
                _timer -= FrameDuration;
                _index++;
                
                if (_index >= _frames.Count)
                {
                    if (_loop)
                    {
                        _index = 0;
                    }
                    else
                    {
                        _index = _frames.Count - 1;
                        _finished = true;
                        return;
                    }
                }
                
                // Additional safety check before accessing frame
                if (_index < _frames.Count && _frames[_index] != null)
                {
                    Renderer.sprite = _frames[_index];
                }
            }
            
            public void Restart()
            {
                _index = 0;
                _timer = 0f;
                _finished = false;
                if (Renderer != null && _frames.Count > 0)
                    Renderer.sprite = _frames[0];
            }
            
            public void PlayAnimation(List<Sprite> newFrames, bool loop = true)
            {
                _frames = newFrames ?? new List<Sprite>();
                _loop = loop;
                Restart();
            }
        }
    }
}
