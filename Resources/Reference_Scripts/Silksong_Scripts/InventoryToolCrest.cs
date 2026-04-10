using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using GlobalSettings;
using TeamCherry.Localization;
using TeamCherry.NestedFadeGroup;
using UnityEngine;

// Token: 0x020006BD RID: 1725
[DefaultExecutionOrder(1)]
public class InventoryToolCrest : InventoryItemSelectableDirectional
{
	// Token: 0x17000724 RID: 1828
	// (get) Token: 0x06003E66 RID: 15974 RVA: 0x00112402 File Offset: 0x00110602
	public override string DisplayName
	{
		get
		{
			return this.displayName;
		}
	}

	// Token: 0x17000725 RID: 1829
	// (get) Token: 0x06003E67 RID: 15975 RVA: 0x0011240F File Offset: 0x0011060F
	public override string Description
	{
		get
		{
			return this.description;
		}
	}

	// Token: 0x17000726 RID: 1830
	// (get) Token: 0x06003E68 RID: 15976 RVA: 0x0011241C File Offset: 0x0011061C
	public bool IsUnlocked
	{
		get
		{
			return this.CrestData && this.CrestData.IsVisible;
		}
	}

	// Token: 0x17000727 RID: 1831
	// (get) Token: 0x06003E69 RID: 15977 RVA: 0x00112438 File Offset: 0x00110638
	public bool IsHidden
	{
		get
		{
			return this.CrestData && this.CrestData.IsHidden;
		}
	}

	// Token: 0x17000728 RID: 1832
	// (get) Token: 0x06003E6A RID: 15978 RVA: 0x00112454 File Offset: 0x00110654
	// (set) Token: 0x06003E6B RID: 15979 RVA: 0x0011245C File Offset: 0x0011065C
	public ToolCrest CrestData { get; private set; }

	// Token: 0x06003E6C RID: 15980 RVA: 0x00112465 File Offset: 0x00110665
	protected override void OnValidate()
	{
		base.OnValidate();
		ArrayForEnumAttribute.EnsureArraySize<InventoryToolCrestSlot>(ref this.templateSlots, typeof(ToolItemType));
	}

	// Token: 0x06003E6D RID: 15981 RVA: 0x00112482 File Offset: 0x00110682
	protected override void Awake()
	{
		base.Awake();
		this.manager = base.GetComponentInParent<InventoryItemToolManager>();
		if (this.newIndicator)
		{
			this.newIndicatorInitialScale = this.newIndicator.transform.localScale;
		}
	}

	// Token: 0x06003E6E RID: 15982 RVA: 0x001124B9 File Offset: 0x001106B9
	protected override void OnDisable()
	{
		base.OnDisable();
		if (this.equipRoutine != null)
		{
			base.StopCoroutine(this.equipRoutine);
		}
	}

