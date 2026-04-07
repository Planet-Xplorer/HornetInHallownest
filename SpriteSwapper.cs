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
        // Clip names here are guesses; the debug log will reveal any wrong ones.
        internal static readonly Dictionary<string, string[]> AnimMap =
            new Dictionary<string, string[]>
        {
            { "Idle",        new[] { "idle0000","idle0001","idle0002","idle0003","idle0004","idle0005" } },
            { "Run",         new[] { "run0000","run0001","run0002","run0003","run0004","run0005","run0006","run0007" } },
            { "Walk",        new[] { "run0000","run0001","run0002","run0003","run0004","run0005","run0006","run0007" } },
            { "Jump",        new[] { "jump0000","jump0001","jump0002","jump0003" } },
            { "Double Jump", new[] { "jump0000","jump0001","jump0002","jump0003" } },
            { "Fall",        new[] { "jump0004","jump0005","jump0006","jump0007","jump0008","jump0009" } },
            { "Turn",        new[] { "turn0000","turn0001","turn0002" } },
            { "TurnToIdle",  new[] { "turn0000","turn0001","turn0002","idle0000","idle0001","idle0002","idle0003","idle0004","idle0005" } },
            { "Land",        new[] { "soft_land0000","soft_land0001","soft_land0002" } },
            { "Hard Land",   new[] { "soft_land0000","soft_land0001","soft_land0002" } },
            { "Soft Land",   new[] { "soft_land0000","soft_land0001","soft_land0002" } },
            { "Dash",        new[] { "evade0000","evade0001","evade0002","evade0003","evade0004","evade0005" } },
            { "Dash Back",   new[] { "evade0000","evade0001","evade0002","evade0003","evade0004","evade0005" } },
            { "Dash Antic",  new[] { "evade0000","evade0001","evade0002" } },
            { "Slash",       new[] { "throw_side0000","throw_side0001","throw_side0002","throw_side0003","throw_side0004","throw_side0005","throw_side0006","throw_side0007","throw_side0008" } },
            { "Slash2",      new[] { "throw_side0000","throw_side0001","throw_side0002","throw_side0003","throw_side0004" } },
            { "Slash3",      new[] { "throw_side0000","throw_side0001","throw_side0002","throw_side0003","throw_side0004" } },
            { "Slash4",      new[] { "throw_side0000","throw_side0001","throw_side0002","throw_side0003","throw_side0004" } },
            { "Alt Slash",   new[] { "throw_side0000","throw_side0001","throw_side0002","throw_side0003","throw_side0004" } },
            { "Air Slash",   new[] { "throw_side0000","throw_side0001","throw_side0002","throw_side0003","throw_side0004" } },
            { "Wall Slide",  new[] { "jump0004","jump0005","jump0006" } },
        };

        private tk2dSpriteAnimator _animator;
        private SpriteRenderer _overlay;
        private string _lastClip;
        private int _lastFrame = -1;

        void Awake()
        {
            _animator = GetComponent<tk2dSpriteAnimator>();

            // Create overlay child
            var go = new GameObject("HornetOverlay");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = Vector3.zero;
            _overlay = go.AddComponent<SpriteRenderer>();

            // Use tk2dSprite to get the correct renderer + sorting info
            var knightSprite = GetComponent<tk2dSprite>();
            if (knightSprite != null)
            {
                var r = knightSprite.GetComponent<Renderer>();
                _overlay.sortingLayerName = r.sortingLayerName;
                _overlay.sortingOrder     = r.sortingOrder + 1;
                // Make the Knight body invisible via tk2d's own color property (safer than disabling renderer)
                knightSprite.color = new Color(1f, 1f, 1f, 0f);
            }

            // Show first idle frame immediately
            if (FrameSprites != null && FrameSprites.TryGetValue("idle0000", out var first))
                _overlay.sprite = first;
        }

        void LateUpdate()
        {
            if (_overlay == null || _animator == null || FrameSprites == null) return;

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
            var knightSprite = GetComponent<tk2dSprite>();
            if (knightSprite != null)
                knightSprite.color = Color.white;
        }
    }
}
