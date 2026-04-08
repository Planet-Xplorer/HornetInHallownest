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

                // Silksong's atlas packs some sprites with FlipMode.Tk2d (stored 90° CW).
                // The extractor doesn't un-rotate them, so landscape PNGs need a 90° CCW fix.
                if (tex.width > tex.height * 1.08f)
                    tex = Rotate90CCW(tex);

                var spr = Sprite.Create(
                    tex,
                    new Rect(0, 0, tex.width, tex.height),
                    new Vector2(0.5f, 0.0f), // bottom-center pivot: anchors sprite at feet level
                    ppu
                );
                sprites[frameName] = spr;
            }

            HornetSpriteDriver.FrameSprites = sprites;
            Log($"Loaded {sprites.Count} Hornet frames");
        }

        // Rotate a texture 90° counter-clockwise.
        // Formula (bottom-left origin): new(nx, ny) = old(ny, H-1-nx)
        // New dimensions: newW=H_old, newH=W_old
        private static Texture2D Rotate90CCW(Texture2D src)
        {
            int w = src.width, h = src.height;
            var pixels = src.GetPixels();
            var rot    = new Color[w * h];
            for (int nx = 0; nx < h; nx++)
                for (int ny = 0; ny < w; ny++)
                    rot[ny * h + nx] = pixels[(h - 1 - nx) * w + ny];
            var dst = new Texture2D(h, w, TextureFormat.RGBA32, false);
            dst.filterMode = FilterMode.Bilinear;
            dst.SetPixels(rot);
            dst.Apply();
            return dst;
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
