using System;
using System.Collections;
using GlobalSettings;
using TeamCherry.NestedFadeGroup;
using UnityEngine;

// Token: 0x02000610 RID: 1552
public class BindOrbHudFrame : MonoBehaviour
{
	// Token: 0x17000660 RID: 1632
	// (get) Token: 0x0600379A RID: 14234 RVA: 0x000F56E2 File Offset: 0x000F38E2
	// (set) Token: 0x0600379B RID: 14235 RVA: 0x000F56E9 File Offset: 0x000F38E9
	public static bool SkipToNextAppear { get; set; }

	// Token: 0x17000661 RID: 1633
	// (get) Token: 0x0600379C RID: 14236 RVA: 0x000F56F1 File Offset: 0x000F38F1
	// (set) Token: 0x0600379D RID: 14237 RVA: 0x000F56F8 File Offset: 0x000F38F8
	public static bool ForceNextInstant { get; set; }

	// Token: 0x0600379E RID: 14238 RVA: 0x000F5700 File Offset: 0x000F3900
	private void Awake()
	{
		this.animator = base.GetComponent<tk2dSpriteAnimator>();
		this.animProxy = base.GetComponent<SteelSoulAnimProxy>();
		EventRegister.GetRegisterGuaranteed(base.gameObject, "POST TOOL EQUIPS CHANGED").ReceivedEvent += delegate()
		{
			this.Refresh(false, false);
		};
		EventRegister.GetRegisterGuaranteed(base.gameObject, "TOOLMASTER QUICK CRAFTING").ReceivedEvent += delegate()
		{
			this.queuedToolmasterSpin = true;
		};
		EventRegister.GetRegisterGuaranteed(base.gameObject, "HEALTH UPDATE").ReceivedEvent += this.RefreshLifebloodTint;
		EventRegister.GetRegisterGuaranteed(base.gameObject, "CHARM INDICATOR CHECK").ReceivedEvent += this.RefreshLifebloodTint;
	}

	// Token: 0x0600379F RID: 14239 RVA: 0x000F57AC File Offset: 0x000F39AC
	private void OnEnable()
	{
		BindOrbHudFrame.MeterAnims[] array = this.warriorMeterAnims;
		for (int i = 0; i < array.Length; i++)
		{
			foreach (GameObject gameObject in array[i].IncreaseEffects)
			{
				if (gameObject)
				{
					gameObject.SetActive(false);
				}
			}
		}
		this.hunterV2Bar.gameObject.SetActive(false);
		this.hunterV3BarA.gameObject.SetActive(false);
		this.hunterV3BarB.gameObject.SetActive(false);
		this.hunterV3ExtraHitEffect.SetActive(false);
	}

	// Token: 0x060037A0 RID: 14240 RVA: 0x000F583A File Offset: 0x000F3A3A
	private void OnDisable()
	{
		if (this.animRoutine != null)
		{
			base.StopCoroutine(this.animRoutine);
			this.animRoutine = null;
		}
		this.isActive = false;
	}

	// Token: 0x060037A1 RID: 14241 RVA: 0x000F585E File Offset: 0x000F3A5E
	public void FirstAppear()
	{
		if (this.isActive || this.animRoutine != null)
		{
			return;
		}
		this.isActive = true;
		this.Refresh(false, true);
	}

	// Token: 0x060037A2 RID: 14242 RVA: 0x000F5880 File Offset: 0x000F3A80
	public void AlreadyAppeared()
	{
		if (this.isActive || this.animRoutine != null)
		{
			return;
		}
		this.isActive = true;
		this.Refresh(true, false);
	}

	// Token: 0x060037A3 RID: 14243 RVA: 0x000F58A2 File Offset: 0x000F3AA2
	public void Disappeared()
	{
		this.isActive = false;
		if (this.animRoutine != null)
		{
			base.StopCoroutine(this.animRoutine);
			this.animRoutine = null;
		}
	}

	// Token: 0x060037A4 RID: 14244 RVA: 0x000F58C6 File Offset: 0x000F3AC6
	private void Refresh(bool isInstant, bool isFirst)
	{
		if (!base.isActiveAndEnabled)
		{
			return;
		}
		if (!this.DoChangeFrame(isInstant, isFirst))
		{
			this.ChangeEnded();
		}
	}

