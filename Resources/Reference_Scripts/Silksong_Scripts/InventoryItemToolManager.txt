using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using GlobalEnums;
using GlobalSettings;
using TeamCherry.Localization;
using TeamCherry.NestedFadeGroup;
using TMProOld;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x020006AD RID: 1709
public class InventoryItemToolManager : InventoryItemListManager<InventoryItemTool, ToolItem>, IInventoryPaneAvailabilityProvider
{
	// Token: 0x17000700 RID: 1792
	// (get) Token: 0x06003D4D RID: 15693 RVA: 0x0010D5AB File Offset: 0x0010B7AB
	// (set) Token: 0x06003D4E RID: 15694 RVA: 0x0010D5B3 File Offset: 0x0010B7B3
	public InventoryItemTool HoveringTool { get; private set; }

	// Token: 0x17000701 RID: 1793
	// (get) Token: 0x06003D4F RID: 15695 RVA: 0x0010D5BC File Offset: 0x0010B7BC
	// (set) Token: 0x06003D50 RID: 15696 RVA: 0x0010D5C4 File Offset: 0x0010B7C4
	public InventoryItemToolManager.EquipStates EquipState
	{
		get
		{
			return this.equipState;
		}
		private set
		{
			this.equipState = value;
			this.UpdateButtonPrompts();
			this.paneList.CanSwitchPanes = (value != InventoryItemToolManager.EquipStates.SwitchCrest);
			this.paneList.InSubMenu = (value > InventoryItemToolManager.EquipStates.None);
		}
	}

	// Token: 0x17000702 RID: 1794
	// (get) Token: 0x06003D51 RID: 15697 RVA: 0x0010D5F4 File Offset: 0x0010B7F4
	// (set) Token: 0x06003D52 RID: 15698 RVA: 0x0010D5FC File Offset: 0x0010B7FC
	public bool ShowingToolMsg { get; private set; }

	// Token: 0x17000703 RID: 1795
	// (get) Token: 0x06003D53 RID: 15699 RVA: 0x0010D605 File Offset: 0x0010B805
	// (set) Token: 0x06003D54 RID: 15700 RVA: 0x0010D60D File Offset: 0x0010B80D
	public bool ShowingCrestMsg { get; private set; }

	// Token: 0x17000704 RID: 1796
	// (get) Token: 0x06003D55 RID: 15701 RVA: 0x0010D616 File Offset: 0x0010B816
	// (set) Token: 0x06003D56 RID: 15702 RVA: 0x0010D61E File Offset: 0x0010B81E
	public bool ShowingCursedMsg { get; private set; }

	// Token: 0x17000705 RID: 1797
	// (get) Token: 0x06003D57 RID: 15703 RVA: 0x0010D627 File Offset: 0x0010B827
	public bool IsHoldingTool
	{
		get
		{
			return this.PickedUpTool != null;
		}
	}

	// Token: 0x17000706 RID: 1798
	// (get) Token: 0x06003D58 RID: 15704 RVA: 0x0010D635 File Offset: 0x0010B835
	// (set) Token: 0x06003D59 RID: 15705 RVA: 0x0010D63D File Offset: 0x0010B83D
	public ToolItem PickedUpTool { get; private set; }

	// Token: 0x17000707 RID: 1799
	// (get) Token: 0x06003D5A RID: 15706 RVA: 0x0010D646 File Offset: 0x0010B846
	// (set) Token: 0x06003D5B RID: 15707 RVA: 0x0010D64E File Offset: 0x0010B84E
	public InventoryToolCrestSlot SelectedSlot { get; private set; }

	// Token: 0x17000708 RID: 1800
	// (get) Token: 0x06003D5C RID: 15708 RVA: 0x0010D657 File Offset: 0x0010B857
	public CollectableItem SlotUnlockItem
	{
		get
		{
			return this.slotUnlockItem;
		}
	}

	// Token: 0x17000709 RID: 1801
	// (get) Token: 0x06003D5D RID: 15709 RVA: 0x0010D65F File Offset: 0x0010B85F
	public bool CanUnlockSlot
	{
		get
		{
			return this.slotUnlockItem.CollectedAmount > 0;
		}
	}

	// Token: 0x1700070A RID: 1802
	// (get) Token: 0x06003D5E RID: 15710 RVA: 0x0010D66F File Offset: 0x0010B86F
	public InventoryItemCollectable SlotUnlockItemDisplay
	{
		get
		{
			return this.slotUnlockItemDisplay;
		}
	}

	// Token: 0x1700070B RID: 1803
	// (get) Token: 0x06003D5F RID: 15711 RVA: 0x0010D677 File Offset: 0x0010B877
	public CrestSocketUnlockInventoryDescription SocketUnlockInventoryDescription
	{
		get
		{
			return this.slotUnlockDescExtra;
		}
	}

	// Token: 0x06003D60 RID: 15712 RVA: 0x0010D67F File Offset: 0x0010B87F
	protected override void OnValidate()
	{
		base.OnValidate();
		ArrayForEnumAttribute.EnsureArraySize<NestedFadeGroupSpriteRenderer>(ref this.listSectionHeaders, typeof(ToolItemType));
		ArrayForEnumAttribute.EnsureArraySize<GameObject>(ref this.reloadCurrencyCounters, typeof(CurrencyType));
	}

	// Token: 0x06003D61 RID: 15713 RVA: 0x0010D6B4 File Offset: 0x0010B8B4
	protected override void Awake()
	{
		this.pane = base.GetComponent<InventoryPane>();
		base.Awake();
		this.OnValidate();
		if (this.toolAmountText)
		{
			this.initialToolAmountText = this.toolAmountText.text;
		}
		this.paneList = base.GetComponentInParent<InventoryPaneList>();
		if (this.pane)
		{
			this.pane.OnPaneEnd += delegate()
			{
				if (this.tweenTool)
				{
					this.tweenTool.Cancel();
				}
				this.HideEquipMsgsInstant();
				this.EndSwitchingCrest();
				this.EquipState = InventoryItemToolManager.EquipStates.None;
				if (this.crestGroup)
				{
					this.crestGroup.AlphaSelf = 0f;
				}
				if (this.toolGroup)
				{
					this.toolGroup.AlphaSelf = 1f;
				}
				this.PickedUpTool = null;
				this.selectedBeforePickup = null;
				this.SelectedSlot = null;
				base.GetSelectables(null).ForEach(delegate(InventoryItemTool tool)
				{
					tool.ItemData.HasBeenSeen = true;
				});
			};
			this.pane.OnPaneStart += this.UpdateTextDisplays;
		}
		this.UpdateTextDisplays();
		this.SetToolUsePrompt(null, false, 0f);
	}

	// Token: 0x06003D62 RID: 15714 RVA: 0x0010D75C File Offset: 0x0010B95C
	public override void InstantScroll()
	{
		if (!base.CurrentSelected)
		{
			ToolItem unlockedTool = ToolItemManager.UnlockedTool;
			InventoryItemSelectable startSelectable = this.GetStartSelectable();
			ToolItemManager.UnlockedTool = unlockedTool;
			if (startSelectable != null)
			{
				if (!startSelectable.transform.IsChildOf(base.ItemList.transform))
				{
					return;
				}
				base.ItemList.ScrollTo(startSelectable, true);
			}
			return;
		}
		if (!base.CurrentSelected.transform.IsChildOf(base.ItemList.transform))
		{
			return;
		}
		base.ItemList.ScrollTo(base.CurrentSelected, true);
	}

