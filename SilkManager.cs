using UnityEngine;
using GlobalEnums;

namespace HornetInHallownest
{
    // Manages Hornet's silk system (replaces soul mechanic)
    // - Spool + Cap display (18 silk max)
    // - +1 silk per enemy hit
    // - Blocks soul mechanic when Hornet active
    // - Blocks normal healing when Hornet active
    public class SilkManager : MonoBehaviour
    {
        public static SilkManager Instance { get; private set; }

        // Silk system variables
        public static int CurrentSilk = 0;
        public const int MaxSilk = 18;
        
        // Saved soul values for character switching
        private static int _savedSoulOrbs;
        private static int _savedMPCharge;
        private static int _savedMPReserve;
        
        // Saved silk values for character switching  
        private static int _savedSilk;

        void Awake()
        {
            Instance = this;
        }

        void OnEnable()
        {
            // Hook into enemy hit events for silk gain
            On.HealthManager.Hit += OnEnemyHit;
            
            // Hook into soul gain to block it when Hornet is active
            On.PlayerData.AddMP += OnAddMP;
            On.PlayerData.AddMPCharge += OnAddMPCharge;
            
            // Hook into healing to block it when Hornet is active
            On.PlayerData.AddHealth += OnAddHealth;
        }

        void OnDisable()
        {
            On.HealthManager.Hit -= OnEnemyHit;
            On.PlayerData.AddMP -= OnAddMP;
            On.PlayerData.AddMPCharge -= OnAddMPCharge;
            On.PlayerData.AddHealth -= OnAddHealth;
        }

        void Start()
        {
            // Initialize silk system when Hornet becomes active
            if (HornetSpriteDriver.IsEnabled)
            {
                EnableSilkSystem();
            }
        }

        void Update()
        {
            // Check for Hornet state changes
            bool isHornet = HornetSpriteDriver.IsEnabled;
            
            if (isHornet && !_silkSystemActive)
            {
                EnableSilkSystem();
            }
            else if (!isHornet && _silkSystemActive)
            {
                DisableSilkSystem();
            }
        }

        private bool _silkSystemActive = false;

        private void EnableSilkSystem()
        {
            if (_silkSystemActive) return;

            // Save current soul values
            if (PlayerData.instance != null)
            {
                _savedSoulOrbs = PlayerData.instance.soulOrbs;
                _savedMPCharge = PlayerData.instance.MPCharge;
                _savedMPReserve = PlayerData.instance.MPReserve;
                
                // Clear soul display while Hornet is active
                PlayerData.instance.soulOrbs = 0;
                PlayerData.instance.MPCharge = 0;
                PlayerData.instance.MPReserve = 0;
            }

            // Restore saved silk if switching back to Hornet
            CurrentSilk = _savedSilk;
            
            _silkSystemActive = true;
            HornetInHallownest.Instance?.Log("[SilkManager] Silk system enabled");
            
            // Update HUD displays
            UpdateSilkHUD();
        }

        private void DisableSilkSystem()
        {
            if (!_silkSystemActive) return;

            // Save current silk for later
            _savedSilk = CurrentSilk;
            
            // Restore soul values
            if (PlayerData.instance != null)
            {
                PlayerData.instance.soulOrbs = _savedSoulOrbs;
                PlayerData.instance.MPCharge = _savedMPCharge;
                PlayerData.instance.MPReserve = _savedMPReserve;
            }

            CurrentSilk = 0;
            _silkSystemActive = false;
            HornetInHallownest.Instance?.Log("[SilkManager] Silk system disabled");
        }

        private void OnEnemyHit(On.HealthManager.orig_Hit orig, HealthManager self, HitInstance hitInstance)
        {
            // Only give silk if Hornet is active and hit was from Hornet
            if (!_silkSystemActive) 
            {
                orig(self, hitInstance);
                return;
            }

            // Check if this hit was from the player (Hornet)
            if (hitInstance.AttackType == AttackTypeEnum.Generic || 
                hitInstance.AttackType == AttackTypeEnum.Regular)
            {
                AddSilk(1);
            }

            orig(self, hitInstance);
        }

        private void OnAddMP(On.PlayerData.orig_AddMP orig, PlayerData self, int amount)
        {
            // Block soul gain when Hornet is active
            if (_silkSystemActive)
            {
                HornetInHallownest.Instance?.Log("[SilkManager] Blocked soul gain");
                return;
            }
            
            orig(self, amount);
        }

        private void OnAddMPCharge(On.PlayerData.orig_AddMPCharge orig, PlayerData self)
        {
            // Block soul charge gain when Hornet is active
            if (_silkSystemActive)
            {
                HornetInHallownest.Instance?.Log("[SilkManager] Blocked soul charge gain");
                return;
            }
            
            orig(self);
        }

        private void OnAddHealth(On.PlayerData.orig_AddHealth orig, PlayerData self, int amount)
        {
            // Block normal healing when Hornet is active
            if (_silkSystemActive)
            {
                HornetInHallownest.Instance?.Log("[SilkManager] Blocked normal healing");
                return;
            }
            
            orig(self, amount);
        }

        public static void AddSilk(int amount)
        {
            if (Instance == null || !Instance._silkSystemActive) return;
            
            CurrentSilk = Mathf.Min(CurrentSilk + amount, MaxSilk);
            Instance.UpdateSilkHUD();
            
            HornetInHallownest.Instance?.Log($"[SilkManager] Silk: {CurrentSilk}/{MaxSilk}");
        }

        public static void UseSilk(int amount)
        {
            if (Instance == null || !Instance._silkSystemActive) return;
            
            CurrentSilk = Mathf.Max(CurrentSilk - amount, 0);
            Instance.UpdateSilkHUD();
            
            HornetInHallownest.Instance?.Log($"[SilkManager] Silk used: {CurrentSilk}/{MaxSilk}");
        }

        private void UpdateSilkHUD()
        {
            // Update HUD displays for spool and cap
            var hudManager = FindObjectOfType<SilksongHUDManager>();
            if (hudManager != null)
            {
                hudManager.RefreshSilkDisplay();
            }
            else
            {
                HornetInHallownest.Instance?.Log("[SilkManager] SilksongHUDManager not found for silk update");
            }

            var spriteManager = FindObjectOfType<HudSpriteManager>();
            if (spriteManager != null)
            {
                spriteManager.UpdateSilkSpoolVisual();
            }
            else
            {
                HornetInHallownest.Instance?.Log("[SilkManager] HudSpriteManager not found for silk update");
            }
        }

        public static bool CanUseSilk(int amount)
        {
            return Instance != null && Instance._silkSystemActive && CurrentSilk >= amount;
        }

        public static float GetSilkPercentage()
        {
            return MaxSilk > 0 ? (float)CurrentSilk / MaxSilk : 0f;
        }
    }
}