	// Token: 0x060037A5 RID: 14245 RVA: 0x000F58E4 File Offset: 0x000F3AE4
	private void RefreshLifebloodTint()
	{
		tk2dSprite tk2dSprite = this.animator.Sprite as tk2dSprite;
		if (!tk2dSprite)
		{
			return;
		}
		if (HeroController.instance.IsInLifebloodState)
		{
			tk2dSprite.color = this.lifebloodTint;
			tk2dSprite.EnableKeyword("RECOLOUR");
			return;
		}
		this.animator.Sprite.color = Color.white;
		tk2dSprite.DisableKeyword("RECOLOUR");
	}

	// Token: 0x060037A6 RID: 14246 RVA: 0x000F5950 File Offset: 0x000F3B50
	private bool DoChangeFrame(bool isInstant, bool isFirst)
	{
		BindOrbHudFrame.<>c__DisplayClass68_0 CS$<>8__locals1 = new BindOrbHudFrame.<>c__DisplayClass68_0();
		CS$<>8__locals1.<>4__this = this;
		CS$<>8__locals1.isInstant = isInstant;
		CS$<>8__locals1.isFirst = isFirst;
		if (!this.isActive && !CS$<>8__locals1.isFirst)
		{
			return false;
		}
		if (!CS$<>8__locals1.isInstant && BindOrbHudFrame.ForceNextInstant)
		{
			CS$<>8__locals1.isInstant = true;
		}
		ToolCrest hunterCrest = Gameplay.HunterCrest;
		ToolCrest hunterCrest2 = Gameplay.HunterCrest2;
		ToolCrest hunterCrest3 = Gameplay.HunterCrest3;
		ToolCrest cloaklessCrest = Gameplay.CloaklessCrest;
		ToolCrest warriorCrest = Gameplay.WarriorCrest;
		ToolCrest reaperCrest = Gameplay.ReaperCrest;
		ToolCrest wandererCrest = Gameplay.WandererCrest;
		ToolCrest cursedCrest = Gameplay.CursedCrest;
		ToolCrest witchCrest = Gameplay.WitchCrest;
		ToolCrest toolmasterCrest = Gameplay.ToolmasterCrest;
		ToolCrest spellCrest = Gameplay.SpellCrest;
		CS$<>8__locals1.newFrameAnims = null;
		CS$<>8__locals1.customAnimRoutine = null;
		this.isCursed = false;
		if (hunterCrest.IsEquipped)
		{
			if (this.currentFrameCrest == hunterCrest)
			{
				return false;
			}
			this.currentFrameCrest = hunterCrest;
			CS$<>8__locals1.newFrameAnims = this.defaultFrameAnims;
		}
		else if (hunterCrest2.IsEquipped)
		{
			if (this.currentFrameCrest == hunterCrest2)
			{
				return false;
			}
			this.currentFrameCrest = hunterCrest2;
			CS$<>8__locals1.newFrameAnims = this.hunterV2FrameAnims;
			CS$<>8__locals1.customAnimRoutine = new BindOrbHudFrame.CoroutineFunction(this.HunterCrestV2Routine);
		}
		else if (hunterCrest3.IsEquipped)
		{
			if (this.currentFrameCrest == hunterCrest3)
			{
				return false;
			}
			this.currentFrameCrest = hunterCrest3;
			CS$<>8__locals1.newFrameAnims = this.hunterV3FrameAnims;
			CS$<>8__locals1.customAnimRoutine = new BindOrbHudFrame.CoroutineFunction(this.HunterCrestV3Routine);
		}
		else if (cloaklessCrest.IsEquipped)
		{
			if (this.currentFrameCrest == cloaklessCrest)
			{
				return false;
			}
			this.currentFrameCrest = cloaklessCrest;
			CS$<>8__locals1.newFrameAnims = this.cloaklessFrameAnims;
		}
		else if (warriorCrest.IsEquipped)
		{
			if (this.currentFrameCrest == warriorCrest)
			{
				return false;
			}
			this.currentFrameCrest = warriorCrest;
			CS$<>8__locals1.newFrameAnims = this.warriorFrameAnims;
			CS$<>8__locals1.customAnimRoutine = new BindOrbHudFrame.CoroutineFunction(this.WarriorCrestRoutine);
		}
		else if (reaperCrest.IsEquipped)
		{
			if (this.currentFrameCrest == reaperCrest)
			{
				return false;
			}
			this.currentFrameCrest = reaperCrest;
			CS$<>8__locals1.newFrameAnims = this.reaperFrameAnims;
			CS$<>8__locals1.customAnimRoutine = new BindOrbHudFrame.CoroutineFunction(this.ReaperCrestRoutine);
		}
		else if (wandererCrest.IsEquipped)
		{
			if (this.currentFrameCrest == wandererCrest)
			{
				return false;
			}
			this.currentFrameCrest = wandererCrest;
			CS$<>8__locals1.newFrameAnims = this.wandererFrameAnims;
			CS$<>8__locals1.customAnimRoutine = new BindOrbHudFrame.CoroutineFunction(this.WandererCrestRoutine);
		}
		else if (cursedCrest.IsEquipped)
		{
			if (!CS$<>8__locals1.isFirst && this.currentFrameCrest == cursedCrest)
			{
				return false;
			}
			this.currentFrameCrest = cursedCrest;
			CS$<>8__locals1.newFrameAnims = this.cursedV1FrameAnims;
			this.isCursed = true;
		}
		else if (witchCrest.IsEquipped)
		{
			if (this.currentFrameCrest == witchCrest)
			{
				return false;
			}
			this.currentFrameCrest = witchCrest;
			CS$<>8__locals1.newFrameAnims = this.witchFrameAnims;
		}
		else if (toolmasterCrest.IsEquipped)
		{
			if (this.currentFrameCrest == toolmasterCrest)
			{
				return false;
			}
			this.currentFrameCrest = toolmasterCrest;
			CS$<>8__locals1.newFrameAnims = this.toolmasterFrameAnims;
			CS$<>8__locals1.customAnimRoutine = new BindOrbHudFrame.CoroutineFunction(this.ToolmasterCrestRoutine);
		}
		else if (spellCrest.IsEquipped)
		{
			if (this.currentFrameCrest == spellCrest)
			{
				return false;
			}
			this.currentFrameCrest = spellCrest;
			CS$<>8__locals1.newFrameAnims = this.spellFrameAnims;
		}
		else
		{
			if (!CS$<>8__locals1.isFirst && this.currentFrameCrest == null)
			{
				return false;
			}
			this.currentFrameCrest = null;
			CS$<>8__locals1.newFrameAnims = this.defaultFrameAnims;
		}
		if (this.activateEventsTarget)
		{
			this.activateEventsTarget.enabled = true;
			this.activateEventsTarget.SendEvent("DEACTIVATE");
		}
		BindOrbHudFrame.BasicFrameAnims basicFrameAnims = this.currentFrameAnims;
		this.currentFrameAnims = CS$<>8__locals1.newFrameAnims;
		if (!CS$<>8__locals1.isFirst && basicFrameAnims != null && this.currentFrameAnims != null && basicFrameAnims.Idle == this.currentFrameAnims.Idle && CS$<>8__locals1.customAnimRoutine == null)
		{
			return false;
		}
		if (this.animRoutine != null)
		{
			base.StopCoroutine(this.animRoutine);
		}
		if (CS$<>8__locals1.isInstant | CS$<>8__locals1.isFirst)
		{
			CS$<>8__locals1.<DoChangeFrame>g__StartNextFrameAnims|0();
		}
		else if (this.onEndFrameAnim != null)
		{
			this.onEndFrameAnim(new Action(CS$<>8__locals1.<DoChangeFrame>g__StartNextFrameAnims|0));
		}
		else
		{
			this.animRoutine = base.StartCoroutine(this.FrameDisappear(null, new Action(CS$<>8__locals1.<DoChangeFrame>g__StartNextFrameAnims|0)));
		}
		return true;
	}

