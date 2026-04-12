using System;
using System.Collections.Generic;
using UnityEngine;
using GlobalEnums;

namespace HornetInHallownest
{
    /// <summary>
    /// Manages crest switching functionality for gameplay menu integration.
    /// Handles input detection, crest cycling, and UI updates.
    /// 
    /// NOTE: This component is designed to be easily removable.
    /// Set EnableCrestSwitching = false to disable all functionality.
    /// Remove this component from MyFirstMod.cs to completely remove hotkey switching.
    /// </summary>
    public class CrestSwitchManager : MonoBehaviour
    {
        public static CrestSwitchManager Instance { get; private set; }

        [Header("Crest Switch Settings")]
        public KeyCode CycleCrestKey = KeyCode.C;
        public KeyCode PreviousCrestKey = KeyCode.Q;
        public KeyCode NextCrestKey = KeyCode.E;
        public bool AllowCrestSwitching = true;
        
        [Header("Easy Removal Settings")]
        [Tooltip("Set to false to completely disable crest switching functionality")]
        public bool EnableCrestSwitching = true;
        
        [Tooltip("Set to false to disable hotkey switching (keep for programmatic switching)")]
        public bool EnableHotkeySwitching = true;

        [Header("UI Settings")]
        public bool ShowCrestSwitchNotification = true;
        public float NotificationDuration = 2.0f;
        public Vector3 NotificationOffset = new Vector3(0f, 1f, 0f);

        // Crest cycling
        private CrestType[] _crestOrder = new[]
        {
            CrestType.Hunter, CrestType.Reaper, CrestType.Wanderer, CrestType.Warrior,
            CrestType.Witch, CrestType.Toolmaster, CrestType.Spinner, CrestType.Cloakless, CrestType.Cursed
        };

        private int _currentCrestIndex = 0;

        // UI notification
        private GameObject _notificationObject;
        private SpriteRenderer _notificationRenderer;
        private TextMesh _notificationText;
        private float _notificationTimer;

        // Input buffering
        private float _lastSwitchTime;
        private const float SWITCH_COOLDOWN = 0.3f;
        
        // Coroutine tracking for safe cleanup
        private System.Collections.Coroutine _notificationAnimationCoroutine;
        private System.Collections.Coroutine _fadeoutNotificationCoroutine;
        
        // Cached component references for performance
        private SilksongHUDManager _cachedSilksongHUDManager;
        private HudSpriteManager _cachedHudSpriteManager;

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
            InitializeCrestIndex();
            SetupNotificationUI();
            CacheComponentReferences();
        }

        void Update()
        {
            if (!HornetSpriteDriver.IsEnabled || !AllowCrestSwitching) return;

            HandleCrestSwitchInput();
            UpdateNotification();
        }

        private void InitializeCrestIndex()
        {
            _currentCrestIndex = Array.IndexOf(_crestOrder, CrestManager.CurrentCrest);
            if (_currentCrestIndex < 0)
            {
                _currentCrestIndex = 0; // Default to Hunter
                CrestManager.CurrentCrest = _crestOrder[0];
            }
        }

        private void SetupNotificationUI()
        {
            if (!ShowCrestSwitchNotification) return;

            // Create notification object
            _notificationObject = new GameObject("Crest Switch Notification");
            _notificationObject.transform.SetParent(transform);
            _notificationObject.transform.localPosition = NotificationOffset;

            // Add sprite renderer for crest icon
            _notificationRenderer = _notificationObject.AddComponent<SpriteRenderer>();
            _notificationRenderer.sortingLayerName = "UI";
            _notificationRenderer.sortingOrder = 200;

            // Add text for crest name
            var textObject = new GameObject("Crest Name");
            textObject.transform.SetParent(_notificationObject.transform);
            textObject.transform.localPosition = new Vector3(0f, -0.5f, 0f);

            _notificationText = textObject.AddComponent<TextMesh>();
            _notificationText.fontSize = 12;
            _notificationText.alignment = TextAlignment.Center;
            _notificationText.anchor = TextAnchor.MiddleCenter;
            _notificationText.color = Color.white;

            // Hide initially
            _notificationObject.SetActive(false);
        }

        private void HandleCrestSwitchInput()
        {
            // Easy removal check - completely disable if EnableCrestSwitching is false
            if (!EnableCrestSwitching || !EnableHotkeySwitching) return;
            
            // Check cooldown
            if (Time.time - _lastSwitchTime < SWITCH_COOLDOWN) return;

            bool switched = false;

            // Handle different input methods
            if (Input.GetKeyDown(CycleCrestKey))
            {
                SwitchToNextCrest();
                switched = true;
            }
            else if (Input.GetKeyDown(NextCrestKey))
            {
                SwitchToNextCrest();
                switched = true;
            }
            else if (Input.GetKeyDown(PreviousCrestKey))
            {
                SwitchToPreviousCrest();
                switched = true;
            }

            if (switched)
            {
                _lastSwitchTime = Time.time;
                ShowCrestSwitchNotification();
            }
        }