	// Token: 0x06003D63 RID: 15715 RVA: 0x0010D7E8 File Offset: 0x0010B9E8
	private void Start()
	{
		this.EquipState = InventoryItemToolManager.EquipStates.None;
		if (this.toolEquipMsg)
		{
			this.toolEquipMsg.gameObject.SetActive(true);
			this.toolEquipMsg.AlphaSelf = 0f;
		}
		if (this.crestEquipMsg)
		{
			this.crestEquipMsg.gameObject.SetActive(true);
			this.crestEquipMsg.AlphaSelf = 0f;
		}
		if (this.cursedEquipMsg)
		{
			this.cursedEquipMsg.gameObject.SetActive(true);
			this.cursedEquipMsg.AlphaSelf = 0f;
		}
	}

	// Token: 0x06003D64 RID: 15716 RVA: 0x0010D888 File Offset: 0x0010BA88
	public override void SetDisplay(GameObject selectedGameObject)
	{
		base.SetDisplay(selectedGameObject);
		if (this.displayIcon)
		{
			this.displayIcon.gameObject.SetActive(false);
		}
		this.HideEquipMsgs(true);
		this.SetToolUsePrompt(null, false, 0f);
		if (this.toolAmountText)
		{
			this.toolAmountText.gameObject.SetActive(false);
		}
		this.descriptionIconGroup.SetActive(true);
		this.slotUnlockDescExtra.gameObject.SetActive(false);
		this.showEquipPrompt = false;
		this.showReloadPrompt = false;
		this.showCustomTogglePrompt = false;
		this.UpdateButtonPrompts();
		this.reloadCurrencyCounters.SetAllActive(false);
		this.currencyParent.gameObject.SetActive(false);
	}

	// Token: 0x06003D65 RID: 15717 RVA: 0x0010D94C File Offset: 0x0010BB4C
	public override void SetDisplay(InventoryItemSelectable selectable)
	{
		base.SetDisplay(selectable);
		ToolItem toolItem = null;
		bool flag = true;
		InventoryItemTool inventoryItemTool = selectable as InventoryItemTool;
		if (inventoryItemTool != null)
		{
			toolItem = inventoryItemTool.ItemData;
			flag = this.CrestHasSlot(toolItem.Type);
		}
		InventoryItemToolBase inventoryItemToolBase = selectable as InventoryItemToolBase;
		InventoryToolCrestSlot inventoryToolCrestSlot = selectable as InventoryToolCrestSlot;
		Sprite sprite;
		Color color;
		if (inventoryItemToolBase != null)
		{
			sprite = inventoryItemToolBase.Sprite;
			color = inventoryItemToolBase.SpriteTint;
			if (inventoryToolCrestSlot == null || !inventoryToolCrestSlot.IsLocked)
			{
				if (this.displayIcon)
				{
					this.displayIcon.gameObject.SetActive(true);
					this.displayIcon.sprite = sprite;
					this.displayIcon.color = color;
				}
				this.showEquipPrompt = true;
			}
		}
		else
		{
			sprite = null;
			color = Color.white;
		}
		if (inventoryToolCrestSlot != null)
		{
			if (inventoryToolCrestSlot.IsLocked)
			{
				if (this.CanUnlockSlot)
				{
					this.slotUnlockDescExtra.SetSlotSprite(sprite, color);
					this.slotUnlockDescExtra.gameObject.SetActive(true);
					this.slotUnlockItemDisplay.Item = this.slotUnlockItem;
				}
			}
			else
			{
				if (inventoryToolCrestSlot.EquippedItem)
				{
					toolItem = inventoryToolCrestSlot.EquippedItem;
				}
				flag = this.ToolListHasType(inventoryToolCrestSlot.Type);
			}
		}
		bool flag2 = this.CanChangeEquips();
		bool isHeroCursed = this.IsHeroCursed;
		if (toolItem)
		{
			if (toolItem.IsUnlockedNotHidden)
			{
				if (this.toolAmountText && toolItem.DisplayAmountText)
				{
					ToolItemsData.Data toolData = PlayerData.instance.GetToolData(toolItem.name);
					int toolStorageAmount = ToolItemManager.GetToolStorageAmount(toolItem);
					this.toolAmountText.text = string.Format(this.initialToolAmountText, toolData.AmountLeft, toolStorageAmount);
					this.toolAmountText.gameObject.SetActive(true);
				}
				if (toolItem.DisplayTogglePrompt)
				{
					this.showCustomTogglePrompt = true;
					if (this.customTogglePromptText)
					{
						this.customTogglePromptText.text = toolItem.CustomToggleText;
					}
				}
				if (toolItem.ReplenishUsage == ToolItem.ReplenishUsages.OneForOne)
				{
					this.showReloadPrompt = true;
					if (this.reloadPrompt)
					{
						this.reloadPrompt.AlphaSelf = ((toolItem.CanReload() && flag2) ? 1f : this.disabledListSectionOpacity);
					}
					if (toolItem.ReplenishResource != ToolItem.ReplenishResources.None)
					{
						this.currencyParent.gameObject.SetActive(true);
						GameObject gameObject = this.reloadCurrencyCounters[(int)toolItem.ReplenishResource];
						if (gameObject)
						{
							gameObject.SetActive(true);
						}
					}
				}
				if ((!this.showReloadPrompt || !this.showCustomTogglePrompt) && !toolItem.HideUsePrompt)
				{
					this.SetToolUsePrompt(ToolItemManager.GetAttackToolBinding(toolItem), toolItem.ShowPromptHold, toolItem.ExtraDescriptionSection ? this.buttonPromptExtraDescOffset : 0f);
				}
				if (this.customTogglePrompt)
				{
					if (this.showCustomTogglePrompt)
					{
						this.customTogglePrompt.AlphaSelf = ((flag2 && !isHeroCursed) ? 1f : this.disabledListSectionOpacity);
					}
					else
					{
						this.customTogglePrompt.AlphaSelf = 1f;
					}
				}
				if (toolItem.HasCustomAction)
				{
					this.comboButtonPromptDisplay.Show(toolItem.CustomButtonCombo);
				}
			}
			else
			{
				this.showEquipPrompt = false;
			}
		}
		if (inventoryItemTool != null || inventoryToolCrestSlot != null)
		{
			if (this.equipPrompt)
			{
				this.equipPrompt.AlphaSelf = ((flag && flag2 && !isHeroCursed) ? 1f : this.disabledListSectionOpacity);
			}
			if (this.equipPromptText)
			{
				ToolItemType toolItemType = (inventoryItemTool != null) ? inventoryItemTool.ToolType : inventoryToolCrestSlot.Type;
				if (toolItem != null && toolItem.IsEquipped)
				{
					this.equipPromptText.text = ((toolItemType == ToolItemType.Skill) ? this.unequipSkillText : this.unequipText);
				}
				else
				{
					this.equipPromptText.text = ((toolItemType == ToolItemType.Skill) ? this.equipSkillText : this.equipText);
				}
			}
		}
		else if (selectable is InventoryItemSelectableButtonEvent)
		{
			if (this.equipPrompt)
			{
				this.equipPrompt.AlphaSelf = 1f;
			}
			if (this.equipPromptText)
			{
				this.equipPromptText.text = (flag2 ? this.changeCrestText : this.viewCrestsText);
			}
		}
		this.UpdateButtonPrompts();
	}