	// Token: 0x060037A7 RID: 14247 RVA: 0x000F5DA7 File Offset: 0x000F3FA7
	private IEnumerator FrameAppear(BindOrbHudFrame.BasicFrameAnims frameAnims, BindOrbHudFrame.CoroutineFunction customAnimRoutine, bool isFirst)
	{
		string text;
		if (isFirst)
		{
			text = ((!string.IsNullOrEmpty(frameAnims.AppearFromNone)) ? frameAnims.AppearFromNone : frameAnims.Appear);
			if (GameCameras.instance.IsHudVisible)
			{
				(Gameplay.CursedCrest.IsEquipped ? this.hudChangeCursedAudio : this.hudAppearAudio).SpawnAndPlayOneShot(Audio.DefaultUIAudioSourcePrefab, base.transform.position, null);
			}
		}
		else
		{
			text = frameAnims.Appear;
		}
		if (!string.IsNullOrEmpty(text))
		{
			tk2dSpriteAnimationClip clip = this.animProxy.GetClip(text);
			if (clip != null)
			{
				float seconds = this.animator.PlayAnimGetTime(clip);
				this.animator.PlayFromFrame(0);
				yield return new WaitForSeconds(seconds);
			}
		}
		this.PlayFrameAnim(frameAnims.Idle, 0);
		if (isFirst)
		{
			this.isActive = true;
		}
		this.ChangeEnded();
		if (customAnimRoutine != null)
		{
			this.animRoutine = base.StartCoroutine(customAnimRoutine());
		}
		yield break;
	}

