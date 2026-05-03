using System.Collections.Generic;
using System.Reflection;
using Modding;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using System.Linq;

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

            // Resource name format example: HornetInHallownest.Resources.Every_Hornet_Animation.<Folder>.<AnimName>.<NNN-FF-ID>.png
            // but we are finding jsons this time
            var assembly = Assembly.GetExecutingAssembly();
            var sprites  = new Dictionary<string, Sprite>();
            HornetSpriteDriver.FrameAnimSprites = new Dictionary<string, List<Sprite>>();
            // Hornet sprites are ~180-215 px; at 64 PPU they match tk2d world-unit scale
            const float ppu = 64f;


            foreach (var resourceName in assembly.GetManifestResourceNames())
            {
                // retrieving JSON file
                if (!resourceName.EndsWith("SpriteInfo.json")) continue;

                SpriteInfoJSONParser spriteInfo = null;

                using (var stream = assembly.GetManifestResourceStream(resourceName)) {
                    if (stream == null) {
                        Modding.Logger.Log("Found a SpriteInfo.json file in assembly resources but could not establish a file stream.");
                        continue;
                    }
                    
                    using (var reader = new StreamReader(stream)) {
                            string json = reader.ReadToEnd();
                            spriteInfo = JsonConvert.DeserializeObject<SpriteInfoJSONParser>(json);
                    }
                }

                if (spriteInfo == null) {
                    Modding.Logger.Log("Failed to parse SpriteInfo.json file into SpriteInfoJSONParser object.");
                    continue;
                }

                var currAtlasTextures = new Dictionary<string, Texture2D>();
                var skippedFrames = 0;

                foreach (var collectionname in spriteInfo.scollectionname.Distinct().ToList()) {
                    var parts = resourceName.Split('.');
                    var atlasPath = string.Join(".", parts.Take(parts.Length - 2)) + "." + collectionname + ".png";

                    using (Stream stream = assembly.GetManifestResourceStream(atlasPath)) {
                        if (stream == null) {
                            Modding.Logger.Log($"Failed to find atlas image for collection '{collectionname}' at expected path '{atlasPath}'.");
                            continue;
                        }
                        var bytes = new byte[stream.Length];
                        stream.Read(bytes, 0, bytes.Length);

                        var atlasTex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                        atlasTex.filterMode = FilterMode.Bilinear;
                        atlasTex.LoadImage(bytes);
                        currAtlasTextures[collectionname] = atlasTex;
                    }
                }

                // performing JSON file:
                for (var n = 0; n < spriteInfo.sid.Length; n++) { // each item in JSON
                    var sid = spriteInfo.sid[n];
                    var sx = spriteInfo.sx[n];
                    var sy = spriteInfo.sy[n];
                    var sxr = spriteInfo.sxr[n];
                    var syr = spriteInfo.syr[n];
                    var swidth = spriteInfo.swidth[n];
                    var sheight = spriteInfo.sheight[n];
                    var scollectionname = spriteInfo.scollectionname[n];
                    var spath = spriteInfo.spath[n].Split(new char[] {'/','.'});
                    var sflipped = spriteInfo.sflipped?[n] ?? false;

                    var frameName = spath[spath.Length - 2]; // e.g. "001-00-873"
                    var animFolder = spath[spath.Length - 3];
                    var animName = spath.Length >= 4 ? spath[spath.Length - 4] : string.Empty;
                    var fullAnimKey = string.IsNullOrEmpty(animName) ? animFolder : animFolder + "/" + animName;

                    if (!currAtlasTextures.TryGetValue(scollectionname, out var tex)) {
                        Modding.Logger.Log($"SpriteInfo JSON references collection '{scollectionname}' which was not found among the embedded atlas resources. Skipping sprite '{frameName}'.");
                        skippedFrames++;
                        continue;
                    }

                    var rect = !sflipped ? 
                        new Rect(sx, sy, swidth, sheight)
                        : new Rect(sx, sy, sheight, swidth);

                    if (rect.xMin < 0 || rect.yMin < 0 || rect.xMax > tex.width || rect.yMax > tex.height) {
                        Modding.Logger.Log($"Invalid sprite rect {rect} in atlas '{scollectionname}' for frame '{frameName}'. Skipping.");
                        skippedFrames++;
                        continue;
                    }


                    var pivot = 
                        new Vector2(
                            0.5f + sxr / (float)swidth,
                            0.5f + syr / (float)sheight
                        );

                    Sprite spr;
                    if (sflipped) {
                        var flippedTex = new Texture2D(swidth, sheight, TextureFormat.RGBA32, false);
                        flippedTex.filterMode = FilterMode.Bilinear;
                        for (int x = 0; x < swidth; x++) {
                            for (int y = 0; y < sheight; y++) {
                                flippedTex.SetPixel(x, y, tex.GetPixel(sx + y, sy + x));
                            }
                        }
                        flippedTex.Apply();

                        spr = Sprite.Create(
                            flippedTex,
                            new Rect(0, 0, swidth, sheight),
                            pivot,
                            ppu
                        );

                    } else {
                        spr = Sprite.Create(
                            tex,
                            rect,
                            pivot,
                            ppu
                        );
                    }
                    sprites[frameName] = spr;

                    if (!HornetSpriteDriver.FrameAnimSprites.TryGetValue(fullAnimKey, out var animList))
                    {
                        animList = new List<Sprite>();
                        HornetSpriteDriver.FrameAnimSprites[fullAnimKey] = animList;
                    }
                    animList.Add(spr);
                }
                Log($"Loaded {sprites.Count} Hornet frames into {HornetSpriteDriver.FrameAnimSprites.Count} animation categories, skipped {skippedFrames} invalid frames");
            }

            HornetSpriteDriver.FrameSprites = sprites;
            
        }

        private class SpriteInfoJSONParser {
            public int[] sid {get; set;}
            public int[] sx {get; set;}
            public int[] sy {get; set;}
            public int[] sxr {get; set;}
            public int[] syr {get; set;}
            public int[] swidth {get; set;}
            public int[] sheight {get; set;}
            public string[] scollectionname {get; set;}
            public string[] spath {get; set;}
            [JsonProperty("sfilpped")]
            public bool[] sflipped {get; set;}
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
            self.gameObject.AddComponent<SilksongHUDManager>();
            self.gameObject.AddComponent<CrestSwitchManager>();
            self.gameObject.AddComponent<SilksongInventoryManager>();
            self.gameObject.AddComponent<SilkManager>();

            if (!_maxHpBoosted && PlayerData.instance != null)
            {
                PlayerData.instance.AddToMaxHealth(1);
                self.MaxHealth();
                _maxHpBoosted = true;
                Log("Max HP boosted by 1");
            }

            Log("HornetSpriteDriver + CrestManager + MovementManager + HudSpriteManager + SilksongHUDManager + CrestSwitchManager + SilksongInventoryManager + SilkManager attached");
        }
    }
}
