using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GlobalSettings;
using TeamCherry.NestedFadeGroup;
using TMProOld;
using UnityEngine;

// Token: 0x020006BE RID: 1726
[DefaultExecutionOrder(0)]
public class InventoryToolCrestList : InventoryItemSelectableDirectional
{
	// Token: 0x17000729 RID: 1833
	// (get) Token: 0x06003E82 RID: 16002 RVA: 0x00113248 File Offset: 0x00111448
	// (set) Token: 0x06003E83 RID: 16003 RVA: 0x00113250 File Offset: 0x00111450
	public bool IsSwitchingCrests { get; private set; }

	// Token: 0x1700072A RID: 1834
	// (get) Token: 0x06003E84 RID: 16004 RVA: 0x00113259 File Offset: 0x00111459
	// (set) Token: 0x06003E85 RID: 16005 RVA: 0x00113261 File Offset: 0x00111461
	public bool IsBlocked { get; set; }

	// Token: 0x1700072B RID: 1835
	// (get) Token: 0x06003E86 RID: 16006 RVA: 0x0011326A File Offset: 0x0011146A
	// (set) Token: 0x06003E87 RID: 16007 RVA: 0x00113272 File Offset: 0x00111472
	public bool IsSetupComplete { get; private set; }

	// Token: 0x1700072C RID: 1836
	// (get) Token: 0x06003E88 RID: 16008 RVA: 0x0011327B File Offset: 0x0011147B
	// (set) Token: 0x06003E89 RID: 16009 RVA: 0x00113283 File Offset: 0x00111483
	public InventoryToolCrest CurrentCrest { get; private set; }

	// Token: 0x1700072D RID: 1837
	// (get) Token: 0x06003E8A RID: 16010 RVA: 0x0011328C File Offset: 0x0011148C
	public Vector2 HomePosition
	{
		get
		{
			if (this.nudgeIfActive && this.nudgeIfActive.activeInHierarchy)
			{
				return this.initialPosition + this.nudgeOffset;
			}
			return this.initialPosition;
		}
	}

	// Token: 0x06003E8B RID: 16011 RVA: 0x001132C0 File Offset: 0x001114C0
	protected override void Awake()
	{
		base.Awake();
		this.manager = base.GetComponentInParent<InventoryItemToolManager>();
		this.pane = base.GetComponentInParent<InventoryPaneBase>();
		this.paneInput = base.GetComponentInParent<InventoryPaneInput>();
		if (this.pane)
		{
			this.pane.OnPaneEnd += delegate()
			{
				this.queuedPaneEnded = true;
				this.IsSwitchingCrests = false;
				this.isWaitingForApply = false;
			};
			this.pane.OnPaneStart += this.Setup;
			this.pane.OnInputLeft += delegate()
			{
				this.SwitchSelectedCrest(-1);
			};
			this.pane.OnInputRight += delegate()
			{
				this.SwitchSelectedCrest(1);
			};
		}
		this.initialPosition = base.transform.localPosition;
		this.inputHandler = GameManager.instance.inputHandler;
		this.Setup();
		if (this.changeCrestButton)
		{
			InventoryItemSelectableButtonEvent inventoryItemSelectableButtonEvent = this.changeCrestButton;
			inventoryItemSelectableButtonEvent.ButtonActivated = (Action)Delegate.Combine(inventoryItemSelectableButtonEvent.ButtonActivated, new Action(delegate()
			{
				this.wasChangeCrestButtonPressed = true;
			}));
		}
	}

