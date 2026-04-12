using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000677 RID: 1655
public class HudGlobalHide : MonoBehaviour
{
	// Token: 0x170006B8 RID: 1720
	// (get) Token: 0x06003B90 RID: 15248 RVA: 0x001069AE File Offset: 0x00104BAE
	// (set) Token: 0x06003B91 RID: 15249 RVA: 0x001069B8 File Offset: 0x00104BB8
	public static bool IsHidden
	{
		get
		{
			return HudGlobalHide._isHidden;
		}
		set
		{
			HudGlobalHide._isHidden = value;
			foreach (HudGlobalHide hudGlobalHide in HudGlobalHide._activeObjs)
			{
				hudGlobalHide.UpdateLocation();
			}
		}
	}

	// Token: 0x170006B9 RID: 1721
	// (get) Token: 0x06003B92 RID: 15250 RVA: 0x00106A10 File Offset: 0x00104C10
	// (set) Token: 0x06003B93 RID: 15251 RVA: 0x00106A18 File Offset: 0x00104C18
	public static bool IsReduced
	{
		get
		{
			return HudGlobalHide._isReduced;
		}
		set
		{
			HudGlobalHide._isReduced = value;
			foreach (HudGlobalHide hudGlobalHide in HudGlobalHide._activeObjs)
			{
				hudGlobalHide.UpdateLocation();
			}
		}
	}

	// Token: 0x06003B94 RID: 15252 RVA: 0x00106A70 File Offset: 0x00104C70
	private void OnEnable()
	{
		HudGlobalHide._activeObjs.Add(this);
		this.UpdateLocation();
	}

	// Token: 0x06003B95 RID: 15253 RVA: 0x00106A84 File Offset: 0x00104C84
	private void OnDisable()
	{
		HudGlobalHide._activeObjs.Remove(this);
		if (HudGlobalHide._activeObjs.Count == 0)
		{
			HudGlobalHide._isHidden = false;
		}
	}

	// Token: 0x06003B96 RID: 15254 RVA: 0x00106AA4 File Offset: 0x00104CA4
	private void UpdateLocation()
	{
		Transform transform = base.transform;
		if (HudGlobalHide._isHidden)
		{
			transform.localPosition = new Vector3(0f, -200f, 0f);
			return;
		}
		if (HudGlobalHide._isReduced)
		{
			transform.localPosition = this.reducedPos;
			transform.localScale = this.reducedScale.ToVector3(1f);
			return;
		}
		transform.localPosition = Vector3.zero;
		transform.localScale = Vector3.one;
	}

	// Token: 0x04003DC4 RID: 15812
	[SerializeField]
	private Vector2 reducedPos;

	// Token: 0x04003DC5 RID: 15813
	[SerializeField]
	private Vector2 reducedScale;

	// Token: 0x04003DC6 RID: 15814
	private static readonly HashSet<HudGlobalHide> _activeObjs = new HashSet<HudGlobalHide>();

	// Token: 0x04003DC7 RID: 15815
	private static bool _isHidden;

	// Token: 0x04003DC8 RID: 15816
	private static bool _isReduced;
}
