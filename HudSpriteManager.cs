using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HornetInHallownest
{
    internal class HudSpriteManager : MonoBehaviour
    {
        private const float FrameDuration = 1f / 12f;
        private bool _initialized;
        private readonly List<AnimatedHudOverlay> _overlays = new List<AnimatedHudOverlay>();
        private readonly HashSet<string> _missingLogged = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        void Start()
        {
            EnsureHudOverlays();
        }

        void Update()
        {
            if (!_initialized)
            {
                EnsureHudOverlays();
            }

            foreach (var overlay in _overlays)
            {
                overlay.Tick(Time.deltaTime);
                overlay.Enabled = HornetSpriteDriver.IsEnabled;
            }
        }

        private void EnsureHudOverlays()
        {
            if (_initialized) return;
            if (HornetSpriteDriver.FrameSprites == null || HornetSpriteDriver.FrameAnimSprites == null) return;

            var frameAnim = FindAnimation("HUD Frame Hunter v2 Idle", "HUD Frame Hunter v3 Idle", "HUD Frame Hunter v2", "HUD Frame Hunter v3");
            var spoolAnim = FindAnimation("Spool Appear", "Spool Extender Appear", "Spool Cursed Bob", "Spool Cursed");
            var maskAnim = FindAnimation("Fractured Mask Appear", "Fractured Mask Shine", "Fractured Mask Disappear");

            var frameTarget = FindTargetComponent(new[] { "HUD Frame", "HUDFrame", "HUD", "Hud" }, new[] { "frame" });
            var soulTarget = FindTargetComponent(new[] { "normalSoulOrb", "soulOrbIcon", "hardcoreSoulOrbCg", "boundSoulDisplay", "boundSoulButton" }, new[] { "soul" });
            var maskTargets = FindMaskTargets();

            if (frameTarget != null && frameAnim != null)
            {
                var overlay = CreateOverlay(frameTarget, "HornetHudFrameOverlay", frameAnim[0]);
                if (overlay != null) _overlays.Add(new AnimatedHudOverlay(overlay, frameAnim));
            }
            else
            {
                LogMissing("HUD frame hunter overlay");
            }

            if (soulTarget != null && spoolAnim != null)
            {
                var overlay = CreateOverlay(soulTarget, "HornetSoulSpoolOverlay", spoolAnim[0]);
                if (overlay != null) _overlays.Add(new AnimatedHudOverlay(overlay, spoolAnim));
            }
            else
            {
                LogMissing("soul spool overlay");
            }

            if (maskAnim != null)
            {
                foreach (var target in maskTargets)
                {
                    var overlay = CreateOverlay(target, "HornetMaskOverlay", maskAnim[0]);
                    if (overlay != null) _overlays.Add(new AnimatedHudOverlay(overlay, maskAnim));
                }
            }
            else
            {
                LogMissing("mask overlays");
            }

            _initialized = true;
        }

        private List<Sprite> FindAnimation(params string[] names)
        {
            foreach (var candidate in names)
            {
                var match = HornetSpriteDriver.FrameAnimSprites.FirstOrDefault(kv =>
                    kv.Key.IndexOf(candidate, StringComparison.OrdinalIgnoreCase) >= 0);
                if (match.Value != null && match.Value.Count > 0)
                {
                    return match.Value;
                }
            }

            return null;
        }

        private Component FindTargetComponent(string[] exactNames, string[] fallbackContains)
        {
            foreach (var name in exactNames)
            {
                var go = GameObject.Find(name);
                if (go == null) continue;
                var component = GetGraphicComponent(go);
                if (component != null) return component;
            }

            foreach (var sprite in FindObjectsOfType<tk2dSprite>())
            {
                var objectName = sprite.gameObject.name;
                if (exactNames.Any(n => objectName.IndexOf(n, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    fallbackContains.Any(c => objectName.IndexOf(c, StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    return sprite;
                }
            }

            foreach (var sr in FindObjectsOfType<SpriteRenderer>())
            {
                var objectName = sr.gameObject.name;
                if (exactNames.Any(n => objectName.IndexOf(n, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    fallbackContains.Any(c => objectName.IndexOf(c, StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    return sr;
                }
            }

            return null;
        }

        private IEnumerable<Component> FindMaskTargets()
        {
            foreach (var sprite in FindObjectsOfType<tk2dSprite>())
            {
                if (sprite.gameObject.name.IndexOf("mask", StringComparison.OrdinalIgnoreCase) >= 0)
                    yield return sprite;
            }

            foreach (var sr in FindObjectsOfType<SpriteRenderer>())
            {
                if (sr.gameObject.name.IndexOf("mask", StringComparison.OrdinalIgnoreCase) >= 0)
                    yield return sr;
            }
        }

        private Component GetGraphicComponent(GameObject go)
        {
            var tk2d = go.GetComponent<tk2dSprite>();
            if (tk2d != null) return tk2d;
            var sr = go.GetComponent<SpriteRenderer>();
            if (sr != null) return sr;
            return null;
        }

        private SpriteRenderer CreateOverlay(Component target, string overlayName, Sprite sprite)
        {
            if (target == null || sprite == null) return null;

            var parent = target.gameObject;
            var go = new GameObject(overlayName);
            go.transform.SetParent(parent.transform, false);
            go.transform.localPosition = Vector3.zero;

            var renderer = go.AddComponent<SpriteRenderer>();
            var targetRenderer = GetTargetRenderer(target);
            if (targetRenderer != null)
            {
                renderer.sortingLayerName = targetRenderer.sortingLayerName;
                renderer.sortingOrder = targetRenderer.sortingOrder + 1;
            }

            renderer.sprite = sprite;
            renderer.color = Color.white;
            return renderer;
        }

        private Renderer GetTargetRenderer(Component target)
        {
            if (target is tk2dSprite tk2d)
                return tk2d.GetComponent<Renderer>();
            if (target is SpriteRenderer sr)
                return sr;
            return target.GetComponent<Renderer>();
        }

        private void LogMissing(string item)
        {
            if (_missingLogged.Contains(item)) return;
            _missingLogged.Add(item);
            if (HornetInHallownest.Instance != null)
                HornetInHallownest.Instance.Log($"[HudSpriteManager] no target found for {item}");
        }

        private class AnimatedHudOverlay
        {
            private readonly List<Sprite> _frames;
            private float _timer;
            private int _index;
            private bool _enabled = true;

            public SpriteRenderer Renderer { get; }
            public bool Enabled
            {
                get => _enabled;
                set
                {
                    _enabled = value;
                    if (Renderer != null)
                        Renderer.enabled = value;
                }
            }

            public AnimatedHudOverlay(SpriteRenderer renderer, List<Sprite> frames)
            {
                Renderer = renderer;
                _frames = frames ?? new List<Sprite>();
                if (Renderer != null && _frames.Count > 0)
                    Renderer.sprite = _frames[0];
            }

            public void Tick(float deltaTime)
            {
                if (_frames.Count <= 1 || Renderer == null) return;
                _timer += deltaTime;
                if (_timer < FrameDuration) return;
                _timer -= FrameDuration;
                _index = (_index + 1) % _frames.Count;
                Renderer.sprite = _frames[_index];
            }
        }
    }
}