	// Token: 0x06003E6F RID: 15983 RVA: 0x001124D8 File Offset: 0x001106D8
	public void Setup(ToolCrest newCrestData)
	{
		this.CrestData = newCrestData;
		base.gameObject.name = (newCrestData ? newCrestData.name : "Spare Crest");
		if (this.crestSubmitAnimator && this.crestSubmitAnimator.isActiveAndEnabled)
		{
			this.crestSubmitAnimator.Play(InventoryToolCrest._inertAnim);
		}
		if (newCrestData)
		{
			this.displayName = newCrestData.DisplayName;
			this.description = newCrestData.Description;
			if (this.crestSprite)
			{
				this.crestSprite.Sprite = newCrestData.CrestSprite;
			}
			if (this.crestSilhouette)
			{
				this.crestSilhouette.Sprite = newCrestData.CrestSilhouette;
			}
			if (this.crestGlowSprite)
			{
				this.crestGlowSprite.sprite = newCrestData.CrestGlow;
			}
			GameObject displayPrefab = newCrestData.DisplayPrefab;
			if (displayPrefab)
			{
				GameObject gameObject;
				GameObject gameObject2;
				if (this.spawnedDisplayObjects.TryGetValue(displayPrefab, out gameObject))
				{
					gameObject2 = gameObject;
				}
				else
				{
					gameObject2 = Object.Instantiate<GameObject>(displayPrefab, base.transform);
					gameObject2.transform.localPosition = Vector3.zero;
					this.spawnedDisplayObjects[displayPrefab] = gameObject2;
				}
				if (this.activeDisplayObject && this.activeDisplayObject != gameObject2)
				{
					this.activeDisplayObject.SetActive(false);
				}
				gameObject2.SetActive(true);
				this.activeDisplayObject = gameObject2;
			}
			if (this.spawnedSlots == null)
			{
				this.spawnedSlots = new Dictionary<ToolItemType, List<InventoryToolCrestSlot>>();
				foreach (ToolItemType key in InventoryToolCrest.TOOL_TYPES)
				{
					this.spawnedSlots[key] = new List<InventoryToolCrestSlot>();
				}
			}
			if (this.spawnedSlotsRemaining == null)
			{
				this.spawnedSlotsRemaining = new Dictionary<ToolItemType, Queue<InventoryToolCrestSlot>>();
				foreach (ToolItemType key2 in InventoryToolCrest.TOOL_TYPES)
				{
					this.spawnedSlotsRemaining[key2] = new Queue<InventoryToolCrestSlot>();
				}
			}
			ToolItemType[] tool_TYPES = InventoryToolCrest.TOOL_TYPES;
			for (int i = 0; i < tool_TYPES.Length; i++)
			{
				ToolItemType type = tool_TYPES[i];
				int num = newCrestData.Slots.Count((ToolCrest.SlotInfo slotData) => slotData.Type == type);
				int count = this.spawnedSlots[type].Count;
				int j = num - count;
				InventoryToolCrestSlot inventoryToolCrestSlot = this.templateSlots[(int)type];
				inventoryToolCrestSlot.gameObject.SetActive(false);
				while (j > 0)
				{
					InventoryToolCrestSlot inventoryToolCrestSlot2 = Object.Instantiate<InventoryToolCrestSlot>(inventoryToolCrestSlot, inventoryToolCrestSlot.transform.parent);
					this.spawnedSlots[type].Add(inventoryToolCrestSlot2);
					inventoryToolCrestSlot2.OnSetEquipSaved += this.SaveEquips;
					j--;
				}
				this.spawnedSlotsRemaining[type].Clear();
				foreach (InventoryToolCrestSlot inventoryToolCrestSlot3 in this.spawnedSlots[type])
				{
					inventoryToolCrestSlot3.gameObject.SetActive(false);
					this.spawnedSlotsRemaining[type].Enqueue(inventoryToolCrestSlot3);
				}
			}
			this.activeSlots.Clear();
			this.activeSlotsData.Clear();
			for (int k = 0; k < newCrestData.Slots.Length; k++)
			{
				ToolCrest.SlotInfo slotInfo = newCrestData.Slots[k];
				InventoryToolCrestSlot inventoryToolCrestSlot4 = this.spawnedSlotsRemaining[slotInfo.Type].Dequeue();
				inventoryToolCrestSlot4.gameObject.SetActive(true);
				inventoryToolCrestSlot4.SetCrestInfo(this, k, null, null);
				inventoryToolCrestSlot4.transform.SetLocalPosition2D(slotInfo.Position);
				this.activeSlots.Add(inventoryToolCrestSlot4);
				this.activeSlotsData.Add(slotInfo);
			}
		}
		for (int l = 0; l < this.activeSlots.Count; l++)
		{
			InventoryToolCrestSlot inventoryToolCrestSlot5 = this.activeSlots[l];
			ToolCrest.SlotInfo slotInfo2 = this.activeSlotsData[l];
			inventoryToolCrestSlot5.Selectables[0] = this.GetActiveSlot(slotInfo2.NavUpIndex);
			inventoryToolCrestSlot5.Selectables[1] = this.GetActiveSlot(slotInfo2.NavDownIndex);
			inventoryToolCrestSlot5.Selectables[2] = this.GetActiveSlot(slotInfo2.NavLeftIndex);
			inventoryToolCrestSlot5.Selectables[3] = this.GetActiveSlot(slotInfo2.NavRightIndex);
			this.SetListSlotIndex(inventoryToolCrestSlot5.FallbackSelectables[0].Selectables, slotInfo2.NavUpFallbackIndex);
			this.SetListSlotIndex(inventoryToolCrestSlot5.FallbackSelectables[1].Selectables, slotInfo2.NavDownFallbackIndex);
			this.SetListSlotIndex(inventoryToolCrestSlot5.FallbackSelectables[2].Selectables, slotInfo2.NavLeftFallbackIndex);
			this.SetListSlotIndex(inventoryToolCrestSlot5.FallbackSelectables[3].Selectables, slotInfo2.NavRightFallbackIndex);
			inventoryToolCrestSlot5.SlotInfo = slotInfo2;
		}
		foreach (InventoryToolCrestSlot target in this.activeSlots)
		{
			InventoryItemManager.PropagateSelectables(this, target);
		}
		this.isNew = (!newCrestData.IsHidden && newCrestData.SaveData.DisplayNewIndicator);
	}

