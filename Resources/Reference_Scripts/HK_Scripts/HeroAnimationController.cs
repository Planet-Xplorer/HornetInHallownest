using System;
using GlobalEnums;
using UnityEngine;

// Token: 0x02000078 RID: 120
public class HeroAnimationController : MonoBehaviour
{
	// Token: 0x1700003E RID: 62
	// (get) Token: 0x0600027B RID: 635 RVA: 0x0000DF6D File Offset: 0x0000C16D
	// (set) Token: 0x0600027C RID: 636 RVA: 0x0000DF75 File Offset: 0x0000C175
	public ActorStates actorState { get; private set; }

	// Token: 0x1700003F RID: 63
	// (get) Token: 0x0600027D RID: 637 RVA: 0x0000DF7E File Offset: 0x0000C17E
	// (set) Token: 0x0600027E RID: 638 RVA: 0x0000DF86 File Offset: 0x0000C186
	public ActorStates prevActorState { get; private set; }

	// Token: 0x17000040 RID: 64
	// (get) Token: 0x0600027F RID: 639 RVA: 0x0000DF8F File Offset: 0x0000C18F
	// (set) Token: 0x06000280 RID: 640 RVA: 0x0000DF97 File Offset: 0x0000C197
	public ActorStates stateBeforeControl { get; private set; }

	// Token: 0x17000041 RID: 65
	// (get) Token: 0x06000281 RID: 641 RVA: 0x0000DFA0 File Offset: 0x0000C1A0
	// (set) Token: 0x06000282 RID: 642 RVA: 0x0000DFA8 File Offset: 0x0000C1A8
	public bool controlEnabled { get; private set; }

	// Token: 0x06000283 RID: 643 RVA: 0x0000DFB1 File Offset: 0x0000C1B1
	private void Awake()
	{
		this.animator = base.GetComponent<tk2dSpriteAnimator>();
		this.heroCtrl = base.GetComponent<HeroController>();
		this.cState = this.heroCtrl.cState;
	}

	// Token: 0x06000284 RID: 644 RVA: 0x0000DFDC File Offset: 0x0000C1DC
	private void Start()
	{
		this.pd = PlayerData.instance;
		this.ResetAll();
		this.actorState = this.heroCtrl.hero_state;
		if (!this.controlEnabled)
		{
			this.animator.Stop();
			return;
		}
		if (this.heroCtrl.hero_state == ActorStates.airborne)
		{
			this.PlayFromFrame("Airborne", 7);
			return;
		}
		this.PlayIdle();
	}

	// Token: 0x06000285 RID: 645 RVA: 0x0000E040 File Offset: 0x0000C240
	private void Update()
	{
		if (this.controlEnabled)
		{
			this.UpdateAnimation();
			return;
		}
		if (this.cState.facingRight)
		{
			this.wasFacingRight = true;
			return;
		}
		this.wasFacingRight = false;
	}

	// Token: 0x06000286 RID: 646 RVA: 0x0000E06D File Offset: 0x0000C26D
	private void ResetAll()
	{
		this.playLanding = false;
		this.playRunToIdle = false;
		this.playDashToIdle = false;
		this.wasFacingRight = false;
		this.controlEnabled = true;
	}

	// Token: 0x06000287 RID: 647 RVA: 0x0000E092 File Offset: 0x0000C292
	private void ResetPlays()
	{
		this.playLanding = false;
		this.playRunToIdle = false;
		this.playDashToIdle = false;
	}

	// Token: 0x06000288 RID: 648 RVA: 0x0000E0AC File Offset: 0x0000C2AC
	public void UpdateState(ActorStates newState)
	{
		if (this.controlEnabled && newState != this.actorState)
		{
			if (this.actorState == ActorStates.airborne && newState == ActorStates.idle && !this.playLanding)
			{
				this.playLanding = true;
			}
			if (this.actorState == ActorStates.running && newState == ActorStates.idle && !this.playRunToIdle && !this.cState.inWalkZone && !this.cState.attacking)
			{
				this.playRunToIdle = true;
			}
			this.prevActorState = this.actorState;
			this.actorState = newState;
		}
	}

	// Token: 0x06000289 RID: 649 RVA: 0x0000E132 File Offset: 0x0000C332
	public void PlayClip(string clipName)
	{
		if (this.controlEnabled)
		{
			if (clipName == "Exit Door To Idle")
			{
				this.animator.AnimationCompleted = new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(this.AnimationCompleteDelegate);
			}
			this.Play(clipName);
		}
	}

