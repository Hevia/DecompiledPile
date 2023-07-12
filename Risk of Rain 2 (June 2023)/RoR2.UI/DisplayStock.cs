using UnityEngine;
using UnityEngine.UI;

namespace RoR2.UI;

public class DisplayStock : MonoBehaviour
{
	public SkillSlot skillSlot;

	public Image[] stockImages;

	public Sprite fullStockSprite;

	public Color fullStockColor;

	public Sprite emptyStockSprite;

	public Color emptyStockColor;

	private HudElement hudElement;

	private SkillLocator skillLocator;

	private void Awake()
	{
		hudElement = ((Component)this).GetComponent<HudElement>();
	}

	private void Update()
	{
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)hudElement.targetCharacterBody))
		{
			return;
		}
		if (!Object.op_Implicit((Object)(object)skillLocator))
		{
			skillLocator = ((Component)hudElement.targetCharacterBody).GetComponent<SkillLocator>();
		}
		if (!Object.op_Implicit((Object)(object)skillLocator))
		{
			return;
		}
		GenericSkill skill = skillLocator.GetSkill(skillSlot);
		if (!Object.op_Implicit((Object)(object)skill))
		{
			return;
		}
		for (int i = 0; i < stockImages.Length; i++)
		{
			if (skill.stock > i)
			{
				stockImages[i].sprite = fullStockSprite;
				((Graphic)stockImages[i]).color = fullStockColor;
			}
			else
			{
				stockImages[i].sprite = emptyStockSprite;
				((Graphic)stockImages[i]).color = emptyStockColor;
			}
		}
	}
}
