using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x020005E0 RID: 1504
public abstract class RadialHudIcon : MonoBehaviour
{
	// Token: 0x06003595 RID: 13717 RVA: 0x000EDB96 File Offset: 0x000EBD96
	private void Start()
	{
		if (!this.updated)
		{
			this.UpdateDisplay();
		}
	}

	// Token: 0x06003596 RID: 13718 RVA: 0x000EDBA8 File Offset: 0x000EBDA8
	private void Update()
	{
		if (this.radialLerpRoutine != null)
		{
			return;
		}
		float targetFillAmount = this.GetTargetFillAmount();
		if (Math.Abs(targetFillAmount - this.previousFillAmount) <= Mathf.Epsilon)
		{
			return;
		}
		this.SetFillAmount(targetFillAmount);
	}

	// Token: 0x06003597 RID: 13719 RVA: 0x000EDBE4 File Offset: 0x000EBDE4
	private float GetTargetFillAmount()
	{
		if (this.storageAmount <= 0)
		{
			return 1f;
		}
		float num = (float)this.amountLeft / (float)this.storageAmount;
		float midProgress = this.GetMidProgress();
		if (midProgress <= Mathf.Epsilon)
		{
			return num;
		}
		float b = (float)Mathf.Clamp(this.amountLeft - 1, 0, this.storageAmount) / (float)this.storageAmount;
		return Mathf.Lerp(num, b, midProgress);
	}

	// Token: 0x06003598 RID: 13720 RVA: 0x000EDC48 File Offset: 0x000EBE48
	private void SetFillAmount(float value)
	{
		if (this.radialImage)
		{
			this.radialImage.fillAmount = value;
		}
		if (this.radialImageBg)
		{
			this.radialImageBg.fillAmount = 1f - value;
		}
		this.previousFillAmount = value;
	}

	// Token: 0x06003599 RID: 13721 RVA: 0x000EDC94 File Offset: 0x000EBE94
	protected virtual void OnPreUpdateDisplay()
	{
	}

	// Token: 0x0600359A RID: 13722
	protected abstract bool GetIsActive();

	// Token: 0x0600359B RID: 13723
	protected abstract void GetAmounts(out int amountLeft, out int totalCount);

	// Token: 0x0600359C RID: 13724
	protected abstract bool TryGetHudSprite(out Sprite sprite);

	// Token: 0x0600359D RID: 13725
	public abstract bool GetIsEmpty();

	// Token: 0x0600359E RID: 13726
	protected abstract bool HasTargetChanged();

	// Token: 0x0600359F RID: 13727 RVA: 0x000EDC96 File Offset: 0x000EBE96
	protected virtual bool TryGetBarColour(out Color color)
	{
		color = Color.black;
		return false;
	}

	// Token: 0x060035A0 RID: 13728 RVA: 0x000EDCA4 File Offset: 0x000EBEA4
	protected virtual float GetMidProgress()
	{
		return 0f;
	}

	// Token: 0x060035A1 RID: 13729 RVA: 0x000EDCAC File Offset: 0x000EBEAC
	protected void UpdateDisplay()
	{
		this.updated = true;
		this.OnPreUpdateDisplay();
		if (!this.GetIsActive())
		{
			base.gameObject.SetActive(false);
			return;
		}
		base.gameObject.SetActive(true);
		this.GetAmounts(out this.amountLeft, out this.storageAmount);
		bool isEmpty = this.GetIsEmpty();
		if (this.icon)
		{
			Sprite sprite;
			if (this.TryGetHudSprite(out sprite))
			{
				this.icon.sprite = sprite;
				this.icon.transform.localScale = new Vector3(1.4f, 1.4f, 1f);
			}
			else
			{
				this.icon.sprite = sprite;
				this.icon.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
			}
			this.SetIconColour(this.icon, isEmpty ? this.inactiveColor : this.activeColor);
		}
		if (this.radialImage)
		{
			Color color;
			if (!this.TryGetBarColour(out color))
			{
				color = Color.white;
			}
			float targetFillAmount;
			if (this.storageAmount > 0)
			{
				targetFillAmount = this.GetTargetFillAmount();
			}
			else
			{
				targetFillAmount = 1f;
				if (isEmpty)
				{
					color = color.MultiplyElements(this.inactiveColor);
				}
			}
			this.radialImage.color = color;
			if (this.radialLerpRoutine != null)
			{
				base.StopCoroutine(this.radialLerpRoutine);
			}
			if (!this.HasTargetChanged())
			{
				float initialFillAmount = this.radialImage.fillAmount;
				float duration = (targetFillAmount > initialFillAmount) ? this.radialLerpUpTime : this.radialLerpDownTime;
				this.radialLerpRoutine = this.StartTimerRoutine(0f, duration, delegate(float time)
				{
					this.SetFillAmount(Mathf.Lerp(initialFillAmount, targetFillAmount, this.radialLerpCurve.Evaluate(time)));
				}, null, delegate
				{
					this.radialLerpRoutine = null;
				}, false);
			}
			else
			{
				this.SetFillAmount(targetFillAmount);
			}
		}
		if (!this.templateNotch)
		{
			return;
		}
		this.templateNotch.SetActive(false);
		for (int i = this.storageAmount - this.notches.Count; i > 0; i--)
		{
			GameObject item = Object.Instantiate<GameObject>(this.templateNotch, this.templateNotch.transform.parent);
			this.notches.Add(item);
		}
		for (int j = 0; j < this.notches.Count; j++)
		{
			this.notches[j].SetActive(j < this.storageAmount);
		}
	}

	// Token: 0x060035A2 RID: 13730 RVA: 0x000EDF25 File Offset: 0x000EC125
	protected virtual void SetIconColour(SpriteRenderer spriteRenderer, Color color)
	{
		spriteRenderer.color = color;
	}

	// Token: 0x040038E6 RID: 14566
	[SerializeField]
	private SpriteRenderer icon;

	// Token: 0x040038E7 RID: 14567
	[SerializeField]
	protected Color activeColor;

	// Token: 0x040038E8 RID: 14568
	[SerializeField]
	protected Color inactiveColor;

	// Token: 0x040038E9 RID: 14569
	[Space]
	[SerializeField]
	private Image radialImage;

	// Token: 0x040038EA RID: 14570
	[SerializeField]
	private Image radialImageBg;

	// Token: 0x040038EB RID: 14571
	[SerializeField]
	private float radialLerpDownTime;

	// Token: 0x040038EC RID: 14572
	[SerializeField]
	private float radialLerpUpTime;

	// Token: 0x040038ED RID: 14573
	[SerializeField]
	private AnimationCurve radialLerpCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	// Token: 0x040038EE RID: 14574
	[SerializeField]
	private GameObject templateNotch;

	// Token: 0x040038EF RID: 14575
	private readonly List<GameObject> notches = new List<GameObject>();

	// Token: 0x040038F0 RID: 14576
	private Coroutine radialLerpRoutine;

	// Token: 0x040038F1 RID: 14577
	private int amountLeft;

	// Token: 0x040038F2 RID: 14578
	private int storageAmount;

	// Token: 0x040038F3 RID: 14579
	private float previousFillAmount;

	// Token: 0x040038F4 RID: 14580
	private bool updated;
}
