using System.Collections.Generic;
using UnityEngine;

namespace HornetInHallownest
{
    // Attached to the Knight's GameObject; replaces the body sprite with Hornet's.
    internal class HornetSpriteDriver : MonoBehaviour
    {
        // Populated by HornetInHallownest.LoadHornetSprites()
        internal static Dictionary<string, Sprite> FrameSprites;

        // Toggle: press ToggleKey to switch between Hornet and Knight skins.
        // Static so state persists across scene loads.
        public static bool    IsEnabled  = true;
        public static KeyCode ToggleKey  = KeyCode.F5;

        // Maps HK animation clip names → ordered arrays of Hornet sprite frame keys.
        //
        // Frame key format: "{anim_number}-{frame_index}-{sprite_id}"
        // Source folder:    Resources/Every_Hornet_Animation/Knight/{anim_number}.{AnimName}/
        // Each key is the filename minus ".png" and is unique across all animations.
        //
        // HK clip names were verified from Assembly-CSharp.dll (HeroAnimationController).
        // Hornet animation folder names come from Silksong's exported sprite library.
        // Where no direct Silksong equivalent exists, the closest motion is used as a fallback.
        internal static readonly Dictionary<string, string[]> AnimMap =
            new Dictionary<string, string[]>
        {
            // ── Idle (001.Idle) ───────────────────────────────────────────
            { "Idle",              new[] { "001-00-873","001-01-875","001-02-869","001-03-874","001-04-877","001-05-879" } },
            { "Idle Hurt",         new[] { "001-00-873","001-01-875","001-02-869","001-03-874","001-04-877","001-05-879" } },
            { "Lantern Idle",      new[] { "001-00-873","001-01-875","001-02-869","001-03-874","001-04-877","001-05-879" } },

            // ── Look Up / Down (014.LookUp, 013.LookDown) ────────────────
            { "LookUp",            new[] { "014-00-1118","014-01-1116","014-02-1120","014-03-1117","014-04-1119","014-05-1121","014-06-1123","014-07-1122" } },
            { "Lookup",            new[] { "014-00-1118","014-01-1116","014-02-1120","014-03-1117","014-04-1119","014-05-1121","014-06-1123","014-07-1122" } },
            { "LookUpEnd",         new[] { "014-07-1122","014-05-1121","014-02-1120","014-01-1116","014-00-1118" } },
            { "LookDown",          new[] { "013-00-1108","013-01-1115","013-02-1113","013-03-1111","013-04-1112","013-05-1109","013-06-1110","013-07-1114" } },
            { "LookDownEnd",       new[] { "013-07-1114","013-05-1109","013-02-1113","013-01-1115","013-00-1108" } },

            // ── Run / Walk / Sprint (005.Run, 132.Sprint) ─────────────────
            { "Run",               new[] { "005-00-1162","005-01-1164","005-02-1171","005-03-1170","005-04-1166","005-05-1168","005-06-1167","005-07-1165","005-08-1163","005-09-1169" } },
            { "Walk",              new[] { "005-00-1162","005-01-1164","005-02-1171","005-03-1170","005-04-1166","005-05-1168","005-06-1167","005-07-1165","005-08-1163","005-09-1169" } },
            { "Sprint",            new[] { "132-00-1213","132-01-1214","132-02-1212","132-03-1215","132-04-1208","132-05-1211","132-06-1207","132-07-1210" } },
            { "Lantern Run",       new[] { "005-00-1162","005-01-1164","005-02-1171","005-03-1170","005-04-1166","005-05-1168","005-06-1167","005-07-1165","005-08-1163","005-09-1169" } },

            // ── Turn (048.Turn) ───────────────────────────────────────────
            { "Turn",              new[] { "048-00-815","048-01-814","048-02-817","048-03-705","048-04-820","048-05-816" } },

            // ── Idle ↔ Run transitions (049.Run To Idle, 237.Idle To Run, 062.TurnToIdle) ──
            { "Run To Idle",       new[] { "049-00-096","049-01-095","049-02-099","049-03-100","049-04-097","049-05-098" } },
            { "Backdash To Idle",  new[] { "062-00-1244","062-01-1245","062-02-1243","062-03-873","062-04-875","062-05-869","062-06-874","062-07-877","062-08-879" } },
            { "Dash To Idle",      new[] { "062-00-1244","062-01-1245","062-02-1243","062-03-873","062-04-875","062-05-869","062-06-874","062-07-877","062-08-879" } },
            { "Idle To Run",       new[] { "237-00-1063","237-01-1060","237-02-1062","237-03-340","237-04-341","237-05-342","237-06-344","237-07-1061","237-08-1064","237-09-094" } },
            { "Land To Run",       new[] { "237-00-1063","237-01-1060","237-02-1062","237-03-340","237-04-341","237-05-342","237-06-344","237-07-1061","237-08-1064","237-09-094" } },

            // ── Airborne / Jump (003.Airborne) ────────────────────────────
            { "Airborne",          new[] { "003-00-1099","003-01-1103","003-02-695","003-03-1105","003-04-1095","003-05-1094","003-06-1096","003-07-1106","003-08-1098","003-09-1100","003-10-1107","003-11-1101","003-12-1104","003-13-1097","003-14-694" } },

            // ── Double Jump (098.Double Jump) ─────────────────────────────
            // Hornet always has double jump (see MovementManager.CanDoubleJump)
            { "Double Jump",       new[] { "098-00-1067","098-01-1077","098-02-506","098-03-1074","098-04-505","098-05-1065","098-06-1068","098-07-1078","098-08-345","098-09-005","098-10-002","098-11-003","098-12-004","098-13-002","098-14-003","098-15-004","098-16-006" } },

            // ── Wall Jump / Cling (133.Walljump, 035.Mantle Cling) ────────
            { "Walljump",          new[] { "133-00-1084","133-01-1092","133-02-1086","133-03-1079","133-04-1084","133-05-1092","133-06-1086","133-07-1079","133-08-1085","133-09-1090","133-10-1089","133-11-1093","133-12-1088","133-13-1083" } },
            { "Wall Slide",        new[] { "084-00-1681","084-01-1687","084-02-386","084-03-388","084-04-390","084-05-507","084-06-387","084-07-389","084-08-391" } },
            { "Super Dash",        new[] { "174-00-1368" } },
            { "SuperDash",         new[] { "174-00-1368" } },
            { "Harpoon Throw",     new[] { "173-00-1378","173-01-1372","173-02-1373" } },
            { "Harpoon Catch",     new[] { "175-00-1377","175-01-1367" } },
            { "Harpoon Thread",    new[] { "089-00-231","089-01-231","089-02-247","089-03-248","089-04-249","089-05-250" } },
            { "Harpoon Needle",    new[] { "176-00-1374","176-01-1371","176-02-1369" } },
            { "Harpoon Needle Wall Hit", new[] { "289-00-1371","289-01-351","289-02-347","289-03-346","289-04-348","289-05-1369" } },
            { "Harpoon Needle Return", new[] { "290-00-349","290-01-350","290-02-352" } },
            { "Harpoon Catch Back", new[] { "293-00-1372","293-01-1378" } },

            // ── Landing (011.Land, 012.HardLand) ─────────────────────────
            { "Land",              new[] { "011-00-1046","011-01-1049","011-02-1204","011-03-1204","011-04-1203","011-05-1203","011-06-099","011-07-099","011-08-100","011-09-100","011-10-097","011-11-097","011-12-098","011-13-098" } },
            { "HardLand",          new[] { "012-00-1046","012-01-1049","012-02-1047","012-03-1050","012-04-1050","012-05-1050","012-06-1048","012-07-1045","012-08-1045","012-09-1045","012-10-1045","012-11-1045" } },
            { "Backdash Land",     new[] { "011-00-1046","011-01-1049","011-02-1204","011-03-1204","011-04-1203","011-05-1203","011-06-099","011-07-099","011-08-100","011-09-100","011-10-097","011-11-097","011-12-098","011-13-098" } },
            { "Backdash Land 2",   new[] { "011-00-1046","011-01-1049","011-02-1204","011-03-1204","011-04-1203","011-05-1203","011-06-099","011-07-099","011-08-100","011-09-100","011-10-097","011-11-097","011-12-098","011-13-098" } },
            { "Dash Down Land",    new[] { "012-00-1046","012-01-1049","012-02-1047","012-03-1050","012-04-1050","012-05-1050","012-06-1048","012-07-1045","012-08-1045","012-09-1045","012-10-1045","012-11-1045" } },

            // ── Dash (002.Dash) ───────────────────────────────────────────
            // Shadow Dash is locked out by MovementManager — HK falls back to Dash automatically.
            { "Dash",              new[] { "002-00-953","002-01-955","002-02-952","002-03-954","002-04-954","002-05-954","002-06-954","002-07-954","002-08-954" } },
            { "Back Dash",         new[] { "002-00-953","002-01-955","002-02-952","002-03-954","002-04-954","002-05-954","002-06-954","002-07-954","002-08-954" } },
            { "Shadow Dash",       new[] { "002-00-953","002-01-955","002-02-952","002-03-954","002-04-954","002-05-954","002-06-954","002-07-954","002-08-954" } },
            { "Shadow Dash Sharp", new[] { "002-00-953","002-01-955","002-02-952","002-03-954","002-04-954","002-05-954","002-06-954","002-07-954","002-08-954" } },
            // Dash Down / Quake Fall → use Airborne (Hornet falls needle-first)
            { "Dash Down",         new[] { "003-07-1106","003-08-1098","003-09-1100","003-10-1107","003-11-1101","003-12-1104","003-13-1097","003-14-694" } },
            { "Shadow Dash Down",  new[] { "003-07-1106","003-08-1098","003-09-1100","003-10-1107","003-11-1101","003-12-1104","003-13-1097","003-14-694" } },
            { "Shadow Dash Down Sharp", new[] { "003-07-1106","003-08-1098","003-09-1100","003-10-1107","003-11-1101","003-12-1104","003-13-1097","003-14-694" } },

            // ── Attacks (006.Slash, 004.SlashAlt, 009.UpSlash, 010.DownSpike, 125.Wall Slash) ──
            { "Slash",             new[] { "006-00-901","006-01-728","006-02-907","006-03-826","006-04-901" } },
            { "SlashAlt",          new[] { "004-00-828","004-01-904","004-02-903","004-03-850","004-04-155" } },
            { "UpSlash",           new[] { "009-00-718","009-01-718","009-02-720","009-03-649","009-04-714","009-05-714","009-06-715" } },
            { "DownSlash",         new[] { "010-00-593","010-01-647","010-02-596","010-03-591" } },
            { "Wall Slash",        new[] { "125-00-947","125-01-948","125-02-949","125-03-1227","125-04-1231","125-05-1226","125-06-1236" } },

            // ── Spells → mapped to closest Hornet equivalent (no true spell analogue) ──
            // "Fireball" in HK = forward spell cast. Hornet has no spells; nearest motion is a side slash.
            { "Fireball",          new[] { "006-00-901","006-01-728","006-02-907","006-03-826","006-04-901" } },
            { "Quake Antic",       new[] { "009-00-718","009-01-718","009-02-720","009-03-649","009-04-714" } },
            { "Quake Fall",        new[] { "003-07-1106","003-08-1098","003-09-1100","003-10-1107","003-11-1101","003-12-1104","003-13-1097","003-14-694" } },
            { "Quake Fall 2",      new[] { "003-07-1106","003-08-1098","003-09-1100","003-10-1107","003-11-1101","003-12-1104","003-13-1097","003-14-694" } },
            { "Quake Land",        new[] { "012-00-1046","012-01-1049","012-02-1047","012-03-1050","012-04-1050","012-05-1050","012-06-1048","012-07-1045","012-08-1045","012-09-1045","012-10-1045","012-11-1045" } },

            // ── Taking damage / stun (021.Recoil) ─────────────────────────
            { "Recoil",            new[] { "021-00-1284","021-01-1284","021-02-1285","021-03-1285","021-04-1283","021-05-1283","021-06-1287","021-07-1287","021-08-1286" } },
            { "Stun",              new[] { "021-00-1284","021-01-1284","021-02-1285","021-03-1285","021-04-1283","021-05-1283","021-06-1287","021-07-1287","021-08-1286" } },

            // ── Death (017.Death, 024.Acid Death, 025.Spike Death) ────────
            { "Death",             new[] { "017-00-968","017-01-966","017-02-969","017-03-964","017-04-962","017-05-967","017-06-970","017-07-965","017-08-963","017-09-967","017-10-970","017-11-965","017-12-963","017-13-970","017-14-965","017-15-963" } },
            { "Acid Death",        new[] { "024-00-1285","024-01-1286","024-02-1622","024-03-1351","024-04-1353","024-05-1350","024-06-1352" } },
            { "Spike Death",       new[] { "025-00-1173","025-01-880","025-02-1174","025-03-885","025-04-883","025-05-882","025-06-881" } },

            // ── Misc / game states ────────────────────────────────────────
            // Swim: use Surface TurnToSwim as nearest motion
            { "Swim",              new[] { "020-00-143","020-01-142","020-02-1315","020-03-1318","020-04-1311","020-05-1302","020-06-1306","020-07-1312","020-08-1317","020-09-1300","020-10-1313","020-11-1307" } },
            // Bench sit
            { "Wake Up Ground",    new[] { "039-00-1291","039-01-1177","039-02-1176","039-03-1178" } },
            { "Sit",               new[] { "039-00-1291","039-01-1177","039-02-1176","039-03-1178" } },
            // Hazard respawn / door transitions → hold first Idle frame
            { "Hazard Respawn",    new[] { "001-00-873" } },
            { "Exit Door To Idle", new[] { "001-00-873","001-01-875","001-02-869","001-03-874" } },
        };

