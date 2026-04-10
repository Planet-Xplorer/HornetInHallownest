using System;
using System.Collections.Generic;

// Token: 0x020005E6 RID: 1510
[Serializable]
public class ToolCrestsData : SerializableNamedList<ToolCrestsData.Data, ToolCrestsData.NamedData>
{
	// Token: 0x0200191C RID: 6428
	[Serializable]
	public struct SlotData
	{
		// Token: 0x040094D6 RID: 38102
		public string EquippedTool;

		// Token: 0x040094D7 RID: 38103
		public bool IsUnlocked;
	}

	// Token: 0x0200191D RID: 6429
	[Serializable]
	public struct Data
	{
		// Token: 0x040094D8 RID: 38104
		public bool IsUnlocked;

		// Token: 0x040094D9 RID: 38105
		public List<ToolCrestsData.SlotData> Slots;

		// Token: 0x040094DA RID: 38106
		public bool DisplayNewIndicator;
	}

	// Token: 0x0200191E RID: 6430
	[Serializable]
	public class NamedData : SerializableNamedData<ToolCrestsData.Data>
	{
	}
}
