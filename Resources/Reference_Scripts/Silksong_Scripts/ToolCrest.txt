using System;
using System.Collections.Generic;
using System.Linq;
using TeamCherry.Localization;
using UnityEngine;

// Token: 0x020005E4 RID: 1508
[CreateAssetMenu(fileName = "New Crest", menuName = "Hornet/Tool Crest")]
public class ToolCrest : ToolBase
{
	// Token: 0x170005E4 RID: 1508
	// (get) Token: 0x060035AB RID: 13739 RVA: 0x000EE080 File Offset: 0x000EC280
	public LocalisedString DisplayName
	{
		get
		{
			return this.displayName;
		}
	}

	// Token: 0x170005E5 RID: 1509
	// (get) Token: 0x060035AC RID: 13740 RVA: 0x000EE088 File Offset: 0x000EC288
	public LocalisedString Description
	{
		get
		{
			return this.description;
		}
	}

	// Token: 0x170005E6 RID: 1510
	// (get) Token: 0x060035AD RID: 13741 RVA: 0x000EE090 File Offset: 0x000EC290
	public LocalisedString ItemNamePrefix
	{
		get
		{
			return this.itemNamePrefix;
		}
	}

	// Token: 0x170005E7 RID: 1511
	// (get) Token: 0x060035AE RID: 13742 RVA: 0x000EE098 File Offset: 0x000EC298
	public LocalisedString GetPromptDesc
	{
		get
		{
			return this.getPromptDesc;
		}
	}

	// Token: 0x170005E8 RID: 1512
	// (get) Token: 0x060035AF RID: 13743 RVA: 0x000EE0A0 File Offset: 0x000EC2A0
	public LocalisedString EquipText
	{
		get
		{
			return this.equipText;
		}
	}

	// Token: 0x170005E9 RID: 1513
	// (get) Token: 0x060035B0 RID: 13744 RVA: 0x000EE0A8 File Offset: 0x000EC2A8
	public Sprite CrestSprite
	{
		get
		{
			return this.crestSprite;
		}
	}

	// Token: 0x170005EA RID: 1514
	// (get) Token: 0x060035B1 RID: 13745 RVA: 0x000EE0B0 File Offset: 0x000EC2B0
	public Sprite CrestSilhouette
	{
		get
		{
			return this.crestSilhouette;
		}
	}

	// Token: 0x170005EB RID: 1515
	// (get) Token: 0x060035B2 RID: 13746 RVA: 0x000EE0B8 File Offset: 0x000EC2B8
	public Sprite CrestGlow
	{
		get
		{
			return this.crestGlow;
		}
	}

	// Token: 0x170005EC RID: 1516
	// (get) Token: 0x060035B3 RID: 13747 RVA: 0x000EE0C0 File Offset: 0x000EC2C0
	public bool IsHidden
	{
		get
		{
			return this.isHidden;
		}
	}

	// Token: 0x170005ED RID: 1517
	// (get) Token: 0x060035B4 RID: 13748 RVA: 0x000EE0C8 File Offset: 0x000EC2C8
	public GameObject DisplayPrefab
	{
		get
		{
			return this.displayPrefab;
		}
	}

	// Token: 0x170005EE RID: 1518
	// (get) Token: 0x060035B5 RID: 13749 RVA: 0x000EE0D0 File Offset: 0x000EC2D0
	public ToolCrest.SlotInfo[] Slots
	{
		get
		{
			return this.slots;
		}
	}

	// Token: 0x170005EF RID: 1519
	// (get) Token: 0x060035B6 RID: 13750 RVA: 0x000EE0D8 File Offset: 0x000EC2D8
	public bool HasCustomAction
	{
		get
		{
			return this.hasCustomAction;
		}
	}

	// Token: 0x170005F0 RID: 1520
	// (get) Token: 0x060035B7 RID: 13751 RVA: 0x000EE0E0 File Offset: 0x000EC2E0
	public InventoryItemComboButtonPromptDisplay.Display CustomButtonCombo
	{
		get
		{
			return this.customButtonCombo;
		}
	}

	// Token: 0x170005F1 RID: 1521
	// (get) Token: 0x060035B8 RID: 13752 RVA: 0x000EE0E8 File Offset: 0x000EC2E8
	public HeroControllerConfig HeroConfig
	{
		get
		{
			return this.heroConfig;
		}
	}