        private tk2dSpriteAnimator _animator;
        private tk2dSprite         _knightSprite;
        private SpriteRenderer     _overlay;
        private string _lastClip;
        private int    _lastFrame = -1;

        void Awake()
        {
            _animator     = GetComponent<tk2dSpriteAnimator>();
            _knightSprite = GetComponent<tk2dSprite>();

            var go = new GameObject("HornetOverlay");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = Vector3.zero;
            _overlay = go.AddComponent<SpriteRenderer>();

            if (_knightSprite != null)
            {
                var r = _knightSprite.GetComponent<Renderer>();
                _overlay.sortingLayerName = r.sortingLayerName;
                _overlay.sortingOrder     = r.sortingOrder + 1;
            }

            ApplyToggleState();

            // Show first Idle frame immediately on spawn
            if (FrameSprites != null && FrameSprites.TryGetValue("001-00-873", out var first))
                _overlay.sprite = first;
        }

        void Update()
        {
            if (Input.GetKeyDown(ToggleKey))
            {
                IsEnabled = !IsEnabled;
                ApplyToggleState();
                // Reset frame cache so the overlay refreshes immediately on re-enable
                _lastClip  = null;
                _lastFrame = -1;
            }
        }

        void LateUpdate()
        {
            if (!IsEnabled) return;
            if (_overlay == null || _animator == null || FrameSprites == null) return;

            // Re-enforce Knight invisibility every frame — HK's damage flash resets alpha to 1
            if (_knightSprite != null && _knightSprite.color.a > 0f)
                _knightSprite.color = new Color(1f, 1f, 1f, 0f);

            var clip = _animator.CurrentClip;
            if (clip == null) return;

            int frame = _animator.CurrentFrame;
            if (clip.name == _lastClip && frame == _lastFrame) return;

            _lastClip  = clip.name;
            _lastFrame = frame;

            if (!AnimMap.ContainsKey(clip.name))
                HornetInHallownest.Instance.Log($"[unmapped] \"{clip.name}\" f{frame}");

            if (AnimMap.TryGetValue(clip.name, out var names))
            {
                var fname = names[frame % names.Length];
                if (FrameSprites.TryGetValue(fname, out var spr))
                    _overlay.sprite = spr;
            }
        }

        void OnDestroy()
        {
            if (_knightSprite != null)
                _knightSprite.color = Color.white;
        }

        void ApplyToggleState()
        {
            if (_overlay != null)
                _overlay.enabled = IsEnabled;
            if (_knightSprite != null)
                _knightSprite.color = IsEnabled ? new Color(1f, 1f, 1f, 0f) : Color.white;
        }
    }
}
