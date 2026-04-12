using System;
using System.Collections;
using System.Collections.Generic;
using GlobalSettings;
using TeamCherry.NestedFadeGroup;
using TeamCherry.SharedUtils;
using TMProOld;
using UnityEngine;

// Token: 0x020006BF RID: 1727
[DefaultExecutionOrder(2)]
public class InventoryToolCrestSlot : InventoryItemToolBase
{
	// Token: 0x140000DA RID: 218
	// (add) Token: 0x06003EAA RID: 16042 RVA: 0x001141E0 File Offset: 0x001123E0
	// (remove) Token: 0x06003EAB RID: 16043 RVA: 0x00114218 File Offset: 0x00112418
	public event Action OnSetEquipSaved;

	// Token: 0x1700072E RID: 1838
	// (get) Token: 0x06003EAC RID: 16044 RVA: 0x00114250 File Offset: 0x00112450
	private MaterialPropertyBlock Block
	{
		get
		{
			MaterialPropertyBlock result;
			if ((result = this.block) == null)
			{
				result = (this.block = new MaterialPropertyBlock());
			}
			return result;
		}
	}

	// Token: 0x1700072F RID: 1839
	// (get) Token: 0x06003EAD RID: 16045 RVA: 0x00114275 File Offset: 0x00112475
	// (set) Token: 0x06003EAE RID: 16046 RVA: 0x0011427D File Offset: 0x0011247D
	public InventoryToolCrest Crest { get; private set; }

	// Token: 0x17000730 RID: 1840
	// (get) Token: 0x06003EAF RID: 16047 RVA: 0x00114286 File Offset: 0x00112486
	// (set) Token: 0x06003EB0 RID: 16048 RVA: 0x0011428E File Offset: 0x0011248E
	public int SlotIndex { get; private set; }

	// Token: 0x17000731 RID: 1841
	// (get) Token: 0x06003EB1 RID: 16049 RVA: 0x00114297 File Offset: 0x00112497
	public override string DisplayName
	{
		get
		{
			if (!this.EquippedItem)
			{
				return string.Empty;
			}
			return this.EquippedItem.DisplayName;
		}
	}

	// Token: 0x17000732 RID: 1842
	// (get) Token: 0x06003EB2 RID: 16050 RVA: 0x001142BC File Offset: 0x001124BC
	public override string Description
	{
		get
		{
			if (!this.EquippedItem)
			{
				return string.Empty;
			}
			return this.EquippedItem.Description;
		}
	}

	// Token: 0x17000733 RID: 1843
	// (get) Token: 0x06003EB3 RID: 16051 RVA: 0x001142E1 File Offset: 0x001124E1
	public override Sprite Sprite
	{
		get
		{
			if (!this.EquippedItem)
			{
				return this.slotTypeSprite;
			}
			return this.EquippedItem.InventorySpriteBase;
		}
	}

	// Token: 0x17000734 RID: 1844
	// (get) Token: 0x06003EB4 RID: 16052 RVA: 0x00114302 File Offset: 0x00112502
	private Sprite ItemSprite
	{
		get
		{
			if (!this.EquippedItem)
			{
				return null;
			}
			return this.EquippedItem.GetInventorySprite((this.EquippedItem.PoisonDamageTicks > 0 && this.IsToolEquipped(Gameplay.PoisonPouchTool)) ? ToolItem.IconVariants.Poison : ToolItem.IconVariants.Default);
		}
	}

	// Token: 0x17000735 RID: 1845
	// (get) Token: 0x06003EB5 RID: 16053 RVA: 0x00114340 File Offset: 0x00112540
	public override Color SpriteTint
	{
		get
		{
			if (this.EquippedItem && this.itemIcon)
			{
				return Color.white;
			}
			if (this.IsLocked && (!this.manager || !this.manager.CanUnlockSlot))
			{
				return new Color(0.5f, 0.5f, 0.5f, 1f);
			}
			if (this.slotTypeIcon)
			{
				return this.slotTypeIcon.Color;
			}
			return Color.white;
		}
	}

	// Token: 0x17000736 RID: 1846
	// (get) Token: 0x06003EB6 RID: 16054 RVA: 0x001143C8 File Offset: 0x001125C8
	public override Color? CursorColor
	{
		get
		{
			if (!this.manager)
			{
				return base.CursorColor;
			}
			if (this.isPulsingColour)
			{
				return new Color?(this.pulseColourA);
			}
			if (this.IsLocked)
			{
				return base.CursorColor;
			}
			return new Color?(this.manager.GetToolTypeColor(this.SlotInfo.Type));
		}
	}

	// Token: 0x17000737 RID: 1847
	// (get) Token: 0x06003EB7 RID: 16055 RVA: 0x00114427 File Offset: 0x00112627
	// (set) Token: 0x06003EB8 RID: 16056 RVA: 0x00114454 File Offset: 0x00112654
	public float ItemFlashAmount
	{
		get
		{
			if (this.itemIcon)
			{
				return this.itemIcon.sharedMaterial.GetFloat(InventoryToolCrestSlot._flashAmountProp);
			}
			return 0f;
		}
		set
		{
			value = Mathf.Clamp01(value);
			if (this.itemIcon)
			{
				MaterialPropertyBlock materialPropertyBlock = this.Block;
				materialPropertyBlock.Clear();
				this.itemIcon.GetPropertyBlock(materialPropertyBlock);
				materialPropertyBlock.SetFloat(InventoryToolCrestSlot._flashAmountProp, value);
				this.itemIcon.SetPropertyBlock(materialPropertyBlock);
			}
			if (this.slotTypeIconGroup)
			{
				this.slotTypeIconGroup.AlphaSelf = 1f - value;
			}
			if (this.slotTypeIconFilled)
			{
				this.slotTypeIconFilled.AlphaSelf = (this.EquippedItem ? 0f : value);
			}
		}
	}