	// Token: 0x060037A8 RID: 14248 RVA: 0x000F5DCB File Offset: 0x000F3FCB
	private IEnumerator FrameDisappear(BindOrbHudFrame.BasicFrameAnims frameAnims, Action startNextFrameAnims)
	{
		if (BindOrbHudFrame.SkipToNextAppear)
		{
			this.PlayChangeEffects();
		}
		else
		{
			if (this.refreshDelay > 0f)
			{
				yield return new WaitForSeconds(this.refreshDelay);
			}
			if (!this.currentFrameCrest)
			{
				this.PlayChangeEffects();
			}
			if (frameAnims != null && !string.IsNullOrEmpty(frameAnims.Disappear))
			{
				tk2dSpriteAnimationClip clip = this.animProxy.GetClip(frameAnims.Disappear);
				if (clip != null)
				{
					float seconds = this.animator.PlayAnimGetTime(clip);
					this.animator.PlayFromFrame(0);
					yield return new WaitForSeconds(seconds);
				}
			}
			if (this.currentFrameCrest)
			{
				this.PlayChangeEffects();
			}
		}
		startNextFrameAnims();
		yield break;
	}

	// Token: 0x060037A9 RID: 14249 RVA: 0x000F5DE8 File Offset: 0x000F3FE8
	private void PlayChangeEffects()
	{
		if (this.changeParticle)
		{
			if (this.changeParticle.IsAlive(true))
			{
				this.changeParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
			}
			this.changeParticle.Play(true);
		}
		if (!GameCameras.instance.IsHudVisible)
		{
			return;
		}
		(this.isCursed ? this.hudChangeCursedAudio : this.hudChangeAudio).SpawnAndPlayOneShot(Audio.DefaultUIAudioSourcePrefab, base.transform.position, null);
	}

	// Token: 0x060037AA RID: 14250 RVA: 0x000F5E68 File Offset: 0x000F4068
	private void PlayFrameAnim(string animName, int frame = 0)
	{
		if (string.IsNullOrEmpty(animName))
		{
			return;
		}
		tk2dSpriteAnimationClip clip = this.animProxy.GetClip(animName);
		if (clip != null)
		{
			this.animator.PlayFromFrame(clip, frame);
		}
		this.RefreshLifebloodTint();
	}