	// Token: 0x0600028A RID: 650 RVA: 0x0000E168 File Offset: 0x0000C368
	private void UpdateAnimation()
	{
		this.changedClipFromLastFrame = false;
		if (this.playLanding)
		{
			this.Play("Land");
			this.animator.AnimationCompleted = new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(this.AnimationCompleteDelegate);
			this.playLanding = false;
		}
		if (this.playRunToIdle)
		{
			this.Play("Run To Idle");
			this.animator.AnimationCompleted = new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(this.AnimationCompleteDelegate);
			this.playRunToIdle = false;
		}
		if (this.playBackDashToIdleEnd)
		{
			this.Play("Backdash Land 2");
			this.animator.AnimationCompleted = new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(this.AnimationCompleteDelegate);
			this.playBackDashToIdleEnd = false;
		}
		if (this.playDashToIdle)
		{
			this.Play("Dash To Idle");
			this.animator.AnimationCompleted = new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(this.AnimationCompleteDelegate);
			this.playDashToIdle = false;
		}
		if (this.actorState == ActorStates.no_input)
		{
			if (this.cState.recoilFrozen)
			{
				this.Play("Stun");
			}
			else if (this.cState.recoiling)
			{
				this.Play("Recoil");
			}
			else if (this.cState.transitioning)
			{
				if (this.cState.onGround)
				{
					if (this.heroCtrl.transitionState == HeroTransitionState.EXITING_SCENE)
					{
						if (!this.animator.IsPlaying("Run"))
						{
							if (!this.pd.GetBool("equippedCharm_37"))
							{
								this.Play("Run");
							}
							else
							{
								this.Play("Sprint");
							}
						}
					}
					else if (this.heroCtrl.transitionState == HeroTransitionState.ENTERING_SCENE)
					{
						if (!this.pd.GetBool("equippedCharm_37"))
						{
							if (!this.animator.IsPlaying("Run"))
							{
								this.PlayFromFrame("Run", 3);
							}
						}
						else
						{
							this.Play("Sprint");
						}
					}
				}
				else if (this.heroCtrl.transitionState == HeroTransitionState.EXITING_SCENE)
				{
					if (!this.animator.IsPlaying("Airborne"))
					{
						this.PlayFromFrame("Airborne", 7);
					}
				}
				else if (this.heroCtrl.transitionState == HeroTransitionState.WAITING_TO_ENTER_LEVEL)
				{
					if (!this.animator.IsPlaying("Airborne"))
					{
						this.PlayFromFrame("Airborne", 7);
					}
				}
				else if (this.heroCtrl.transitionState == HeroTransitionState.ENTERING_SCENE && !this.setEntryAnim)
				{
					if (this.heroCtrl.gatePosition == GatePosition.top)
					{
						this.PlayFromFrame("Airborne", 7);
					}
					else if (this.heroCtrl.gatePosition == GatePosition.bottom)
					{
						this.PlayFromFrame("Airborne", 3);
					}
					this.setEntryAnim = true;
				}
			}
		}
		else if (this.setEntryAnim)
		{
			this.setEntryAnim = false;
		}
		else if (this.cState.dashing)
		{
			if (this.heroCtrl.dashingDown)
			{
				if (this.cState.shadowDashing)
				{
					if (this.pd.GetBool("equippedCharm_16"))
					{
						this.Play("Shadow Dash Down Sharp");
					}
					else
					{
						this.Play("Shadow Dash Down");
					}
				}
				else
				{
					this.Play("Dash Down");
				}
			}
			else if (this.cState.shadowDashing)
			{
				if (this.pd.GetBool("equippedCharm_16"))
				{
					this.Play("Shadow Dash Sharp");
				}
				else
				{
					this.Play("Shadow Dash");
				}
			}
			else
			{
				this.Play("Dash");
			}
		}
		else if (this.cState.backDashing)
		{
			this.Play("Back Dash");
		}
		else if (this.cState.attacking)
		{
			if (this.cState.upAttacking)
			{
				this.Play("UpSlash");
			}
			else if (this.cState.downAttacking)
			{
				this.Play("DownSlash");
			}
			else if (this.cState.wallSliding)
			{
				this.Play("Wall Slash");
			}
			else if (!this.cState.altAttack)
			{
				this.Play("Slash");
			}
			else
			{
				this.Play("SlashAlt");
			}
		}
		else if (this.cState.casting)
		{
			this.Play("Fireball");
		}
		else if (this.cState.wallSliding)
		{
			this.Play("Wall Slide");
		}
		else if (this.actorState == ActorStates.idle)
		{
			if (this.cState.lookingUpAnim && !this.animator.IsPlaying("LookUp"))
			{
				this.Play("LookUp");
			}
			else if (this.CanPlayLookDown())
			{
				this.Play("LookDown");
			}
			else if (!this.cState.lookingUpAnim && !this.cState.lookingDownAnim && this.CanPlayIdle())
			{
				this.PlayIdle();
			}
		}
		else if (this.actorState == ActorStates.running)
		{
			if (!this.animator.IsPlaying("Turn"))
			{
				if (this.cState.inWalkZone)
				{
					if (!this.animator.IsPlaying("Walk"))
					{
						this.Play("Walk");
					}
				}
				else
				{
					this.PlayRun();
				}
			}
		}
		else if (this.actorState == ActorStates.airborne)
		{
			if (this.cState.swimming)
			{
				this.Play("Swim");
			}
			else if (this.heroCtrl.wallLocked)
			{
				this.Play("Walljump");
			}
			else if (this.cState.doubleJumping)
			{
				this.Play("Double Jump");
			}
			else if (this.cState.jumping)
			{
				if (!this.animator.IsPlaying("Airborne"))
				{
					this.PlayFromFrame("Airborne", 0);
				}
			}
			else if (this.cState.falling)
			{
				if (!this.animator.IsPlaying("Airborne"))
				{
					this.PlayFromFrame("Airborne", 5);
				}
			}
			else if (!this.animator.IsPlaying("Airborne"))
			{
				this.PlayFromFrame("Airborne", 3);
			}
		}
		else if (this.actorState == ActorStates.dash_landing)
		{
			this.Play("Dash Down Land");
		}
		else if (this.actorState == ActorStates.hard_landing)
		{
			this.Play("HardLand");
		}
		if (this.cState.facingRight)
		{
			if (!this.wasFacingRight && this.cState.onGround && this.canPlayTurn())
			{
				this.Play("Turn");
			}
			this.wasFacingRight = true;
		}
		else
		{
			if (this.wasFacingRight && this.cState.onGround && this.canPlayTurn())
			{
				this.Play("Turn");
			}
			this.wasFacingRight = false;
		}
		if (this.cState.attacking)
		{
			this.wasAttacking = true;
		}
		else
		{
			this.wasAttacking = false;
		}
		this.ResetPlays();
	}

