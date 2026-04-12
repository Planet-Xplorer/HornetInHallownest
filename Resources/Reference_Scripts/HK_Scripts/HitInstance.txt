// SOURCE: Assembly-CSharp.dll — Hollow Knight v1.5.78.11833
// Decompiled with ilspycmd v10.0.0.8330
// DO NOT USE FOR RUNNING CODE. Reference only.
//
// HitInstance is the struct HK uses to describe every hit/damage event.
// Every attack (nail, spell, hazard) constructs one of these and passes it
// to HealthManager.Hit(HitInstance) on the target.
//
// Relevant for mod work:
//   - DamageDealt: base damage number to apply
//   - Multiplier:  scaled by the nail upgrade tier (1.0 = base)
//   - AttackType:  AttackTypes enum distinguishes Nail, Spell, Acid, etc.
//   - SpecialType: SpecialTypes enum for things like Dream Nail, extra particles
//   - CircleDirection / Direction: control knockback angle
//   - MoveAngle / MoveDirection: control victim's physics response
//   - IgnoreInvulnerable: bypass i-frames (used by hazards, not normal attacks)
//
// When implementing Hornet's crests, we'll construct HitInstances with
// appropriate AttackType and Multiplier to match each crest's damage profile.

using System;
using UnityEngine;

[Serializable]
public struct HitInstance
{
	public GameObject Source;

	public AttackTypes AttackType;

	public bool CircleDirection;

	public int DamageDealt;

	public float Direction;

	public bool IgnoreInvulnerable;

	public float MagnitudeMultiplier;

	public float MoveAngle;

	public bool MoveDirection;

	public float Multiplier;

	public SpecialTypes SpecialType;

	public bool IsExtraDamage;

	public float GetActualDirection(Transform target)
	{
		if (Source != null && target != null && CircleDirection)
		{
			Vector2 vector = (Vector2)target.position - (Vector2)Source.transform.position;
			return Mathf.Atan2(vector.y, vector.x) * 57.29578f;
		}
		return Direction;
	}
}
