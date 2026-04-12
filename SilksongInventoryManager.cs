using System;
using System.Collections.Generic;
using UnityEngine;
using GlobalEnums;
using Modding;

namespace HornetInHallownest
{
    /// <summary>
    /// Manages Silksong-style inventory integration with Hollow Knight's UI system.
    /// Replaces Knight inventory tab with Hornet's end-game inventory and adds dual-mode charms tab.
    /// </summary>
    public class SilksongInventoryManager : MonoBehaviour
    {
        public static SilksongInventoryManager Instance { get; private set; }

        [Header("Inventory Configuration")]
        public bool EnableSilksongInventory = true;
        public bool EnableDualModeCharms = true;
        public KeyCode ToggleModeKey = KeyCode.Tab;

        // Inventory state
        private bool _isSilksongMode = false;
        private bool _charmsTabInHornetMode = false;

        // UI references
        private GameObject _inventoryUI;
        private GameObject _charmsUI;
        private GameObject _modeToggleButton;

        // Inventory data placeholders (for future asset integration)
        private List<InventoryItem> _hornetItems = new List<InventoryItem>();
        private List<ToolItem> _hornetTools = new List<ToolItem>();
        private List<CrestItem> _hornetCrests = new List<CrestItem>();

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
            InitializeInventorySystem();
            RegisterUIHooks();
        }

        void OnDestroy()
        {
            UnregisterUIHooks();
        }

        private void InitializeInventorySystem()
        {
            // Initialize placeholder inventory data
            InitializePlaceholderData();

            HornetInHallownest.Instance.Log("[SilksongInventoryManager] Inventory system initialized");
        }

        private void InitializePlaceholderData()
        {
            // TODO: Replace with actual Silksong assets when available
            _hornetItems = new List<InventoryItem>
            {
                new InventoryItem { Name = "Hornet's Needle", Description = "Primary weapon", IconName = "hornet_needle" },
                new InventoryItem { Name = "Silk Spool", Description = "Silk reservoir", IconName = "silk_spool" },
                new InventoryItem { Name = "Hunter's Journal", Description = "Quest log", IconName = "hunters_journal" }
            };

            _hornetTools = new List<ToolItem>
            {
                new ToolItem { Name = "Crest Slot", Description = "Crest equipment", IconName = "crest_slot" },
                new ToolItem { Name = "Tool Belt", Description = "Tool storage", IconName = "tool_belt" }
            };

            _hornetCrests = new List<CrestItem>
            {
                new CrestItem { Type = CrestType.Hunter, Name = "Hunter's Crest", Description = "Balanced combat", IconName = "hunter_crest_v3" },
                new CrestItem { Type = CrestType.Reaper, Name = "Reaper Crest", Description = "High damage", IconName = "reaper_crest" },
                new CrestItem { Type = CrestType.Wanderer, Name = "Wanderer's Crest", Description = "Piercing attacks", IconName = "wanderer_crest" },
                new CrestItem { Type = CrestType.Warrior, Name = "Beast Crest", Description = "Heavy damage", IconName = "warrior_crest" },
                new CrestItem { Type = CrestType.Witch, Name = "Witch Crest", Description = "Magic attacks", IconName = "witch_crest" },
                new CrestItem { Type = CrestType.Toolmaster, Name = "Architect Crest", Description = "Tool-based", IconName = "toolmaster_crest" },
                new CrestItem { Type = CrestType.Spinner, Name = "Shaman Crest", Description = "Spinning attacks", IconName = "spinner_crest" },
                new CrestItem { Type = CrestType.Cloakless, Name = "Cloakless Crest", Description = "No restrictions", IconName = "cloakless_crest" },
                new CrestItem { Type = CrestType.Cursed, Name = "Cursed Crest", Description = "Corrupted power", IconName = "cursed_crest" }
            };
        }

        private void RegisterUIHooks()
        {
            // Hook into inventory UI events
            On.UIManager.ShowInventory += OnShowInventory;
            On.UIManager.ShowCharms += OnShowCharms;
            On.UIManager.HideInventory += OnHideInventory;
            On.UIManager.HideCharms += OnHideCharms;
        }

        private void UnregisterUIHooks()
        {
            On.UIManager.ShowInventory -= OnShowInventory;
            On.UIManager.ShowCharms -= OnShowCharms;
            On.UIManager.HideInventory -= OnHideInventory;
            On.UIManager.HideCharms -= OnHideCharms;
        }