	// Token: 0x17000738 RID: 1848
	// (get) Token: 0x06003EB9 RID: 16057 RVA: 0x001144F3 File Offset: 0x001126F3
	// (set) Token: 0x06003EBA RID: 16058 RVA: 0x001144FB File Offset: 0x001126FB
	public ToolItem EquippedItem { get; private set; }

	// Token: 0x17000739 RID: 1849
	// (get) Token: 0x06003EBB RID: 16059 RVA: 0x00114504 File Offset: 0x00112704
	public override ToolItem ItemData
	{
		get
		{
			return this.EquippedItem;
		}
	}

	// Token: 0x1700073A RID: 1850
	// (get) Token: 0x06003EBC RID: 16060 RVA: 0x0011450C File Offset: 0x0011270C
	public ToolItemType Type
	{
		get
		{
			return this.SlotInfo.Type;
		}
	}

	// Token: 0x1700073B RID: 1851
	// (get) Token: 0x06003EBD RID: 16061 RVA: 0x0011451C File Offset: 0x0011271C
	public bool IsLocked
	{
		get
		{
			return !(this.Crest == null) && !(this.Crest.CrestData == null) && this.slotInfo.IsLocked && !this.SaveData.IsUnlocked;
		}
	}

	// Token: 0x1700073C RID: 1852
	// (get) Token: 0x06003EBE RID: 16062 RVA: 0x0011456C File Offset: 0x0011276C
	// (set) Token: 0x06003EBF RID: 16063 RVA: 0x001145D4 File Offset: 0x001127D4
	public ToolCrestsData.SlotData SaveData
	{
		get
		{
			if (this.getSavedDataOverride != null)
			{
				return this.getSavedDataOverride();
			}
			List<ToolCrestsData.SlotData> slots = PlayerData.instance.ToolEquips.GetData(this.Crest.name).Slots;
			if (slots == null || this.SlotIndex >= slots.Count)
			{
				return default(ToolCrestsData.SlotData);
			}
			return slots[this.SlotIndex];
		}
		private set
		{
			if (this.setSavedDataOverride != null)
			{
				this.setSavedDataOverride(value);
				return;
			}
			PlayerData instance = PlayerData.instance;
			ToolCrestsData.Data data = instance.ToolEquips.GetData(this.Crest.name);
			List<ToolCrestsData.SlotData> list = data.Slots;
			if (list == null)
			{
				list = (data.Slots = new List<ToolCrestsData.SlotData>());
				instance.ToolEquips.SetData(this.Crest.name, data);
			}
			while (list.Count < this.SlotIndex + 1)
			{
				list.Add(default(ToolCrestsData.SlotData));
			}
			list[this.SlotIndex] = value;
		}
	}

	// Token: 0x1700073D RID: 1853
	// (get) Token: 0x06003EC0 RID: 16064 RVA: 0x00114670 File Offset: 0x00112870
	// (set) Token: 0x06003EC1 RID: 16065 RVA: 0x00114678 File Offset: 0x00112878
	public ToolCrest.SlotInfo SlotInfo
	{
		get
		{
			return this.slotInfo;
		}
		set
		{
			this.slotInfo = value;
			this.GetComponentsIfNeeded();
			if (this.itemIcon)
			{
				MaterialPropertyBlock materialPropertyBlock = this.Block;
				materialPropertyBlock.Clear();
				this.itemIcon.GetPropertyBlock(materialPropertyBlock);
				materialPropertyBlock.SetColor(InventoryToolCrestSlot._flashColorProp, this.manager.GetToolTypeColor(this.slotInfo.Type));
				this.itemIcon.SetPropertyBlock(materialPropertyBlock);
			}
			if (this.IsLocked && !this.spawnedUnlockBurstEffect && this.unlockBurstEffectPrefab)
			{
				PassColour passColour = Object.Instantiate<PassColour>(this.unlockBurstEffectPrefab, base.transform);
				passColour.gameObject.SetActive(false);
				passColour.transform.localPosition = Vector3.zero;
				this.spawnedUnlockBurstEffect = passColour;
			}
			if (this.spawnedUnlockBurstEffect)
			{
				this.spawnedUnlockBurstEffect.SetColour(this.manager.GetToolTypeColor(this.Type));
			}
			if (this.slotAnimator && this.slotInfo.Type.IsAttackType())
			{
				this.slotAnimator.runtimeAnimatorController = this.attackAnimatorControllers[(int)this.slotInfo.AttackBinding];
			}
		}
	}

	// Token: 0x1700073E RID: 1854
	// (get) Token: 0x06003EC2 RID: 16066 RVA: 0x001147A1 File Offset: 0x001129A1
	// (set) Token: 0x06003EC3 RID: 16067 RVA: 0x001147A8 File Offset: 0x001129A8
	protected override bool IsSeen
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	// Token: 0x1700073F RID: 1855
	// (get) Token: 0x06003EC4 RID: 16068 RVA: 0x001147AF File Offset: 0x001129AF
	protected override bool IsAutoNavSelectable
	{
		get
		{
			return this.wasVisible;
		}
	}

