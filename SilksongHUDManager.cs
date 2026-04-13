using System;
using System.Collections.Generic;
using UnityEngine;
using GlobalEnums;

namespace HornetInHallownest
{
    /// <summary>
    /// Complete Silksong-style HUD system that replaces Hollow Knight's UI elements.
    /// Manages crest display, silk spool, mask system, and dynamic crest switching.
    /// </summary>
    public class SilksongHUDManager : MonoBehaviour
    {
        public static SilksongHUDManager Instance { get; private set; }

        [Header("HUD Configuration")]
        public Vector3 CrestDisplayPosition = new Vector3(-4f, 2.5f, 0f);
        public Vector3 SilkSpoolPosition = new Vector3(0f, -2.5f, 0f);
        public Vector3 MaskDisplayOffset = new Vector3(0f, 0.5f, 0f);

        // Core HUD components
        private GameObject _hudRoot;
        private GameObject _crestDisplay;
        private GameObject _silkSpoolDisplay;
        private List<GameObject> _maskDisplays;

        // Crest system
        private CrestType _currentCrest = CrestType.Hunter;
        private SpriteRenderer _crestIconRenderer;
        private tk2dSprite _crestIconTk2d;
        private Animator _crestAnimator;

        // Silk display system (text-based)
        private TextMesh _silkTextMesh;
        private GameObject _silkTextObject;

        // Animation states
        private readonly Dictionary<CrestType, string> _crestAnimationNames = new()
        {
            { CrestType.Hunter, "Hunter Crest Idle" },
            { CrestType.Reaper, "Reaper Crest Idle" },
            { CrestType.Wanderer, "Wanderer Crest Idle" },
            { CrestType.Warrior, "Warrior Crest Idle" },
            { CrestType.Witch, "Witch Crest Idle" },
            { CrestType.Toolmaster, "Toolmaster Crest Idle" },
            { CrestType.Spinner, "Spinner Crest Idle" },
            { CrestType.Cloakless, "Cloakless Crest Idle" },
            { CrestType.Cursed, "Cursed Crest Idle" }
        };

        private readonly Dictionary<int, string> _silkLevelAnimations = new()
        {
            { 0, "Spool Empty" },
            { 1, "Spool Low" },
            { 2, "Spool Medium" },
            { 3, "Spool Full" }
        };
        
        // Coroutine tracking for safe cleanup
        private System.Collections.Coroutine _crestSwitchEffectCoroutine;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        void Start()
        {
            InitializeHUD();
            RegisterEvents();
        }

        void OnDestroy()
        {
            UnregisterEvents();
        }

        void Update()
        {
            if (!HornetSpriteDriver.IsEnabled) return;

            UpdateCrestDisplay();
            UpdateSilkSpoolDisplay();
            UpdateMaskDisplays();
        }

        private void InitializeHUD()
        {
            // Create HUD root container
            _hudRoot = new GameObject("Silksong HUD");
            _hudRoot.transform.position = Vector3.zero;

            // Initialize crest display
            SetupCrestDisplay();

            // Initialize silk spool
            SetupSilkSpool();

            // Initialize mask displays
            SetupMaskDisplays();

            HornetInHallownest.Instance.Log("[SilksongHUDManager] HUD initialized");
        }

        private void SetupCrestDisplay()
        {
            _crestDisplay = new GameObject("Crest Display");
            _crestDisplay.transform.SetParent(_hudRoot.transform);
            _crestDisplay.transform.position = CrestDisplayPosition;

            // Add sprite renderer
            _crestIconRenderer = _crestDisplay.AddComponent<SpriteRenderer>();
            _crestIconRenderer.sortingLayerName = "UI";
            _crestIconRenderer.sortingOrder = 100;

            // Try to add tk2d sprite for compatibility
            _crestIconTk2d = _crestDisplay.AddComponent<tk2dSprite>();

            // Add animator for crest animations
            _crestAnimator = _crestDisplay.AddComponent<Animator>();

            // Set initial crest
            UpdateCrestIcon();
        }

