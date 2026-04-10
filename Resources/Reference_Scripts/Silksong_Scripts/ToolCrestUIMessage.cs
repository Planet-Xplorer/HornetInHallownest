using System;
using TMProOld;
using UnityEngine;

// Token: 0x0200073F RID: 1855
public class ToolCrestUIMsg : UIMsgBase<ToolCrest>
{
	// Token: 0x06004279 RID: 17017 RVA: 0x00124D9C File Offset: 0x00122F9C
	public static void Spawn(ToolCrest crest, GameObject prefab, Action afterMsg = null)
	{
		ToolCrestUIMsg component = prefab.GetComponent<ToolCrestUIMsg>();
		if (!component)
		{
			return;
		}
		UIMsgBase<ToolCrest>.Spawn(crest, component, afterMsg);
	}

	// Token: 0x0600427A RID: 17018 RVA: 0x00124DC4 File Offset: 0x00122FC4
	protected override void Setup(ToolCrest crest)
	{
		if (this.crestDisplay)
		{
			this.crestDisplay.sprite = crest.CrestSprite;
		}
		if (this.nameText)
		{
			this.nameText.text = crest.DisplayName;
		}
		if (this.descText)
		{
			this.descText.text = crest.GetPromptDesc;
		}
		if (this.itemPrefixText)
		{
			this.itemPrefixText.text = crest.ItemNamePrefix;
		}
		if (this.equipText)
		{
			this.equipText.text = crest.EquipText;
		}
	}

	// Token: 0x040043E6 RID: 17382
	[SerializeField]
	private SpriteRenderer crestDisplay;

	// Token: 0x040043E7 RID: 17383
	[SerializeField]
	private TMP_Text nameText;

	// Token: 0x040043E8 RID: 17384
	[SerializeField]
	private TMP_Text descText;

	// Token: 0x040043E9 RID: 17385
	[SerializeField]
	private TMP_Text itemPrefixText;

	// Token: 0x040043EA RID: 17386
	[SerializeField]
	private TMP_Text equipText;
}