	// Token: 0x060037AB RID: 14251 RVA: 0x000F5EA1 File Offset: 0x000F40A1
	private IEnumerator WarriorCrestRoutine()
	{
		HeroController hc = HeroController.instance;
		bool wasInRageMode = false;
		for (;;)
		{
			if (!hc.IsPaused())
			{
				bool flag = hc.WarriorState.RageEffectTimeLeft > 0f;
				if (flag)
				{
					if (!wasInRageMode)
					{
						this.PlayFrameAnim(this.warriorRageAnim, 0);
					}
				}
				else if (wasInRageMode)
				{
					this.PlayFrameAnim(this.warriorRageEndAnim, 0);
				}
				wasInRageMode = flag;
				yield return null;
			}
			else
			{
				yield return null;
			}
		}
		yield break;
	}

	// Token: 0x060037AC RID: 14252 RVA: 0x000F5EB0 File Offset: 0x000F40B0
	private IEnumerator ReaperCrestRoutine()
	{
		HeroController hc = HeroController.instance;
		bool wasInReaperMode = false;
		float reaperEffectTimeLeft = 0f;
		for (;;)
		{
			if (!hc.IsPaused())
			{
				HeroController.ReaperCrestStateInfo reaperState = hc.ReaperState;
				if (reaperState.IsInReaperMode)
				{
					if (!wasInReaperMode)
					{
						this.PlayFrameAnim(this.reaperModeBeginAnim, 0);
						if (this.reaperModeEffect)
						{
							this.reaperModeEffect.gameObject.SetActive(false);
							this.reaperModeEffect.gameObject.SetActive(true);
							this.reaperModeEffect.AlphaSelf = 1f;
							reaperEffectTimeLeft = 0f;
						}
					}
				}
				else if (wasInReaperMode)
				{
					this.PlayFrameAnim(this.reaperModeEndAnim, 0);
					if (this.reaperModeEffect)
					{
						reaperEffectTimeLeft = this.reaperModeEffect.FadeTo(0f, this.reaperModeEffectFadeOutTime, null, false, null);
					}
				}
				if (reaperEffectTimeLeft > 0f)
				{
					reaperEffectTimeLeft -= Time.deltaTime;
					if (reaperEffectTimeLeft <= 0f)
					{
						this.reaperModeEffect.gameObject.SetActive(false);
					}
				}
				wasInReaperMode = reaperState.IsInReaperMode;
				yield return null;
			}
			else
			{
				yield return null;
			}
		}
		yield break;
	}

	// Token: 0x060037AD RID: 14253 RVA: 0x000F5EBF File Offset: 0x000F40BF
	private IEnumerator WandererCrestRoutine()
	{
		HeroController hc = HeroController.instance;
		bool wasLucky = false;
		for (;;)
		{
			if (!hc.IsPaused())
			{
				bool isWandererLucky = hc.IsWandererLucky;
				if (isWandererLucky && !wasLucky)
				{
					this.PlayFrameAnim(this.wandererFullAnim, 0);
					if (!hc.IsRefillSoundsSuppressed && HudCanvas.IsVisible && ScreenFaderState.Alpha < 0.5f)
					{
						this.wandererHarpAppearAudio.SpawnAndPlayOneShot(Audio.DefaultUIAudioSourcePrefab, base.transform.position, null);
					}
				}
				else if (!isWandererLucky && wasLucky)
				{
					this.PlayFrameAnim(this.wandererFullEndAnim, 0);
					if (!hc.IsRefillSoundsSuppressed && HudCanvas.IsVisible && ScreenFaderState.Alpha < 0.5f)
					{
						this.wandererHarpDisappearAudio.SpawnAndPlayOneShot(Audio.DefaultUIAudioSourcePrefab, base.transform.position, null);
					}
				}
				wasLucky = isWandererLucky;
				yield return null;
			}
			else
			{
				yield return null;
			}
		}
		yield break;
	}

	// Token: 0x060037AE RID: 14254 RVA: 0x000F5ECE File Offset: 0x000F40CE
	private IEnumerator ToolmasterCrestRoutine()
	{
		PlayerData pd = PlayerData.instance;
		HeroController hc = HeroController.instance;
		bool couldBind = (float)pd.silk >= SilkSpool.BindCost;
		for (;;)
		{
			if (!hc.IsPaused())
			{
				bool flag = (float)pd.silk >= SilkSpool.BindCost;
				if (flag != couldBind || this.queuedToolmasterSpin)
				{
					this.PlayFrameAnim(this.toolmasterSilkGetAnim, 0);
				}
				couldBind = flag;
				this.queuedToolmasterSpin = false;
				yield return null;
			}
			else
			{
				yield return null;
			}
		}
		yield break;
	}

