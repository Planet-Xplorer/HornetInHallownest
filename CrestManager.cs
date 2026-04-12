using GlobalEnums;
using Modding;
using UnityEngine;

namespace HornetInHallownest
{
    // Mirrors GlobalEnums.CrestType from Silksong's Assembly-CSharp.dll exactly,
    // plus Cursed which is a custom addition based on the cursed-attack sprites.
    //
    // Internal code name  →  Display name in game
    // ─────────────────────────────────────────────
    // Hunter              →  Hunter's Crest       (default starting crest)
    // Wanderer            →  Wanderer's Crest
    // Warrior             →  Beast Crest          (bundle: crestbeast)
    // Reaper              →  Reaper Crest
    // Spinner             →  Shaman Crest         (bundle: crestshaman)
    // Toolmaster          →  Architect Crest      (bundle: crestarchitect)
    // Cloakless           →  Cloakless Crest      (special, active only in prison segment normally: can be chosen to be a choice along with other crests in config menu)
    // Cursed              →  Cursed Crest         (special, active only during cursed quest normally: just like cloakless, can be chosen to be a choice along with other crests in config menu)
    // Witch               →  Witch Crest
    public enum CrestType
    {
        Hunter,      // Hunter's Crest — default starting crest
        Reaper,      // Reaper Crest
        Wanderer,    // Wanderer's Crest
        Warrior,     // Beast Crest          (internal: Warrior, bundle: crestbeast)
        Witch,       // Witch Crest
        Toolmaster,  // Architect Crest      (internal: Toolmaster, bundle: crestarchitect)
        Spinner,     // Shaman Crest         (internal: Spinner, bundle: crestshaman)
        Cloakless,   // Cloakless Crest      (prison segment only)
        Cursed,      // Cursed Crest         (cursed quest only — Hornet_Cursed_attack_tendril sprites)
    }

    public class CrestManager : MonoBehaviour
    {
        public static CrestManager Instance { get; private set; }

        // Hunter is the default starting crest in Silksong.
        public static CrestType CurrentCrest = CrestType.Hunter;

        // Silk resource — mirrors PlayerData.instance.MPCharge in concept.
        // Full silk = 100f.
        public static float SilkAmount = 100f;
        public const  float SilkMax    = 100f;

        void Awake()
        {
            Instance = this;
        }

        void OnEnable()
        {
            On.HeroController.Attack += OnAttack;
            On.HealthManager.Hit += OnHealthHit;
        }

        void OnDisable()
        {
            On.HeroController.Attack -= OnAttack;
            On.HealthManager.Hit -= OnHealthHit;
        }

        // Intercept every attack. When Hornet is active, dispatch to the
        // active crest instead of letting the normal nail logic run.
        private void OnAttack(On.HeroController.orig_Attack orig, HeroController self, AttackDirection dir)
        {
            if (!HornetSpriteDriver.IsEnabled)
            {
                orig(self, dir);
                return;
            }

            switch (CurrentCrest)
            {
                case CrestType.Hunter:
                    ActivateHunter(self, dir);
                    break;
                case CrestType.Reaper:
                    ActivateReaper(self, dir);
                    break;
                case CrestType.Wanderer:
                    ActivateWanderer(self, dir);
                    break;
                case CrestType.Warrior:
                    ActivateWarrior(self, dir);
                    break;
                case CrestType.Witch:
                    ActivateWitch(self, dir);
                    break;
                case CrestType.Toolmaster:
                    ActivateToolmaster(self, dir);
                    break;
                case CrestType.Spinner:
                    ActivateSpinner(self, dir);
                    break;
                case CrestType.Cursed:
                    ActivateCursed(self, dir);
                    break;
                case CrestType.Cloakless:
                    orig(self, dir);
                    break;
                default:
                    orig(self, dir);
                    break;
            }
        }

        // ── Crest stubs ────────────────────────────────────────────────────
        // Each method will eventually trigger its visual effect, sound,
        // and gameplay modifier. For now they fall through to the base attack
        // so the game doesn't break.
        //
        // NOTE — Each crest in Silksong carries a rich, individual combat identity
        // that these stubs do not remotely capture. Hunter's Crest alone has timed
        // parry windows, follow-up cancel options, needle-combo extensions, and
        // separate damage tiers for grounded vs. airborne hits. Warrior/Beast,
        // Spinner/Shaman, and Toolmaster/Architect crests each introduce distinct
        // projectile behaviours, silk interactions, and cooldown rhythms defined in
        // Silksong's BasicSpriteAnimator and crest-specific FSMs. Cursed and Cloakless
        // have entirely separate animation rigs (see Hornet_Cursed_attack_tendril sprites).
        // Implementing any crest faithfully requires reading those scripts first —
        // paste the relevant Silksong decompilations into
        // Resources/Reference_Scripts/Silksong_Scripts/ before writing the actual logic.

