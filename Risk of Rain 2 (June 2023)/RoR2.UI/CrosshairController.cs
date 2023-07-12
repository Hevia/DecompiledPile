using System;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.UI;

namespace RoR2.UI;

[RequireComponent(typeof(HudElement))]
[RequireComponent(typeof(RectTransform))]
public class CrosshairController : MonoBehaviour
{
	[Serializable]
	public struct SpritePosition
	{
		public RectTransform target;

		public Vector3 zeroPosition;

		public Vector3 onePosition;
	}

	[Serializable]
	public struct SkillStockSpriteDisplay
	{
		public GameObject target;

		public SkillSlot skillSlot;

		public SkillDef requiredSkillDef;

		public int minimumStockCountToBeValid;

		public int maximumStockCountToBeValid;
	}

	public SpritePosition[] spriteSpreadPositions;

	public SkillStockSpriteDisplay[] skillStockSpriteDisplays;

	public RawImage[] remapSprites;

	public float minSpreadAlpha;

	public float maxSpreadAlpha;

	[Tooltip("The angle the crosshair represents when alpha = 1")]
	public float maxSpreadAngle;

	private MaterialPropertyBlock _propBlock;

	public RectTransform rectTransform { get; private set; }

	public HudElement hudElement { get; private set; }

	private void Awake()
	{
		rectTransform = ((Component)this).GetComponent<RectTransform>();
		hudElement = ((Component)this).GetComponent<HudElement>();
		SetCrosshairSpread();
		SetSkillStockDisplays();
	}

	private void SetCrosshairSpread()
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f;
		if (Object.op_Implicit((Object)(object)hudElement.targetCharacterBody))
		{
			num = hudElement.targetCharacterBody.spreadBloomAngle;
		}
		for (int i = 0; i < spriteSpreadPositions.Length; i++)
		{
			SpritePosition spritePosition = spriteSpreadPositions[i];
			((Transform)spritePosition.target).localPosition = Vector3.Lerp(spritePosition.zeroPosition, spritePosition.onePosition, num / maxSpreadAngle);
		}
		for (int j = 0; j < remapSprites.Length; j++)
		{
			((Graphic)remapSprites[j]).color = new Color(1f, 1f, 1f, Util.Remap(num / maxSpreadAngle, 0f, 1f, minSpreadAlpha, maxSpreadAlpha));
		}
	}

	private void SetSkillStockDisplays()
	{
		if (!Object.op_Implicit((Object)(object)hudElement.targetCharacterBody))
		{
			return;
		}
		SkillLocator component = ((Component)hudElement.targetCharacterBody).GetComponent<SkillLocator>();
		for (int i = 0; i < skillStockSpriteDisplays.Length; i++)
		{
			bool active = false;
			SkillStockSpriteDisplay skillStockSpriteDisplay = skillStockSpriteDisplays[i];
			GenericSkill skill = component.GetSkill(skillStockSpriteDisplay.skillSlot);
			if (Object.op_Implicit((Object)(object)skill) && skill.stock >= skillStockSpriteDisplay.minimumStockCountToBeValid && (skill.stock <= skillStockSpriteDisplay.maximumStockCountToBeValid || skillStockSpriteDisplay.maximumStockCountToBeValid < 0) && ((Object)(object)skillStockSpriteDisplay.requiredSkillDef == (Object)null || (Object)(object)skill.skillDef == (Object)(object)skillStockSpriteDisplay.requiredSkillDef))
			{
				active = true;
			}
			skillStockSpriteDisplay.target.SetActive(active);
		}
	}

	private void LateUpdate()
	{
		SetCrosshairSpread();
		SetSkillStockDisplays();
	}
}