	// Token: 0x060037AF RID: 14255 RVA: 0x000F5EDD File Offset: 0x000F40DD
	private IEnumerator HunterCrestV2Routine()
	{
		return this.HunterCrestUpgradedRoutine(Gameplay.HunterComboHits, 0, this.hunterV2Bar, null, this.hunterV2FullAnim, null, this.hunterV2FrameAnims.Idle);
	}

	// Token: 0x060037B0 RID: 14256 RVA: 0x000F5F04 File Offset: 0x000F4104
	private IEnumerator HunterCrestV3Routine()
	{
		return this.HunterCrestUpgradedRoutine(Gameplay.HunterCombo2Hits, Gameplay.HunterCombo2ExtraHits, this.hunterV3BarA, this.hunterV3BarB, this.hunterV3FullAnimA, this.hunterV3FullAnimB, this.hunterV3FrameAnims.Idle);
	}

	// Token: 0x060037B1 RID: 14257 RVA: 0x000F5F3C File Offset: 0x000F413C
	private IEnumerator HunterCrestUpgradedRoutine(int maxHits, int extraMaxHits, UiProgressBar bar, UiProgressBar extraBar, string fullAnimA, string fullAnimB, string idleAnim)
	{
		HeroController hc = HeroController.instance;
		bar.Value = 0f;
		bar.gameObject.SetActive(true);
		if (extraBar != null)
		{
			extraBar.Value = 0f;
			extraBar.gameObject.SetActive(true);
		}
		int previousHits = -1;
		bool wasFull = false;
		bool wasFullExtra = false;
		for (;;)
		{
			if (!hc.IsPaused())
			{
				HeroController.HunterUpgCrestStateInfo hunterUpgState = hc.HunterUpgState;
				bool flag = hunterUpgState.CurrentMeterHits >= maxHits;
				int num = hunterUpgState.CurrentMeterHits - maxHits;
				bool flag2 = extraMaxHits > 0 && num >= extraMaxHits;
				if (num > 0 && extraMaxHits > 0 && hunterUpgState.CurrentMeterHits != previousHits)
				{
					this.hunterV3ExtraHitEffect.SetActive(false);
					this.hunterV3ExtraHitEffect.SetActive(true);
				}
				if (flag)
				{
					if (!wasFull)
					{
						bar.gameObject.SetActive(false);
						this.PlayFrameAnim(fullAnimA, 0);
						this.hunterV2ChargedAudio.SpawnAndPlayOneShot(Audio.DefaultUIAudioSourcePrefab, base.transform.position, null);
					}
					if (flag2)
					{
						if (!wasFullExtra)
						{
							if (extraBar)
							{
								extraBar.gameObject.SetActive(false);
							}
							this.PlayFrameAnim(fullAnimB, 0);
							this.hunterV3ChargedAudio.SpawnAndPlayOneShot(Audio.DefaultUIAudioSourcePrefab, base.transform.position, null);
						}
					}
					else if (extraBar != null)
					{
						if (!extraBar.gameObject.activeSelf)
						{
							extraBar.gameObject.SetActive(true);
						}
						if (hunterUpgState.CurrentMeterHits != previousHits)
						{
							extraBar.Value = (float)num / (float)extraMaxHits;
						}
					}
				}
				else
				{
					if (wasFull)
					{
						this.PlayFrameAnim(idleAnim, 0);
						if (extraBar != null)
						{
							extraBar.SetValueInstant(0f);
						}
					}
					if (hunterUpgState.CurrentMeterHits > previousHits)
					{
						bar.Value = (float)hunterUpgState.CurrentMeterHits / (float)maxHits;
					}
					else if (hunterUpgState.CurrentMeterHits < previousHits)
					{
						bar.SetValueInstant(0f);
					}
					if (wasFull)
					{
						bar.gameObject.SetActive(true);
					}
				}
				previousHits = hunterUpgState.CurrentMeterHits;
				wasFull = flag;
				wasFullExtra = flag2;
				yield return null;
			}
			else
			{
				yield return null;
			}
		}
		yield break;
	}