	// Token: 0x06003EC5 RID: 16069 RVA: 0x001147B8 File Offset: 0x001129B8
	protected override void Awake()
	{
		base.Awake();
		if (this.itemIcon)
		{
			this.itemIcon.sprite = null;
		}
		if (this.itemIconMask)
		{
			this.itemIconMask.sprite = null;
		}
		this.GetComponentsIfNeeded();
		InventoryPaneBase componentInParent = base.GetComponentInParent<InventoryPaneBase>();
		if (componentInParent)
		{
			componentInParent.OnPaneEnd += delegate()
			{
				if (this.spawnedUnlockBurstEffect)
				{
					this.spawnedUnlockBurstEffect.gameObject.SetActive(false);
				}
			};
		}
	}

	// Token: 0x06003EC6 RID: 16070 RVA: 0x00114824 File Offset: 0x00112A24
	protected override void OnValidate()
	{
		base.OnValidate();
		ArrayForEnumAttribute.EnsureArraySize<RuntimeAnimatorController>(ref this.attackAnimatorControllers, typeof(AttackToolBinding));
	}

	// Token: 0x06003EC7 RID: 16071 RVA: 0x00114841 File Offset: 0x00112A41
	protected override void OnEnable()
	{
		base.OnEnable();
		this.previousAnimId = -1;
		if (this.queuedAnimId != 0)
		{
			this.PlayAnim(this.queuedAnimId);
		}
		if (this.queuedSmallAnimId != 0)
		{
			this.PlayAnimSmall(this.queuedSmallAnimId);
		}
	}

	// Token: 0x06003EC8 RID: 16072 RVA: 0x00114878 File Offset: 0x00112A78
	protected override void OnDisable()
	{
		base.OnDisable();
		this.queuedAnimId = 0;
		this.queuedSmallAnimId = 0;
	}

	// Token: 0x06003EC9 RID: 16073 RVA: 0x0011488E File Offset: 0x00112A8E
	public void SetIsVisible(bool isVisible)
	{
		this.wasVisible = isVisible;
		base.EvaluateAutoNav();
	}

	// Token: 0x06003ECA RID: 16074 RVA: 0x001148A0 File Offset: 0x00112AA0
	protected override void Update()
	{
		base.Update();
		if (this.isPulsingColour)
		{
			this.pulseColourTimeElapsed += Time.unscaledDeltaTime;
			if (this.pulseColourTimeElapsed > this.unlockReadyColorPulseDuration)
			{
				this.pulseColourTimeElapsed %= this.unlockReadyColorPulseDuration;
			}
			float t = this.unlockReadyColourPulseCurve.Evaluate(this.pulseColourTimeElapsed / this.unlockReadyColorPulseDuration);
			Color color = Color.Lerp(this.pulseColourA, this.pulseColourB, t);
			float groupAlpha = Mathf.Lerp(0.5f, 1f, t);
			this.SetSlotColour(color, groupAlpha, false);
			if (this.isSelected)
			{
				this.UpdateDisplay();
			}
		}
	}

	// Token: 0x06003ECB RID: 16075 RVA: 0x00114948 File Offset: 0x00112B48
	private void GetComponentsIfNeeded()
	{
		if (!this.manager)
		{
			this.manager = base.GetComponentInParent<InventoryItemToolManager>();
			if (this.manager)
			{
				InventoryItemToolManager inventoryItemToolManager = this.manager;
				inventoryItemToolManager.OnToolRefresh = (Action<bool>)Delegate.Combine(inventoryItemToolManager.OnToolRefresh, new Action<bool>(this.UpdateSlotDisplay));
			}
			else
			{
				Debug.LogWarningFormat(this, "Tool Slot \"{0}\" couldn't find parent manager!", new object[]
				{
					base.gameObject.name
				});
			}
		}
		if (!this.Crest)
		{
			this.Crest = base.GetComponentInParent<InventoryToolCrest>();
		}
		if (!this.crestList)
		{
			this.crestList = base.GetComponentInParent<InventoryToolCrestList>();
		}
	}

	// Token: 0x06003ECC RID: 16076 RVA: 0x001149F5 File Offset: 0x00112BF5
	public void SetCrestInfo(InventoryToolCrest crest, int slotIndex, Func<ToolCrestsData.SlotData> getSavedDataOverrideFunc = null, Action<ToolCrestsData.SlotData> setSavedDataOverrideAction = null)
	{
		this.Crest = crest;
		this.SlotIndex = slotIndex;
		this.getSavedDataOverride = getSavedDataOverrideFunc;
		this.setSavedDataOverride = setSavedDataOverrideAction;
	}

	// Token: 0x06003ECD RID: 16077 RVA: 0x00114A14 File Offset: 0x00112C14
	public void PreOpenSlot()
	{
		if (!this.EquippedItem)
		{
			this.PlayAnim(InventoryToolCrestSlot._equipAnim);
		}
		this.isPreOpened = true;
	}

	// Token: 0x06003ECE RID: 16078 RVA: 0x00114A38 File Offset: 0x00112C38
	public void SetEquipped(ToolItem toolItem, bool isManual, bool refreshTools)
	{
		this.GetComponentsIfNeeded();
		bool flag = this.EquippedItem != toolItem;
		this.EquippedItem = toolItem;
		if (flag)
		{
			base.ItemDataUpdated();
		}
		if (isManual)
		{
			if (this.OnSetEquipSaved != null)
			{
				this.OnSetEquipSaved();
			}
			if (this.slotAnimator)
			{
				if (toolItem)
				{
					if (!this.isPreOpened)
					{
						this.PlayAnim(InventoryToolCrestSlot._equipAnim);
					}
				}
				else
				{
					this.PlayAnim(InventoryToolCrestSlot._unequipAnim);
				}
				this.isPreOpened = false;
			}
			if (refreshTools)
			{
				this.manager.RefreshTools();
			}
		}
		else
		{
			this.PlaySlotStateAnims(this.IsLocked, this.manager.CanUnlockSlot, true);
		}
		this.RefreshIcon();
		this.UpdateDisplay();
	}

