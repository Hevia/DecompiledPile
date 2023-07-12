using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RoR2.UI;

public class ScoreboardStrip : MonoBehaviour
{
	public ItemInventoryDisplay itemInventoryDisplay;

	public EquipmentIcon equipmentIcon;

	public SocialUserIcon userAvatar;

	public TextMeshProUGUI nameLabel;

	public RawImage classIcon;

	public TextMeshProUGUI moneyText;

	private CharacterMaster master;

	private CharacterBody userBody;

	private PlayerCharacterMasterController userPlayerCharacterMasterController;

	public void SetMaster(CharacterMaster newMaster)
	{
		if (!((Object)(object)master == (Object)(object)newMaster))
		{
			userBody = null;
			master = newMaster;
			if (Object.op_Implicit((Object)(object)master))
			{
				userBody = master.GetBody();
				userPlayerCharacterMasterController = ((Component)master).GetComponent<PlayerCharacterMasterController>();
				itemInventoryDisplay.SetSubscribedInventory(master.inventory);
				equipmentIcon.targetInventory = master.inventory;
				UpdateMoneyText();
			}
			if (Object.op_Implicit((Object)(object)userAvatar) && ((Behaviour)userAvatar).isActiveAndEnabled)
			{
				userAvatar.SetFromMaster(newMaster);
			}
			((TMP_Text)nameLabel).text = Util.GetBestMasterName(master);
			classIcon.texture = FindMasterPortrait();
		}
	}

	private void UpdateMoneyText()
	{
		if (Object.op_Implicit((Object)(object)master))
		{
			((TMP_Text)moneyText).text = $"${master.money}";
		}
	}

	private void Update()
	{
		UpdateMoneyText();
	}

	private Texture FindMasterPortrait()
	{
		if (Object.op_Implicit((Object)(object)userBody))
		{
			return userBody.portraitIcon;
		}
		if (Object.op_Implicit((Object)(object)master))
		{
			GameObject bodyPrefab = master.bodyPrefab;
			if (Object.op_Implicit((Object)(object)bodyPrefab))
			{
				CharacterBody component = bodyPrefab.GetComponent<CharacterBody>();
				if (Object.op_Implicit((Object)(object)component))
				{
					return component.portraitIcon;
				}
			}
		}
		return null;
	}
}