	// Token: 0x06003E8C RID: 16012 RVA: 0x001133C0 File Offset: 0x001115C0
	private void Update()
	{
		if (this.IsBlocked || !this.pane || !this.manager || this.manager.IsActionsBlocked || this.isWaitingForApply || !this.paneInput || !this.paneInput.enabled)
		{
			return;
		}
		HeroActions inputActions = this.inputHandler.inputActions;
		Platform.MenuActions menuAction = Platform.Current.GetMenuAction(inputActions, false, false);
		InventoryItemToolManager.EquipStates equipState = this.manager.EquipState;
		if (equipState != InventoryItemToolManager.EquipStates.None)
		{
			if (equipState == InventoryItemToolManager.EquipStates.SwitchCrest)
			{
				if (((menuAction == Platform.MenuActions.Cancel || menuAction == Platform.MenuActions.Submit || InventoryPaneInput.IsInventoryButtonPressed(inputActions)) && this.pane.IsPaneActive) || this.queuedPaneEnded)
				{
					if (this.queuedPaneEnded || menuAction == Platform.MenuActions.Cancel)
					{
						if (this.manager.EndSwitchingCrest())
						{
							this.StopSwitchingCrests(false);
						}
					}
					else if (this.CanApplyCrest())
					{
						this.isWaitingForApply = true;
						if (this.CurrentCrest != this.previousEquippedCrest)
						{
							this.CurrentCrest.DoEquip(new Action(this.ApplyCurrentCrest));
						}
						else
						{
							this.ApplyCurrentCrest();
							this.changeCrestExitAudio.SpawnAndPlayOneShot(Audio.DefaultUIAudioSourcePrefab, base.transform.position, null);
						}
					}
				}
			}
		}
		else if ((menuAction == Platform.MenuActions.Super || this.wasChangeCrestButtonPressed) && this.pane.IsPaneActive && this.CanChangeCrests())
		{
			if (this.CurrentCrest && this.CurrentCrest.IsHidden)
			{
				if (this.changeCrestIconAnimator)
				{
					this.changeCrestIconAnimator.SetTrigger(InventoryToolCrestList._failed);
				}
				if (this.manager.ShowingCursedMsg)
				{
					this.manager.HideCursedMsg(false);
				}
				else
				{
					this.manager.ShowCursedMsg(true, ToolItemType.Red);
				}
			}
			else if (this.manager.BeginSwitchingCrest())
			{
				this.StartSwitchingCrests();
			}
			else if (this.CanApplyCrest() && this.manager.EndSwitchingCrest())
			{
				this.StopSwitchingCrests(true);
			}
		}
		this.queuedPaneEnded = false;
		this.wasChangeCrestButtonPressed = false;
	}

	// Token: 0x06003E8D RID: 16013 RVA: 0x001135DA File Offset: 0x001117DA
	private void ApplyCurrentCrest()
	{
		this.isWaitingForApply = false;
		if (!this.manager.EndSwitchingCrest())
		{
			return;
		}
		this.StopSwitchingCrests(true);
		this.manager.OnAppliedCrest();
	}

	// Token: 0x06003E8E RID: 16014 RVA: 0x00113604 File Offset: 0x00111804
	private bool CanApplyCrest()
	{
		bool flag = this.CurrentCrest == this.previousEquippedCrest || this.manager.CanChangeEquips();
		if (!flag && this.manager.EquipState == InventoryItemToolManager.EquipStates.SwitchCrest)
		{
			if (this.manager.ShowingCrestMsg)
			{
				this.manager.HideCrestEquipMsg(false);
				return flag;
			}
			this.manager.ShowCrestEquipMsg();
		}
		return flag;
	}

	// Token: 0x06003E8F RID: 16015 RVA: 0x00113668 File Offset: 0x00111868
	private void SetupCrests()
	{
		if (!this.templateCrest)
		{
			return;
		}
		this.templateCrest.gameObject.SetActive(true);
		List<ToolCrest> allCrests = ToolItemManager.GetAllCrests();
		for (int i = allCrests.Count - this.crests.Count; i > 0; i--)
		{
			InventoryToolCrest item = Object.Instantiate<InventoryToolCrest>(this.templateCrest, this.templateCrest.transform.parent);
			this.crests.Add(item);
		}
		for (int j = 0; j < this.crests.Count; j++)
		{
			InventoryItemManager.PropagateSelectables(this, this.crests[j]);
			this.crests[j].Setup((j < allCrests.Count) ? allCrests[j] : null);
		}
		this.templateCrest.gameObject.SetActive(false);
		if (this.changeCrestButton)
		{
			this.changeCrestButton.gameObject.SetActive(this.CanChangeCrests());
		}
	}

	// Token: 0x06003E90 RID: 16016 RVA: 0x0011375F File Offset: 0x0011195F
	public bool CanChangeCrests()
	{
		return this.crests.Count((InventoryToolCrest crest) => crest.IsUnlocked) > 1;
	}