	// Token: 0x1700070C RID: 1804
	// (get) Token: 0x06003D66 RID: 15718 RVA: 0x0010DD87 File Offset: 0x0010BF87
	public bool IsHeroCursed
	{
		get
		{
			return Gameplay.CursedCrest.IsEquipped;
		}
	}

	// Token: 0x06003D67 RID: 15719 RVA: 0x0010DD94 File Offset: 0x0010BF94
	public bool TryPickupOrPlaceTool(ToolItem tool)
	{
		this.PickedUpTool = tool;
		if (!tool)
		{
			return false;
		}
		IEnumerable<InventoryToolCrestSlot> enumerable = null;
		IEnumerable<InventoryToolCrestSlot> enumerable2 = null;
		IEnumerable<InventoryToolCrestSlot> enumerable3 = null;
		if (this.crestList)
		{
			enumerable2 = this.crestList.GetSlots();
			if (InventoryItemToolManager.GetAvailableSlotCount(enumerable2, new ToolItemType?(tool.Type), true) > 0)
			{
				enumerable = enumerable2;
			}
		}
		if (enumerable == null && this.extraSlots)
		{
			enumerable3 = this.extraSlots.GetSlots();
			if (InventoryItemToolManager.GetAvailableSlotCount(enumerable3, new ToolItemType?(tool.Type), true) > 0)
			{
				enumerable = enumerable3;
			}
		}
		if (enumerable == null)
		{
			if (InventoryItemToolManager.GetAvailableSlotCount(enumerable2, new ToolItemType?(tool.Type), false) > 0)
			{
				enumerable = enumerable2;
			}
			else if (InventoryItemToolManager.GetAvailableSlotCount(enumerable3, new ToolItemType?(tool.Type), false) > 0)
			{
				enumerable = enumerable3;
			}
		}
		if (enumerable != null)
		{
			InventoryToolCrestSlot availableSlot = this.GetAvailableSlot(enumerable, tool.Type);
			if (availableSlot)
			{
				this.EquipState = InventoryItemToolManager.EquipStates.PlaceTool;
				this.selectedBeforePickup = base.CurrentSelected;
				if (availableSlot.Type.IsAttackType())
				{
					if (InventoryItemToolManager.GetAvailableSlotCount(enumerable, new ToolItemType?(availableSlot.Type), false) == 1)
					{
						this.PlaceTool(availableSlot, true);
					}
					else
					{
						base.PlayMoveSound();
						base.SetSelected(availableSlot, null, false);
					}
				}
				else if (InventoryItemToolManager.GetAvailableSlotCount(enumerable, new ToolItemType?(availableSlot.Type), true) > 0)
				{
					this.PlaceTool(availableSlot, true);
				}
				else
				{
					int availableSlotCount = InventoryItemToolManager.GetAvailableSlotCount(enumerable2, new ToolItemType?(availableSlot.Type), false);
					int availableSlotCount2 = InventoryItemToolManager.GetAvailableSlotCount(enumerable3, new ToolItemType?(availableSlot.Type), false);
					if (availableSlotCount + availableSlotCount2 == 1)
					{
						this.PlaceTool(availableSlot, true);
					}
					else
					{
						base.PlayMoveSound();
						base.SetSelected(availableSlot, null, false);
					}
				}
				this.RefreshTools();
				return true;
			}
		}
		this.PickedUpTool = null;
		return false;
	}

	// Token: 0x06003D68 RID: 15720 RVA: 0x0010DF48 File Offset: 0x0010C148
	public void PlaceTool(InventoryToolCrestSlot slot, bool isManual)
	{
		if (slot && this.PickedUpTool.Type != slot.Type)
		{
			return;
		}
		ToolItem pickedUpTool = this.PickedUpTool;
		this.PickedUpTool = null;
		this.EquipState = InventoryItemToolManager.EquipStates.None;
		if (isManual)
		{
			slot.SetEquipped(pickedUpTool, true, true);
		}
		if (!this.selectedBeforePickup)
		{
			return;
		}
		if (isManual)
		{
			slot.PreOpenSlot();
		}
		if (this.tweenTool && slot)
		{
			this.tweenTool.DoPlace(this.selectedBeforePickup.transform.position, slot.transform.position, pickedUpTool, new Action(this.<PlaceTool>g__Selected|121_0));
			return;
		}
		this.<PlaceTool>g__Selected|121_0();
	}

	// Token: 0x06003D69 RID: 15721 RVA: 0x0010E004 File Offset: 0x0010C204
	public InventoryToolCrestSlot GetAvailableSlot(IEnumerable<InventoryToolCrestSlot> slots, ToolItemType toolType)
	{
		InventoryToolCrestSlot inventoryToolCrestSlot = null;
		foreach (InventoryToolCrestSlot inventoryToolCrestSlot2 in slots)
		{
			if (!inventoryToolCrestSlot2.IsLocked && inventoryToolCrestSlot2.Type == toolType)
			{
				if (!inventoryToolCrestSlot)
				{
					inventoryToolCrestSlot = inventoryToolCrestSlot2;
				}
				if (!inventoryToolCrestSlot2.EquippedItem)
				{
					return inventoryToolCrestSlot2;
				}
			}
		}
		return inventoryToolCrestSlot;
	}

	// Token: 0x06003D6A RID: 15722 RVA: 0x0010E078 File Offset: 0x0010C278
	private static int GetAvailableSlotCount(IEnumerable<InventoryToolCrestSlot> slots, ToolItemType? toolType, bool checkEmpty)
	{
		return slots.Count(delegate(InventoryToolCrestSlot slot)
		{
			if (!slot.IsLocked)
			{
				if (toolType != null)
				{
					ToolItemType type = slot.Type;
					ToolItemType? toolType2 = toolType;
					if (!(type == toolType2.GetValueOrDefault() & toolType2 != null))
					{
						return false;
					}
				}
				return !checkEmpty || slot.EquippedItem == null;
			}
			return false;
		});
	}

	// Token: 0x06003D6B RID: 15723 RVA: 0x0010E0AB File Offset: 0x0010C2AB
	public static bool IsToolEquipped(ToolItem toolItem)
	{
		return ToolItemManager.IsToolEquipped(toolItem, ToolEquippedReadSource.Hud);
	}

	// Token: 0x06003D6C RID: 15724 RVA: 0x0010E0B4 File Offset: 0x0010C2B4
	public bool CrestHasSlot(ToolItemType type)
	{
		return (this.crestList && this.crestList.CrestHasSlot(type)) || (this.extraSlots && InventoryItemToolManager.GetAvailableSlotCount(this.extraSlots.GetSlots(), new ToolItemType?(type), false) > 0);
	}

	// Token: 0x06003D6D RID: 15725 RVA: 0x0010E108 File Offset: 0x0010C308
	public bool CrestHasAnySlots()
	{
		return (this.crestList && this.crestList.CrestHasAnySlots()) || (!this.IsHeroCursed && (this.extraSlots && InventoryItemToolManager.GetAvailableSlotCount(this.extraSlots.GetSlots(), null, false) > 0));
	}

