using UnityEngine;
using Modding;
using System.Collections.Generic;

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
        private static int _savedMPCharge;
        private static int _savedMPReserve;

        // Saved silk values for character switching
        private static int _savedSilk;

        private bool _silkSystemActive = false;

        void Awake()
        {
            Instance = this;
        }

        void OnEnable()
        {
            // Hook into enemy hit events for silk gain
            On.HealthManager.Hit += OnEnemyHit;

            // Hook into healing to block it when Hornet is active
            // (Hornet uses silk-based healing instead of Knight's focus)
            On.PlayerData.AddHealth += OnAddHealth;
        }

        void OnDisable()
        {
            On.HealthManager.Hit -= OnEnemyHit;
            On.PlayerData.AddHealth -= OnAddHealth;
        }

        void Start()
        {
            if (HornetSpriteDriver.IsEnabled)
            {
                EnableSilkSystem();
            }
        }

        void Update()
        {
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

        private void EnableSilkSystem()
        {
            if (_silkSystemActive) return;

            // Save current soul values and zero them out while Hornet is active
            if (PlayerData.instance != null)
            {
                _savedMPCharge = PlayerData.instance.MPCharge;
                _savedMPReserve = PlayerData.instance.MPReserve;

                PlayerData.instance.MPCharge = 0;
                PlayerData.instance.MPReserve = 0;
            }

            // Restore saved silk if switching back to Hornet
            CurrentSilk = _savedSilk;

            _silkSystemActive = true;
            HornetInHallownest.Instance?.Log("[SilkManager] Silk system enabled");

            UpdateSilkHUD();
        }

        private void DisableSilkSystem()
        {
            if (!_silkSystemActive) return;

            _savedSilk = CurrentSilk;

            // Restore Knight's soul values
            if (PlayerData.instance != null)
            {
                PlayerData.instance.MPCharge = _savedMPCharge;
                PlayerData.instance.MPReserve = _savedMPReserve;
            }

            CurrentSilk = 0;
            _silkSystemActive = false;
            HornetInHallownest.Instance?.Log("[SilkManager] Silk system disabled");
        }

        private void OnEnemyHit(On.HealthManager.orig_Hit orig, HealthManager self, HitInstance hitInstance)
        {
            orig(self, hitInstance);

            // Grant silk for any hit while Hornet is active
            if (_silkSystemActive)
            {
                AddSilk(1);
            }
        }

        private void OnAddHealth(On.PlayerData.orig_AddHealth orig, PlayerData self, int amount)
        {
            // Block Knight's focus healing while Hornet is active
            // Hornet uses silk-based thread healing instead
            if (_silkSystemActive)
            {
                HornetInHallownest.Instance?.Log("[SilkManager] Blocked focus heal (Hornet uses silk healing)");
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
            // SilksongHUDManager owns the text-based silk display
            var hudManager = FindObjectOfType<SilksongHUDManager>();
            if (hudManager != null)
            {
                hudManager.RefreshSilkDisplay();
            }
            else
            {
                HornetInHallownest.Instance?.Log("[SilkManager] SilksongHUDManager not found for silk update");
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