	// Token: 0x06003ECF RID: 16079 RVA: 0x00114AEC File Offset: 0x00112CEC
	public override bool Submit()
	{
		this.GetComponentsIfNeeded();
		if (!this.manager)
		{
			return false;
		}
		if (this.IsLocked)
		{
			if (this.EquippedItem)
			{
				this.manager.UnequipTool(this.EquippedItem, this);
				return true;
			}
			if (this.manager.CanUnlockSlot)
			{
				this.unlockHoldRoutine = base.StartCoroutine(this.UnlockHoldRoutine());
				this.UpdateSlotDisplay(false);
				return true;
			}
			return false;
		}
		else
		{
			if (!this.manager.CanChangeEquips(this.Type, InventoryItemToolManager.CanChangeEquipsTypes.Regular))
			{
				return false;
			}
			if (this.manager.EquipState == InventoryItemToolManager.EquipStates.PlaceTool)
			{
				this.DoPlace();
				return true;
			}
			return base.Submit();
		}
	}

	// Token: 0x06003ED0 RID: 16080 RVA: 0x00114B93 File Offset: 0x00112D93
	public override bool SubmitReleased()
	{
		return this.TryCancelUnlockHold() || base.SubmitReleased();
	}

	// Token: 0x06003ED1 RID: 16081 RVA: 0x00114BA8 File Offset: 0x00112DA8
	protected override bool DoPress()
	{
		InventoryItemToolManager.EquipStates equipState = this.manager.EquipState;
		if (equipState == InventoryItemToolManager.EquipStates.None)
		{
			if (this.EquippedItem)
			{
				this.manager.UnequipTool(this.EquippedItem, this);
			}
			else
			{
				this.manager.StartSelection(this);
			}
			return true;
		}
		if (equipState != InventoryItemToolManager.EquipStates.PlaceTool)
		{
			return false;
		}
		this.DoPlace();
		return true;
	}

	// Token: 0x06003ED2 RID: 16082 RVA: 0x00114C02 File Offset: 0x00112E02
	private void DoPlace()
	{
		if (this.manager.IsHoldingTool)
		{
			this.manager.PlaceTool(this, true);
		}
	}

	// Token: 0x06003ED3 RID: 16083 RVA: 0x00114C20 File Offset: 0x00112E20
	public override bool Cancel()
	{
		this.GetComponentsIfNeeded();
		if (!this.manager)
		{
			return base.Cancel();
		}
		if (this.manager.ShowingToolMsg)
		{
			this.manager.HideToolEquipMsg(false);
			return false;
		}
		if (this.manager.IsHoldingTool)
		{
			this.manager.PlayMoveSound();
			this.manager.PlaceTool(null, false);
			return true;
		}
		return base.Cancel();
	}

	// Token: 0x06003ED4 RID: 16084 RVA: 0x00114C90 File Offset: 0x00112E90
	public override InventoryItemSelectable GetNextSelectable(InventoryItemManager.SelectionDirection direction)
	{
		InventoryItemSelectable nextFallbackSelectable;
		InventoryToolCrestSlot inventoryToolCrestSlot;
		this.GetNextSelectableAndSlot(direction, out nextFallbackSelectable, out inventoryToolCrestSlot);
		if (!this.manager || this.manager.EquipState != InventoryItemToolManager.EquipStates.PlaceTool)
		{
			return nextFallbackSelectable;
		}
		if (!inventoryToolCrestSlot)
		{
			return this.GetSlotFromAutoNavGroup(direction, this.Type);
		}
		if (this.IsSlotInvalid(this.Type, inventoryToolCrestSlot))
		{
			inventoryToolCrestSlot = inventoryToolCrestSlot.GetNextSlotOfType(direction, this.Type);
		}
		if (inventoryToolCrestSlot == null)
		{
			nextFallbackSelectable = base.GetNextFallbackSelectable(direction);
			inventoryToolCrestSlot = (nextFallbackSelectable as InventoryToolCrestSlot);
			if (inventoryToolCrestSlot == null)
			{
				return this.GetSlotFromAutoNavGroup(direction, this.Type);
			}
			if (inventoryToolCrestSlot.Type != this.Type)
			{
				inventoryToolCrestSlot = inventoryToolCrestSlot.GetNextSlotOfType(direction, this.Type);
			}
		}
		if (!inventoryToolCrestSlot)
		{
			return this.GetSlotFromAutoNavGroup(direction, this.Type);
		}
		return inventoryToolCrestSlot;
	}

	// Token: 0x06003ED5 RID: 16085 RVA: 0x00114D60 File Offset: 0x00112F60
	private InventoryItemSelectable GetSlotFromAutoNavGroup(InventoryItemManager.SelectionDirection direction, ToolItemType type)
	{
		return base.GetSelectableFromAutoNavGroup<InventoryToolCrestSlot>(direction, (InventoryToolCrestSlot slot) => !this.IsSlotInvalid(type, slot));
	}