	// Token: 0x06003E91 RID: 16017 RVA: 0x00113790 File Offset: 0x00111990
	private void Setup()
	{
		this.IsSetupComplete = false;
		base.transform.SetLocalPosition2D(this.HomePosition);
		this.SetupCrests();
		foreach (InventoryToolCrest inventoryToolCrest in this.crests)
		{
			inventoryToolCrest.GetEquippedForSlots();
		}
		this.SetupUnlockedCrests();
		InventoryToolCrest inventoryToolCrest2 = null;
		string currentCrestId = GameManager.instance.playerData.CurrentCrestID;
		if (!string.IsNullOrEmpty(currentCrestId))
		{
			inventoryToolCrest2 = this.crests.FirstOrDefault((InventoryToolCrest c) => c.gameObject.name == currentCrestId);
		}
		if (!inventoryToolCrest2 && this.crests.Count > 0)
		{
			inventoryToolCrest2 = this.crests[0];
		}
		if (inventoryToolCrest2)
		{
			this.SetCurrentCrest(inventoryToolCrest2, false, false);
			foreach (InventoryToolCrest inventoryToolCrest3 in this.crests)
			{
				inventoryToolCrest3.UpdateListDisplay(true);
				inventoryToolCrest3.Show(inventoryToolCrest3 == this.CurrentCrest, true);
			}
		}
		this.UpdateEnabledCrests(false);
		if (this.manager)
		{
			this.manager.RefreshTools();
		}
		this.IsSetupComplete = true;
	}

	// Token: 0x06003E92 RID: 16018 RVA: 0x001138F4 File Offset: 0x00111AF4
	public override InventoryItemSelectable Get(InventoryItemManager.SelectionDirection? direction)
	{
		List<InventoryToolCrest> list = (from crest in this.crests
		where crest.gameObject.activeSelf
		select crest).ToList<InventoryToolCrest>();
		if (this.CurrentCrest)
		{
			return this.CurrentCrest.Get(direction);
		}
		if (list.Count > 0)
		{
			return list[0].Get(direction);
		}
		return base.Get(direction);
	}

	// Token: 0x06003E93 RID: 16019 RVA: 0x00113969 File Offset: 0x00111B69
	public bool CrestHasSlot(ToolItemType type)
	{
		return this.CurrentCrest && this.CurrentCrest.HasSlot(type);
	}

	// Token: 0x06003E94 RID: 16020 RVA: 0x00113986 File Offset: 0x00111B86
	public bool CrestHasAnySlots()
	{
		return this.CurrentCrest && this.CurrentCrest.HasAnySlots();
	}

	// Token: 0x06003E95 RID: 16021 RVA: 0x001139A2 File Offset: 0x00111BA2
	public InventoryToolCrestSlot GetEquippedToolSlot(ToolItem itemData)
	{
		if (this.CurrentCrest)
		{
			return this.CurrentCrest.GetEquippedToolSlot(itemData);
		}
		return null;
	}

	// Token: 0x06003E96 RID: 16022 RVA: 0x001139BF File Offset: 0x00111BBF
	public IEnumerable<InventoryToolCrestSlot> GetSlots()
	{
		if (this.CurrentCrest)
		{
			return this.CurrentCrest.GetSlots();
		}
		return Enumerable.Empty<InventoryToolCrestSlot>();
	}