        private void ActivateHunter(HeroController hc, AttackDirection dir)
        {
            // TODO: Hunter crest visual + gameplay effect
            HornetInHallownest.Instance.Log("[CrestManager] Hunter crest triggered");
            On.HeroController.Attack -= OnAttack;
            hc.Attack(dir);
            On.HeroController.Attack += OnAttack;
        }

        private void ActivateReaper(HeroController hc, AttackDirection dir)
        {
            HornetInHallownest.Instance.Log("[CrestManager] Reaper crest triggered");
            On.HeroController.Attack -= OnAttack;
            hc.Attack(dir);
            On.HeroController.Attack += OnAttack;
        }

        private void ActivateWanderer(HeroController hc, AttackDirection dir)
        {
            HornetInHallownest.Instance.Log("[CrestManager] Wanderer crest triggered");
            On.HeroController.Attack -= OnAttack;
            hc.Attack(dir);
            On.HeroController.Attack += OnAttack;
        }

        private void ActivateWarrior(HeroController hc, AttackDirection dir)
        {
            HornetInHallownest.Instance.Log("[CrestManager] Warrior crest triggered");
            On.HeroController.Attack -= OnAttack;
            hc.Attack(dir);
            On.HeroController.Attack += OnAttack;
        }

        private void ActivateWitch(HeroController hc, AttackDirection dir)
        {
            HornetInHallownest.Instance.Log("[CrestManager] Witch crest triggered");
            On.HeroController.Attack -= OnAttack;
            hc.Attack(dir);
            On.HeroController.Attack += OnAttack;
        }

        private void ActivateToolmaster(HeroController hc, AttackDirection dir)
        {
            HornetInHallownest.Instance.Log("[CrestManager] Toolmaster crest triggered");
            On.HeroController.Attack -= OnAttack;
            hc.Attack(dir);
            On.HeroController.Attack += OnAttack;
        }

        private void ActivateSpinner(HeroController hc, AttackDirection dir)
        {
            HornetInHallownest.Instance.Log("[CrestManager] Spinner crest triggered");
            On.HeroController.Attack -= OnAttack;
            hc.Attack(dir);
            On.HeroController.Attack += OnAttack;
        }

        private void ActivateCursed(HeroController hc, AttackDirection dir)
        {
            // TODO: Cursed crest visual (Hornet_Cursed_attack_tendril sprites) + gameplay effect
            HornetInHallownest.Instance.Log("[CrestManager] Cursed crest triggered");
            On.HeroController.Attack -= OnAttack;
            hc.Attack(dir);
            On.HeroController.Attack += OnAttack;
        }

        // ── Damage Modulation ──────────────────────────────────────────────
        // Intercept every hit on enemies to apply crest-specific damage multipliers,
        // knockback scaling, and special effects.
        //
        // Modify hitInstance fields BEFORE calling orig() to change:
        //   - Multiplier:         1.0 = normal, 1.25 = +25% damage
        //   - MagnitudeMultiplier: knockback force scaling (1.0 = normal)
        //   - Direction:          0 = right, 90 = up, 180 = left, 270 = down
        //   - DamageDealt:        base damage value (usually leaves this alone)
        private void OnHealthHit(On.HealthManager.orig_Hit orig,
                                 HealthManager self,
                                 HitInstance hitInstance)
        {
            if (!HornetSpriteDriver.IsEnabled || hitInstance.AttackType != AttackTypes.Nail)
            {
                orig(self, hitInstance);
                return;
            }

            // Apply crest-specific damage modifications
            switch (CurrentCrest)
            {
                case CrestType.Hunter:
                    // Hunter's Crest — balanced baseline damage
                    hitInstance.Multiplier *= 1.0f;  // 100% of normal dagger damage
                    hitInstance.MagnitudeMultiplier *= 1.0f;  // Normal knockback
                    break;

                case CrestType.Reaper:
                    // TODO: Reaper Crest — higher damage, lower speed
                    break;

                case CrestType.Wanderer:
                    // TODO: Wanderer's Crest — pierce through enemies
                    break;

                case CrestType.Warrior:
                    // TODO: Beast Crest — massive damage, slow attacks
                    break;

                case CrestType.Witch:
                    // TODO: Witch Crest — projectile spawning
                    break;

                case CrestType.Toolmaster:
                    // TODO: Architect Crest — tool-based attacks
                    break;

                case CrestType.Spinner:
                    // TODO: Shaman Crest — spinning attack
                    break;

                case CrestType.Cursed:
                    // TODO: Cursed Crest — special tendril behavior
                    break;

                case CrestType.Cloakless:
                    // Cloakless: Hornet's base attack (no crest passive)
                    break;
            }

            orig(self, hitInstance);
        }
    }
}