	// Token: 0x06003ED6 RID: 16086 RVA: 0x00114D94 File Offset: 0x00112F94
	private InventoryToolCrestSlot GetNextSlotOfType(InventoryItemManager.SelectionDirection direction, ToolItemType type)
	{
		InventoryItemSelectable inventoryItemSelectable;
		InventoryToolCrestSlot nextSlotOfType;
		this.GetNextSelectableAndSlot(direction, out inventoryItemSelectable, out nextSlotOfType);
		if (nextSlotOfType && this.IsSlotInvalid(type, nextSlotOfType))
		{
			nextSlotOfType = nextSlotOfType.GetNextSlotOfType(direction, type);
		}
		return nextSlotOfType;
	}

	// Token: 0x06003ED7 RID: 16087 RVA: 0x00114DC8 File Offset: 0x00112FC8
	private void GetNextSelectableAndSlot(InventoryItemManager.SelectionDirection direction, out InventoryItemSelectable nextSelectable, out InventoryToolCrestSlot nextSlot)
	{
		nextSelectable = base.GetNextSelectable(direction);
		nextSlot = (nextSelectable ? (nextSelectable.Get(new InventoryItemManager.SelectionDirection?(direction)) as InventoryToolCrestSlot) : null);
	}

	// Token: 0x06003ED8 RID: 16088 RVA: 0x00114DF3 File Offset: 0x00112FF3
	private bool IsSlotInvalid(ToolItemType type, InventoryToolCrestSlot nextSlot)
	{
		return nextSlot.Type != type || (nextSlot.IsLocked && !this.manager.CanUnlockSlot);
	}

	// Token: 0x06003ED9 RID: 16089 RVA: 0x00114E18 File Offset: 0x00113018
	private void UpdateSlotDisplay(bool isInstant)
	{
		int frameCount = Time.frameCount;
		if (this.lastUpdate == frameCount && this.lastEquipState == this.manager.EquipState && this.isSelected == this.lastSelectState)
		{
			return;
		}
		this.lastEquipState = this.manager.EquipState;
		this.lastSelectState = this.isSelected;
		this.lastUpdate = frameCount;
		Color toolTypeColor = this.manager.GetToolTypeColor(this.Type);
		float h;
		float num;
		float v;
		Color.RGBToHSV(toolTypeColor, out h, out num, out v);
		Color color = Color.HSVToRGB(h, num * 0.4f, v);
		Color color2 = Color.HSVToRGB(h, 0f, v);
		bool flag;
		bool flag2;
		bool flag3;
		bool fadeAlpha;
		if (this.Crest)
		{
			flag = (this.crestList.CurrentCrest == this.Crest);
			flag2 = this.IsLocked;
			flag3 = (flag2 && this.manager.CanUnlockSlot);
			fadeAlpha = this.crestList.IsSetupComplete;
		}
		else
		{
			flag = true;
			flag2 = false;
			flag3 = false;
			fadeAlpha = true;
		}
		this.PlaySlotStateAnims(flag2, flag3, false);
		if (this.wasSelected)
		{
			bool flag4 = this.manager.EquipState == InventoryItemToolManager.EquipStates.SwitchCrest;
		}
		bool flag5 = this.isSelected && (this.wasSelected || this.manager.EquipState != InventoryItemToolManager.EquipStates.SwitchCrest);
		this.wasSelected = flag5;
		float groupAlpha;
		Color color3;
		if (flag2)
		{
			if (flag3 && flag5)
			{
				groupAlpha = 1f;
			}
			else
			{
				InventoryItemToolManager.EquipStates equipState = this.manager.EquipState;
				groupAlpha = ((equipState == InventoryItemToolManager.EquipStates.PlaceTool || equipState == InventoryItemToolManager.EquipStates.SelectTool) ? 0.3f : 0.5f);
			}
			color3 = Color.white;
		}
		else if (this.manager.EquipState != InventoryItemToolManager.EquipStates.SwitchCrest && flag)
		{
			groupAlpha = 1f;
			color3 = Color.white;
			switch (this.manager.EquipState)
			{
			case InventoryItemToolManager.EquipStates.None:
				if (this.manager.HoveringTool && this.manager.HoveringTool.ToolType != this.Type)
				{
					groupAlpha = 0.3f;
				}
				break;
			case InventoryItemToolManager.EquipStates.PlaceTool:
				if (this.manager.PickedUpTool && this.manager.PickedUpTool.Type != this.Type)
				{
					groupAlpha = 0.3f;
					color3 = InventoryToolCrestSlot.InvalidItemColor;
				}
				break;
			case InventoryItemToolManager.EquipStates.SelectTool:
				if (this.manager.SelectedSlot && this.manager.SelectedSlot != this)
				{
					groupAlpha = 0.3f;
					color3 = InventoryToolCrestSlot.InvalidItemColor;
				}
				break;
			}
		}
		else
		{
			groupAlpha = 1f;
			color3 = Color.white;
		}
		if (this.unlockHoldRoutine != null)
		{
			this.isPulsingColour = false;
			this.SetSlotColour(toolTypeColor, 1f, fadeAlpha);
		}
		else if (flag3 && flag5)
		{
			if (!this.isPulsingColour)
			{
				this.isPulsingColour = true;
				this.pulseColourA = color;
				this.pulseColourB = toolTypeColor;
				this.pulseColourTimeElapsed = 0f;
				this.SetSlotColour(this.pulseColourA, 0.5f, fadeAlpha);
			}
		}
		else
		{
			this.isPulsingColour = false;
			this.SetSlotColour(flag2 ? color2 : toolTypeColor, groupAlpha, fadeAlpha);
		}
		if (this.amountText)
		{
			if (flag && this.EquippedItem && this.EquippedItem.DisplayAmountText)
			{
				ToolItemsData.Data toolData = PlayerData.instance.GetToolData(this.EquippedItem.name);
				this.amountText.text = toolData.AmountLeft.ToString();
				this.amountText.color = color3;
				this.amountText.gameObject.SetActive(true);
			}
			else
			{
				this.amountText.gameObject.SetActive(false);
			}
		}
		(this.slotTypeIconGroup ? this.slotTypeIconGroup.transform : base.transform).localScale = (flag2 ? new Vector3(0.8f, 0.8f, 1f) : Vector3.one);
		if (this.itemIcon)
		{
			this.itemIcon.color = base.UpdateGetIconColour(this.itemIcon, color3, !isInstant);
		}
		this.RefreshIcon();
		this.UpdateDisplay();
	}

