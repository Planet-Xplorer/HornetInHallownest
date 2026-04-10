using System;
using HutongGames.PlayMaker;
using UnityEngine;

// Token: 0x0200043F RID: 1087
public sealed class HudCanvas : MonoBehaviour
{
	// Token: 0x170003D8 RID: 984
	// (get) Token: 0x06002580 RID: 9600 RVA: 0x000AC38F File Offset: 0x000AA58F
	public static bool IsVisible
	{
		get
		{
			return HudCanvas.fsmBool == null || HudCanvas.fsmBool.Value;
		}
	}

	// Token: 0x06002581 RID: 9601 RVA: 0x000AC3A4 File Offset: 0x000AA5A4
	private void Awake()
	{
		if (HudCanvas.instance == null)
		{
			HudCanvas.instance = this;
			if (this.targetFsm)
			{
				HudCanvas.fsmBool = this.GetFsmBool();
			}
		}
	}

	// Token: 0x06002582 RID: 9602 RVA: 0x000AC3D1 File Offset: 0x000AA5D1
	private void OnDestroy()
	{
		if (HudCanvas.instance == this)
		{
			HudCanvas.instance = null;
			HudCanvas.fsmBool = null;
		}
	}

	// Token: 0x06002583 RID: 9603 RVA: 0x000AC3EC File Offset: 0x000AA5EC
	private bool? IsFsmBoolValid(string boolName)
	{
		return new bool?(this.GetFsmBool(boolName) != null);
	}

	// Token: 0x06002584 RID: 9604 RVA: 0x000AC3FD File Offset: 0x000AA5FD
	private FsmBool GetFsmBool(string boolName)
	{
		if (!this.targetFsm || string.IsNullOrEmpty(boolName))
		{
			return null;
		}
		return this.targetFsm.FsmVariables.FindFsmBool(boolName);
	}

	// Token: 0x06002585 RID: 9605 RVA: 0x000AC427 File Offset: 0x000AA627
	private FsmBool GetFsmBool()
	{
		return this.GetFsmBool(this.visibilityBool);
	}

	// Token: 0x04002330 RID: 9008
	[SerializeField]
	private PlayMakerFSM targetFsm;

	// Token: 0x04002331 RID: 9009
	[SerializeField]
	[ModifiableProperty]
	[Conditional("targetFsm", true, false, false)]
	[InspectorValidation("IsFsmBoolValid")]
	private string visibilityBool = "Is Visible";

	// Token: 0x04002332 RID: 9010
	private static HudCanvas instance;

	// Token: 0x04002333 RID: 9011
	private static FsmBool fsmBool;
}