	// Token: 0x06003E70 RID: 15984 RVA: 0x00112A20 File Offset: 0x00110C20
	private InventoryToolCrestSlot GetActiveSlot(int index)
	{
		if (index < 0)
		{
			return null;
		}
		if (index >= this.activeSlots.Count)
		{
			Debug.LogError("Crest slot index out of range!", this);
			return null;
		}
		return this.activeSlots[index];
	}

	// Token: 0x06003E71 RID: 15985 RVA: 0x00112A4F File Offset: 0x00110C4F
	private void SetListSlotIndex(List<InventoryItemSelectable> selectables, int slotIndex)
	{
		selectables.Clear();
		if (slotIndex < 0)
		{
			return;
		}
		if (slotIndex >= this.activeSlots.Count)
		{
			Debug.LogError("Crest slot index out of range!", this);
			return;
		}
		selectables.Add(this.activeSlots[slotIndex]);
	}

	// Token: 0x06003E72 RID: 15986 RVA: 0x00112A88 File Offset: 0x00110C88
	public void GetEquippedForSlots()
	{
		List<ToolItem> equippedToolsForCrest = ToolItemManager.GetEquippedToolsForCrest(base.gameObject.name);
		if (equippedToolsForCrest == null)
		{
			return;
		}
		for (int i = 0; i < Mathf.Min(equippedToolsForCrest.Count, this.activeSlots.Count); i++)
		{
			this.activeSlots[i].SetEquipped(equippedToolsForCrest[i], false, false);
		}
	}

	// Token: 0x06003E73 RID: 15987 RVA: 0x00112AE8 File Offset: 0x00110CE8
	private void SaveEquips()
	{
		ToolItemManager.SetEquippedTools(base.gameObject.name, this.activeSlots.Select(delegate(InventoryToolCrestSlot slot)
		{
			if (!slot.EquippedItem)
			{
				return null;
			}
			return slot.EquippedItem.name;
		}).ToList<string>());
	}

	// Token: 0x06003E74 RID: 15988 RVA: 0x00112B34 File Offset: 0x00110D34
	public override InventoryItemSelectable Get(InventoryItemManager.SelectionDirection? direction)
	{
		if (this.activeSlots.Count <= 0)
		{
			return null;
		}
		if (direction == null)
		{
			return this.activeSlots[0].Get(null);
		}
		if (this.manager && this.manager.CurrentSelected)
		{
			InventoryToolCrestSlot closestOnAxis = InventoryItemNavigationHelper.GetClosestOnAxis<InventoryToolCrestSlot>(direction.Value, this.manager.CurrentSelected, this.activeSlots);
			if (closestOnAxis)
			{
				return closestOnAxis.Get(direction);
			}
		}
		return this.activeSlots[0].Get(direction);
	}

