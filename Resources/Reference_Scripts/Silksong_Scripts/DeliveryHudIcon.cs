using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

// Token: 0x020005D9 RID: 1497
public class DeliveryHudIcon : RadialHudIcon
{
	// Token: 0x0600355E RID: 13662 RVA: 0x000ECF50 File Offset: 0x000EB150
	private void Awake()
	{
		this.gm = GameManager.instance;
		this.gm.SceneInit += base.UpdateDisplay;
		EventRegister.GetRegisterGuaranteed(base.gameObject, "DELIVERY HUD REFRESH").ReceivedEvent += base.UpdateDisplay;
		EventRegister.GetRegisterGuaranteed(base.gameObject, "DELIVERY HUD HIT").ReceivedEvent += this.SpawnHitEffect;
		EventRegister.GetRegisterGuaranteed(base.gameObject, "DELIVERY HUD BREAK").ReceivedEvent += this.SpawnBreakEffect;
		EventRegister.GetRegisterGuaranteed(base.gameObject, "TOOLS APPEAR").ReceivedEvent += this.StartUp;
	}

	// Token: 0x0600355F RID: 13663 RVA: 0x000ED003 File Offset: 0x000EB203
	private void OnEnable()
	{
		if (this.burstAppearChild)
		{
			this.burstAppearChild.SetActive(false);
		}
	}

	// Token: 0x06003560 RID: 13664 RVA: 0x000ED01E File Offset: 0x000EB21E
	private void OnDestroy()
	{
		if (this.gm)
		{
			this.gm.SceneInit -= base.UpdateDisplay;
		}
	}

	// Token: 0x06003561 RID: 13665 RVA: 0x000ED044 File Offset: 0x000EB244
	private void SpawnHitEffect()
	{
		this.OnHit.Invoke();
		Vector3 position = base.transform.position;
		if (this.hitEffectPrefab)
		{
			this.hitEffectPrefab.Spawn(position);
		}
		if (this.hasCustomHitEffect)
		{
			this.customHitEffectPrefab.Spawn(position);
		}
	}

	// Token: 0x06003562 RID: 13666 RVA: 0x000ED098 File Offset: 0x000EB298
	private void SpawnBreakEffect()
	{
		this.OnBreak.Invoke();
		this.StopLoopEffect();
		this.CleanLoopEffect();
		if (!this.currentItem)
		{
			return;
		}
		GameObject breakUIEffect = this.currentItem.BreakUIEffect;
		if (!breakUIEffect)
		{
			return;
		}
		breakUIEffect.Spawn(base.transform.position);
	}

	// Token: 0x06003563 RID: 13667 RVA: 0x000ED0F1 File Offset: 0x000EB2F1
	private void StartUp()
	{
		this.started = true;
		this.isHudOut = false;
		base.UpdateDisplay();
		if (base.gameObject.activeSelf && this.burstAppearChild)
		{
			this.burstAppearChild.SetActive(true);
		}
	}

	// Token: 0x06003564 RID: 13668 RVA: 0x000ED12D File Offset: 0x000EB32D
	public void HudOut()
	{
		this.isHudOut = true;
		this.StopLoopEffect();
	}

	// Token: 0x06003565 RID: 13669 RVA: 0x000ED13C File Offset: 0x000EB33C
	public void HudIn()
	{
		this.isHudOut = false;
		if (this.queuedAppear && this.started)
		{
			base.UpdateDisplay();
			this.StartLoopEffect();
			if (this.burstAppearChild)
			{
				this.burstAppearChild.SetActive(true);
			}
		}
	}