	// Token: 0x06003D6E RID: 15726 RVA: 0x0010E168 File Offset: 0x0010C368
	public bool ToolListHasType(ToolItemType type)
	{
		if (this.toolList)
		{
			using (List<InventoryItemTool>.Enumerator enumerator = this.toolList.GetListItems<InventoryItemTool>(null).GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.ToolType == type)
					{
						return true;
					}
				}
			}
			return false;
		}
		return false;
	}

	// Token: 0x06003D6F RID: 15727 RVA: 0x0010E1D8 File Offset: 0x0010C3D8
	public void UnequipTool(ToolItem toolItem, InventoryToolCrestSlot slot)
	{
		if (!toolItem)
		{
			return;
		}
		ToolItemManager.UnequipTool(toolItem);
		if (!slot && this.crestList)
		{
			slot = this.crestList.GetEquippedToolSlot(toolItem);
		}
		if (!slot && this.extraSlots)
		{
			slot = this.extraSlots.GetEquippedToolSlot(toolItem);
		}
		Vector3? vector = null;
		Vector3? vector2 = null;
		if (slot)
		{
			vector = new Vector3?(slot.transform.position);
			slot.SetEquipped(null, true, false);
		}
		if (this.toolList)
		{
			InventoryItemTool inventoryItemTool = this.toolList.GetListItems<InventoryItemTool>((InventoryItemTool t) => t.ItemData == toolItem).FirstOrDefault<InventoryItemTool>();
			if (inventoryItemTool != null)
			{
				this.toolList.ScrollTo(inventoryItemTool, true);
				vector2 = new Vector3?(inventoryItemTool.transform.position);
			}
		}
		if (vector != null && vector2 != null && this.tweenTool)
		{
			this.tweenTool.DoReturn(vector.Value, vector2.Value, toolItem, new Action(this.RefreshTools));
			return;
		}
		this.RefreshTools();
	}

	// Token: 0x06003D70 RID: 15728 RVA: 0x0010E33B File Offset: 0x0010C53B
	public void RefreshTools()
	{
		this.RefreshTools(false, true);
	}

	// Token: 0x06003D71 RID: 15729 RVA: 0x0010E348 File Offset: 0x0010C548
	public void RefreshTools(bool isInstant, bool updateCrest)
	{
		for (int i = 0; i < this.listSectionHeaders.Length; i++)
		{
			Color color = this.listSectionHeaders[i].Color;
			if (this.SelectedSlot && i != (int)this.SelectedSlot.Type)
			{
				color.a = this.disabledListSectionOpacity;
			}
			else
			{
				color.a = 1f;
			}
			this.listSectionHeaders[i].Color = color;
		}
		bool isHidden = this.crestList.CurrentCrest.IsHidden;
		if (this.crestButtonLockedDisplay)
		{
			this.crestButtonLockedDisplay.SetActive(isHidden);
		}
		if (this.crestButtonNormalDisplay)
		{
			this.crestButtonNormalDisplay.SetActive(!isHidden);
		}
		if (updateCrest)
		{
			InventoryToolCrest currentCrest = this.crestList.CurrentCrest;
			if (currentCrest)
			{
				currentCrest.UpdateListDisplay(isInstant);
			}
			InventoryFloatingToolSlots inventoryFloatingToolSlots = this.extraSlots;
			InventoryItemToolManager.EquipStates equipStates = this.EquipState;
			inventoryFloatingToolSlots.SetInEquipMode(equipStates == InventoryItemToolManager.EquipStates.PlaceTool || equipStates == InventoryItemToolManager.EquipStates.SelectTool);
		}
		Action<bool> onToolRefresh = this.OnToolRefresh;
		if (onToolRefresh != null)
		{
			onToolRefresh(isInstant);
		}
		if (this.refreshCurrentSelected)
		{
			if (base.CurrentSelected == null || !base.CurrentSelected.gameObject.activeInHierarchy)
			{
				base.SetSelected(InventoryItemManager.SelectedActionType.LeftMost, true);
			}
			this.refreshCurrentSelected = false;
		}
	}

	// Token: 0x06003D72 RID: 15730 RVA: 0x0010E48E File Offset: 0x0010C68E
	public void OnAppliedCrest()
	{
		this.refreshCurrentSelected = true;
	}

	// Token: 0x06003D73 RID: 15731 RVA: 0x0010E498 File Offset: 0x0010C698
	public void StartSelection(InventoryToolCrestSlot slot)
	{
		if (this.toolList == null)
		{
			return;
		}
		List<InventoryItemTool> listItems = this.toolList.GetListItems<InventoryItemTool>((InventoryItemTool toolItem) => toolItem.ToolType == slot.Type);
		List<InventoryItemTool> list = (from toolItem in listItems
		where !InventoryItemToolManager.IsToolEquipped(toolItem.ItemData)
		select toolItem).ToList<InventoryItemTool>();
		InventoryItemTool inventoryItemTool = null;
		if (list.Count > 0)
		{
			inventoryItemTool = list[0];
		}
		else if (listItems.Count > 0)
		{
			inventoryItemTool = listItems[0];
		}
		if (inventoryItemTool == null)
		{
			return;
		}
		this.SelectedSlot = slot;
		this.EquipState = InventoryItemToolManager.EquipStates.SelectTool;
		base.PlayMoveSound();
		base.SetSelected(inventoryItemTool, null, false);
		this.RefreshTools();
	}

	// Token: 0x06003D74 RID: 15732 RVA: 0x0010E568 File Offset: 0x0010C768
	public void EndSelection(InventoryItemTool tool)
	{
		if (!this.SelectedSlot)
		{
			return;
		}
		if (tool && tool.ItemData && this.SelectedSlot.Type == tool.ToolType)
		{
			if (this.tweenTool)
			{
				this.SelectedSlot.SetEquipped(tool.ItemData, true, true);
				this.tweenTool.DoPlace(tool.transform.position, this.SelectedSlot.transform.position, tool.ItemData, new Action(this.<EndSelection>g__SelectionEnd|134_0));
				return;
			}
			this.SelectedSlot.SetEquipped(tool.ItemData, true, true);
		}
		this.<EndSelection>g__SelectionEnd|134_0();
	}

	// Token: 0x06003D75 RID: 15733 RVA: 0x0010E62D File Offset: 0x0010C82D
	public bool BeginSwitchingCrest()
	{
		if (this.EquipState != InventoryItemToolManager.EquipStates.None)
		{
			return false;
		}
		this.EquipState = InventoryItemToolManager.EquipStates.SwitchCrest;
		this.HideEquipMsgs(true);
		this.RefreshTools();
		return true;
	}

	// Token: 0x06003D76 RID: 15734 RVA: 0x0010E64E File Offset: 0x0010C84E
	public void PaneMovePrevented()
	{
		if (this.equipState != InventoryItemToolManager.EquipStates.SwitchCrest)
		{
			return;
		}
		this.crestList.PaneMovePrevented();
	}

	// Token: 0x06003D77 RID: 15735 RVA: 0x0010E665 File Offset: 0x0010C865
	public bool EndSwitchingCrest()
	{
		if (this.EquipState != InventoryItemToolManager.EquipStates.SwitchCrest)
		{
			return false;
		}
		this.EquipState = InventoryItemToolManager.EquipStates.None;
		this.HideCrestEquipMsg(true);
		return true;
	}

	// Token: 0x06003D78 RID: 15736 RVA: 0x0010E684 File Offset: 0x0010C884
	public float FadeToolGroup(bool fadeIn)
	{
		if (!this.toolGroup)
		{
			return 0f;
		}
		float num = this.toolGroup.FadeTo((float)(fadeIn ? 1 : 0), this.groupFadeTime, null, true, null);
		if (this.groupFadeRoutine != null)
		{
			base.StopCoroutine(this.groupFadeRoutine);
		}
		if (fadeIn)
		{
			this.groupFadeRoutine = this.StartTimerRoutine(0f, num, null, null, delegate
			{
				if (this.cursor)
				{
					this.cursor.Activate();
				}
				if (!this.pane.IsPaneActive)
				{
					return;
				}
				InventoryItemManager.SelectedActionType select = InventoryItemManager.SelectedActionType.Previous;
				InventoryToolCrestSlot inventoryToolCrestSlot = base.CurrentSelected as InventoryToolCrestSlot;
				if (inventoryToolCrestSlot != null && !this.crestList.CurrentCrest.HasSlot(inventoryToolCrestSlot))
				{
					select = InventoryItemManager.SelectedActionType.LeftMost;
				}
				base.SetProxyActive(true, select);
			}, true);
		}
		else
		{
			if (this.cursor)
			{
				this.cursor.Deactivate();
			}
			if (this.pane.IsPaneActive)
			{
				base.SetProxyActive(false, InventoryItemManager.SelectedActionType.Default);
			}
		}
		return num;
	}

	// Token: 0x06003D79 RID: 15737 RVA: 0x0010E729 File Offset: 0x0010C929
	public float FadeCrestGroup(bool fadeIn)
	{
		if (this.crestGroup)
		{
			return this.crestGroup.FadeTo((float)(fadeIn ? 1 : 0), this.groupFadeTime, null, true, null);
		}
		return 0f;
	}

	// Token: 0x06003D7A RID: 15738 RVA: 0x0010E75A File Offset: 0x0010C95A
	public Color GetToolTypeColor(ToolItemType type)
	{
		return UI.GetToolTypeColor(type);
	}

	// Token: 0x06003D7B RID: 15739 RVA: 0x0010E762 File Offset: 0x0010C962
	public bool CanChangeEquips()
	{
		return GameManager.instance.playerData.atBench || CheatManager.CanChangeEquipsAnywhere;
	}

	// Token: 0x06003D7C RID: 15740 RVA: 0x0010E77C File Offset: 0x0010C97C
	public bool CanChangeEquips(ToolItemType promptToolType, InventoryItemToolManager.CanChangeEquipsTypes changeType)
	{
		if (changeType == InventoryItemToolManager.CanChangeEquipsTypes.Regular && this.IsHeroCursed)
		{
			if (this.ShowingCursedMsg)
			{
				this.HideCursedMsg(false);
			}
			else
			{
				this.ShowCursedMsg(false, promptToolType);
			}
			return false;
		}
		if (this.CanChangeEquips())
		{
			return true;
		}
		if (this.ShowingToolMsg)
		{
			this.HideToolEquipMsg(false);
		}
		else
		{
			this.ShowToolEquipMsg(promptToolType, changeType);
		}
		return false;
	}

	// Token: 0x06003D7D RID: 15741 RVA: 0x0010E7D4 File Offset: 0x0010C9D4
	public void ShowToolEquipMsg(ToolItemType type, InventoryItemToolManager.CanChangeEquipsTypes changeType)
	{
		if (!this.toolEquipMsg || this.ShowingToolMsg)
		{
			return;
		}
		if (this.toolEquipMsgText)
		{
			TMP_Text tmp_Text = this.toolEquipMsgText;
			LocalisedString s;
			if (changeType != InventoryItemToolManager.CanChangeEquipsTypes.Reload)
			{
				if (changeType != InventoryItemToolManager.CanChangeEquipsTypes.Transform)
				{
					s = ((type == ToolItemType.Skill) ? this.toolEquipMsgSkill : this.toolEquipMsgTool);
				}
				else
				{
					s = this.transformMsg;
				}
			}
			else
			{
				s = this.reloadMsg;
			}
			tmp_Text.text = s;
		}
		this.toolEquipMsg.FadeTo(1f, this.toolMsgFadeInTime, null, true, null);
		this.ShowingToolMsg = true;
		this.paneList.InSubMenu = true;
		this.hideEquipMessageAllowedTime = Time.unscaledTimeAsDouble + (double)this.toolMsgFadeInTime;
		this.failedAudioTable.SpawnAndPlayOneShot(Audio.DefaultUIAudioSourcePrefab, base.transform.position, false, 1f, null);
	}

	// Token: 0x06003D7E RID: 15742 RVA: 0x0010E8AC File Offset: 0x0010CAAC
	public void HideToolEquipMsg(bool force = false)
	{
		if (!this.toolEquipMsg || !this.ShowingToolMsg)
		{
			return;
		}
		if (!force && Time.unscaledTimeAsDouble < this.hideEquipMessageAllowedTime)
		{
			return;
		}
		this.toolEquipMsg.FadeTo(0f, this.toolMsgFadeOutTime, null, true, null);
		this.ShowingToolMsg = false;
		this.paneList.InSubMenu = false;
	}

	// Token: 0x06003D7F RID: 15743 RVA: 0x0010E910 File Offset: 0x0010CB10
	public void HideToolEquipMsgInstant()
	{
		if (!this.toolEquipMsg || !this.ShowingToolMsg)
		{
			return;
		}
		this.toolEquipMsg.FadeTo(0f, 0f, null, true, null);
		this.ShowingToolMsg = false;
		this.paneList.InSubMenu = false;
	}

	// Token: 0x06003D80 RID: 15744 RVA: 0x0010E95F File Offset: 0x0010CB5F
	public void HideEquipMsgs(bool force = false)
	{
		this.HideToolEquipMsg(force);
		this.HideCrestEquipMsg(force);
		this.HideCursedMsg(force);
	}

	// Token: 0x06003D81 RID: 15745 RVA: 0x0010E976 File Offset: 0x0010CB76
	public void HideEquipMsgsInstant()
	{
		this.HideToolEquipMsgInstant();
		this.HideCrestEquipMsgInstant();
		this.HideCursedMsgInstant();
	}

	// Token: 0x06003D82 RID: 15746 RVA: 0x0010E98A File Offset: 0x0010CB8A
	public void ShowCrestEquipMsg()
	{
		this.ShowingCrestMsg = this.ShowBasicEquipMsg(this.crestEquipMsg, this.ShowingCrestMsg);
	}

	// Token: 0x06003D83 RID: 15747 RVA: 0x0010E9A4 File Offset: 0x0010CBA4
	public void HideCrestEquipMsg(bool force = false)
	{
		this.ShowingCrestMsg = this.HideBasicEquipMsg(this.crestEquipMsg, this.toolMsgFadeOutTime, this.ShowingCrestMsg, force);
	}

	// Token: 0x06003D84 RID: 15748 RVA: 0x0010E9C5 File Offset: 0x0010CBC5
	public void HideCrestEquipMsgInstant()
	{
		this.ShowingCrestMsg = this.HideBasicEquipMsg(this.crestEquipMsg, 0f, this.ShowingCrestMsg, true);
	}

	// Token: 0x06003D85 RID: 15749 RVA: 0x0010E9E8 File Offset: 0x0010CBE8
	public void ShowCursedMsg(bool isCrestEquip, ToolItemType toolType)
	{
		if (this.cursedEquipMsgText)
		{
			if (isCrestEquip)
			{
				this.cursedEquipMsgText.text = this.cursedEquipMsgCrest;
			}
			else if (toolType == ToolItemType.Skill)
			{
				this.cursedEquipMsgText.text = this.cursedEquipMsgSkill;
			}
			else
			{
				this.cursedEquipMsgText.text = this.cursedEquipMsgTool;
			}
		}
		this.ShowingCursedMsg = this.ShowBasicEquipMsg(this.cursedEquipMsg, this.ShowingCursedMsg);
	}

	// Token: 0x06003D86 RID: 15750 RVA: 0x0010EA67 File Offset: 0x0010CC67
	public void HideCursedMsg(bool force = false)
	{
		this.ShowingCursedMsg = this.HideBasicEquipMsg(this.cursedEquipMsg, this.toolMsgFadeOutTime, this.ShowingCursedMsg, force);
	}

	// Token: 0x06003D87 RID: 15751 RVA: 0x0010EA88 File Offset: 0x0010CC88
	public void HideCursedMsgInstant()
	{
		this.ShowingCursedMsg = this.HideBasicEquipMsg(this.cursedEquipMsg, 0f, this.ShowingCursedMsg, true);
	}

	// Token: 0x06003D88 RID: 15752 RVA: 0x0010EAA8 File Offset: 0x0010CCA8
	private bool ShowBasicEquipMsg(NestedFadeGroupBase msgGroup, bool showingBool)
	{
		if (!msgGroup || showingBool)
		{
			return showingBool;
		}
		msgGroup.FadeTo(1f, this.toolMsgFadeInTime, null, true, null);
		this.paneList.InSubMenu = true;
		this.hideEquipMessageAllowedTime = Time.unscaledTimeAsDouble + (double)this.toolMsgFadeInTime;
		this.failedAudioTable.SpawnAndPlayOneShot(Audio.DefaultUIAudioSourcePrefab, base.transform.position, false, 1f, null);
		return true;
	}

	// Token: 0x06003D89 RID: 15753 RVA: 0x0010EB1C File Offset: 0x0010CD1C
	private bool HideBasicEquipMsg(NestedFadeGroupBase msgGroup, float fadeTime, bool showingBool, bool force)
	{
		if (!msgGroup || !showingBool)
		{
			return showingBool;
		}
		if (!force && Time.unscaledTimeAsDouble < this.hideEquipMessageAllowedTime)
		{
			return true;
		}
		msgGroup.FadeTo(0f, fadeTime, null, true, null);
		this.paneList.InSubMenu = false;
		return false;
	}

	// Token: 0x06003D8A RID: 15754 RVA: 0x0010EB5C File Offset: 0x0010CD5C
	protected override List<ToolItem> GetItems()
	{
		if (!PlayerData.instance.ConstructedFarsight)
		{
			List<ToolItem> list = ToolItemManager.GetUnlockedTools().ToList<ToolItem>();
			this.currentToolCount = list.Count;
			return list;
		}
		this.currentToolCount = 0;
		List<ToolItem> list2 = ToolItemManager.GetAllTools().ToList<ToolItem>();
		for (int i = list2.Count - 1; i >= 0; i--)
		{
			ToolItem toolItem = list2[i];
			if (toolItem.IsUnlockedNotHidden)
			{
				this.currentToolCount++;
			}
			else if (!toolItem.IsCounted)
			{
				list2.RemoveAt(i);
			}
			else
			{
				SavedItem countKey = toolItem.CountKey;
				foreach (ToolItem toolItem2 in list2)
				{
					if (!(toolItem2 == toolItem) && toolItem2.CountKey == countKey)
					{
						list2.RemoveAt(i);
						break;
					}
				}
			}
		}
		return list2;
	}

	// Token: 0x06003D8B RID: 15755 RVA: 0x0010EC54 File Offset: 0x0010CE54
	protected override List<InventoryItemGrid.GridSection> GetGridSections(List<InventoryItemTool> selectableItems, List<ToolItem> items)
	{
		for (int k = 0; k < selectableItems.Count; k++)
		{
			selectableItems[k].gameObject.SetActive(true);
			selectableItems[k].SetData(items[k]);
		}
		int[] array = typeof(ToolItemType).GetValuesWithOrder().ToArray<int>();
		List<InventoryItemGrid.GridSection> list = new List<InventoryItemGrid.GridSection>(array.Length);
		int[] array2 = array;
		for (int j = 0; j < array2.Length; j++)
		{
			int i = array2[j];
			list.Add(new InventoryItemGrid.GridSection
			{
				Header = this.listSectionHeaders[i].transform,
				Items = (from item in selectableItems
				where item.ToolType == (ToolItemType)i
				select item).Cast<InventoryItemSelectableDirectional>().ToList<InventoryItemSelectableDirectional>()
			});
		}
		return list;
	}

	// Token: 0x06003D8C RID: 15756 RVA: 0x0010ED1C File Offset: 0x0010CF1C
	protected override void OnItemListSetup()
	{
		if (!this.completionText)
		{
			return;
		}
		if (PlayerData.instance.ConstructedFarsight)
		{
			this.completionText.gameObject.SetActive(true);
			int count = ToolItemManager.GetCount(ToolItemManager.GetAllTools(), null);
			int num = Mathf.Min(this.currentToolCount, count);
			this.completionText.text = string.Format("{0} / {1}", num, count);
			return;
		}
		this.completionText.gameObject.SetActive(false);
	}

	// Token: 0x06003D8D RID: 15757 RVA: 0x0010EDA0 File Offset: 0x0010CFA0
	public bool IsAvailable()
	{
		if (CollectableItemManager.IsInHiddenMode())
		{
			return false;
		}
		if (ToolItemManager.GetAllCrests().Count((ToolCrest crest) => crest.IsVisible) > 1)
		{
			return true;
		}
		if (this.GetItems().Count <= 0)
		{
			return false;
		}
		using (List<ToolCrest>.Enumerator enumerator = ToolItemManager.GetAllCrests().GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.IsVisible)
				{
					return true;
				}
			}
		}
		return false;
	}

	// Token: 0x06003D8E RID: 15758 RVA: 0x0010EE40 File Offset: 0x0010D040
	public void SetToolUsePrompt(AttackToolBinding? binding, bool showHold, float offsetY)
	{
		if (this.comboButtonPromptDisplay == null)
		{
			return;
		}
		Vector2 value = this.currencyParentInitialPos.GetValueOrDefault();
		if (this.currencyParentInitialPos == null)
		{
			value = this.currencyParent.transform.localPosition;
			this.currencyParentInitialPos = new Vector2?(value);
		}
		if (binding == null)
		{
			this.comboButtonPromptDisplay.Hide();
			this.currencyParent.transform.SetLocalPosition2D(this.currencyParentInitialPos.Value);
			return;
		}
		this.currencyParent.transform.SetLocalPosition2D(this.currencyPromptAltPos);
		Vector3 vector = this.comboButtonPromptDisplay.transform.localPosition;
		value = this.buttonPromptInitialPos.GetValueOrDefault();
		if (this.buttonPromptInitialPos == null)
		{
			value = vector;
			this.buttonPromptInitialPos = new Vector2?(value);
		}
		if (this.currencyParent.gameObject.activeSelf)
		{
			vector = this.buttonPromptCurrencyAltPos;
		}
		else
		{
			vector = this.buttonPromptInitialPos.Value;
			vector.y += offsetY;
		}
		this.comboButtonPromptDisplay.transform.localPosition = vector;
		this.comboButtonPromptDisplay.Show(new InventoryItemComboButtonPromptDisplay.Display
		{
			ActionButton = HeroActionButton.QUICK_CAST,
			DirectionModifier = binding.Value,
			PromptText = this.toolUsePromptText,
			ShowHold = showHold
		});
	}

	// Token: 0x06003D8F RID: 15759 RVA: 0x0010EFA8 File Offset: 0x0010D1A8
	private void UpdateTextDisplays()
	{
		LocalisedString localisedString = this.CanChangeEquips() ? this.changeCrestText : this.viewCrestsText;
		if (this.changeCrestButton)
		{
			this.changeCrestButton.InteractionText = localisedString;
		}
		if (this.changeCrestButtonText)
		{
			this.changeCrestButtonText.Text = localisedString;
		}
	}

	// Token: 0x06003D90 RID: 15760 RVA: 0x0010F000 File Offset: 0x0010D200
	protected override InventoryItemSelectable GetStartSelectable()
	{
		InventoryItemTool inventoryItemTool = base.GetSelectables(null).FirstOrDefault((InventoryItemTool tool) => ToolItemManager.UnlockedTool == tool.ItemData);
		ToolItemManager.UnlockedTool = null;
		if (inventoryItemTool)
		{
			return inventoryItemTool;
		}
		return base.GetStartSelectable();
	}

	// Token: 0x06003D91 RID: 15761 RVA: 0x0010F050 File Offset: 0x0010D250
	private void UpdateButtonPrompts()
	{
		bool active = this.showEquipPrompt && this.equipState != InventoryItemToolManager.EquipStates.SwitchCrest;
		bool active2 = this.equipState == InventoryItemToolManager.EquipStates.None;
		bool active3 = this.equipState == InventoryItemToolManager.EquipStates.SwitchCrest;
		bool active4 = this.equipState > InventoryItemToolManager.EquipStates.None;
		if (this.equipPrompt)
		{
			this.equipPrompt.gameObject.SetActive(active);
		}
		if (this.changeCrestPrompt)
		{
			this.changeCrestPrompt.AlphaSelf = (this.IsHeroCursed ? this.disabledListSectionOpacity : 1f);
			this.changeCrestPrompt.gameObject.SetActive(active2);
		}
		if (this.selectCrestPrompt)
		{
			this.selectCrestPrompt.gameObject.SetActive(active3);
			this.selectCrestPrompt.AlphaSelf = (this.CanChangeEquips() ? 1f : this.disabledListSectionOpacity);
		}
		if (this.cancelPrompt)
		{
			this.cancelPrompt.SetActive(active4);
		}
		if (this.reloadPrompt)
		{
			this.reloadPrompt.gameObject.SetActive(this.showReloadPrompt && this.equipState != InventoryItemToolManager.EquipStates.SwitchCrest);
		}
		if (this.customTogglePrompt)
		{
			this.customTogglePrompt.gameObject.SetActive(this.showCustomTogglePrompt && this.equipState != InventoryItemToolManager.EquipStates.SwitchCrest);
		}
		if (this.boolToggleParent)
		{
			this.boolToggleParent.SetActive(this.showReloadPrompt && this.equipState != InventoryItemToolManager.EquipStates.SwitchCrest);
		}
		if (this.boolToggleFill)
		{
			this.boolToggleFill.SetActive(false);
		}
		if (this.promptLayout)
		{
			this.promptLayout.ForceUpdateLayoutNoCanvas();
		}
	}

	// Token: 0x06003D92 RID: 15762 RVA: 0x0010F20E File Offset: 0x0010D40E
	public void SetHoveringTool(InventoryItemTool tool, bool refreshTools)
	{
		this.HoveringTool = tool;
		if (refreshTools)
		{
			this.RefreshTools();
		}
	}

	// Token: 0x06003D93 RID: 15763 RVA: 0x0010F220 File Offset: 0x0010D420
	public override bool MoveSelection(InventoryItemManager.SelectionDirection direction)
	{
		bool flag = base.MoveSelection(direction);
		if (!flag)
		{
			InventoryItemToolManager.EquipStates equipStates = this.equipState;
			if (equipStates == InventoryItemToolManager.EquipStates.PlaceTool || equipStates == InventoryItemToolManager.EquipStates.SelectTool)
			{
				return true;
			}
		}
		return flag;
	}

	// Token: 0x06003D96 RID: 15766 RVA: 0x0010F31C File Offset: 0x0010D51C
	[CompilerGenerated]
	private void <PlaceTool>g__Selected|121_0()
	{
		base.SetSelected(this.selectedBeforePickup, null, false);
		this.selectedBeforePickup = null;
	}

	// Token: 0x06003D97 RID: 15767 RVA: 0x0010F348 File Offset: 0x0010D548
	[CompilerGenerated]
	private void <EndSelection>g__SelectionEnd|134_0()
	{
		base.PlayMoveSound();
		base.SetSelected(this.SelectedSlot, null, false);
		this.SelectedSlot = null;
		this.EquipState = InventoryItemToolManager.EquipStates.None;
		this.RefreshTools();
	}

	// Token: 0x04003EEB RID: 16107
	public Action<bool> OnToolRefresh;

	// Token: 0x04003EEC RID: 16108
	[SerializeField]
	private SpriteRenderer displayIcon;

	// Token: 0x04003EED RID: 16109
	[SerializeField]
	private InventoryItemGrid toolList;

	// Token: 0x04003EEE RID: 16110
	[SerializeField]
	private InventoryToolCrestList crestList;

	// Token: 0x04003EEF RID: 16111
	[SerializeField]
	private InventoryFloatingToolSlots extraSlots;

	// Token: 0x04003EF0 RID: 16112
	[SerializeField]
	[ArrayForEnum(typeof(ToolItemType))]
	private NestedFadeGroupSpriteRenderer[] listSectionHeaders;

	// Token: 0x04003EF1 RID: 16113
	[SerializeField]
	private float disabledListSectionOpacity = 0.5f;

	// Token: 0x04003EF2 RID: 16114
	[Space]
	[SerializeField]
	private LayoutGroup promptLayout;

	// Token: 0x04003EF3 RID: 16115
	[SerializeField]
	private NestedFadeGroupBase equipPrompt;

	// Token: 0x04003EF4 RID: 16116
	[SerializeField]
	private TMP_Text equipPromptText;

	// Token: 0x04003EF5 RID: 16117
	[SerializeField]
	private LocalisedString equipText;

	// Token: 0x04003EF6 RID: 16118
	[SerializeField]
	private LocalisedString unequipText;

	// Token: 0x04003EF7 RID: 16119
	[SerializeField]
	private LocalisedString equipSkillText;

	// Token: 0x04003EF8 RID: 16120
	[SerializeField]
	private LocalisedString unequipSkillText;

	// Token: 0x04003EF9 RID: 16121
	[SerializeField]
	private NestedFadeGroupBase changeCrestPrompt;

	// Token: 0x04003EFA RID: 16122
	[SerializeField]
	private NestedFadeGroupBase selectCrestPrompt;

	// Token: 0x04003EFB RID: 16123
	[SerializeField]
	private GameObject cancelPrompt;

	// Token: 0x04003EFC RID: 16124
	[SerializeField]
	private NestedFadeGroupBase reloadPrompt;

	// Token: 0x04003EFD RID: 16125
	[SerializeField]
	private NestedFadeGroupBase customTogglePrompt;

	// Token: 0x04003EFE RID: 16126
	[SerializeField]
	private TMP_Text customTogglePromptText;

	// Token: 0x04003EFF RID: 16127
	[SerializeField]
	[ArrayForEnum(typeof(CurrencyType))]
	private GameObject[] reloadCurrencyCounters;

	// Token: 0x04003F00 RID: 16128
	[SerializeField]
	private GameObject boolToggleParent;

	// Token: 0x04003F01 RID: 16129
	[SerializeField]
	private GameObject boolToggleFill;

	// Token: 0x04003F02 RID: 16130
	[Space]
	[SerializeField]
	private NestedFadeGroupBase toolGroup;

	// Token: 0x04003F03 RID: 16131
	[SerializeField]
	private NestedFadeGroupBase crestGroup;

	// Token: 0x04003F04 RID: 16132
	[SerializeField]
	private float groupFadeTime = 0.1f;

	// Token: 0x04003F05 RID: 16133
	private Coroutine groupFadeRoutine;

	// Token: 0x04003F06 RID: 16134
	[SerializeField]
	private InventoryItemToolTween tweenTool;

	// Token: 0x04003F07 RID: 16135
	[SerializeField]
	private NestedFadeGroupBase toolEquipMsg;

	// Token: 0x04003F08 RID: 16136
	[SerializeField]
	private TMP_Text toolEquipMsgText;

	// Token: 0x04003F09 RID: 16137
	[SerializeField]
	private LocalisedString toolEquipMsgTool;

	// Token: 0x04003F0A RID: 16138
	[SerializeField]
	private LocalisedString toolEquipMsgSkill;

	// Token: 0x04003F0B RID: 16139
	[SerializeField]
	private LocalisedString reloadMsg;

	// Token: 0x04003F0C RID: 16140
	[SerializeField]
	private LocalisedString transformMsg;

	// Token: 0x04003F0D RID: 16141
	[SerializeField]
	private NestedFadeGroupBase crestEquipMsg;

	// Token: 0x04003F0E RID: 16142
	[SerializeField]
	private float toolMsgFadeInTime;

	// Token: 0x04003F0F RID: 16143
	[SerializeField]
	private float toolMsgFadeOutTime;

	// Token: 0x04003F10 RID: 16144
	[SerializeField]
	private NestedFadeGroupBase cursedEquipMsg;

	// Token: 0x04003F11 RID: 16145
	[SerializeField]
	private TMP_Text cursedEquipMsgText;

	// Token: 0x04003F12 RID: 16146
	[SerializeField]
	private LocalisedString cursedEquipMsgTool;

	// Token: 0x04003F13 RID: 16147
	[SerializeField]
	private LocalisedString cursedEquipMsgSkill;

	// Token: 0x04003F14 RID: 16148
	[SerializeField]
	private LocalisedString cursedEquipMsgCrest;

	// Token: 0x04003F15 RID: 16149
	[Space]
	[SerializeField]
	private TMP_Text toolAmountText;

	// Token: 0x04003F16 RID: 16150
	[SerializeField]
	private LocalisedString toolUsePromptText;

	// Token: 0x04003F17 RID: 16151
	[SerializeField]
	private InventoryItemComboButtonPromptDisplay comboButtonPromptDisplay;

	// Token: 0x04003F18 RID: 16152
	[SerializeField]
	private float buttonPromptExtraDescOffset;

	// Token: 0x04003F19 RID: 16153
	[SerializeField]
	private Vector2 buttonPromptCurrencyAltPos;

	// Token: 0x04003F1A RID: 16154
	[SerializeField]
	private Transform currencyParent;

	// Token: 0x04003F1B RID: 16155
	[SerializeField]
	private Vector2 currencyPromptAltPos;

	// Token: 0x04003F1C RID: 16156
	[Space]
	[SerializeField]
	private InventoryItemSelectableButtonEvent changeCrestButton;

	// Token: 0x04003F1D RID: 16157
	[SerializeField]
	private GameObject crestButtonNormalDisplay;

	// Token: 0x04003F1E RID: 16158
	[SerializeField]
	private GameObject crestButtonLockedDisplay;

	// Token: 0x04003F1F RID: 16159
	[SerializeField]
	private SetTextMeshProGameText changeCrestButtonText;

	// Token: 0x04003F20 RID: 16160
	[SerializeField]
	private LocalisedString changeCrestText;

	// Token: 0x04003F21 RID: 16161
	[SerializeField]
	private LocalisedString viewCrestsText;

	// Token: 0x04003F22 RID: 16162
	[SerializeField]
	private GameObject descriptionIconGroup;

	// Token: 0x04003F23 RID: 16163
	[Space]
	[SerializeField]
	private CollectableItem slotUnlockItem;

	// Token: 0x04003F24 RID: 16164
	[SerializeField]
	private CrestSocketUnlockInventoryDescription slotUnlockDescExtra;

	// Token: 0x04003F25 RID: 16165
	[SerializeField]
	private InventoryItemCollectable slotUnlockItemDisplay;

	// Token: 0x04003F26 RID: 16166
	[Space]
	[SerializeField]
	private RandomAudioClipTable failedAudioTable;

	// Token: 0x04003F27 RID: 16167
	[Space]
	[SerializeField]
	private TMP_Text completionText;

	// Token: 0x04003F28 RID: 16168
	private string initialToolAmountText;

	// Token: 0x04003F29 RID: 16169
	private double hideEquipMessageAllowedTime;

	// Token: 0x04003F2A RID: 16170
	private bool showReloadPrompt;

	// Token: 0x04003F2B RID: 16171
	private bool showCustomTogglePrompt;

	// Token: 0x04003F2C RID: 16172
	private bool showEquipPrompt;

	// Token: 0x04003F2D RID: 16173
	private int currentToolCount;

	// Token: 0x04003F2E RID: 16174
	private Vector2? buttonPromptInitialPos;

	// Token: 0x04003F2F RID: 16175
	private Vector2? currencyParentInitialPos;

	// Token: 0x04003F30 RID: 16176
	private InventoryPane pane;

	// Token: 0x04003F31 RID: 16177
	private InventoryPaneList paneList;

	// Token: 0x04003F33 RID: 16179
	private InventoryItemToolManager.EquipStates equipState;

	// Token: 0x04003F38 RID: 16184
	private InventoryItemSelectable selectedBeforePickup;

	// Token: 0x04003F3A RID: 16186
	private bool refreshCurrentSelected;

	// Token: 0x020019DA RID: 6618
	public enum EquipStates
	{
		// Token: 0x040097AE RID: 38830
		None,
		// Token: 0x040097AF RID: 38831
		PlaceTool,
		// Token: 0x040097B0 RID: 38832
		SelectTool,
		// Token: 0x040097B1 RID: 38833
		SwitchCrest
	}

	// Token: 0x020019DB RID: 6619
	public enum CanChangeEquipsTypes
	{
		// Token: 0x040097B3 RID: 38835
		Regular,
		// Token: 0x040097B4 RID: 38836
		Reload,
		// Token: 0x040097B5 RID: 38837
		Transform
	}
}