	// Token: 0x06003E97 RID: 16023 RVA: 0x001139E0 File Offset: 0x00111BE0
	private void SetCurrentCrest(InventoryToolCrest crest, bool doScroll, bool doSave)
	{
		this.previousSelectedCrest = this.CurrentCrest;
		this.CurrentCrest = crest;
		if (this.previousSelectedCrest)
		{
			this.previousSelectedCrest.UpdateListDisplay(!doScroll);
		}
		if (this.CurrentCrest)
		{
			this.CurrentCrest.UpdateListDisplay(!doScroll);
		}
		this.ScrollToCrest(crest, doScroll ? this.scrollTime : 0f);
		if (Application.isPlaying)
		{
			if (doSave)
			{
				ToolItemManager.SetEquippedCrest(crest.gameObject.name);
			}
			if (this.manager)
			{
				this.manager.RefreshTools(true, false);
			}
			foreach (TextMeshPro textMeshPro in this.crestNameDisplays)
			{
				if (textMeshPro)
				{
					textMeshPro.text = crest.DisplayName;
				}
			}
			if (this.crestDescriptionDisplay)
			{
				this.crestDescriptionDisplay.text = crest.Description;
			}
			ToolCrest crestData = crest.CrestData;
			if (crestData.HasCustomAction)
			{
				this.comboButtonPromptDisplay.Show(crestData.CustomButtonCombo);
			}
			else
			{
				this.comboButtonPromptDisplay.Hide();
			}
		}
		if (this.CurrentCrest == this.previousSelectedCrest)
		{
			return;
		}
		if (this.CurrentCrest)
		{
			foreach (InventoryToolCrestSlot inventoryToolCrestSlot in this.CurrentCrest.GetSlots())
			{
				inventoryToolCrestSlot.SetIsVisible(true);
			}
		}
		if (this.previousSelectedCrest)
		{
			foreach (InventoryToolCrestSlot inventoryToolCrestSlot2 in this.previousSelectedCrest.GetSlots())
			{
				inventoryToolCrestSlot2.SetIsVisible(false);
			}
		}
	}

	// Token: 0x06003E98 RID: 16024 RVA: 0x00113BB8 File Offset: 0x00111DB8
	private void SetupUnlockedCrests()
	{
		this.unlockedCrests.Clear();
		foreach (InventoryToolCrest inventoryToolCrest in this.crests)
		{
			if (inventoryToolCrest.IsUnlocked)
			{
				this.unlockedCrests.Add(inventoryToolCrest);
			}
		}
	}

	// Token: 0x06003E99 RID: 16025 RVA: 0x00113C24 File Offset: 0x00111E24
	private void UpdateEnabledCrests(bool setAllEnabled)
	{
		foreach (InventoryToolCrest inventoryToolCrest in this.crests)
		{
			inventoryToolCrest.gameObject.SetActive(setAllEnabled || inventoryToolCrest == this.CurrentCrest);
		}
	}

	// Token: 0x06003E9A RID: 16026 RVA: 0x00113C90 File Offset: 0x00111E90
	private void StartSwitchingCrests()
	{
		this.IsSwitchingCrests = true;
		this.previousEquippedCrest = this.CurrentCrest;
		this.UpdateEnabledCrests(true);
		this.scrollLeftArrowGroup.AlphaSelf = 0f;
		this.scrollRightArrowGroup.AlphaSelf = 0f;
		this.CurrentCrest.UpdateListDisplay(false);
		this.ScrollToCrest(this.CurrentCrest, 0f);
		foreach (InventoryToolCrestSlot inventoryToolCrestSlot in this.CurrentCrest.GetSlots())
		{
			inventoryToolCrestSlot.Deselect();
		}
		this.changeCrestEnterAudio.SpawnAndPlayOneShot(Audio.DefaultUIAudioSourcePrefab, base.transform.position, null);
		if (this.crestSwitchSequenceRoutine != null)
		{
			base.StopCoroutine(this.crestSwitchSequenceRoutine);
		}
		this.crestSwitchSequenceRoutine = base.StartCoroutine(this.ModeSwitchSequence(true));
	}

	// Token: 0x06003E9B RID: 16027 RVA: 0x00113D7C File Offset: 0x00111F7C
	public void StopSwitchingCrests(bool keepNewSelection)
	{
		if (this.IsSwitchingCrests)
		{
			if (keepNewSelection)
			{
				this.SetCurrentCrest(this.CurrentCrest, true, true);
			}
			else
			{
				this.SetCurrentCrest(this.previousEquippedCrest, false, true);
				this.changeCrestExitAudio.SpawnAndPlayOneShot(Audio.DefaultUIAudioSourcePrefab, base.transform.position, null);
			}
		}
		this.IsSwitchingCrests = false;
		this.previousEquippedCrest = null;
		if (this.crestSwitchSequenceRoutine != null)
		{
			base.StopCoroutine(this.crestSwitchSequenceRoutine);
		}
		this.crestSwitchSequenceRoutine = base.StartCoroutine(this.ModeSwitchSequence(false));
	}