	// Token: 0x060037B2 RID: 14258 RVA: 0x000F5F8C File Offset: 0x000F418C
	private void ChangeEnded()
	{
		EventRegister.SendEvent(EventRegisterEvents.HudFrameChanged, null);
		if (this.activateEventsTarget && this.currentFrameAnims != null && !string.IsNullOrEmpty(this.currentFrameAnims.ActivateEvent))
		{
			this.activateEventsTarget.enabled = true;
			this.activateEventsTarget.SendEvent(this.currentFrameAnims.ActivateEvent);
		}
	}

	// Token: 0x04003A56 RID: 14934
	[SerializeField]
	private BindOrbHudFrame.BasicFrameAnims defaultFrameAnims;

	// Token: 0x04003A57 RID: 14935
	[SerializeField]
	private AudioEvent hudAppearAudio;

	// Token: 0x04003A58 RID: 14936
	[Header("Common")]
	[SerializeField]
	private float refreshDelay;

	// Token: 0x04003A59 RID: 14937
	[SerializeField]
	private ParticleSystem changeParticle;

	// Token: 0x04003A5A RID: 14938
	[SerializeField]
	private PlayMakerFSM activateEventsTarget;

	// Token: 0x04003A5B RID: 14939
	[SerializeField]
	private AudioEvent hudChangeAudio;

	// Token: 0x04003A5C RID: 14940
	[SerializeField]
	private Color lifebloodTint;

	// Token: 0x04003A5D RID: 14941
	[Header("Cloakless")]
	[SerializeField]
	private BindOrbHudFrame.BasicFrameAnims cloaklessFrameAnims;

	// Token: 0x04003A5E RID: 14942
	[Header("Hunter")]
	[SerializeField]
	private BindOrbHudFrame.BasicFrameAnims hunterV2FrameAnims;

	// Token: 0x04003A5F RID: 14943
	[SerializeField]
	private string hunterV2FullAnim;

	// Token: 0x04003A60 RID: 14944
	[SerializeField]
	private UiProgressBar hunterV2Bar;

	// Token: 0x04003A61 RID: 14945
	[SerializeField]
	private AudioEvent hunterV2ChargedAudio;

	// Token: 0x04003A62 RID: 14946
	[Space]
	[SerializeField]
	private BindOrbHudFrame.BasicFrameAnims hunterV3FrameAnims;

	// Token: 0x04003A63 RID: 14947
	[SerializeField]
	private string hunterV3FullAnimA;

	// Token: 0x04003A64 RID: 14948
	[SerializeField]
	private string hunterV3FullAnimB;

	// Token: 0x04003A65 RID: 14949
	[SerializeField]
	private UiProgressBar hunterV3BarA;

	// Token: 0x04003A66 RID: 14950
	[SerializeField]
	private UiProgressBar hunterV3BarB;

	// Token: 0x04003A67 RID: 14951
	[SerializeField]
	private GameObject hunterV3ExtraHitEffect;

	// Token: 0x04003A68 RID: 14952
	[SerializeField]
	private AudioEvent hunterV3ChargedAudio;

	// Token: 0x04003A69 RID: 14953
	[Header("Warrior")]
	[SerializeField]
	private BindOrbHudFrame.BasicFrameAnims warriorFrameAnims;

	// Token: 0x04003A6A RID: 14954
	[SerializeField]
	private BindOrbHudFrame.MeterAnims[] warriorMeterAnims;

	// Token: 0x04003A6B RID: 14955
	[SerializeField]
	private string warriorRageAnim;

	// Token: 0x04003A6C RID: 14956
	[SerializeField]
	private string warriorRageEndAnim;

	// Token: 0x04003A6D RID: 14957
	[Header("Reaper")]
	[SerializeField]
	private BindOrbHudFrame.BasicFrameAnims reaperFrameAnims;