	// Token: 0x0600028B RID: 651 RVA: 0x0000E854 File Offset: 0x0000CA54
	private bool CanPlayIdle()
	{
		return !this.animator.IsPlaying("Land") && !this.animator.IsPlaying("Run To Idle") && !this.animator.IsPlaying("Dash To Idle") && !this.animator.IsPlaying("Backdash Land") && !this.animator.IsPlaying("Backdash Land 2") && !this.animator.IsPlaying("LookUpEnd") && !this.animator.IsPlaying("LookDownEnd") && !this.animator.IsPlaying("Exit Door To Idle") && !this.animator.IsPlaying("Wake Up Ground") && !this.animator.IsPlaying("Hazard Respawn");
	}

	// Token: 0x0600028C RID: 652 RVA: 0x0000E924 File Offset: 0x0000CB24
	private bool CanPlayLookDown()
	{
		return this.cState.lookingDownAnim && !this.animator.IsPlaying("Lookup");
	}

	// Token: 0x0600028D RID: 653 RVA: 0x0000E948 File Offset: 0x0000CB48
	private bool canPlayTurn()
	{
		return !this.animator.IsPlaying("Wake Up Ground") && !this.animator.IsPlaying("Hazard Respawn");
	}

	// Token: 0x0600028E RID: 654 RVA: 0x0000E974 File Offset: 0x0000CB74
	private void AnimationCompleteDelegate(tk2dSpriteAnimator sprite, tk2dSpriteAnimationClip clip)
	{
		if (clip.name == "Land")
		{
			this.PlayIdle();
		}
		if (clip.name == "Run To Idle")
		{
			this.PlayIdle();
		}
		if (clip.name == "Backdash To Idle")
		{
			this.PlayIdle();
		}
		if (clip.name == "Dash To Idle")
		{
			this.PlayIdle();
		}
		if (clip.name == "Exit Door To Idle")
		{
			this.PlayIdle();
		}
	}

	// Token: 0x0600028F RID: 655 RVA: 0x0000E9FC File Offset: 0x0000CBFC
	public void PlayIdle()
	{
		if (this.pd.GetInt("health") == 1 && this.pd.GetInt("healthBlue") < 1)
		{
			if (this.pd.GetBool("equippedCharm_6"))
			{
				this.animator.Play("Idle");
				return;
			}
			this.animator.Play("Idle Hurt");
			return;
		}
		else
		{
			if (this.animator.IsPlaying("LookUp"))
			{
				this.animator.Play("LookUpEnd");
				return;
			}
			if (this.animator.IsPlaying("LookDown"))
			{
				this.animator.Play("LookDownEnd");
				return;
			}
			if (this.heroCtrl.wieldingLantern)
			{
				this.animator.Play("Lantern Idle");
				return;
			}
			this.animator.Play("Idle");
			return;
		}
	}