	// Token: 0x06003EDA RID: 16090 RVA: 0x00115234 File Offset: 0x00113434
	protected override bool IsToolEquipped(ToolItem toolItem)
	{
		InventoryToolCrest crest = this.Crest;
		if (!crest)
		{
			return false;
		}
		if (!this.crestList.IsSwitchingCrests && crest == this.crestList.CurrentCrest)
		{
			return toolItem.IsEquippedHud;
		}
		return crest.GetEquippedToolSlot(toolItem);
	}

	// Token: 0x06003EDB RID: 16091 RVA: 0x00115288 File Offset: 0x00113488
	private void RefreshIcon()
	{
		Sprite itemSprite = this.ItemSprite;
		if (this.itemIcon)
		{
			this.itemIcon.sprite = itemSprite;
		}
		if (this.itemIconMask)
		{
			this.itemIconMask.sprite = itemSprite;
		}
	}

	// Token: 0x06003EDC RID: 16092 RVA: 0x001152D0 File Offset: 0x001134D0
	private void PlaySlotStateAnims(bool isLocked, bool isUnlockReady, bool force)
	{
		if (!isLocked)
		{
			if (this.EquippedItem)
			{
				if (force || this.previousAnimId != InventoryToolCrestSlot._equipAnim)
				{
					this.PlayAnim(InventoryToolCrestSlot._fullAnim);
				}
			}
			else if (force || this.previousAnimId != InventoryToolCrestSlot._unequipAnim)
			{
				this.PlayAnim(InventoryToolCrestSlot._emptyAnim);
			}
			this.PlayAnimSmall(InventoryToolCrestSlot._filledAnim);
			return;
		}
		if (isUnlockReady)
		{
			this.PlayAnim((this.isSelected && this.manager.EquipState != InventoryItemToolManager.EquipStates.SwitchCrest) ? InventoryToolCrestSlot._unlockReadySelectedAnim : InventoryToolCrestSlot._unlockReadyIdleAnim);
			this.PlayAnimSmall(InventoryToolCrestSlot._lockedAnim);
			return;
		}
		this.PlayAnim(InventoryToolCrestSlot._lockedAnim);
		this.PlayAnimSmall(InventoryToolCrestSlot._lockedAnim);
	}

	// Token: 0x06003EDD RID: 16093 RVA: 0x0011537E File Offset: 0x0011357E
	private void PlayAnim(int animId)
	{
		if (this.slotAnimator && this.slotAnimator.isActiveAndEnabled)
		{
			this.slotAnimator.Play(animId);
		}
		else
		{
			this.queuedAnimId = animId;
		}
		this.previousAnimId = animId;
	}

	// Token: 0x06003EDE RID: 16094 RVA: 0x001153B6 File Offset: 0x001135B6
	private void PlayAnimSmall(int animId)
	{
		if (this.slotFilledAnimator && this.slotFilledAnimator.isActiveAndEnabled)
		{
			this.slotFilledAnimator.Play(animId);
			return;
		}
		this.queuedSmallAnimId = animId;
	}

	// Token: 0x06003EDF RID: 16095 RVA: 0x001153E8 File Offset: 0x001135E8
	private void SetSlotColour(Color color, float groupAlpha, bool fadeAlpha)
	{
		if (this.slotTypeIcon)
		{
			this.slotTypeIcon.Color = color;
		}
		if (this.slotTypeIconFilled)
		{
			this.slotTypeIconFilled.BaseColor = color;
		}
		if (this.slotTypeGroup)
		{
			if (fadeAlpha)
			{
				this.slotTypeGroup.FadeTo(groupAlpha, 0.1f, null, true, null);
				return;
			}
			this.slotTypeGroup.AlphaSelf = groupAlpha;
		}
	}

	// Token: 0x06003EE0 RID: 16096 RVA: 0x00115459 File Offset: 0x00113659
	public override void Select(InventoryItemManager.SelectionDirection? direction)
	{
		if (!this.isSelected)
		{
			this.isSelected = true;
			this.UpdateSlotDisplay(false);
		}
		base.Select(direction);
	}

	// Token: 0x06003EE1 RID: 16097 RVA: 0x00115478 File Offset: 0x00113678
	public override void Deselect()
	{
		if (this.isSelected)
		{
			this.isSelected = false;
			this.UpdateSlotDisplay(false);
		}
		base.Deselect();
	}