        private void OnShowInventory(On.UIManager.orig_ShowInventory orig, UIManager self)
        {
            if (!EnableSilksongInventory || !HornetSpriteDriver.IsEnabled)
            {
                orig(self);
                return;
            }

            // Show Silksong-style inventory instead of Knight inventory
            ShowSilksongInventory();
        }

        private void OnShowCharms(On.UIManager.orig_ShowCharms orig, UIManager self)
        {
            if (!EnableDualModeCharms || !HornetSpriteDriver.IsEnabled)
            {
                orig(self);
                return;
            }

            // Show dual-mode charms interface
            ShowDualModeCharms();
        }

        private void OnHideInventory(On.UIManager.orig_HideInventory orig, UIManager self)
        {
            orig(self);
            CleanupSilksongInventory();
        }

        private void OnHideCharms(On.UIManager.orig_HideCharms orig, UIManager self)
        {
            orig(self);
            CleanupDualModeCharms();
        }

        private void ShowSilksongInventory()
        {
            // Find or create inventory UI container
            var uiManager = UIManager.instance;
            if (uiManager == null) return;

            // Hide original inventory
            var originalInventory = GameObject.Find("Inventory");
            if (originalInventory != null)
            {
                originalInventory.SetActive(false);
            }

            // Create Silksong inventory UI
            CreateSilksongInventoryUI();
        }

        private void CreateSilksongInventoryUI()
        {
            _inventoryUI = new GameObject("Silksong Inventory");
            _inventoryUI.transform.SetParent(UIManager.instance.transform);

            // TODO: Replace with actual Silksong UI prefab when assets are available
            // For now, create a basic placeholder structure

            // Main panel
            var panel = new GameObject("Panel");
            panel.transform.SetParent(_inventoryUI.transform);
            
            var panelRenderer = panel.AddComponent<SpriteRenderer>();
            panelRenderer.sprite = CreatePlaceholderSprite("inventory_panel");
            panelRenderer.sortingLayerName = "UI";
            panelRenderer.sortingOrder = 50;

            // Inventory sections
            CreateInventorySection("Items", _hornetItems, new Vector3(-2f, 1f, 0f));
            CreateInventorySection("Tools", _hornetTools, new Vector3(0f, 1f, 0f));
            CreateInventorySection("Crests", _hornetCrests, new Vector3(2f, 1f, 0f));

            HornetInHallownest.Instance.Log("[SilksongInventoryManager] Silksong inventory UI created");
        }

        private void CreateInventorySection(string sectionName, IEnumerable items, Vector3 position)
        {
            var section = new GameObject(sectionName);
            section.transform.SetParent(_inventoryUI.transform);
            section.transform.localPosition = position;

            // Section header
            var header = new GameObject("Header");
            header.transform.SetParent(section.transform);
            
            var headerText = header.AddComponent<TextMesh>();
            headerText.text = sectionName;
            headerText.fontSize = 16;
            headerText.alignment = TextAlignment.Center;
            headerText.color = Color.white;

            // Item grid (placeholder)
            int index = 0;
            foreach (var item in items)
            {
                var itemGO = new GameObject($"Item_{index}");
                itemGO.transform.SetParent(section.transform);
                itemGO.transform.localPosition = new Vector3((index % 3) * 1.5f - 1.5f, -(index / 3) * 1.5f, 0f);

                var itemRenderer = itemGO.AddComponent<SpriteRenderer>();
                itemRenderer.sprite = CreatePlaceholderSprite($"item_{item.GetType().Name}_{index}");
                itemRenderer.sortingLayerName = "UI";
                itemRenderer.sortingOrder = 51;

                index++;
            }
        }

        private void ShowDualModeCharms()
        {
            // Find original charms UI
            var originalCharms = GameObject.Find("Charms");
            if (originalCharms != null)
            {
                originalCharms.SetActive(false);
            }

            // Create dual-mode charms interface
            CreateDualModeCharmsUI();
        }

        private void CreateDualModeCharmsUI()
        {
            _charmsUI = new GameObject("Dual Mode Charms");
            _charmsUI.transform.SetParent(UIManager.instance.transform);

            // Mode toggle button
            _modeToggleButton = new GameObject("Mode Toggle");
            _modeToggleButton.transform.SetParent(_charmsUI.transform);
            _modeToggleButton.transform.localPosition = new Vector3(0f, 2f, 0f);

            var buttonRenderer = _modeToggleButton.AddComponent<SpriteRenderer>();
            buttonRenderer.sprite = CreatePlaceholderSprite("mode_toggle_button");
            buttonRenderer.sortingLayerName = "UI";
            buttonRenderer.sortingOrder = 60;

            var buttonText = _modeToggleButton.AddComponent<TextMesh>();
            buttonText.text = _charmsTabInHornetMode ? "Knight Charms" : "Hornet Tools";
            buttonText.fontSize = 14;
            buttonText.alignment = TextAlignment.Center;
            buttonText.color = Color.white;

            // Current mode content
            if (_charmsTabInHornetMode)
            {
                ShowHornetToolsContent();
            }
            else
            {
                ShowKnightCharmsContent();
            }

            HornetInHallownest.Instance.Log($"[SilksongInventoryManager] Dual-mode charms UI created (Mode: {(_charmsTabInHornetMode ? "Hornet" : "Knight")})");
        }