	// Token: 0x06000290 RID: 656 RVA: 0x0000EAD8 File Offset: 0x0000CCD8
	private void PlayRun()
	{
		if (this.heroCtrl.wieldingLantern)
		{
			this.animator.Play("Lantern Run");
			return;
		}
		if (this.pd.GetBool("equippedCharm_37"))
		{
			this.Play("Sprint");
			return;
		}
		if (this.wasAttacking)
		{
			this.animator.PlayFromFrame("Run", 3);
			return;
		}
		this.animator.Play("Run");
	}

	// Token: 0x06000291 RID: 657 RVA: 0x0000EB4B File Offset: 0x0000CD4B
	private void Play(string clipName)
	{
		if (clipName != this.animator.CurrentClip.name)
		{
			this.changedClipFromLastFrame = true;
		}
		this.animator.Play(clipName);
	}

	// Token: 0x06000292 RID: 658 RVA: 0x0000EB78 File Offset: 0x0000CD78
	private void PlayFromFrame(string clipName, int frame)
	{
		if (clipName != this.animator.CurrentClip.name)
		{
			this.changedClipFromLastFrame = true;
		}
		this.animator.PlayFromFrame(clipName, frame);
	}

	// Token: 0x06000293 RID: 659 RVA: 0x0000EBA6 File Offset: 0x0000CDA6
	public void StopControl()
	{
		if (this.controlEnabled)
		{
			this.controlEnabled = false;
			this.stateBeforeControl = this.actorState;
		}
	}

	// Token: 0x06000294 RID: 660 RVA: 0x0000EBC3 File Offset: 0x0000CDC3
	public void StartControl()
	{
		this.actorState = this.heroCtrl.hero_state;
		this.controlEnabled = true;
		this.PlayIdle();
	}

	// Token: 0x06000295 RID: 661 RVA: 0x0000EBE3 File Offset: 0x0000CDE3
	public void StartControlWithoutSettingState()
	{
		this.controlEnabled = true;
		if (this.stateBeforeControl == ActorStates.running && this.actorState == ActorStates.running)
		{
			this.actorState = ActorStates.idle;
		}
	}

	// Token: 0x06000296 RID: 662 RVA: 0x0000EC05 File Offset: 0x0000CE05
	public void FinishedDash()
	{
		this.playDashToIdle = true;
	}

	// Token: 0x06000297 RID: 663 RVA: 0x0000EC0E File Offset: 0x0000CE0E
	public void StopAttack()
	{
		if (this.animator.IsPlaying("UpSlash") || this.animator.IsPlaying("DownSlash"))
		{
			this.animator.Stop();
		}
	}

	// Token: 0x06000298 RID: 664 RVA: 0x0000EC3F File Offset: 0x0000CE3F
	public float GetCurrentClipDuration()
	{
		return (float)this.animator.CurrentClip.frames.Length / this.animator.CurrentClip.fps;
	}

	// Token: 0x06000299 RID: 665 RVA: 0x0000EC68 File Offset: 0x0000CE68
	public float GetClipDuration(string clipName)
	{
		if (this.animator == null)
		{
			this.animator = base.GetComponent<tk2dSpriteAnimator>();
		}
		tk2dSpriteAnimationClip clipByName = this.animator.GetClipByName(clipName);
		if (clipByName == null)
		{
			Debug.LogError("HeroAnim: Could not find animation clip with the name " + clipName);
			return -1f;
		}
		return (float)clipByName.frames.Length / clipByName.fps;
	}

	// Token: 0x04000213 RID: 531
	public tk2dSpriteAnimator animator;

	// Token: 0x04000214 RID: 532
	private HeroController heroCtrl;

	// Token: 0x04000215 RID: 533
	private HeroControllerStates cState;

	// Token: 0x04000216 RID: 534
	private PlayerData pd;

	// Token: 0x04000218 RID: 536
	[HideInInspector]
	public bool playLanding;

	// Token: 0x04000219 RID: 537
	private bool playRunToIdle;

	// Token: 0x0400021A RID: 538
	private bool playDashToIdle;

	// Token: 0x0400021B RID: 539
	private bool playBackDashToIdleEnd;

	// Token: 0x0400021C RID: 540
	public bool wasAttacking;

	// Token: 0x0400021D RID: 541
	private bool wasFacingRight;

	// Token: 0x0400021E RID: 542
	[HideInInspector]
	public bool setEntryAnim;

	// Token: 0x0400021F RID: 543
	private bool changedClipFromLastFrame;

	// Token: 0x04000220 RID: 544
	private bool attackComplete;
}