	// Token: 0x06003E9C RID: 16028 RVA: 0x00113E04 File Offset: 0x00112004
	private IEnumerator ModeSwitchSequence(bool isSwitching)
	{
		if (this.crestSwitchMoveRoutine != null)
		{
			base.StopCoroutine(this.crestSwitchMoveRoutine);
		}
		if (isSwitching)
		{
			yield return new WaitForSecondsRealtime(this.manager.FadeToolGroup(false));
			this.manager.RefreshTools();
			this.crestSwitchMoveRoutine = base.StartCoroutine(this.CrestListMove(this.initialPosition + this.crestModeSwitchOffset));
			yield return this.crestSwitchMoveRoutine;
			foreach (InventoryToolCrest inventoryToolCrest in this.unlockedCrests)
			{
				if (inventoryToolCrest != this.CurrentCrest)
				{
					inventoryToolCrest.Show(true, false);
				}
			}
			this.manager.FadeCrestGroup(true);
		}
		else
		{
			float num = this.manager.FadeCrestGroup(false);
			foreach (InventoryToolCrest inventoryToolCrest2 in this.unlockedCrests)
			{
				if (inventoryToolCrest2 != this.CurrentCrest)
				{
					num = Mathf.Max(num, inventoryToolCrest2.Show(false, false));
				}
			}
			yield return new WaitForSecondsRealtime(num);
			this.UpdateEnabledCrests(false);
			this.CurrentCrest.GetEquippedForSlots();
			this.crestSwitchMoveRoutine = base.StartCoroutine(this.CrestListMove(this.HomePosition));
			yield return this.crestSwitchMoveRoutine;
			this.manager.FadeToolGroup(true);
			this.manager.RefreshTools();
		}
		if (this.CurrentCrest)
		{
			this.CurrentCrest.UpdateListDisplay(false);
		}
		this.crestSwitchSequenceRoutine = null;
		yield break;
	}

	// Token: 0x06003E9D RID: 16029 RVA: 0x00113E1C File Offset: 0x0011201C
	public void PaneMovePrevented()
	{
		HeroActions inputActions = ManagerSingleton<InputHandler>.Instance.inputActions;
		if (inputActions.PaneLeft.IsPressed)
		{
			this.SwitchSelectedCrest(-1);
			return;
		}
		if (inputActions.PaneRight.IsPressed)
		{
			this.SwitchSelectedCrest(1);
		}
	}

	// Token: 0x06003E9E RID: 16030 RVA: 0x00113E60 File Offset: 0x00112060
	private void SwitchSelectedCrest(int direction)
	{
		if (!this.IsSwitchingCrests || direction == 0 || this.isWaitingForApply || this.crestSwitchSequenceRoutine != null)
		{
			return;
		}
		this.manager.HideCrestEquipMsg(true);
		direction = (int)Mathf.Sign((float)direction);
		int num = this.unlockedCrests.IndexOf(this.CurrentCrest);
		num += direction;
		if (num < 0 || num >= this.unlockedCrests.Count)
		{
			return;
		}
		BaseAnimator baseAnimator = (direction > 0) ? this.scrollRightArrow : this.scrollLeftArrow;
		if (baseAnimator)
		{
			baseAnimator.StartAnimation();
		}
		this.SetCurrentCrest(this.unlockedCrests[num], true, false);
	}

	// Token: 0x06003E9F RID: 16031 RVA: 0x00113F00 File Offset: 0x00112100
	private void ScrollToCrest(InventoryToolCrest crest, float duration)
	{
		if (this.scrollRoutine != null)
		{
			base.StopCoroutine(this.scrollRoutine);
			this.scrollRoutine = null;
		}
		if (this.onScrollEnd != null)
		{
			this.onScrollEnd();
		}
		if (base.isActiveAndEnabled)
		{
			this.scrollRoutine = base.StartCoroutine(this.ScrollToCrestRoutine(crest, duration));
			return;
		}
		this.ScrollToCrestRoutine(crest, 0f).MoveNext();
	}