	// Token: 0x06003EE2 RID: 16098 RVA: 0x00115496 File Offset: 0x00113696
	private IEnumerator UnlockHoldRoutine()
	{
		this.unlockHoldShakeTransform = (this.slotTypeGroup ? this.slotTypeGroup.transform : base.transform);
		this.unlockHoldInitialPosition = this.unlockHoldShakeTransform.localPosition;
		this.onUnlockHoldEnd = delegate()
		{
			this.crestList.IsBlocked = false;
			this.unlockHoldShakeTransform.localPosition = this.unlockHoldInitialPosition;
			if (this.unlockHoldParticles)
			{
				this.unlockHoldParticles.StopParticleSystems();
			}
		};
		this.crestList.IsBlocked = true;
		if (this.unlockHoldParticles)
		{
			this.unlockHoldParticles.PlayParticleSystems();
		}
		InventoryItemCollectable unlockItem = this.manager.SlotUnlockItemDisplay;
		CrestSocketUnlockInventoryDescription unlockDesc = this.manager.SocketUnlockInventoryDescription;
		unlockDesc.StartConsume();
		WaitForSecondsRealtime wait = new WaitForSecondsRealtime(0.016666668f);
		double beforeWaitTime;
		for (float elapsed = 0f; elapsed < this.unlockHoldDuration; elapsed += (float)(Time.unscaledTimeAsDouble - beforeWaitTime))
		{
			float num = elapsed / this.unlockHoldDuration;
			this.unlockHoldShakeTransform.localPosition = this.unlockHoldInitialPosition + Random.insideUnitCircle * this.unlockHoldShakeMagnitude.GetLerpedValue(num);
			unlockItem.SetConsumeShakeAmount(num, 1f);
			unlockDesc.SetConsumeShakeAmount(num);
			this.UpdateUnlockRumble(num);
			beforeWaitTime = Time.unscaledTimeAsDouble;
			yield return wait;
		}
		unlockItem.SetConsumeShakeAmount(0f, 1f);
		unlockDesc.SetConsumeShakeAmount(0f);
		unlockDesc.ConsumeCompleted();
		this.onUnlockHoldEnd();
		this.onUnlockHoldEnd = null;
		this.unlockHoldRoutine = null;
		ToolCrestsData.SlotData saveData = this.SaveData;
		saveData.IsUnlocked = true;
		this.SaveData = saveData;
		this.manager.SlotUnlockItem.Take(1, false);
		if (this.spawnedUnlockBurstEffect)
		{
			this.spawnedUnlockBurstEffect.gameObject.SetActive(false);
			this.spawnedUnlockBurstEffect.gameObject.SetActive(true);
		}
		unlockItem.PlayConsumeEffect();
		this.StopUnlockRumble();
		this.PlayFinalShake();
		this.UpdateSlotDisplay(false);
		this.manager.SetDisplay(this);
		this.manager.RefreshTools();
		yield break;
	}

	// Token: 0x06003EE3 RID: 16099 RVA: 0x001154A8 File Offset: 0x001136A8
	private bool TryCancelUnlockHold()
	{
		if (this.unlockHoldRoutine == null)
		{
			return false;
		}
		base.StopCoroutine(this.unlockHoldRoutine);
		this.unlockHoldRoutine = null;
		InventoryItemCollectable slotUnlockItemDisplay = this.manager.SlotUnlockItemDisplay;
		slotUnlockItemDisplay.SetConsumeShakeAmount(0f, 1f);
		slotUnlockItemDisplay.StopConsumeRumble();
		this.UpdateSlotDisplay(false);
		this.onUnlockHoldEnd();
		this.onUnlockHoldEnd = null;
		this.manager.SocketUnlockInventoryDescription.CancelConsume();
		this.StopUnlockRumble();
		return true;
	}

	// Token: 0x06003EE4 RID: 16100 RVA: 0x00115524 File Offset: 0x00113724
	private void UpdateUnlockRumble(float strength)
	{
		if (this.consumeRumbleEmission == null)
		{
			this.consumeRumbleEmission = VibrationManager.PlayVibrationClipOneShot(this.unlockRumble, null, true, "", true);
		}
		VibrationEmission vibrationEmission = this.consumeRumbleEmission;
		if (vibrationEmission == null)
		{
			return;
		}
		vibrationEmission.SetStrength(strength);
	}

	// Token: 0x06003EE5 RID: 16101 RVA: 0x00115570 File Offset: 0x00113770
	public void StopUnlockRumble()
	{
		VibrationEmission vibrationEmission = this.consumeRumbleEmission;
		if (vibrationEmission != null)
		{
			vibrationEmission.Stop();
		}
		this.consumeRumbleEmission = null;
	}

	// Token: 0x06003EE6 RID: 16102 RVA: 0x0011558C File Offset: 0x0011378C
	private void PlayFinalShake()
	{
		VibrationManager.PlayVibrationClipOneShot(this.unlockShake, null, false, "", true);
	}

	// Token: 0x04004040 RID: 16448
	private const float DISABLED_SLOT_OPACITY = 0.5f;

	// Token: 0x04004041 RID: 16449
	private const float WRONG_SLOT_OPACITY = 0.3f;

	// Token: 0x04004042 RID: 16450
	private const float SLOT_FADE_DURATION = 0.1f;

	// Token: 0x04004043 RID: 16451
	private const float LOCKED_SLOT_SCALE = 0.8f;

	// Token: 0x04004044 RID: 16452
	public static readonly Color InvalidItemColor = new Color(0.3f, 0.3f, 0.3f, 1f);

	// Token: 0x04004046 RID: 16454
	[Header("Tool Crest Slot")]
	[SerializeField]
	private Sprite slotTypeSprite;

	// Token: 0x04004047 RID: 16455
	[SerializeField]
	private NestedFadeGroupBase slotTypeGroup;

	// Token: 0x04004048 RID: 16456
	[SerializeField]
	private NestedFadeGroupBase slotTypeIconGroup;

	// Token: 0x04004049 RID: 16457
	[SerializeField]
	private NestedFadeGroupSpriteRenderer slotTypeIcon;

	// Token: 0x0400404A RID: 16458
	[SerializeField]
	private NestedFadeGroupSpriteRenderer slotTypeIconFilled;