	// Token: 0x04003A6E RID: 14958
	[SerializeField]
	private string reaperModeBeginAnim;

	// Token: 0x04003A6F RID: 14959
	[SerializeField]
	private string reaperModeEndAnim;

	// Token: 0x04003A70 RID: 14960
	[SerializeField]
	private NestedFadeGroupBase reaperModeEffect;

	// Token: 0x04003A71 RID: 14961
	[SerializeField]
	private float reaperModeEffectFadeOutTime;

	// Token: 0x04003A72 RID: 14962
	[Header("Wanderer")]
	[SerializeField]
	private BindOrbHudFrame.BasicFrameAnims wandererFrameAnims;

	// Token: 0x04003A73 RID: 14963
	[SerializeField]
	private string wandererFullAnim;

	// Token: 0x04003A74 RID: 14964
	[SerializeField]
	private string wandererFullEndAnim;

	// Token: 0x04003A75 RID: 14965
	[SerializeField]
	private AudioEvent wandererHarpAppearAudio;

	// Token: 0x04003A76 RID: 14966
	[SerializeField]
	private AudioEvent wandererHarpDisappearAudio;

	// Token: 0x04003A77 RID: 14967
	[Header("Witch")]
	[SerializeField]
	private AudioEvent hudChangeCursedAudio;

	// Token: 0x04003A78 RID: 14968
	[SerializeField]
	private BindOrbHudFrame.BasicFrameAnims cursedV1FrameAnims;

	// Token: 0x04003A79 RID: 14969
	[SerializeField]
	private BindOrbHudFrame.BasicFrameAnims witchFrameAnims;

	// Token: 0x04003A7A RID: 14970
	[Header("Toolmaster")]
	[SerializeField]
	private BindOrbHudFrame.BasicFrameAnims toolmasterFrameAnims;

	// Token: 0x04003A7B RID: 14971
	[SerializeField]
	private string toolmasterSilkGetAnim;

	// Token: 0x04003A7C RID: 14972
	[Header("Spell")]
	[SerializeField]
	private BindOrbHudFrame.BasicFrameAnims spellFrameAnims;

	// Token: 0x04003A7D RID: 14973
	private bool queuedToolmasterSpin;

	// Token: 0x04003A7E RID: 14974
	private bool isActive;

	// Token: 0x04003A7F RID: 14975
	private bool isCursed;

	// Token: 0x04003A80 RID: 14976
	private BindOrbHudFrame.BasicFrameAnims currentFrameAnims;

	// Token: 0x04003A81 RID: 14977
	private ToolCrest currentFrameCrest;

	// Token: 0x04003A82 RID: 14978
	private Coroutine animRoutine;

	// Token: 0x04003A83 RID: 14979
	private BindOrbHudFrame.FrameAnimEndDelegate onEndFrameAnim;

	// Token: 0x04003A84 RID: 14980
	private tk2dSpriteAnimator animator;

	// Token: 0x04003A85 RID: 14981
	private SteelSoulAnimProxy animProxy;

	// Token: 0x0200194E RID: 6478
	[Serializable]
	private class BasicFrameAnims
	{
		// Token: 0x0400957F RID: 38271
		public string AppearFromNone;

		// Token: 0x04009580 RID: 38272
		public string Appear;

		// Token: 0x04009581 RID: 38273
		public string Idle;

		// Token: 0x04009582 RID: 38274
		public string Disappear;

		// Token: 0x04009583 RID: 38275
		public string ActivateEvent;
	}

	// Token: 0x0200194F RID: 6479
	[Serializable]
	private class MeterAnims
	{
		// Token: 0x04009584 RID: 38276
		public GameObject[] IncreaseEffects;
	}

	// Token: 0x02001950 RID: 6480
	// (Invoke) Token: 0x0600945E RID: 37982
	private delegate void FrameAnimEndDelegate(Action onFrameEnded);

	// Token: 0x02001951 RID: 6481
	// (Invoke) Token: 0x06009462 RID: 37986
	private delegate IEnumerator CoroutineFunction();
}
