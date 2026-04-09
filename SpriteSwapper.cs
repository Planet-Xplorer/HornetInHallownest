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

        // Maps HK animation clip names (from Assembly-CSharp HeroAnimationController)
        // to ordered Hornet sprite frame name arrays.
        // Clip names were verified by decompiling HK's Assembly-CSharp.dll with dnSpy.
        internal static readonly Dictionary<string, string[]> AnimMap =
            new Dictionary<string, string[]>
        {
            // --- Idle ---
            { "Idle",              new[] { "idle0000","idle0001","idle0002","idle0003","idle0004","idle0005" } },
            { "Idle Hurt",         new[] { "idle0000" } },
            { "Lantern Idle",      new[] { "idle0000","idle0001","idle0002","idle0003","idle0004","idle0005" } },

            // --- Looking ---
            { "LookUp",            new[] { "idle0000","idle0001","idle0002","idle0003","idle0004","idle0005" } },
            { "Lookup",            new[] { "idle0000","idle0001","idle0002","idle0003","idle0004","idle0005" } }, // lowercase variant in HK
            { "LookUpEnd",         new[] { "idle0000","idle0001","idle0002" } },
            { "LookDown",          new[] { "look_down0000","look_down0001","look_down0002","look_down0003","look_down0004","look_down0005" } },
            { "LookDownEnd",       new[] { "look_down0000","look_down0001","look_down0002" } },

            // --- Movement ---
            // run0002.png is a blank frame in the extracted assets — skip it by repeating run0001
            { "Run",               new[] { "run0000","run0001","run0001","run0003","run0004","run0005","run0006","run0007" } },
            { "Walk",              new[] { "run0000","run0001","run0001","run0003","run0004","run0005","run0006","run0007" } },
            { "Sprint",            new[] { "run0000","run0001","run0001","run0003","run0004","run0005","run0006","run0007" } },
            { "Lantern Run",       new[] { "run0000","run0001","run0001","run0003","run0004","run0005","run0006","run0007" } },
            { "Turn",              new[] { "turn0000","turn0001","turn0002" } },

            // --- Idle ↔ Run transitions ---
            { "Run To Idle",       new[] { "to_idle0000","to_idle0001","to_idle0002" } },
            { "Backdash To Idle",  new[] { "to_idle0000","to_idle0001","to_idle0002" } },
            { "Dash To Idle",      new[] { "to_idle0000","to_idle0001","to_idle0002" } },
            // Possible run-start clips (may appear in tk2d lib even if not in HeroAnimationController)
            { "Idle To Run",       new[] { "to_run0000","to_run0001","to_run0002","to_run0003","to_run0004","to_run0005" } },
            { "Land To Run",       new[] { "to_run0000","to_run0001","to_run0002","to_run0003","to_run0004","to_run0005" } },

            // --- Jumping / airborne ---
            // HK's actual clip name is "Airborne" (not "AirBorne" — verified from DLL)
            { "Airborne",          new[] { "jump0000","jump0001","jump0002","jump0003","jump0004","jump0005","jump0006","jump0007","jump0008","jump0009" } },
            { "Double Jump",       new[] { "jump0000","jump0001","jump0002","jump0003" } },
            { "Walljump",          new[] { "jump0000","jump0001","jump0002","jump0003" } },

            // --- Landing ---
            { "Land",              new[] { "soft_land0000","soft_land0001","soft_land0002" } },
            // HK's actual clip name is "HardLand" (not "Hard Land" — verified from DLL)
            { "HardLand",          new[] { "hard_land0000","hard_land0001","hard_land0002","hard_land0003","hard_land0004","hard_land0005" } },
            { "Backdash Land",     new[] { "soft_land0000","soft_land0001","soft_land0002" } },
            { "Backdash Land 2",   new[] { "soft_land0000","soft_land0001","soft_land0002" } },
            { "Dash Down Land",    new[] { "hard_land0000","hard_land0001","hard_land0002","hard_land0003","hard_land0004","hard_land0005" } },

            // --- Dashing ---
            { "Dash",              new[] { "evade0000","evade0001","evade0002","evade0003","evade0004","evade0005" } },
            // HK's actual clip name is "Back Dash" (not "Dash Back" — verified from DLL)
            { "Back Dash",         new[] { "Tool_Backdash0000","Tool_Backdash0001","Tool_Backdash0002","Tool_Backdash0003","Tool_Backdash0004","Tool_Backdash0005" } },
            { "Shadow Dash",       new[] { "evade0000","evade0001","evade0002","evade0003","evade0004","evade0005" } },
            { "Shadow Dash Sharp", new[] { "evade0000","evade0001","evade0002","evade0003","evade0004","evade0005" } },
            { "Dash Down",         new[] { "jump0004","jump0005","jump0006","jump0007","jump0008","jump0009" } },
            { "Shadow Dash Down",  new[] { "jump0004","jump0005","jump0006","jump0007","jump0008","jump0009" } },
            { "Shadow Dash Down Sharp", new[] { "jump0004","jump0005","jump0006","jump0007","jump0008","jump0009" } },

            // --- Wall ---
            { "Wall Slide",        new[] { "jump0004","jump0005","jump0006" } },

            // --- Attacks ---
            // HK's actual clip names: "Slash", "SlashAlt", "UpSlash", "DownSlash", "Wall Slash" — verified from DLL
            { "Slash",             new[] { "throw_side0000","throw_side0001","throw_side0002","throw_side0003","throw_side0004","throw_side0005","throw_side0006","throw_side0007","throw_side0008" } },
            { "SlashAlt",          new[] { "throw_side0000","throw_side0001","throw_side0002","throw_side0003","throw_side0004","throw_side0005","throw_side0006","throw_side0007","throw_side0008" } },
            { "UpSlash",           new[] { "throw_up0000","throw_up0001","throw_up0002","throw_up0003","throw_up0004" } },
            { "DownSlash",         new[] { "harpoon_up0000","harpoon_up0001" } },
            { "Wall Slash",        new[] { "throw_side0000","throw_side0001","throw_side0002","throw_side0003","throw_side0004" } },

            // --- Spells ---
            // HK's forward spell clip is "Fireball" (not "Cast" — verified from DLL)
            { "Fireball",          new[] { "harpoon_side0000","harpoon_side0001" } },
            // These may be driven by PlayMaker, not HeroAnimationController — kept as fallbacks
            { "Quake Antic",       new[] { "harpoon_up0000","harpoon_up0001" } },
            { "Quake Fall",        new[] { "jump0004","jump0005","jump0006","jump0007","jump0008","jump0009" } },
            { "Quake Fall 2",      new[] { "jump0004","jump0005","jump0006","jump0007","jump0008","jump0009" } },
            { "Quake Land",        new[] { "hard_land0000","hard_land0001","hard_land0002","hard_land0003","hard_land0004","hard_land0005" } },

            // --- Taking damage ---
            { "Recoil",            new[] { "idle0000" } },
            { "Stun",              new[] { "idle0000" } },

            // --- Misc game states ---
            { "Swim",              new[] { "idle0000" } },
            { "Wake Up Ground",    new[] { "idle0000","idle0001","idle0002" } },
            { "Hazard Respawn",    new[] { "idle0000" } },
            { "Exit Door To Idle", new[] { "idle0000","idle0001","idle0002","idle0003" } },
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

            if (FrameSprites != null && FrameSprites.TryGetValue("idle0000", out var first))
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