	// Token: 0x0400404B RID: 16459
	[SerializeField]
	private Animator slotFilledAnimator;

	// Token: 0x0400404C RID: 16460
	[SerializeField]
	private SpriteRenderer itemIcon;

	// Token: 0x0400404D RID: 16461
	[SerializeField]
	private SpriteMask itemIconMask;

	// Token: 0x0400404E RID: 16462
	[SerializeField]
	private Animator slotAnimator;

	// Token: 0x0400404F RID: 16463
	[SerializeField]
	[ArrayForEnum(typeof(AttackToolBinding))]
	private RuntimeAnimatorController[] attackAnimatorControllers;

	// Token: 0x04004050 RID: 16464
	[SerializeField]
	private TextMeshPro amountText;

	// Token: 0x04004051 RID: 16465
	[Space]
	[SerializeField]
	private AnimationCurve unlockReadyColourPulseCurve;

	// Token: 0x04004052 RID: 16466
	[SerializeField]
	private float unlockReadyColorPulseDuration;

	// Token: 0x04004053 RID: 16467
	[SerializeField]
	private float unlockHoldDuration;

	// Token: 0x04004054 RID: 16468
	[SerializeField]
	private MinMaxFloat unlockHoldShakeMagnitude;

	// Token: 0x04004055 RID: 16469
	[SerializeField]
	private PlayParticleEffects unlockHoldParticles;

	// Token: 0x04004056 RID: 16470
	[SerializeField]
	private PassColour unlockBurstEffectPrefab;

	// Token: 0x04004057 RID: 16471
	[Header("Vibrations")]
	[SerializeField]
	private VibrationDataAsset unlockRumble;

	// Token: 0x04004058 RID: 16472
	[SerializeField]
	private VibrationDataAsset unlockShake;

	// Token: 0x04004059 RID: 16473
	private bool isPreOpened;

	// Token: 0x0400405A RID: 16474
	private bool wasVisible;

	// Token: 0x0400405B RID: 16475
	[NonSerialized]
	private InventoryItemToolManager manager;

	// Token: 0x0400405C RID: 16476
	private InventoryToolCrestList crestList;

	// Token: 0x0400405D RID: 16477
	private int previousAnimId;

	// Token: 0x0400405E RID: 16478
	private bool isPulsingColour;

	// Token: 0x0400405F RID: 16479
	private Color pulseColourA;

	// Token: 0x04004060 RID: 16480
	private Color pulseColourB;

	// Token: 0x04004061 RID: 16481
	private float pulseColourTimeElapsed;

	// Token: 0x04004062 RID: 16482
	private Transform unlockHoldShakeTransform;

	// Token: 0x04004063 RID: 16483
	private Vector3 unlockHoldInitialPosition;

	// Token: 0x04004064 RID: 16484
	private Coroutine unlockHoldRoutine;

	// Token: 0x04004065 RID: 16485
	private Action onUnlockHoldEnd;

	// Token: 0x04004066 RID: 16486
	private Func<ToolCrestsData.SlotData> getSavedDataOverride;

	// Token: 0x04004067 RID: 16487
	private Action<ToolCrestsData.SlotData> setSavedDataOverride;

	// Token: 0x04004068 RID: 16488
	private bool isSelected;

	// Token: 0x04004069 RID: 16489
	private bool wasSelected;

	// Token: 0x0400406A RID: 16490
	private PassColour spawnedUnlockBurstEffect;

	// Token: 0x0400406B RID: 16491
	private VibrationEmission consumeRumbleEmission;

	// Token: 0x0400406C RID: 16492
	private int queuedAnimId;

	// Token: 0x0400406D RID: 16493
	private int queuedSmallAnimId;

	// Token: 0x0400406E RID: 16494
	private static readonly int _equipAnim = Animator.StringToHash("Equip");

	// Token: 0x0400406F RID: 16495
	private static readonly int _unequipAnim = Animator.StringToHash("Unequip");

	// Token: 0x04004070 RID: 16496
	private static readonly int _fullAnim = Animator.StringToHash("Full");

	// Token: 0x04004071 RID: 16497
	private static readonly int _emptyAnim = Animator.StringToHash("Empty");

	// Token: 0x04004072 RID: 16498
	private static readonly int _lockedAnim = Animator.StringToHash("Locked");

	// Token: 0x04004073 RID: 16499
	private static readonly int _unlockReadyIdleAnim = Animator.StringToHash("Unlock Ready Idle");

	// Token: 0x04004074 RID: 16500
	private static readonly int _unlockReadySelectedAnim = Animator.StringToHash("Unlock Ready Selected");

	// Token: 0x04004075 RID: 16501
	private static readonly int _filledAnim = Animator.StringToHash("Filled");

	// Token: 0x04004076 RID: 16502
	private static readonly int _flashAmountProp = Shader.PropertyToID("_FlashAmount");

	// Token: 0x04004077 RID: 16503
	private static readonly int _flashColorProp = Shader.PropertyToID("_FlashColor");

	// Token: 0x04004078 RID: 16504
	private MaterialPropertyBlock block;

	// Token: 0x0400407C RID: 16508
	private ToolCrest.SlotInfo slotInfo;

	// Token: 0x0400407D RID: 16509
	private ToolItem itemData;

	// Token: 0x0400407E RID: 16510
	private int lastUpdate;

	// Token: 0x0400407F RID: 16511
	private InventoryItemToolManager.EquipStates lastEquipState;

	// Token: 0x04004080 RID: 16512
	private bool lastSelectState;
}