	// Token: 0x06003EA0 RID: 16032 RVA: 0x00113F6A File Offset: 0x0011216A
	private IEnumerator ScrollToCrestRoutine(InventoryToolCrest crest, float duration)
	{
		this.UpdateCrestPositions(null, null, 0f);
		float x = -crest.transform.localPosition.x;
		Vector3 localPosition = this.scrollParent.localPosition;
		Vector3 targetPosition = localPosition;
		targetPosition.x = x;
		int? previousCrestIndex = null;
		if (this.previousSelectedCrest)
		{
			previousCrestIndex = new int?(this.unlockedCrests.IndexOf(this.previousSelectedCrest));
		}
		int currentCrestIndex = this.unlockedCrests.IndexOf(this.CurrentCrest);
		if (duration > 0f && previousCrestIndex != null)
		{
			int? previousCrestIndex2 = previousCrestIndex;
			int currentCrestIndex2 = currentCrestIndex;
			if (!(previousCrestIndex2.GetValueOrDefault() == currentCrestIndex2 & previousCrestIndex2 != null))
			{
				this.scrollAudio.SpawnAndPlayOneShot(Audio.DefaultUIAudioSourcePrefab, base.transform.position, null);
			}
		}
		this.scrollLeftArrowGroup.FadeTo((float)((currentCrestIndex > 0) ? 1 : 0), this.arrowFadeTime, null, true, null);
		this.scrollRightArrowGroup.FadeTo((float)((currentCrestIndex < this.unlockedCrests.Count - 1) ? 1 : 0), this.arrowFadeTime, null, true, null);
		this.onScrollEnd = delegate()
		{
			this.scrollParent.localPosition = targetPosition;
			this.UpdateCrestPositions(previousCrestIndex, new int?(currentCrestIndex), 1f);
			this.onScrollEnd = null;
		};
		for (float elapsed = 0f; elapsed < duration; elapsed += Time.unscaledDeltaTime)
		{
			float num = elapsed / duration;
			this.scrollParent.localPosition = Vector3.Lerp(localPosition, targetPosition, num);
			this.UpdateCrestPositions(previousCrestIndex, new int?(currentCrestIndex), num);
			yield return null;
		}
		this.onScrollEnd();
		yield break;
	}

	// Token: 0x06003EA1 RID: 16033 RVA: 0x00113F87 File Offset: 0x00112187
	private IEnumerator CrestListMove(Vector2 toPosition)
	{
		Vector2 fromPosition = base.transform.localPosition;
		for (float elapsed = 0f; elapsed < this.crestModeSwitchMoveTime; elapsed += Time.unscaledDeltaTime)
		{
			base.transform.SetLocalPosition2D(Vector2.Lerp(fromPosition, toPosition, elapsed / this.crestModeSwitchMoveTime));
			yield return null;
		}
		base.transform.SetLocalPosition2D(toPosition);
		yield break;
	}

	// Token: 0x06003EA2 RID: 16034 RVA: 0x00113FA0 File Offset: 0x001121A0
	private void UpdateCrestPositions(int? previousCrestIndex, int? currentCrestIndex, float blend)
	{
		for (int i = 0; i < this.unlockedCrests.Count; i++)
		{
			float b = 0f;
			float a = 0f;
			int num = i;
			int? num2 = currentCrestIndex + 1;
			if (num == num2.GetValueOrDefault() & num2 != null)
			{
				b = 1f;
			}
			else
			{
				int num3 = i;
				num2 = currentCrestIndex - 1;
				if (num3 == num2.GetValueOrDefault() & num2 != null)
				{
					b = -1f;
				}
			}
			int num4 = i;
			num2 = previousCrestIndex + 1;
			if (num4 == num2.GetValueOrDefault() & num2 != null)
			{
				a = 1f;
			}
			else
			{
				int num5 = i;
				num2 = previousCrestIndex - 1;
				if (num5 == num2.GetValueOrDefault() & num2 != null)
				{
					a = -1f;
				}
			}
			float num6 = Mathf.Lerp(a, b, blend);
			this.unlockedCrests[i].transform.SetLocalPositionX(this.crestSpacing * (float)i + num6 * this.adjacentCrestOffset);
		}
	}

	// Token: 0x06003EA3 RID: 16035 RVA: 0x00114112 File Offset: 0x00112312
	private void OnDrawGizmosSelected()
	{
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Gizmos.DrawWireSphere(this.crestModeSwitchOffset, 0.25f);
	}

	// Token: 0x04004015 RID: 16405
	[SerializeField]
	private TextMeshPro[] crestNameDisplays;

