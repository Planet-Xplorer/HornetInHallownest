using System;
using System.Collections;
using GlobalSettings;
using UnityEngine;

// Token: 0x020005D8 RID: 1496
public class CrestSlotUnlockMsg : UIMsgPopupBase<CrestSlotUnlockMsg.SlotUnlockPopupInfo, CrestSlotUnlockMsg>
{
	// Token: 0x06003558 RID: 13656 RVA: 0x000ECDB4 File Offset: 0x000EAFB4
	private void OnValidate()
	{
		ArrayForEnumAttribute.EnsureArraySize<CrestSlotUnlockMsg.SlotIcons>(ref this.slotTypeIcons, typeof(ToolItemType));
		for (int i = 0; i < this.slotTypeIcons.Length; i++)
		{
			CrestSlotUnlockMsg.SlotIcons slotIcons = this.slotTypeIcons[i];
			ArrayForEnumAttribute.EnsureArraySize<RuntimeAnimatorController>(ref slotIcons.Icons, typeof(AttackToolBinding));
			this.slotTypeIcons[i] = slotIcons;
		}
	}

	// Token: 0x06003559 RID: 13657 RVA: 0x000ECE19 File Offset: 0x000EB019
	private void Awake()
	{
		this.OnValidate();
	}

	// Token: 0x0600355A RID: 13658 RVA: 0x000ECE24 File Offset: 0x000EB024
	public static void Spawn(ToolItemType toolType, AttackToolBinding attackBinding)
	{
		CrestSlotUnlockMsg crestSlotUnlockMsgPrefab = UI.CrestSlotUnlockMsgPrefab;
		if (!crestSlotUnlockMsgPrefab)
		{
			return;
		}
		UIMsgPopupBase<CrestSlotUnlockMsg.SlotUnlockPopupInfo, CrestSlotUnlockMsg>.SpawnInternal(crestSlotUnlockMsgPrefab, new CrestSlotUnlockMsg.SlotUnlockPopupInfo
		{
			SlotType = toolType,
			AttackBinding = (toolType.IsAttackType() ? attackBinding : AttackToolBinding.Neutral)
		}, null, false);
	}

	// Token: 0x0600355B RID: 13659 RVA: 0x000ECE70 File Offset: 0x000EB070
	protected override void UpdateDisplay(CrestSlotUnlockMsg.SlotUnlockPopupInfo item)
	{
		if (this.icon)
		{
			this.icon.color = UI.GetToolTypeColor(item.SlotType);
		}
		if (this.animator)
		{
			RuntimeAnimatorController runtimeAnimatorController = this.slotTypeIcons[(int)item.SlotType].Icons[(int)item.AttackBinding];
			this.animator.runtimeAnimatorController = runtimeAnimatorController;
			if (this.animateRoutine != null)
			{
				base.StopCoroutine(this.animateRoutine);
				this.animateRoutine = null;
			}
			this.animator.enabled = true;
			this.animator.Play("Unequip", 0, 0f);
			if (this.iconAnimateDelay > 0f)
			{
				this.animateRoutine = base.StartCoroutine(this.AnimateDelayed());
			}
		}
	}

	// Token: 0x0600355C RID: 13660 RVA: 0x000ECF39 File Offset: 0x000EB139
	private IEnumerator AnimateDelayed()
	{
		yield return null;
		this.animator.enabled = false;
		yield return new WaitForSeconds(this.iconAnimateDelay);
		this.animator.enabled = true;
		this.animator.Play("Unequip", 0, 0f);
		this.animateRoutine = null;
		yield break;
	}

	// Token: 0x040038B0 RID: 14512
	[Space]
	[SerializeField]
	private SpriteRenderer icon;

	// Token: 0x040038B1 RID: 14513
	[SerializeField]
	private Animator animator;

	// Token: 0x040038B2 RID: 14514
	[SerializeField]
	private float iconAnimateDelay;

	// Token: 0x040038B3 RID: 14515
	[SerializeField]
	[ArrayForEnum(typeof(ToolItemType))]
	private CrestSlotUnlockMsg.SlotIcons[] slotTypeIcons;

	// Token: 0x040038B4 RID: 14516
	private Coroutine animateRoutine;

	// Token: 0x02001915 RID: 6421
	public struct SlotUnlockPopupInfo : IUIMsgPopupItem
	{
		// Token: 0x17001057 RID: 4183
		// (get) Token: 0x060093C2 RID: 37826 RVA: 0x002A1E20 File Offset: 0x002A0020
		// (set) Token: 0x060093C3 RID: 37827 RVA: 0x002A1E28 File Offset: 0x002A0028
		public ToolItemType SlotType { readonly get; set; }

		// Token: 0x17001058 RID: 4184
		// (get) Token: 0x060093C4 RID: 37828 RVA: 0x002A1E31 File Offset: 0x002A0031
		// (set) Token: 0x060093C5 RID: 37829 RVA: 0x002A1E39 File Offset: 0x002A0039
		public AttackToolBinding AttackBinding { readonly get; set; }

		// Token: 0x060093C6 RID: 37830 RVA: 0x002A1E42 File Offset: 0x002A0042
		public Object GetRepresentingObject()
		{
			return null;
		}
	}

	// Token: 0x02001916 RID: 6422
	[Serializable]
	private struct SlotIcons
	{
		// Token: 0x040094BE RID: 38078
		[ArrayForEnum(typeof(AttackToolBinding))]
		public RuntimeAnimatorController[] Icons;
	}
}
