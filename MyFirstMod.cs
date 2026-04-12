using System.Collections.Generic;
using System.Reflection;
using Modding;
using UnityEngine;

namespace HornetInHallownest
{
    public class HornetInHallownest : Mod
    {
        internal static HornetInHallownest Instance;
    private static bool _maxHpBoosted;
        public HornetInHallownest() : base("Hornet In Hallownest") { }

        public override string GetVersion() => "v1";

        public override void Initialize()
        {
            Instance = this;
            LoadHornetSprites();
            On.HeroController.Start += OnHeroStart;
        }

        private void LoadHornetSprites()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var sprites  = new Dictionary<string, Sprite>();
            HornetSpriteDriver.FrameAnimSprites = new Dictionary<string, List<Sprite>>();
            // Hornet sprites are ~180-215 px; at 64 PPU they match tk2d world-unit scale
            const float ppu = 64f;

            foreach (var resourceName in assembly.GetManifestResourceNames())
            {
                if (!resourceName.EndsWith(".png")) continue;

                // Resource name format: HornetInHallownest.Resources.Every_Hornet_Animation.<Folder>.<AnimName>.<NNN-FF-ID>.png
                // (dots in folder names become extra segments — frame key is always the second-to-last segment)
                var parts = resourceName.Split('.');
                if (parts.Length < 3) continue;
                var frameName = parts[parts.Length - 2]; // e.g. "001-00-873"
                var animFolder = parts[parts.Length - 3];
                var animName = parts.Length >= 4 ? parts[parts.Length - 4] : string.Empty;
                var fullAnimKey = string.IsNullOrEmpty(animName) ? animFolder : animFolder + "/" + animName;

                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream == null) continue;

                var bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);

                var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                tex.filterMode = FilterMode.Bilinear;
                tex.LoadImage(bytes);

                var spr = Sprite.Create(
                    tex,
                    new Rect(0, 0, tex.width, tex.height),
                    new Vector2(0.5f, 0.5f), // center pivot: centers sprite on transform position
                    ppu
                );
                sprites[frameName] = spr;

                if (!HornetSpriteDriver.FrameAnimSprites.TryGetValue(fullAnimKey, out var animList))
                {
                    animList = new List<Sprite>();
                    HornetSpriteDriver.FrameAnimSprites[fullAnimKey] = animList;
                }
                animList.Add(spr);
            }

            HornetSpriteDriver.FrameSprites = sprites;
            Log($"Loaded {sprites.Count} Hornet frames into {HornetSpriteDriver.FrameAnimSprites.Count} animation categories");
        }

        private void OnHeroStart(On.HeroController.orig_Start orig, HeroController self)
        {
            orig(self);

            // Remove any stale components from a previous scene
            var oldDriver = self.GetComponent<HornetSpriteDriver>();
            if (oldDriver != null) Object.Destroy(oldDriver);

            var oldCrest = self.GetComponent<CrestManager>();
            if (oldCrest != null) Object.Destroy(oldCrest);

            var oldMovement = self.GetComponent<MovementManager>();
            if (oldMovement != null) Object.Destroy(oldMovement);

            var oldHud = self.GetComponent<HudSpriteManager>();
            if (oldHud != null) Object.Destroy(oldHud);

            self.gameObject.AddComponent<HornetSpriteDriver>();
            self.gameObject.AddComponent<CrestManager>();
            self.gameObject.AddComponent<MovementManager>();
            self.gameObject.AddComponent<HudSpriteManager>();

            if (!_maxHpBoosted && PlayerData.instance != null)
            {
                PlayerData.instance.AddToMaxHealth(1);
                self.MaxHealth();
                _maxHpBoosted = true;
                Log("Max HP boosted by 1");
            }

            Log("HornetSpriteDriver + CrestManager + MovementManager + HudSpriteManager attached");
        }
    }
}