        private void ShowHornetToolsContent()
        {
            // TODO: Implement Hornet tools interface when assets are available
            var toolsContainer = new GameObject("Hornet Tools");
            toolsContainer.transform.SetParent(_charmsUI.transform);

            // Placeholder for tools interface
            var toolsText = new GameObject("Tools Label");
            toolsText.transform.SetParent(toolsContainer.transform);
            
            var textMesh = toolsText.AddComponent<TextMesh>();
            textMesh.text = "Hornet Tools & Crests\n(Coming Soon)";
            textMesh.fontSize = 12;
            textMesh.alignment = TextAlignment.Center;
            textMesh.color = Color.gray;
        }

        private void ShowKnightCharmsContent()
        {
            // Show original Knight charms interface
            var originalCharms = GameObject.Find("Charms");
            if (originalCharms != null)
            {
                originalCharms.SetActive(true);
                originalCharms.transform.SetParent(_charmsUI.transform);
            }
        }

        private void CleanupSilksongInventory()
        {
            if (_inventoryUI != null)
            {
                Destroy(_inventoryUI);
                _inventoryUI = null;
            }

            // Restore original inventory
            var originalInventory = GameObject.Find("Inventory");
            if (originalInventory != null)
            {
                originalInventory.SetActive(true);
            }
        }

        private void CleanupDualModeCharms()
        {
            if (_charmsUI != null)
            {
                // Restore original charms if it was a child
                var originalCharms = _charmsUI.transform.Find("Charms");
                if (originalCharms != null)
                {
                    originalCharms.SetParent(null);
                    originalCharms.gameObject.SetActive(false);
                }

                Destroy(_charmsUI);
                _charmsUI = null;
            }

            // Restore original charms
            var originalCharms = GameObject.Find("Charms");
            if (originalCharms != null)
            {
                originalCharms.SetActive(false);
            }
        }

        // Placeholder sprite creation (for development)
        private Sprite CreatePlaceholderSprite(string name)
        {
            // Create a simple colored rectangle as placeholder
            var texture = new Texture2D(64, 64);
            var colors = new Color32[64 * 64];
            
            // Different colors for different placeholders
            var color = name switch
            {
                var s when s.Contains("inventory") => new Color32(100, 100, 100, 200),
                var s when s.Contains("item") => new Color32(150, 150, 150, 200),
                var s when s.Contains("button") => new Color32(200, 200, 100, 200),
                var s when s.Contains("hunter_crest") => new Color32(255, 200, 100, 200),
                var s when s.Contains("crest") => new Color32(200, 150, 255, 200),
                _ => new Color32(128, 128, 128, 200)
            };

            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = color;
            }

            texture.SetPixels32(colors);
            texture.Apply();

            return Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));
        }

        // Public API for external systems
        public void ToggleMode()
        {
            _charmsTabInHornetMode = !_charmsTabInHornetMode;
            
            // Refresh UI if currently showing charms
            if (_charmsUI != null)
            {
                CleanupDualModeCharms();
                CreateDualModeCharmsUI();
            }
        }

        public void SetSilksongMode(bool enabled)
        {
            _isSilksongMode = enabled;
        }

        public bool IsInSilksongMode() => _isSilksongMode;

        public bool IsCharmsTabInHornetMode() => _charmsTabInHornetMode;

        // Inventory data classes (placeholders for future implementation)
        [Serializable]
        public class InventoryItem
        {
            public string Name;
            public string Description;
            public string IconName;
            public int Quantity = 1;
        }

        [Serializable]
        public class ToolItem
        {
            public string Name;
            public string Description;
            public string IconName;
            public bool IsEquipped;
        }

        [Serializable]
        public class CrestItem
        {
            public CrestType Type;
            public string Name;
            public string Description;
            public string IconName;
            public bool IsUnlocked = true;
            public int Level = 1; // For v2/v3 upgrades
        }
    }
}