        private void SwitchToNextCrest()
        {
            _currentCrestIndex = (_currentCrestIndex + 1) % _crestOrder.Length;
            SwitchToCrest(_crestOrder[_currentCrestIndex]);
        }

        private void SwitchToPreviousCrest()
        {
            _currentCrestIndex = (_currentCrestIndex - 1 + _crestOrder.Length) % _crestOrder.Length;
            SwitchToCrest(_crestOrder[_currentCrestIndex]);
        }

        private void SwitchToCrest(CrestType newCrest)
        {
            if (newCrest == CrestManager.CurrentCrest) return;

            // Update crest manager
            CrestManager.CurrentCrest = newCrest;

            // Update HUD displays using cached references
            if (_cachedSilksongHUDManager != null)
            {
                _cachedSilksongHUDManager.SwitchToCrest(newCrest);
            }
            else
            {
                // Fallback to FindObjectOfType if cache is empty
                var hudManager = FindObjectOfType<SilksongHUDManager>();
                if (hudManager != null)
                {
                    _cachedSilksongHUDManager = hudManager;
                    hudManager.SwitchToCrest(newCrest);
                }
            }

            if (_cachedHudSpriteManager != null)
            {
                _cachedHudSpriteManager.SwitchToCrest(newCrest);
            }
            else
            {
                // Fallback to FindObjectOfType if cache is empty
                var hudSpriteManager = FindObjectOfType<HudSpriteManager>();
                if (hudSpriteManager != null)
                {
                    _cachedHudSpriteManager = hudSpriteManager;
                    hudSpriteManager.SwitchToCrest(newCrest);
                }
            }

            HornetInHallownest.Instance.Log($"[CrestSwitchManager] Switched to {newCrest} crest");
        }
        
        private void CacheComponentReferences()
        {
            // Cache frequently accessed components to avoid expensive FindObjectOfType calls
            _cachedSilksongHUDManager = FindObjectOfType<SilksongHUDManager>();
            _cachedHudSpriteManager = FindObjectOfType<HudSpriteManager>();
        }

        private void ShowCrestSwitchNotification()
        {
            if (!ShowCrestSwitchNotification || _notificationObject == null) return;

            // Update notification content
            UpdateNotificationContent();

            // Show notification
            _notificationObject.SetActive(true);
            _notificationTimer = NotificationDuration;

            // Stop any existing notification animations
            if (_notificationAnimationCoroutine != null)
            {
                StopCoroutine(_notificationAnimationCoroutine);
                _notificationAnimationCoroutine = null;
            }
            if (_fadeoutNotificationCoroutine != null)
            {
                StopCoroutine(_fadeoutNotificationCoroutine);
                _fadeoutNotificationCoroutine = null;
            }

            // Start animation
            _notificationAnimationCoroutine = StartCoroutine(NotificationAnimation());
        }

        private void UpdateNotificationContent()
        {
            if (_notificationRenderer == null || _notificationText == null) return;

            var currentCrest = CrestManager.CurrentCrest;

            // Update crest icon
            var crestSpriteKey = GetCrestSpriteKey(currentCrest);
            if (HornetSpriteDriver.FrameSprites.TryGetValue(crestSpriteKey, out var crestSprite))
            {
                _notificationRenderer.sprite = crestSprite;
            }

            // Update text
            _notificationText.text = GetCrestDisplayName(currentCrest);
        }

        private string GetCrestSpriteKey(CrestType crestType)
        {
            return crestType switch
            {
                CrestType.Hunter => "Hunter Crest Icon v3", // Use v3 for future upgrade compatibility
                CrestType.Reaper => "Reaper Crest Icon",
                CrestType.Wanderer => "Wanderer Crest Icon",
                CrestType.Warrior => "Warrior Crest Icon",
                CrestType.Witch => "Witch Crest Icon",
                CrestType.Toolmaster => "Toolmaster Crest Icon",
                CrestType.Spinner => "Spinner Crest Icon",
                CrestType.Cloakless => "Cloakless Crest Icon",
                CrestType.Cursed => "Cursed Crest Icon",
                _ => "Hunter Crest Icon v3"
            };
        }

        private string GetCrestDisplayName(CrestType crestType)
        {
            return crestType switch
            {
                CrestType.Hunter => "Hunter's Crest",
                CrestType.Reaper => "Reaper Crest",
                CrestType.Wanderer => "Wanderer's Crest",
                CrestType.Warrior => "Beast Crest",
                CrestType.Witch => "Witch Crest",
                CrestType.Toolmaster => "Architect Crest",
                CrestType.Spinner => "Shaman Crest",
                CrestType.Cloakless => "Cloakless Crest",
                CrestType.Cursed => "Cursed Crest",
                _ => "Unknown Crest"
            };
        }