	// Token: 0x06003E75 RID: 15989 RVA: 0x00112BD4 File Offset: 0x00110DD4
	public bool HasSlot(ToolItemType type)
	{
		foreach (InventoryToolCrestSlot inventoryToolCrestSlot in this.activeSlots)
		{
			if (inventoryToolCrestSlot.Type == type && !inventoryToolCrestSlot.IsLocked)
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x06003E76 RID: 15990 RVA: 0x00112C38 File Offset: 0x00110E38
	public bool HasAnySlots()
	{
		using (List<InventoryToolCrestSlot>.Enumerator enumerator = this.activeSlots.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (!enumerator.Current.IsLocked)
				{
					return true;
				}
			}
		}
		return false;
	}

	// Token: 0x06003E77 RID: 15991 RVA: 0x00112C94 File Offset: 0x00110E94
	public bool HasSlot(InventoryToolCrestSlot otherSlot)
	{
		using (List<InventoryToolCrestSlot>.Enumerator enumerator = this.activeSlots.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current == otherSlot)
				{
					return true;
				}
			}
		}
		return false;
	}

	// Token: 0x06003E78 RID: 15992 RVA: 0x00112CF0 File Offset: 0x00110EF0
	public InventoryToolCrestSlot GetEquippedToolSlot(ToolItem toolItem)
	{
		return this.activeSlots.FirstOrDefault((InventoryToolCrestSlot slot) => slot.EquippedItem == toolItem);
	}

	// Token: 0x06003E79 RID: 15993 RVA: 0x00112D21 File Offset: 0x00110F21
	public IEnumerable<InventoryToolCrestSlot> GetSlots()
	{
		return this.activeSlots;
	}

	// Token: 0x06003E7A RID: 15994 RVA: 0x00112D2C File Offset: 0x00110F2C
	public float Show(bool value, bool isInstant)
	{
		if (!this.fadeGroup)
		{
			this.fadeGroup = base.GetComponent<NestedFadeGroupBase>();
		}
		float result = isInstant ? 0f : this.fadeTime;
		if (!this.fadeGroup)
		{
			return result;
		}
		return this.fadeGroup.FadeTo((float)(value ? 1 : 0), result, null, true, null);
	}

	// Token: 0x06003E7B RID: 15995 RVA: 0x00112D8C File Offset: 0x00110F8C
	public void UpdateListDisplay(bool isInstant = false)
	{
		if (!this.crestList)
		{
			this.crestList = base.GetComponentInParent<InventoryToolCrestList>();
			this.defaultScale = base.transform.localScale;
		}
		bool flag = this.crestList.CurrentCrest == this;
		if (!this.IsUnlocked)
		{
			if (flag)
			{
				if (!base.gameObject.activeSelf)
				{
					base.gameObject.SetActive(true);
				}
			}
			else if (base.gameObject.activeSelf)
			{
				base.gameObject.SetActive(false);
			}
		}
		if (this.crestList)
		{
			Color newColor;
			switch (this.manager.EquipState)
			{
			case InventoryItemToolManager.EquipStates.None:
				newColor = InventoryToolCrest.DeselectedColor;
				break;
			case InventoryItemToolManager.EquipStates.PlaceTool:
			case InventoryItemToolManager.EquipStates.SelectTool:
				newColor = InventoryToolCrestSlot.InvalidItemColor;
				break;
			case InventoryItemToolManager.EquipStates.SwitchCrest:
				newColor = ((this.crestList.CurrentCrest == this) ? Color.white : InventoryToolCrest.DeselectedColor);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			Vector3 newScale = (this.crestList.CurrentCrest == this) ? this.defaultScale : this.deselectedScale;
			newScale.z = 1f;
			if (this.transitionRoutine != null)
			{
				base.StopCoroutine(this.transitionRoutine);
			}
			if (!isInstant)
			{
				this.transitionRoutine = base.StartCoroutine(this.TransitionDisplayState(newColor, newScale, flag, false));
			}
			else
			{
				this.TransitionDisplayState(newColor, newScale, flag, true).MoveNext();
			}
		}
		if (this.isNew)
		{
			if (flag)
			{
				if (this.crestList.IsSwitchingCrests)
				{
					ToolCrestsData.Data saveData = this.CrestData.SaveData;
					saveData.DisplayNewIndicator = false;
					this.CrestData.SaveData = saveData;
					this.isNew = false;
					if (this.newIndicator && this.newIndicator.activeSelf)
					{
						this.newIndicator.transform.ScaleTo(this, Vector3.zero, UI.NewDotScaleTime, 0f, false, true, null);
						return;
					}
				}
				else if (this.newIndicator)
				{
					this.newIndicator.SetActive(false);
					return;
				}
			}
			else if (this.newIndicator)
			{
				this.newIndicator.SetActive(true);
				this.newIndicator.transform.localScale = this.newIndicatorInitialScale;
				return;
			}
		}
		else if (this.newIndicator)
		{
			this.newIndicator.SetActive(false);
		}
	}

	// Token: 0x06003E7C RID: 15996 RVA: 0x00112FE5 File Offset: 0x001111E5
	private IEnumerator TransitionDisplayState(Color newColor, Vector3 newScale, bool isCurrentCrest, bool isInstant)
	{
		InventoryToolCrest.<>c__DisplayClass61_0 CS$<>8__locals1;
		CS$<>8__locals1.<>4__this = this;
		CS$<>8__locals1.newColor = newColor;
		CS$<>8__locals1.newScale = newScale;
		CS$<>8__locals1.startColor = (this.crestSprite ? this.crestSprite.Color : Color.white);
		CS$<>8__locals1.startScale = base.transform.localScale;
		CS$<>8__locals1.oldFlashAmount = (float)(this.wasCurrentCrest ? 0 : 1);
		CS$<>8__locals1.newFlashAmount = (float)(isCurrentCrest ? 0 : 1);
		CS$<>8__locals1.slotScaleStart = (this.wasCurrentCrest ? InventoryToolCrest._slotScaleCurrent : InventoryToolCrest._slotScaleOther);
		CS$<>8__locals1.slotScaleEnd = (isCurrentCrest ? InventoryToolCrest._slotScaleCurrent : InventoryToolCrest._slotScaleOther);
		this.wasCurrentCrest = isCurrentCrest;
		if (!isInstant)
		{
			for (float elapsed = 0f; elapsed < this.lerpTime; elapsed += Time.unscaledDeltaTime)
			{
				this.<TransitionDisplayState>g__SetLerpedValues|61_0(elapsed / this.lerpTime, ref CS$<>8__locals1);
				yield return null;
			}
		}
		this.<TransitionDisplayState>g__SetLerpedValues|61_0(1f, ref CS$<>8__locals1);
		yield break;
	}

	// Token: 0x06003E7D RID: 15997 RVA: 0x00113011 File Offset: 0x00111211
	public void DoEquip(Action onEquip)
	{
		if (!this.crestSubmitAnimator || !this.crestSubmitAnimator.isActiveAndEnabled)
		{
			onEquip();
			return;
		}
		this.equipRoutine = base.StartCoroutine(this.DoEquipAnim(onEquip));
	}

	// Token: 0x06003E7E RID: 15998 RVA: 0x00113047 File Offset: 0x00111247
	private IEnumerator DoEquipAnim(Action onEquip)
	{
		this.crestSubmitAudio.SpawnAndPlayOneShot(Audio.DefaultUIAudioSourcePrefab, base.transform.position, null);
		this.crestSubmitAnimator.Play(InventoryToolCrest._burstAnim, 0, 0f);
		yield return null;
		if (this.crestSubmitAnimator.updateMode == AnimatorUpdateMode.UnscaledTime)
		{
			yield return new WaitForSecondsRealtime(this.crestSubmitAnimator.GetCurrentAnimatorStateInfo(0).length);
		}
		else
		{
			yield return new WaitForSeconds(this.crestSubmitAnimator.GetCurrentAnimatorStateInfo(0).length);
		}
		onEquip();
		yield break;
	}

	// Token: 0x06003E81 RID: 16001 RVA: 0x00113154 File Offset: 0x00111354
	[CompilerGenerated]
	private void <TransitionDisplayState>g__SetLerpedValues|61_0(float time, ref InventoryToolCrest.<>c__DisplayClass61_0 A_2)
	{
		float num = Mathf.Lerp(A_2.oldFlashAmount, A_2.newFlashAmount, time);
		float a = 1f - num;
		if (this.crestSprite)
		{
			Color color = Color.Lerp(A_2.startColor, A_2.newColor, time);
			color.a = a;
			this.crestSprite.Color = color;
		}
		if (this.crestSilhouette)
		{
			this.crestSilhouette.AlphaSelf = num;
		}
		base.transform.localScale = Vector3.Lerp(A_2.startScale, A_2.newScale, time);
		foreach (InventoryToolCrestSlot inventoryToolCrestSlot in this.activeSlots)
		{
			inventoryToolCrestSlot.ItemFlashAmount = num;
			inventoryToolCrestSlot.transform.localScale = Vector3.Lerp(A_2.slotScaleStart, A_2.slotScaleEnd, time);
		}
	}

	// Token: 0x04003FF3 RID: 16371
	private static readonly Vector3 _slotScaleCurrent = new Vector3(1f, 1f, 1f);

	// Token: 0x04003FF4 RID: 16372
	private static readonly Vector3 _slotScaleOther = new Vector3(0.7f, 0.7f, 1f);

	// Token: 0x04003FF5 RID: 16373
	public static readonly Color DeselectedColor = new Color(0.5f, 0.5f, 0.5f, 1f);

	// Token: 0x04003FF6 RID: 16374
	[Space]
	[SerializeField]
	private LocalisedString displayName;

	// Token: 0x04003FF7 RID: 16375
	[SerializeField]
	private LocalisedString description;

	// Token: 0x04003FF8 RID: 16376
	[Space]
	[SerializeField]
	private NestedFadeGroupSpriteRenderer crestSprite;

	// Token: 0x04003FF9 RID: 16377
	[SerializeField]
	private NestedFadeGroupSpriteRenderer crestSilhouette;

	// Token: 0x04003FFA RID: 16378
	[SerializeField]
	private float lerpTime = 0.2f;

	// Token: 0x04003FFB RID: 16379
	[SerializeField]
	private float fadeTime = 0.2f;

	// Token: 0x04003FFC RID: 16380
	private NestedFadeGroupBase fadeGroup;

	// Token: 0x04003FFD RID: 16381
	[SerializeField]
	private Vector2 deselectedScale = new Vector2(0.5f, 0.5f);

	// Token: 0x04003FFE RID: 16382
	private Vector2 defaultScale;

	// Token: 0x04003FFF RID: 16383
	[Space]
	[SerializeField]
	private SpriteRenderer crestGlowSprite;

	// Token: 0x04004000 RID: 16384
	[SerializeField]
	private Animator crestSubmitAnimator;

	// Token: 0x04004001 RID: 16385
	[SerializeField]
	private AudioEvent crestSubmitAudio;

	// Token: 0x04004002 RID: 16386
	[Space]
	[SerializeField]
	[ArrayForEnum(typeof(ToolItemType))]
	private InventoryToolCrestSlot[] templateSlots;

	// Token: 0x04004003 RID: 16387
	[SerializeField]
	private GameObject newIndicator;

	// Token: 0x04004004 RID: 16388
	private Dictionary<ToolItemType, List<InventoryToolCrestSlot>> spawnedSlots;

	// Token: 0x04004005 RID: 16389
	private Dictionary<ToolItemType, Queue<InventoryToolCrestSlot>> spawnedSlotsRemaining;

	// Token: 0x04004006 RID: 16390
	private readonly List<ToolCrest.SlotInfo> activeSlotsData = new List<ToolCrest.SlotInfo>();

	// Token: 0x04004007 RID: 16391
	private readonly List<InventoryToolCrestSlot> activeSlots = new List<InventoryToolCrestSlot>();

	// Token: 0x04004008 RID: 16392
	private Coroutine transitionRoutine;

	// Token: 0x04004009 RID: 16393
	private bool wasCurrentCrest;

	// Token: 0x0400400A RID: 16394
	private Vector3 newIndicatorInitialScale;

	// Token: 0x0400400B RID: 16395
	private bool isNew;

	// Token: 0x0400400C RID: 16396
	private GameObject activeDisplayObject;

	// Token: 0x0400400D RID: 16397
	private readonly Dictionary<GameObject, GameObject> spawnedDisplayObjects = new Dictionary<GameObject, GameObject>();

	// Token: 0x0400400E RID: 16398
	private InventoryToolCrestList crestList;

	// Token: 0x0400400F RID: 16399
	private InventoryItemToolManager manager;

	// Token: 0x04004010 RID: 16400
	private Coroutine equipRoutine;

	// Token: 0x04004011 RID: 16401
	private static readonly int _inertAnim = Animator.StringToHash("Inert");

	// Token: 0x04004012 RID: 16402
	private static readonly int _burstAnim = Animator.StringToHash("Burst");

	// Token: 0x04004013 RID: 16403
	private static ToolItemType[] TOOL_TYPES = (ToolItemType[])Enum.GetValues(typeof(ToolItemType));
}