	// Token: 0x170005F2 RID: 1522
	// (get) Token: 0x060035B9 RID: 13753 RVA: 0x000EE0F0 File Offset: 0x000EC2F0
	// (set) Token: 0x060035BA RID: 13754 RVA: 0x000EE107 File Offset: 0x000EC307
	public ToolCrestsData.Data SaveData
	{
		get
		{
			return PlayerData.instance.ToolEquips.GetData(this.name);
		}
		set
		{
			PlayerData.instance.ToolEquips.SetData(this.name, value);
		}
	}

	// Token: 0x170005F3 RID: 1523
	// (get) Token: 0x060035BB RID: 13755 RVA: 0x000EE11F File Offset: 0x000EC31F
	public bool IsUnlocked
	{
		get
		{
			return this.SaveData.IsUnlocked;
		}
	}

	// Token: 0x170005F4 RID: 1524
	// (get) Token: 0x060035BC RID: 13756 RVA: 0x000EE12C File Offset: 0x000EC32C
	public bool IsUpgradedVersionUnlocked
	{
		get
		{
			return this.upgradedVersion && this.upgradedVersion.IsUnlocked;
		}
	}

	// Token: 0x170005F5 RID: 1525
	// (get) Token: 0x060035BD RID: 13757 RVA: 0x000EE148 File Offset: 0x000EC348
	public bool IsBaseVersion
	{
		get
		{
			return !this.previousVersion;
		}
	}

	// Token: 0x170005F6 RID: 1526
	// (get) Token: 0x060035BE RID: 13758 RVA: 0x000EE158 File Offset: 0x000EC358
	public bool IsVisible
	{
		get
		{
			return !this.IsUpgradedVersionUnlocked && this.IsUnlocked && (!this.IsHidden || this.IsEquipped);
		}
	}

	// Token: 0x170005F7 RID: 1527
	// (get) Token: 0x060035BF RID: 13759 RVA: 0x000EE17E File Offset: 0x000EC37E
	public override bool IsEquipped
	{
		get
		{
			return PlayerData.instance.CurrentCrestID == this.name;
		}
	}

	// Token: 0x170005F8 RID: 1528
	// (get) Token: 0x060035C0 RID: 13760 RVA: 0x000EE195 File Offset: 0x000EC395
	// (set) Token: 0x060035C1 RID: 13761 RVA: 0x000EE1B8 File Offset: 0x000EC3B8
	public new string name
	{
		get
		{
			if (!this.cachedName)
			{
				this.nameCache = base.name;
				this.cachedName = true;
			}
			return this.nameCache;
		}
		set
		{
			this.nameCache = value;
			base.name = value;
		}
	}

	// Token: 0x060035C2 RID: 13762 RVA: 0x000EE1C8 File Offset: 0x000EC3C8
	private void OnValidate()
	{
		if (this.oldPreviousVersion && this.oldPreviousVersion.upgradedVersion == this)
		{
			this.oldPreviousVersion.upgradedVersion = null;
		}
		if (this.previousVersion)
		{
			this.previousVersion.upgradedVersion = this;
		}
		this.oldPreviousVersion = this.previousVersion;
	}

	// Token: 0x060035C3 RID: 13763 RVA: 0x000EE226 File Offset: 0x000EC426
	private void OnEnable()
	{
		this.OnValidate();
	}

	// Token: 0x060035C4 RID: 13764 RVA: 0x000EE230 File Offset: 0x000EC430
	public void Unlock()
	{
		if (this.IsUnlocked)
		{
			return;
		}
		if (this.previousVersion)
		{
			if (!this.previousVersion.IsUnlocked)
			{
				this.previousVersion.Unlock();
			}
			ToolCrestsData.Data saveData = default(ToolCrestsData.Data);
			saveData.IsUnlocked = true;
			List<ToolCrestsData.SlotData> list = this.previousVersion.SaveData.Slots;
			saveData.Slots = ((list != null) ? list.ToList<ToolCrestsData.SlotData>() : null);
			saveData.DisplayNewIndicator = true;
			this.SaveData = saveData;
			if (PlayerData.instance.CurrentCrestID == this.previousVersion.name)
			{
				ToolItemManager.SetEquippedCrest(this.name);
			}
		}
		else
		{
			ToolCrestsData.Data saveData = default(ToolCrestsData.Data);
			saveData.IsUnlocked = true;
			saveData.Slots = this.slots.Select((ToolCrest.SlotInfo slotInfo, int _) => new ToolCrestsData.SlotData
			{
				IsUnlocked = !slotInfo.IsLocked
			}).ToList<ToolCrestsData.SlotData>();
			saveData.DisplayNewIndicator = true;
			this.SaveData = saveData;
		}
		ToolItemManager.ReportCrestUnlocked(this.IsBaseVersion);
		InventoryPaneList.SetNextOpen("Tools");
	}

