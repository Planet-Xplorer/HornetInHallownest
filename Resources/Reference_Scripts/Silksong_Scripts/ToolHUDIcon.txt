using System;
using GlobalSettings;
using UnityEngine;

// Token: 0x020005E8 RID: 1512
public class ToolHudIcon : RadialHudIcon
{
	// Token: 0x140000A5 RID: 165
	// (add) Token: 0x060035D5 RID: 13781 RVA: 0x000EE5A8 File Offset: 0x000EC7A8
	// (remove) Token: 0x060035D6 RID: 13782 RVA: 0x000EE5E0 File Offset: 0x000EC7E0
	public event Action Updated;

	// Token: 0x170005F9 RID: 1529
	// (get) Token: 0x060035D7 RID: 13783 RVA: 0x000EE615 File Offset: 0x000EC815
	// (set) Token: 0x060035D8 RID: 13784 RVA: 0x000EE61D File Offset: 0x000EC81D
	public ToolItem CurrentTool { get; private set; }

	// Token: 0x060035D9 RID: 13785 RVA: 0x000EE628 File Offset: 0x000EC828
	private void Awake()
	{
		ToolItemManager.BoundAttackToolUpdated += this.OnBoundAttackToolUpdated;
		ToolItemManager.BoundAttackToolFailed += this.OnBoundAttackToolFailed;
		EventRegister.GetRegisterGuaranteed(base.gameObject, "TOOL EQUIPS CHANGED").ReceivedEvent += base.UpdateDisplay;
		EventRegister.GetRegisterGuaranteed(base.gameObject, "SILK REFRESHED").ReceivedEvent += this.OnSilkSpoolRefreshed;
		if (this.updateOnHealthChange)
		{
			EventRegister.GetRegisterGuaranteed(base.gameObject, "HEALTH UPDATE").ReceivedEvent += this.OnSilkSpoolRefreshed;
			EventRegister.GetRegisterGuaranteed(base.gameObject, "HERO HEALED").ReceivedEvent += this.OnSilkSpoolRefreshed;
			EventRegister.GetRegisterGuaranteed(base.gameObject, "HERO HEALED TO MAX").ReceivedEvent += this.OnSilkSpoolRefreshed;
		}
	}

	// Token: 0x060035DA RID: 13786 RVA: 0x000EE704 File Offset: 0x000EC904
	private void OnDestroy()
	{
		ToolItemManager.BoundAttackToolUpdated -= this.OnBoundAttackToolUpdated;
		ToolItemManager.BoundAttackToolFailed -= this.OnBoundAttackToolFailed;
	}

	// Token: 0x060035DB RID: 13787 RVA: 0x000EE728 File Offset: 0x000EC928
	protected override void OnPreUpdateDisplay()
	{
		this.previousTool = this.CurrentTool;
		this.CurrentTool = ToolItemManager.GetBoundAttackTool(this.binding, ToolEquippedReadSource.Hud);
		if (this.CurrentTool)
		{
			this.isPoison = (this.CurrentTool.PoisonDamageTicks > 0 && Gameplay.PoisonPouchTool.IsEquippedHud);
			this.isZap = (this.CurrentTool.ZapDamageTicks > 0 && Gameplay.ZapImbuementTool.IsEquippedHud);
		}
		else
		{
			this.isPoison = false;
			this.isZap = false;
		}
		if (this.skillZapIcon)
		{
			if (!this.gotZapIconColor)
			{
				this.zapIconColor = this.skillZapIcon.color;
				this.gotZapIconColor = true;
			}
			if (this.isZap)
			{
				this.skillZapIcon.gameObject.SetActive(true);
				bool isEmpty = this.GetIsEmpty();
				this.skillZapIcon.color = (isEmpty ? this.zapIconColor.MultiplyElements(this.inactiveColor) : this.zapIconColor);
			}
			else
			{
				this.skillZapIcon.gameObject.SetActive(false);
			}
		}
		Action updated = this.Updated;
		if (updated == null)
		{
			return;
		}
		updated();
	}

	// Token: 0x060035DC RID: 13788 RVA: 0x000EE84C File Offset: 0x000ECA4C
	protected override void SetIconColour(SpriteRenderer icon, Color color)
	{
		bool flag = this.CurrentTool.HudSpriteModified == this.CurrentTool.HudSpriteBase;
		if (flag && this.isPoison && this.CurrentTool.UsePoisonTintRecolour)
		{
			base.SetIconColour(icon, color * Gameplay.PoisonPouchTintColour);
			if (!icon.sharedMaterial.IsKeywordEnabled("RECOLOUR"))
			{
				icon.material.EnableKeyword("RECOLOUR");
			}
		}
		else
		{
			base.SetIconColour(icon, color);
			if (icon.sharedMaterial.IsKeywordEnabled("RECOLOUR"))
			{
				icon.material.DisableKeyword("RECOLOUR");
			}
		}
		if (flag && this.isPoison)
		{
			if (!icon.sharedMaterial.IsKeywordEnabled("CAN_HUESHIFT"))
			{
				icon.material.EnableKeyword("CAN_HUESHIFT");
			}
			icon.material.SetFloat(ToolHudIcon._hueShiftPropId, this.CurrentTool.PoisonHueShift);
			return;
		}
		if (icon.sharedMaterial.IsKeywordEnabled("CAN_HUESHIFT"))
		{
			icon.material.DisableKeyword("CAN_HUESHIFT");
		}
	}

