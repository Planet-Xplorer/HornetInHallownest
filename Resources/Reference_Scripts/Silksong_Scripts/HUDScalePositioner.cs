using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000635 RID: 1589
public sealed class HudScalePositioner : MonoBehaviour
{
	// Token: 0x17000678 RID: 1656
	// (get) Token: 0x060038E3 RID: 14563 RVA: 0x000FB7F2 File Offset: 0x000F99F2
	// (set) Token: 0x060038E4 RID: 14564 RVA: 0x000FB7FC File Offset: 0x000F99FC
	public static bool IsReduced
	{
		get
		{
			return HudScalePositioner._isReduced;
		}
		set
		{
			HudScalePositioner._isReduced = value;
			foreach (HudScalePositioner hudScalePositioner in HudScalePositioner._activeObjs)
			{
				hudScalePositioner.UpdatePosition();
			}
		}
	}

	// Token: 0x060038E5 RID: 14565 RVA: 0x000FB854 File Offset: 0x000F9A54
	private void OnEnable()
	{
		this.UpdatePosition();
		HudScalePositioner._activeObjs.Add(this);
	}

	// Token: 0x060038E6 RID: 14566 RVA: 0x000FB868 File Offset: 0x000F9A68
	private void OnDisable()
	{
		HudScalePositioner._activeObjs.Remove(this);
	}

	// Token: 0x060038E7 RID: 14567 RVA: 0x000FB876 File Offset: 0x000F9A76
	private void UpdatePosition()
	{
		if (HudScalePositioner.IsReduced)
		{
			base.transform.localPosition = this.reducedPosition;
			return;
		}
		base.transform.localPosition = this.largePosition;
	}

	// Token: 0x04003BBF RID: 15295
	[SerializeField]
	private Vector3 reducedPosition;

	// Token: 0x04003BC0 RID: 15296
	[SerializeField]
	private Vector3 largePosition;

	// Token: 0x04003BC1 RID: 15297
	private static HashSet<HudScalePositioner> _activeObjs = new HashSet<HudScalePositioner>();

	// Token: 0x04003BC2 RID: 15298
	private static bool _isReduced;
}