	// Token: 0x060035C5 RID: 13765 RVA: 0x000EE343 File Offset: 0x000EC543
	public override void Get(bool showPopup = true)
	{
		this.Unlock();
	}

	// Token: 0x060035C6 RID: 13766 RVA: 0x000EE34B File Offset: 0x000EC54B
	public override bool CanGetMore()
	{
		return !this.IsUnlocked;
	}

	// Token: 0x060035C7 RID: 13767 RVA: 0x000EE356 File Offset: 0x000EC556
	public override Sprite GetPopupIcon()
	{
		return this.CrestSprite;
	}

	// Token: 0x04003902 RID: 14594
	[SerializeField]
	private LocalisedString displayName;

	// Token: 0x04003903 RID: 14595
	[SerializeField]
	private LocalisedString description;

	// Token: 0x04003904 RID: 14596
	[Space]
	[SerializeField]
	private LocalisedString itemNamePrefix;

	// Token: 0x04003905 RID: 14597
	[SerializeField]
	[LocalisedString.NotRequiredAttribute]
	private LocalisedString getPromptDesc;

	// Token: 0x04003906 RID: 14598
	[SerializeField]
	private LocalisedString equipText;

	// Token: 0x04003907 RID: 14599
	[Space]
	[SerializeField]
	private Sprite crestSprite;

	// Token: 0x04003908 RID: 14600
	[SerializeField]
	private Sprite crestSilhouette;

	// Token: 0x04003909 RID: 14601
	[SerializeField]
	private Sprite crestGlow;

	// Token: 0x0400390A RID: 14602
	[Space]
	[SerializeField]
	private bool isHidden;

	// Token: 0x0400390B RID: 14603
	[SerializeField]
	private GameObject displayPrefab;

	// Token: 0x0400390C RID: 14604
	[Space]
	[SerializeField]
	private ToolCrest.SlotInfo[] slots;

	// Token: 0x0400390D RID: 14605
	[Space]
	[SerializeField]
	private bool hasCustomAction;

	// Token: 0x0400390E RID: 14606
	[SerializeField]
	private InventoryItemComboButtonPromptDisplay.Display customButtonCombo;

	// Token: 0x0400390F RID: 14607
	[Space]
	[SerializeField]
	private HeroControllerConfig heroConfig;

	// Token: 0x04003910 RID: 14608
	[Space]
	[SerializeField]
	private ToolCrest previousVersion;

	// Token: 0x04003911 RID: 14609
	[NonSerialized]
	private ToolCrest oldPreviousVersion;

	// Token: 0x04003912 RID: 14610
	[NonSerialized]
	private ToolCrest upgradedVersion;

	// Token: 0x04003913 RID: 14611
	private bool cachedName;

	// Token: 0x04003914 RID: 14612
	private string nameCache;

	// Token: 0x0200191A RID: 6426
	[Serializable]
	public struct SlotInfo
	{
		// Token: 0x060093D5 RID: 37845 RVA: 0x002A2085 File Offset: 0x002A0285
		private bool IsAttackType()
		{
			return this.Type.IsAttackType();
		}

		// Token: 0x040094C8 RID: 38088
		public Vector2 Position;

		// Token: 0x040094C9 RID: 38089
		public ToolItemType Type;

		// Token: 0x040094CA RID: 38090
		[ModifiableProperty]
		[Conditional("IsAttackType", true, true, true)]
		public AttackToolBinding AttackBinding;

		// Token: 0x040094CB RID: 38091
		[Space]
		public int NavUpIndex;

		// Token: 0x040094CC RID: 38092
		public int NavDownIndex;

		// Token: 0x040094CD RID: 38093
		public int NavLeftIndex;

		// Token: 0x040094CE RID: 38094
		public int NavRightIndex;

		// Token: 0x040094CF RID: 38095
		[Space]
		public int NavUpFallbackIndex;

		// Token: 0x040094D0 RID: 38096
		public int NavDownFallbackIndex;

		// Token: 0x040094D1 RID: 38097
		public int NavLeftFallbackIndex;

		// Token: 0x040094D2 RID: 38098
		public int NavRightFallbackIndex;

		// Token: 0x040094D3 RID: 38099
		[Space]
		public bool IsLocked;
	}
}