        private System.Collections.IEnumerator NotificationAnimation()
        {
            if (_notificationObject == null) yield break;

            Vector3 originalScale = _notificationObject.transform.localScale;
            Vector3 targetScale = originalScale * 1.2f;

            // Scale in
            float elapsed = 0f;
            while (elapsed < 0.2f)
            {
                _notificationObject.transform.localScale = Vector3.Lerp(originalScale * 0.5f, targetScale, elapsed / 0.2f);
                elapsed += Time.deltaTime;
                yield return null;
            }

            // Hold
            yield return new WaitForSeconds(0.1f);

            // Scale to normal
            elapsed = 0f;
            while (elapsed < 0.2f)
            {
                _notificationObject.transform.localScale = Vector3.Lerp(targetScale, originalScale, elapsed / 0.2f);
                elapsed += Time.deltaTime;
                yield return null;
            }

            _notificationObject.transform.localScale = originalScale;
        }

        private void UpdateNotification()
        {
            if (_notificationObject == null || !_notificationObject.activeSelf) return;

            _notificationTimer -= Time.deltaTime;
            if (_notificationTimer <= 0f)
            {
                // Stop any existing fadeout and start new one
                if (_fadeoutNotificationCoroutine != null)
                {
                    StopCoroutine(_fadeoutNotificationCoroutine);
                    _fadeoutNotificationCoroutine = null;
                }
                
                // Fade out
                _fadeoutNotificationCoroutine = StartCoroutine(FadeOutNotification());
            }
        }

        private System.Collections.IEnumerator FadeOutNotification()
        {
            if (_notificationRenderer == null) yield break;

            float startAlpha = 1f;
            float elapsed = 0f;

            while (elapsed < 0.5f)
            {
                float alpha = Mathf.Lerp(startAlpha, 0f, elapsed / 0.5f);
                _notificationRenderer.color = new Color(1f, 1f, 1f, alpha);
                
                if (_notificationText != null)
                {
                    _notificationText.color = new Color(1f, 1f, 1f, alpha);
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            _notificationObject.SetActive(false);
            
            // Reset colors
            _notificationRenderer.color = Color.white;
            if (_notificationText != null)
            {
                _notificationText.color = Color.white;
            }
        }

        // Public API for external crest switching
        public void SwitchToCrestByIndex(int index)
        {
            if (index < 0 || index >= _crestOrder.Length) return;

            _currentCrestIndex = index;
            SwitchToCrest(_crestOrder[index]);
        }

        public void SwitchToCrestByName(string crestName)
        {
            if (Enum.TryParse<CrestType>(crestName, true, out var crestType))
            {
                var index = Array.IndexOf(_crestOrder, crestType);
                if (index >= 0)
                {
                    _currentCrestIndex = index;
                    SwitchToCrest(crestType);
                }
            }
        }

        public CrestType[] GetAllCrests() => (CrestType[])_crestOrder.Clone();

        public int GetCurrentCrestIndex() => _currentCrestIndex;

        public void SetAllowCrestSwitching(bool allow)
        {
            AllowCrestSwitching = allow;
        }
        
        // Easy removal API
        public void SetEnableCrestSwitching(bool enable)
        {
            EnableCrestSwitching = enable;
            if (!enable)
            {
                // Clean up any active notifications
                if (_notificationObject != null)
                {
                    _notificationObject.SetActive(false);
                }
            }
        }
        
        public void SetEnableHotkeySwitching(bool enable)
        {
            EnableHotkeySwitching = enable;
        }
        
        public bool IsCrestSwitchingEnabled() => EnableCrestSwitching;
        public bool IsHotkeySwitchingEnabled() => EnableHotkeySwitching;

        // Cleanup
        void OnDestroy()
        {
            // Stop coroutines during cleanup
            if (_notificationAnimationCoroutine != null)
            {
                StopCoroutine(_notificationAnimationCoroutine);
                _notificationAnimationCoroutine = null;
            }
            if (_fadeoutNotificationCoroutine != null)
            {
                StopCoroutine(_fadeoutNotificationCoroutine);
                _fadeoutNotificationCoroutine = null;
            }
            
            if (_notificationObject != null)
            {
                Destroy(_notificationObject);
                _notificationObject = null;
            }
        }
        
        void OnDisable()
        {
            // Stop coroutines when disabled
            if (_notificationAnimationCoroutine != null)
            {
                StopCoroutine(_notificationAnimationCoroutine);
                _notificationAnimationCoroutine = null;
            }
            if (_fadeoutNotificationCoroutine != null)
            {
                StopCoroutine(_fadeoutNotificationCoroutine);
                _fadeoutNotificationCoroutine = null;
            }
            
            // Clear cached references when disabled
            _cachedSilksongHUDManager = null;
            _cachedHudSpriteManager = null;
        }
    }
}
