using System;
using GlobalSettings;
using TeamCherry.NestedFadeGroup;
using UnityEngine;

// Token: 0x02000680 RID: 1664
public class CrestSocketUnlockInventoryDescription : MonoBehaviour
{
	// Token: 0x06003BBF RID: 15295 RVA: 0x0010714F File Offset: 0x0010534F
	private void Awake()
	{
		this.leftLockInitialPosition = this.leftLock.transform.localPosition;
		this.rightLockInitialPosition = this.rightLock.transform.localPosition;
	}

	// Token: 0x06003BC0 RID: 15296 RVA: 0x00107188 File Offset: 0x00105388
	public void SetSlotSprite(Sprite sprite, Color color)
	{
		this.slotIcon.Sprite = sprite;
		this.slotIcon.Color = color;
		this.leftLock.Color = color;
		this.leftLockGlow.BaseColor = color;
		this.rightLock.Color = color;
		this.rightLockGlow.BaseColor = color;
		this.SetConsumeShakeAmount(0f);
	}

	// Token: 0x06003BC1 RID: 15297 RVA: 0x001071E8 File Offset: 0x001053E8
	public void StartConsume()
	{
		this.CancelConsume();
		AudioSource spawnedSource = null;
		spawnedSource = (this.spawnedConsumeAudio = this.consumeAudio.SpawnAndPlayOneShot(Audio.DefaultUIAudioSourcePrefab, base.transform.position, delegate()
		{
			if (this.spawnedConsumeAudio != spawnedSource)
			{
				return;
			}
			this.spawnedConsumeAudio = null;
		}));
	}

	// Token: 0x06003BC2 RID: 15298 RVA: 0x00107245 File Offset: 0x00105445
	public void CancelConsume()
	{
		if (!this.spawnedConsumeAudio)
		{
			return;
		}
		this.spawnedConsumeAudio.Stop();
		this.spawnedConsumeAudio = null;
	}

	// Token: 0x06003BC3 RID: 15299 RVA: 0x00107267 File Offset: 0x00105467
	public void ConsumeCompleted()
	{
		this.spawnedConsumeAudio = null;
	}

	// Token: 0x06003BC4 RID: 15300 RVA: 0x00107270 File Offset: 0x00105470
	public void SetConsumeShakeAmount(float t)
	{
		this.leftLockGlow.AlphaSelf = t;
		this.rightLockGlow.AlphaSelf = t;
		Vector2 b = Random.insideUnitCircle * (t * this.lockJitterMagnitude);
		float num = this.lockMoveXCurve.Evaluate(t) * this.lockMoveX;
		this.leftLock.transform.SetLocalPosition2D(this.leftLockInitialPosition + b + new Vector2(-num, 0f));
		this.rightLock.transform.SetLocalPosition2D(this.rightLockInitialPosition + b + new Vector2(num, 0f));
	}

	// Token: 0x04003DEC RID: 15852
	[SerializeField]
	private NestedFadeGroupSpriteRenderer slotIcon;

	// Token: 0x04003DED RID: 15853
	[SerializeField]
	private NestedFadeGroupSpriteRenderer leftLock;

	// Token: 0x04003DEE RID: 15854
	[SerializeField]
	private NestedFadeGroupSpriteRenderer leftLockGlow;

	// Token: 0x04003DEF RID: 15855
	[SerializeField]
	private NestedFadeGroupSpriteRenderer rightLock;

	// Token: 0x04003DF0 RID: 15856
	[SerializeField]
	private NestedFadeGroupSpriteRenderer rightLockGlow;

	// Token: 0x04003DF1 RID: 15857
	[Space]
	[SerializeField]
	private float lockMoveX;

	// Token: 0x04003DF2 RID: 15858
	[SerializeField]
	private AnimationCurve lockMoveXCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	// Token: 0x04003DF3 RID: 15859
	[SerializeField]
	private float lockJitterMagnitude;

	// Token: 0x04003DF4 RID: 15860
	[Space]
	[SerializeField]
	private AudioEvent consumeAudio;

	// Token: 0x04003DF5 RID: 15861
	private AudioSource spawnedConsumeAudio;

	// Token: 0x04003DF6 RID: 15862
	private Vector2 leftLockInitialPosition;

	// Token: 0x04003DF7 RID: 15863
	private Vector2 rightLockInitialPosition;
}