	// Token: 0x060035DD RID: 13789 RVA: 0x000EE956 File Offset: 0x000ECB56
	protected override bool GetIsActive()
	{
		return this.CurrentTool;
	}

	// Token: 0x060035DE RID: 13790 RVA: 0x000EE964 File Offset: 0x000ECB64
	protected override void GetAmounts(out int amountLeft, out int totalCount)
	{
		PlayerData instance = PlayerData.instance;
		if (this.CurrentTool.Type == ToolItemType.Skill)
		{
			amountLeft = 0;
			totalCount = 0;
			return;
		}
		amountLeft = instance.GetToolData(this.CurrentTool.name).AmountLeft;
		totalCount = ToolItemManager.GetToolStorageAmount(this.CurrentTool);
	}

	// Token: 0x060035DF RID: 13791 RVA: 0x000EE9B4 File Offset: 0x000ECBB4
	protected override bool TryGetHudSprite(out Sprite sprite)
	{
		ToolItemSkill toolItemSkill = this.CurrentTool as ToolItemSkill;
		if (toolItemSkill != null && !this.GetIsEmpty())
		{
			sprite = toolItemSkill.HudGlowSprite;
			if (sprite)
			{
				return true;
			}
		}
		sprite = this.CurrentTool.HudSpriteModified;
		if (sprite)
		{
			return true;
		}
		sprite = this.CurrentTool.InventorySpriteModified;
		return false;
	}

	// Token: 0x060035E0 RID: 13792 RVA: 0x000EEA14 File Offset: 0x000ECC14
	public override bool GetIsEmpty()
	{
		PlayerData instance = PlayerData.instance;
		if (this.CurrentTool.Type != ToolItemType.Skill)
		{
			return this.CurrentTool.IsEmpty && !this.CurrentTool.UsableWhenEmpty;
		}
		return instance.silk < instance.SilkSkillCost;
	}

	// Token: 0x060035E1 RID: 13793 RVA: 0x000EEA61 File Offset: 0x000ECC61
	protected override bool HasTargetChanged()
	{
		return this.CurrentTool is ToolItemSkill || this.CurrentTool != this.previousTool;
	}

	// Token: 0x060035E2 RID: 13794 RVA: 0x000EEA83 File Offset: 0x000ECC83
	private void OnBoundAttackToolUpdated(AttackToolBinding otherBinding)
	{
		if (otherBinding != this.binding)
		{
			return;
		}
		base.UpdateDisplay();
	}

	// Token: 0x060035E3 RID: 13795 RVA: 0x000EEA98 File Offset: 0x000ECC98
	private void OnBoundAttackToolFailed(AttackToolBinding otherBinding)
	{
		if (otherBinding != this.binding)
		{
			return;
		}
		if (this.animator)
		{
			this.animator.SetTrigger(this.animFailedTrigger);
		}
		this.failedAudioTable.SpawnAndPlayOneShot(Audio.DefaultUIAudioSourcePrefab, base.transform.position, false, 1f, null);
	}

	// Token: 0x060035E4 RID: 13796 RVA: 0x000EEAF0 File Offset: 0x000ECCF0
	private void OnSilkSpoolRefreshed()
	{
		if (this.CurrentTool && this.CurrentTool.Type == ToolItemType.Skill)
		{
			base.UpdateDisplay();
		}
	}

	// Token: 0x060035E5 RID: 13797 RVA: 0x000EEB13 File Offset: 0x000ECD13
	public void UpdateDisplayInstant()
	{
		this.previousTool = null;
		base.UpdateDisplay();
	}

	// Token: 0x060035E6 RID: 13798 RVA: 0x000EEB22 File Offset: 0x000ECD22
	protected override bool TryGetBarColour(out Color color)
	{
		if (!this.CurrentTool)
		{
			return base.TryGetBarColour(out color);
		}
		color = UI.GetToolTypeColor(this.CurrentTool.Type);
		return true;
	}

	// Token: 0x04003921 RID: 14625
	[Space]
	[SerializeField]
	private AttackToolBinding binding;

	// Token: 0x04003922 RID: 14626
	[SerializeField]
	private Animator animator;

	// Token: 0x04003923 RID: 14627
	[SerializeField]
	private RandomAudioClipTable failedAudioTable;

	// Token: 0x04003924 RID: 14628
	[Space]
	[SerializeField]
	private SpriteRenderer skillZapIcon;

	// Token: 0x04003925 RID: 14629
	[SerializeField]
	private bool updateOnHealthChange;

	// Token: 0x04003926 RID: 14630
	private ToolItem previousTool;

	// Token: 0x04003927 RID: 14631
	private bool isPoison;

	// Token: 0x04003928 RID: 14632
	private bool isZap;

	// Token: 0x04003929 RID: 14633
	private bool gotZapIconColor;

	// Token: 0x0400392A RID: 14634
	private Color zapIconColor;

	// Token: 0x0400392B RID: 14635
	private readonly int animFailedTrigger = Animator.StringToHash("Failed");

	// Token: 0x0400392C RID: 14636
	private static readonly int _hueShiftPropId = Shader.PropertyToID("_HueShift");
}