	// Token: 0x04004016 RID: 16406
	[SerializeField]
	private TextMeshPro crestDescriptionDisplay;

	// Token: 0x04004017 RID: 16407
	[SerializeField]
	private InventoryItemComboButtonPromptDisplay comboButtonPromptDisplay;

	// Token: 0x04004018 RID: 16408
	[Space]
	[SerializeField]
	private InventoryToolCrest templateCrest;

	// Token: 0x04004019 RID: 16409
	[SerializeField]
	private float crestSpacing = 4f;

	// Token: 0x0400401A RID: 16410
	[SerializeField]
	private float adjacentCrestOffset = 1f;

	// Token: 0x0400401B RID: 16411
	[SerializeField]
	private Transform scrollParent;

	// Token: 0x0400401C RID: 16412
	[SerializeField]
	private float scrollTime = 0.3f;

	// Token: 0x0400401D RID: 16413
	[SerializeField]
	private AudioEvent scrollAudio;

	// Token: 0x0400401E RID: 16414
	[SerializeField]
	private BaseAnimator scrollLeftArrow;

	// Token: 0x0400401F RID: 16415
	[SerializeField]
	private NestedFadeGroupBase scrollLeftArrowGroup;

	// Token: 0x04004020 RID: 16416
	[SerializeField]
	private BaseAnimator scrollRightArrow;

	// Token: 0x04004021 RID: 16417
	[SerializeField]
	private NestedFadeGroupBase scrollRightArrowGroup;

	// Token: 0x04004022 RID: 16418
	[SerializeField]
	private float arrowFadeTime = 0.3f;

	// Token: 0x04004023 RID: 16419
	[SerializeField]
	private AudioEvent changeCrestEnterAudio;

	// Token: 0x04004024 RID: 16420
	[SerializeField]
	private AudioEvent changeCrestExitAudio;

	// Token: 0x04004025 RID: 16421
	[SerializeField]
	private InventoryItemSelectableButtonEvent changeCrestButton;

	// Token: 0x04004026 RID: 16422
	[SerializeField]
	private Animator changeCrestIconAnimator;

	// Token: 0x04004027 RID: 16423
	[SerializeField]
	private Vector2 crestModeSwitchOffset;

	// Token: 0x04004028 RID: 16424
	[Space]
	[SerializeField]
	private float crestModeSwitchMoveTime = 0.2f;

	// Token: 0x04004029 RID: 16425
	[SerializeField]
	private GameObject nudgeIfActive;

	// Token: 0x0400402A RID: 16426
	[SerializeField]
	private Vector2 nudgeOffset;

	// Token: 0x0400402B RID: 16427
	private readonly List<InventoryToolCrest> crests = new List<InventoryToolCrest>();

	// Token: 0x0400402C RID: 16428
	private readonly List<InventoryToolCrest> unlockedCrests = new List<InventoryToolCrest>();

	// Token: 0x0400402D RID: 16429
	private Coroutine crestSwitchMoveRoutine;

	// Token: 0x0400402E RID: 16430
	private Coroutine crestSwitchSequenceRoutine;

	// Token: 0x0400402F RID: 16431
	private Coroutine scrollRoutine;

	// Token: 0x04004030 RID: 16432
	private Action onScrollEnd;

	// Token: 0x04004031 RID: 16433
	private Vector2 initialPosition;

	// Token: 0x04004032 RID: 16434
	private bool wasChangeCrestButtonPressed;

	// Token: 0x04004033 RID: 16435
	private InventoryPaneBase pane;

	// Token: 0x04004034 RID: 16436
	private InventoryPaneInput paneInput;

	// Token: 0x04004035 RID: 16437
	private InventoryItemToolManager manager;

	// Token: 0x04004036 RID: 16438
	private InputHandler inputHandler;

	// Token: 0x04004037 RID: 16439
	private bool queuedPaneEnded;

	// Token: 0x04004038 RID: 16440
	private bool isWaitingForApply;

	// Token: 0x04004039 RID: 16441
	private InventoryToolCrest previousSelectedCrest;

	// Token: 0x0400403A RID: 16442
	private InventoryToolCrest previousEquippedCrest;

	// Token: 0x0400403B RID: 16443
	private static readonly int _failed = Animator.StringToHash("Failed");
}