        private void SetupSilkSpool()
        {
            // Create simple text display for silk
            _silkTextObject = new GameObject("Silk Text");
            _silkTextObject.transform.SetParent(transform);

            // Create TextMesh component
            _silkTextMesh = _silkTextObject.AddComponent<TextMesh>();
            
            // Configure text appearance
            _silkTextMesh.font = Resources.GetBuiltinResource<Font>("Arial.ttf"); // Fallback font
            _silkTextMesh.fontSize = 24;
            _silkTextMesh.color = Color.white;
            _silkTextMesh.anchor = TextAnchor.MiddleCenter;
            _silkTextMesh.alignment = TextAlignment.Center;

            // Position below crest display
            _silkTextObject.transform.localPosition = new Vector3(-2.5f, -2.0f, 0);
            _silkTextObject.transform.localScale = Vector3.one;

            UpdateSilkSpoolVisual();
        }

        private void SetupMaskDisplays()
        {
            _maskDisplays = new List<GameObject>();

            // Find existing mask displays or create new ones
            var existingMasks = GameObject.FindObjectsOfType<SpriteRenderer>();
            foreach (var maskRenderer in existingMasks)
            {
                if (maskRenderer.gameObject.name.IndexOf("mask", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    var maskDisplay = new GameObject("Hornet Mask Display");
                    maskDisplay.transform.SetParent(_hudRoot.transform);
                    maskDisplay.transform.position = maskRenderer.transform.position + MaskDisplayOffset;

                    var hornetMaskRenderer = maskDisplay.AddComponent<SpriteRenderer>();
                    hornetMaskRenderer.sortingLayerName = maskRenderer.sortingLayerName;
                    hornetMaskRenderer.sortingOrder = maskRenderer.sortingOrder + 1;

                    _maskDisplays.Add(maskDisplay);
                }
            }

            HornetInHallownest.Instance.Log($"[SilksongHUDManager] Found {_maskDisplays.Count} mask displays");
        }

        // Removed complex sprite loading - now using simple text display

        private void RegisterEvents()
        {
            // Register for crest changes
            // TODO: Hook into crest management events when implemented
        }

        private void UnregisterEvents()
        {
            // Unregister events
        }

        private void UpdateCrestDisplay()
        {
            if (_crestDisplay == null) return;

            // Only update when crest actually changes
            if (_currentCrest != CrestManager.CurrentCrest)
            {
                _currentCrest = CrestManager.CurrentCrest;
                UpdateCrestIcon();
                PlayCrestSwitchAnimation();
            }

            // Enable/disable based on Hornet state
            _crestDisplay.SetActive(HornetSpriteDriver.IsEnabled);
        }

        private void UpdateCrestIcon()
        {
            var crestSpriteKey = GetCrestSpriteKey(_currentCrest);
            
            if (HornetSpriteDriver.FrameSprites.TryGetValue(crestSpriteKey, out var crestSprite) && crestSprite != null)
            {
                if (_crestIconRenderer != null)
                    _crestIconRenderer.sprite = crestSprite;
                
                if (_crestIconTk2d != null)
                    _crestIconTk2d.sprite = crestSprite;
            }
            else
            {
                // Fallback to Hunter crest with null check
                if (HornetSpriteDriver.FrameSprites.TryGetValue("Hunter Crest Icon", out var fallbackSprite) && fallbackSprite != null)
                {
                    if (_crestIconRenderer != null)
                        _crestIconRenderer.sprite = fallbackSprite;
                    
                    if (_crestIconTk2d != null)
                        _crestIconTk2d.sprite = fallbackSprite;
                }
                else
                {
                    // Ultimate fallback - clear sprites if none available
                    if (_crestIconRenderer != null)
                        _crestIconRenderer.sprite = null;
                    
                    if (_crestIconTk2d != null)
                        _crestIconTk2d.sprite = null;
                }
            }

            // Update animator if available
            if (_crestAnimator != null && _crestAnimationNames.TryGetValue(_currentCrest, out var animName))
            {
                _crestAnimator.Play(animName);
            }
        }

        private string GetCrestSpriteKey(CrestType crestType)
        {
            var animData = CrestManager.GetCrestHUDAnimationData(crestType);
            return animData?.SpriteKey ?? "HUD Frame Hunter v3";
        }

        private void PlayCrestSwitchAnimation()
        {
            if (_crestAnimator != null)
            {
                // Play crest switch animation
                _crestAnimator.SetTrigger("Switch");
            }

            // Stop any existing crest switch effect
            if (_crestSwitchEffectCoroutine != null)
            {
                StopCoroutine(_crestSwitchEffectCoroutine);
                _crestSwitchEffectCoroutine = null;
            }
            
            // Start visual effect coroutine
            _crestSwitchEffectCoroutine = StartCoroutine(CrestSwitchEffect());
        }

        private System.Collections.IEnumerator CrestSwitchEffect()
        {
            if (_crestIconRenderer == null) yield break;

            Vector3 originalScale = _crestIconRenderer.transform.localScale;
            Color originalColor = _crestIconRenderer.color;
            
            // Enhanced animation sequence inspired by Silksong FSM
            
            // Phase 1: Fade out and shrink
            float elapsed = 0f;
            while (elapsed < 0.2f)
            {
                float t = elapsed / 0.2f;
                _crestIconRenderer.transform.localScale = Vector3.Lerp(originalScale, originalScale * 0.3f, t);
                _crestIconRenderer.color = Color.Lerp(originalColor, new Color(1f, 1f, 1f, 0f), t);
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            // Phase 2: Switch sprite during fade
            UpdateCrestIcon();
            
            // Phase 3: Bounce in with fade
            elapsed = 0f;
            while (elapsed < 0.4f)
            {
                float t = elapsed / 0.4f;
                
                // Elastic bounce effect
                float bounce = 1f + Mathf.Sin(t * Mathf.PI) * 0.4f * (1f - t);
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

        private void UpdateSilkSpoolDisplay()
        {
            if (_silkTextObject == null) return;

            _silkTextObject.SetActive(HornetSpriteDriver.IsEnabled);
            UpdateSilkSpoolVisual();
        }

        private void UpdateSilkSpoolVisual()
        {
            if (_silkTextMesh == null) return;

            // Use SilkManager data
            int currentSilk = SilkManager.CurrentSilk;
            int maxSilk = SilkManager.MaxSilk;

            // Update text display
            _silkTextMesh.text = $"{currentSilk}/{maxSilk}";

            HornetInHallownest.Instance?.Log($"[SilksongHUDManager] Silk display updated: {currentSilk}/{maxSilk}");
        }

        private int GetSilkLevel(float percentage)
        {
            return percentage switch
            {
                <= 0.25f => 0, // Empty
                <= 0.5f => 1,  // Low
                <= 0.75f => 2, // Medium
                _ => 3         // Full
            };
        }

        private void UpdateMaskDisplays()
        {
            if (_maskDisplays == null) return;

            var playerData = PlayerData.instance;
            if (playerData == null) return;

            // Update mask displays based on current masks
            int maskCount = playerData.maskCount;
            for (int i = 0; i < _maskDisplays.Count; i++)
            {
                var maskDisplay = _maskDisplays[i];
                if (maskDisplay != null)
                {
                    bool shouldShow = i < maskCount && HornetSpriteDriver.IsEnabled;
                    maskDisplay.SetActive(shouldShow);

                    if (shouldShow)
                    {
                        // Update mask sprite
                        var maskSpriteKey = i == 0 ? "Mask1" : "Mask2";
                        if (HornetSpriteDriver.FrameSprites.TryGetValue(maskSpriteKey, out var maskSprite))
                        {
                            var renderer = maskDisplay.GetComponent<SpriteRenderer>();
                            if (renderer != null)
                                renderer.sprite = maskSprite;
                        }
                    }
                }
            }
        }

        // Public API for crest switching
        public void SwitchToCrest(CrestType newCrest)
        {
            if (newCrest == _currentCrest) return;

            _currentCrest = newCrest;
            CrestManager.CurrentCrest = newCrest;

            // Force immediate update
            UpdateCrestIcon();
            PlayCrestSwitchAnimation();

            HornetInHallownest.Instance.Log($"[SilksongHUDManager] Switched to {newCrest} crest");
        }

        // Enhanced crest switching with disappear/appear animation sequence
        public void SwitchToCrestWithAnimation(CrestType newCrest)
        {
            if (newCrest == _currentCrest) return;

            // Stop any existing crest switch effect
            if (_crestSwitchEffectCoroutine != null)
            {
                StopCoroutine(_crestSwitchEffectCoroutine);
                _crestSwitchEffectCoroutine = null;
            }
            
            // Start the full disappear/appear sequence
            _crestSwitchEffectCoroutine = StartCoroutine(CrestDisappearAppearSequence(newCrest));
        }

        private System.Collections.IEnumerator CrestDisappearAppearSequence(CrestType newCrest)
        {
            if (_crestIconRenderer == null) yield break;

            CrestType oldCrest = _currentCrest;
            Vector3 originalScale = _crestIconRenderer.transform.localScale;
            Color originalColor = _crestIconRenderer.color;

            // Phase 1: Disappear animation for current crest
            float elapsed = 0f;
            while (elapsed < 0.3f)
            {
                float t = elapsed / 0.3f;
                
                // Fade out and shrink
                _crestIconRenderer.color = Color.Lerp(originalColor, new Color(1f, 1f, 1f, 0f), t);
                _crestIconRenderer.transform.localScale = Vector3.Lerp(originalScale, originalScale * 0.1f, t);
                
                elapsed += Time.deltaTime;
                yield return null;
            }

            // Phase 2: Switch crest data
            _currentCrest = newCrest;
            CrestManager.CurrentCrest = newCrest;
            UpdateCrestIcon();

            // Phase 3: Appear animation for new crest
            elapsed = 0f;
            while (elapsed < 0.4f)
            {
                float t = elapsed / 0.4f;
                
                // Elastic bounce effect for appearance
                float bounce = 1f + Mathf.Sin(t * Mathf.PI) * 0.5f * (1f - t);
                _crestIconRenderer.transform.localScale = originalScale * bounce;
                
                // Fade in
                _crestIconRenderer.color = Color.Lerp(new Color(1f, 1f, 1f, 0f), originalColor, t);
                
                elapsed += Time.deltaTime;
                yield return null;
            }

            // Reset to final state
            _crestIconRenderer.transform.localScale = originalScale;
            _crestIconRenderer.color = originalColor;

            HornetInHallownest.Instance.Log($"[SilksongHUDManager] Crest switch: {oldCrest} -> {newCrest}");
        }

        public CrestType GetCurrentCrest() => _currentCrest;

        // Public API for silk updates
        public void RefreshSilkDisplay()
        {
            UpdateSilkSpoolVisual();
        }

        // Cleanup
        public void Cleanup()
        {
            // Stop coroutines during cleanup
            if (_crestSwitchEffectCoroutine != null)
            {
                StopCoroutine(_crestSwitchEffectCoroutine);
                _crestSwitchEffectCoroutine = null;
            }
            
            // Clean up silk text object
            if (_silkTextObject != null)
            {
                Destroy(_silkTextObject);
                _silkTextObject = null;
            }
            
            if (_hudRoot != null)
            {
                Destroy(_hudRoot);
                _hudRoot = null;
            }
        }
        
        void OnDestroy()
        {
            Cleanup();
        }
        
        void OnDisable()
        {
            // Stop coroutines when disabled
            if (_crestSwitchEffectCoroutine != null)
            {
                StopCoroutine(_crestSwitchEffectCoroutine);
                _crestSwitchEffectCoroutine = null;
            }
        }
    }
}
