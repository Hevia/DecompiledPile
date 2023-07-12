using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RoR2.UI;

public class GenericNotification : MonoBehaviour
{
	public LanguageTextMeshController titleText;

	public TextMeshProUGUI titleTMP;

	public LanguageTextMeshController descriptionText;

	public RawImage iconImage;

	public RawImage previousIconImage;

	public CanvasGroup canvasGroup;

	public float fadeOutT = 0.916f;

	public void SetNotificationT(float t)
	{
		canvasGroup.alpha = 1f - Mathf.Clamp01(t - fadeOutT) / (1f - fadeOutT);
	}

	public void SetItem(ItemDef itemDef)
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		titleText.token = itemDef.nameToken;
		descriptionText.token = itemDef.pickupToken;
		if ((Object)(object)itemDef.pickupIconTexture != (Object)null)
		{
			iconImage.texture = itemDef.pickupIconTexture;
		}
		((Graphic)titleTMP).color = Color32.op_Implicit(ColorCatalog.GetColor(itemDef.colorIndex));
	}

	public void SetEquipment(EquipmentDef equipmentDef)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		titleText.token = equipmentDef.nameToken;
		descriptionText.token = equipmentDef.pickupToken;
		if (Object.op_Implicit((Object)(object)equipmentDef.pickupIconTexture))
		{
			iconImage.texture = equipmentDef.pickupIconTexture;
		}
		((Graphic)titleTMP).color = Color32.op_Implicit(ColorCatalog.GetColor(equipmentDef.colorIndex));
	}

	public void SetArtifact(ArtifactDef artifactDef)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		titleText.token = artifactDef.nameToken;
		descriptionText.token = artifactDef.descriptionToken;
		iconImage.texture = (Texture)(object)artifactDef.smallIconSelectedSprite.texture;
		((Graphic)titleTMP).color = Color32.op_Implicit(ColorCatalog.GetColor(ColorCatalog.ColorIndex.Artifact));
	}

	public void SetPreviousItem(ItemDef itemDef)
	{
		if (Object.op_Implicit((Object)(object)previousIconImage) && Object.op_Implicit((Object)(object)itemDef.pickupIconTexture))
		{
			previousIconImage.texture = itemDef.pickupIconTexture;
		}
	}

	public void SetPreviousEquipment(EquipmentDef equipmentDef)
	{
		if (Object.op_Implicit((Object)(object)previousIconImage) && Object.op_Implicit((Object)(object)equipmentDef.pickupIconTexture))
		{
			previousIconImage.texture = equipmentDef.pickupIconTexture;
		}
	}
}
