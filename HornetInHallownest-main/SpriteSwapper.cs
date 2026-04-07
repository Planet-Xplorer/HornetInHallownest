using System.Collections.Generic;
using UnityEngine;

namespace HornetInHallownest
{
    // Attached to the Knight's GameObject; replaces the body sprite with Hornet's
    internal class HornetSpriteDriver : MonoBehaviour
    {
        // Populated by HornetInHallownest.LoadHornetSprites()
        internal static Dictionary<string, Sprite> FrameSprites;

        // Maps HK animation clip names → ordered array of Hornet frame names.
        internal static readonly Dictionary<string, string[]> AnimMap =
            new Dictionary<string, string[]>
        {
            // --- Idle / standing ---
            { "Idle",         new[] { "idle0000","idle0001","idle0002","idle0003","idle0004","idle0005" } },
            { "LookUp",       new[] { "idle0000","idle0001","idle0002","idle0003","idle0004","idle0005" } },
            { "Look Up",      new[] { "idle0000","idle0001","idle0002","idle0003","idle0004","idle0005" } },
            { "LookDown",     new[] { "look_down0000","look_down0001","look_down0002","look_down0003","look_down0004","look_down0005" } },
            { "Look Down",    new[] { "look_down0000","look_down0001","look_down0002","look_down0003","look_down0004","look_down0005" } },

            // --- Movement ---
            { "Run",          new[] { "run0000","run0001","run0002","run0003","run0004","run0005","run0006","run0007" } },
            { "Walk",         new[] { "run0000","run0001","run0002","run0003","run0004","run0005","run0006","run0007" } },
            { "Turn",         new[] { "turn0000","turn0001","turn0002" } },
            { "TurnToIdle",   new[] { "to_idle0000","to_idle0001","to_idle0002" } },
            { "RunToIdle",    new[] { "to_idle0000","to_idle0001","to_idle0002" } },
            { "ToIdle",       new[] { "to_idle0000","to_idle0001","to_idle0002" } },
            { "IdleToRun",    new[] { "to_run0000","to_run0001","to_run0002","to_run0003","to_run0004","to_run0005" } },
            { "RunStart",     new[] { "to_run0000","to_run0001","to_run0002","to_run0003","to_run0004","to_run0005" } },

            // --- Jumping / falling ---
            { "Jump",         new[] { "jump0000","jump0001","jump0002","jump0003" } },
            { "Double Jump",  new[] { "jump0000","jump0001","jump0002","jump0003" } },
            { "Fall",         new[] { "jump0004","jump0005","jump0006","jump0007","jump0008","jump0009" } },
            { "AirBorne",     new[] { "jump0004","jump0005","jump0006","jump0007","jump0008","jump0009" } },

            // --- Landing ---
            { "Land",         new[] { "soft_land0000","soft_land0001","soft_land0002" } },
            { "Soft Land",    new[] { "soft_land0000","soft_land0001","soft_land0002" } },
            { "Hard Land",    new[] { "hard_land0000","hard_land0001","hard_land0002","hard_land0003","hard_land0004","hard_land0005" } },

            // --- Dashing ---
            { "Dash",         new[] { "evade0000","evade0001","evade0002","evade0003","evade0004","evade0005" } },
            { "Dash Back",    new[] { "Tool_Backdash0000","Tool_Backdash0001","Tool_Backdash0002","Tool_Backdash0003","Tool_Backdash0004","Tool_Backdash0005" } },
            { "Dash Antic",   new[] { "evade0000","evade0001","evade0002" } },
            { "Dash End",     new[] { "evade0003","evade0004","evade0005" } },
            { "Dash Down",    new[] { "jump0004","jump0005","jump0006","jump0007","jump0008","jump0009" } },
            { "Dash Down Land", new[] { "hard_land0000","hard_land0001","hard_land0002","hard_land0003","hard_land0004","hard_land0005" } },

            // --- Wall ---
            { "Wall Slide",   new[] { "jump0004","jump0005","jump0006" } },

            // --- Attacks (side) ---
            { "Slash",        new[] { "throw_side0000","throw_side0001","throw_side0002","throw_side0003","throw_side0004","throw_side0005","throw_side0006","throw_side0007","throw_side0008" } },
            { "Slash2",       new[] { "throw_side0000","throw_side0001","throw_side0002","throw_side0003","throw_side0004" } },
            { "Slash3",       new[] { "throw_side0000","throw_side0001","throw_side0002","throw_side0003","throw_side0004" } },
            { "Slash4",       new[] { "throw_side0000","throw_side0001","throw_side0002","throw_side0003","throw_side0004" } },
            { "Alt Slash",    new[] { "throw_side0000","throw_side0001","throw_side0002","throw_side0003","throw_side0004" } },
            { "Air Slash",    new[] { "throw_side0000","throw_side0001","throw_side0002","throw_side0003","throw_side0004" } },
            { "Wall Slash",   new[] { "throw_side0000","throw_side0001","throw_side0002","throw_side0003","throw_side0004" } },

            // --- Attacks (up) ---
            { "Up Slash",     new[] { "throw_up0000","throw_up0001","throw_up0002","throw_up0003","throw_up0004","throw_up0005","throw_up0006","throw_up0007","throw_up0008" } },
            { "Air Up Slash", new[] { "throw_up0000","throw_up0001","throw_up0002","throw_up0003","throw_up0004","throw_up0005","throw_up0006","throw_up0007","throw_up0008" } },

            // --- Attacks (down) ---
            { "Down Slash",   new[] { "harpoon_up0000","harpoon_up0001" } },
            { "Air Down Slash", new[] { "harpoon_up0000","harpoon_up0001" } },

            // --- Spells ---
            { "Cast",         new[] { "harpoon_side0000","harpoon_side0001" } },
            { "Cast Up",      new[] { "throw_up0000","throw_up0001","throw_up0002","throw_up0003","throw_up0004" } },
            { "Quake Antic",  new[] { "harpoon_up0000","harpoon_up0001" } },
            { "Quake Fall",   new[] { "jump0004","jump0005","jump0006","jump0007","jump0008","jump0009" } },
            { "Quake Fall 2", new[] { "jump0004","jump0005","jump0006","jump0007","jump0008","jump0009" } },
            { "Quake Land",   new[] { "hard_land0000","hard_land0001","hard_land0002","hard_land0003","hard_land0004","hard_land0005" } },

            // --- Taking damage ---
            { "Damage",       new[] { "idle0000" } },
            { "Damage Ground",new[] { "idle0000" } },
            { "Recoil",       new[] { "idle0000" } },
            { "Airborne Damage", new[] { "jump0004","jump0005","jump0006" } },

            // --- Death ---
            { "Death",        new[] { "idle0000" } },
            { "Death Resting",new[] { "idle0000" } },
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

            // Create overlay child
            var go = new GameObject("HornetOverlay");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = Vector3.zero;
            _overlay = go.AddComponent<SpriteRenderer>();

            if (_knightSprite != null)
            {
                var r = _knightSprite.GetComponent<Renderer>();
                _overlay.sortingLayerName = r.sortingLayerName;
                _overlay.sortingOrder     = r.sortingOrder + 1;
                _knightSprite.color = new Color(1f, 1f, 1f, 0f);
            }

            // Show first idle frame immediately
            if (FrameSprites != null && FrameSprites.TryGetValue("idle0000", out var first))
                _overlay.sprite = first;
        }

        void LateUpdate()
        {
            if (_overlay == null || _animator == null || FrameSprites == null) return;

            // Re-enforce invisibility every frame — HK's damage flash resets the color otherwise
            if (_knightSprite != null && _knightSprite.color.a > 0f)
                _knightSprite.color = new Color(1f, 1f, 1f, 0f);

            var clip = _animator.CurrentClip;
            if (clip == null) return;

            int frame = _animator.CurrentFrame;
            if (clip.name == _lastClip && frame == _lastFrame) return;

            _lastClip  = clip.name;
            _lastFrame = frame;

            // Log any clip we don't have a mapping for so we can add it
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
    }
}