using System.Collections.Generic;
using System.Reflection;
using Modding;
using UnityEngine;

namespace HornetInHallownest
{
    public class HornetInHallownest : Mod
    {
        internal static HornetInHallownest Instance;

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
            // Hornet sprites are ~180-215 px; at 64 PPU they match tk2d world-unit scale
            const float ppu = 64f;

            foreach (var resourceName in assembly.GetManifestResourceNames())
            {
                if (!resourceName.EndsWith(".png")) continue;

                // Resource name format: HornetInHallownest.Resources.Sprites.<frameName>.png
                var parts = resourceName.Split('.');
                if (parts.Length < 2) continue;
                var frameName = parts[parts.Length - 2]; // e.g. "idle0000"

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
                    new Vector2(0.5f, 0.5f),
                    ppu
                );
                sprites[frameName] = spr;
            }

            HornetSpriteDriver.FrameSprites = sprites;
            Log($"Loaded {sprites.Count} Hornet frames");
        }

        private void OnHeroStart(On.HeroController.orig_Start orig, HeroController self)
        {
            orig(self);

            // Remove any stale driver from a previous scene
            var old = self.GetComponent<HornetSpriteDriver>();
            if (old != null) Object.Destroy(old);

            self.gameObject.AddComponent<HornetSpriteDriver>();
            Log("HornetSpriteDriver attached");
        }
    }
}