	// Token: 0x06003566 RID: 13670 RVA: 0x000ED17C File Offset: 0x000EB37C
	protected override void OnPreUpdateDisplay()
	{
		this.previousItem = this.currentItem;
		this.currentItem = null;
		this.currentQuest = null;
		using (IEnumerator<DeliveryQuestItem.ActiveItem> enumerator = DeliveryQuestItem.GetActiveItems().GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				DeliveryQuestItem.ActiveItem activeItem = enumerator.Current;
				this.currentItem = activeItem.Item;
				this.currentQuest = activeItem.Quest;
				this.maxItemCount = activeItem.MaxCount;
			}
		}
		bool flag = this.currentItem != null;
		if (this.currentItem != this.previousItem)
		{
			this.hasCustomHitEffect = false;
			this.customHitEffectPrefab = null;
			if (this.previousItem != null)
			{
				this.CleanLoopEffect();
			}
			if (flag)
			{
				this.customHitEffectPrefab = this.currentItem.HitUIEffect;
				this.hasCustomHitEffect = (this.customHitEffectPrefab != null);
				this.SpawnLoopEffect();
			}
		}
		if (this.isHudOut && flag)
		{
			this.queuedAppear = true;
			return;
		}
		this.queuedAppear = false;
		if (flag)
		{
			this.StartLoopEffect();
		}
	}

	// Token: 0x06003567 RID: 13671 RVA: 0x000ED294 File Offset: 0x000EB494
	protected override bool GetIsActive()
	{
		return this.started && !this.queuedAppear && this.currentItem && DeliveryQuestItem.CanTakeHit();
	}

	// Token: 0x06003568 RID: 13672 RVA: 0x000ED2BE File Offset: 0x000EB4BE
	protected override void GetAmounts(out int amountLeft, out int totalCount)
	{
		amountLeft = (this.currentQuest ? this.currentQuest.Counters.FirstOrDefault<int>() : this.currentItem.CollectedAmount);
		totalCount = this.maxItemCount;
	}

	// Token: 0x06003569 RID: 13673 RVA: 0x000ED2F4 File Offset: 0x000EB4F4
	protected override bool TryGetHudSprite(out Sprite sprite)
	{
		sprite = this.currentItem.GetIcon(CollectableItem.ReadSource.Tiny);
		if (sprite)
		{
			return true;
		}
		sprite = this.currentItem.GetIcon(CollectableItem.ReadSource.Inventory);
		return false;
	}

	// Token: 0x0600356A RID: 13674 RVA: 0x000ED31E File Offset: 0x000EB51E
	public override bool GetIsEmpty()
	{
		return false;
	}

	// Token: 0x0600356B RID: 13675 RVA: 0x000ED321 File Offset: 0x000EB521
	protected override bool HasTargetChanged()
	{
		return this.currentItem != this.previousItem;
	}

	// Token: 0x0600356C RID: 13676 RVA: 0x000ED334 File Offset: 0x000EB534
	protected override bool TryGetBarColour(out Color color)
	{
		if (!this.currentItem)
		{
			color = Color.white;
			return false;
		}
		color = this.currentItem.BarColour;
		return true;
	}

	// Token: 0x0600356D RID: 13677 RVA: 0x000ED364 File Offset: 0x000EB564
	protected override float GetMidProgress()
	{
		foreach (HeroController.DeliveryTimer deliveryTimer in HeroController.instance.GetDeliveryTimers())
		{
			if (!(deliveryTimer.Item.Item != this.currentItem))
			{
				float timeLeft = deliveryTimer.TimeLeft;
				float chunkDuration = deliveryTimer.Item.Item.GetChunkDuration(deliveryTimer.Item.MaxCount);
				return (chunkDuration - timeLeft) / chunkDuration;
			}
		}
		return 0f;
	}

	// Token: 0x0600356E RID: 13678 RVA: 0x000ED3FC File Offset: 0x000EB5FC
	private void SpawnLoopEffect()
	{
		if (this.hasLoopEffect)
		{
			if (this.loopEffectObject != null)
			{
				Object.Destroy(this.loopEffectObject);
			}
			this.hasLoopEffect = false;
		}
		if (this.currentItem != this.previousItem && this.currentItem != null && this.currentItem.UILoopEffect != null)
		{
			this.loopEffectObject = this.currentItem.UILoopEffect.Spawn(base.transform, Vector3.zero);
			this.hasLoopEffect = (this.loopEffectObject != null);
			if (this.hasLoopEffect)
			{
				this.loopEffectObject.SetActive(false);
			}
		}
	}

	// Token: 0x0600356F RID: 13679 RVA: 0x000ED4AC File Offset: 0x000EB6AC
	private void StartLoopEffect()
	{
		if (this.hasLoopEffect)
		{
			this.loopEffectObject.gameObject.SetActive(true);
		}
	}

	// Token: 0x06003570 RID: 13680 RVA: 0x000ED4C7 File Offset: 0x000EB6C7
	private void StopLoopEffect()
	{
		if (this.hasLoopEffect)
		{
			this.loopEffectObject.gameObject.SetActive(false);
		}
	}

	// Token: 0x06003571 RID: 13681 RVA: 0x000ED4E2 File Offset: 0x000EB6E2
	private void CleanLoopEffect()
	{
		if (this.hasLoopEffect)
		{
			Object.Destroy(this.loopEffectObject);
			this.hasLoopEffect = false;
		}
	}

	// Token: 0x040038B5 RID: 14517
	[Space]
	[SerializeField]
	private GameObject hitEffectPrefab;

	// Token: 0x040038B6 RID: 14518
	[SerializeField]
	private GameObject burstAppearChild;

	// Token: 0x040038B7 RID: 14519
	[Space]
	public UnityEvent OnHit;

	// Token: 0x040038B8 RID: 14520
	public UnityEvent OnBreak;

	// Token: 0x040038B9 RID: 14521
	private DeliveryQuestItem previousItem;

	// Token: 0x040038BA RID: 14522
	private DeliveryQuestItem currentItem;

	// Token: 0x040038BB RID: 14523
	private int maxItemCount;

	// Token: 0x040038BC RID: 14524
	private FullQuestBase currentQuest;

	// Token: 0x040038BD RID: 14525
	private bool isHudOut;

	// Token: 0x040038BE RID: 14526
	private bool queuedAppear;

	// Token: 0x040038BF RID: 14527
	private GameManager gm;

	// Token: 0x040038C0 RID: 14528
	private bool started;

	// Token: 0x040038C1 RID: 14529
	private bool hasLoopEffect;

	// Token: 0x040038C2 RID: 14530
	private GameObject loopEffectObject;

	// Token: 0x040038C3 RID: 14531
	private bool hasCustomHitEffect;

	// Token: 0x040038C4 RID: 14532
	private GameObject customHitEffectPrefab;
}
