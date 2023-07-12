using System.Text;
using HG;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RoR2.UI;

public class EquipmentIcon : MonoBehaviour
{
	private struct DisplayData
	{
		public EquipmentDef equipmentDef;

		public int cooldownValue;

		public int stock;

		public int maxStock;

		public bool hideEntireDisplay;

		public bool isReady => stock > 0;

		public bool hasEquipment => (Object)(object)equipmentDef != (Object)null;

		public bool showCooldown
		{
			get
			{
				if (!isReady)
				{
					return hasEquipment;
				}
				return false;
			}
		}
	}

	public Inventory targetInventory;

	public EquipmentSlot targetEquipmentSlot;

	public GameObject displayRoot;

	public PlayerCharacterMasterController playerCharacterMasterController;

	public RawImage iconImage;

	public TextMeshProUGUI cooldownText;

	public TextMeshProUGUI stockText;

	public GameObject stockFlashPanelObject;

	public GameObject reminderFlashPanelObject;

	public GameObject isReadyPanelObject;

	public GameObject isAutoCastPanelObject;

	public TooltipProvider tooltipProvider;

	public bool displayAlternateEquipment;

	private int previousStockCount;

	private float equipmentReminderTimer;

	private DisplayData currentDisplayData;

	public bool hasEquipment => currentDisplayData.hasEquipment;

	private void SetDisplayData(DisplayData newDisplayData)
	{
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		//IL_026d: Unknown result type (might be due to invalid IL or missing references)
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_0289: Unknown result type (might be due to invalid IL or missing references)
		if (!currentDisplayData.isReady && newDisplayData.isReady)
		{
			DoStockFlash();
		}
		if (Object.op_Implicit((Object)(object)displayRoot))
		{
			displayRoot.SetActive(!newDisplayData.hideEntireDisplay);
		}
		if (newDisplayData.stock > currentDisplayData.stock)
		{
			Util.PlaySound("Play_item_proc_equipMag", ((Component)RoR2Application.instance).gameObject);
			DoStockFlash();
		}
		if (Object.op_Implicit((Object)(object)isReadyPanelObject))
		{
			isReadyPanelObject.SetActive(newDisplayData.isReady);
		}
		if (Object.op_Implicit((Object)(object)isAutoCastPanelObject))
		{
			if (Object.op_Implicit((Object)(object)targetInventory))
			{
				isAutoCastPanelObject.SetActive(targetInventory.GetItemCount(RoR2Content.Items.AutoCastEquipment) > 0);
			}
			else
			{
				isAutoCastPanelObject.SetActive(false);
			}
		}
		if (Object.op_Implicit((Object)(object)iconImage))
		{
			Texture texture = null;
			Color color = Color.clear;
			if (newDisplayData.equipmentDef != null)
			{
				color = ((newDisplayData.stock > 0) ? Color.white : Color.gray);
				texture = newDisplayData.equipmentDef.pickupIconTexture;
			}
			iconImage.texture = texture;
			((Graphic)iconImage).color = color;
		}
		if (Object.op_Implicit((Object)(object)cooldownText))
		{
			((Component)cooldownText).gameObject.SetActive(newDisplayData.showCooldown);
			if (newDisplayData.cooldownValue != currentDisplayData.cooldownValue)
			{
				StringBuilder stringBuilder = StringBuilderPool.RentStringBuilder();
				stringBuilder.AppendInt(newDisplayData.cooldownValue);
				((TMP_Text)cooldownText).SetText(stringBuilder);
				StringBuilderPool.ReturnStringBuilder(stringBuilder);
			}
		}
		if (Object.op_Implicit((Object)(object)stockText))
		{
			if (newDisplayData.hasEquipment && (newDisplayData.maxStock > 1 || newDisplayData.stock > 1))
			{
				((Component)stockText).gameObject.SetActive(true);
				StringBuilder stringBuilder2 = StringBuilderPool.RentStringBuilder();
				stringBuilder2.AppendInt(newDisplayData.stock);
				((TMP_Text)stockText).SetText(stringBuilder2);
				StringBuilderPool.ReturnStringBuilder(stringBuilder2);
			}
			else
			{
				((Component)stockText).gameObject.SetActive(false);
			}
		}
		string titleToken = null;
		string bodyToken = null;
		Color titleColor = Color.white;
		Color gray = Color.gray;
		if ((Object)(object)newDisplayData.equipmentDef != (Object)null)
		{
			titleToken = newDisplayData.equipmentDef.nameToken;
			bodyToken = newDisplayData.equipmentDef.pickupToken;
			titleColor = Color32.op_Implicit(ColorCatalog.GetColor(newDisplayData.equipmentDef.colorIndex));
		}
		if (Object.op_Implicit((Object)(object)tooltipProvider))
		{
			tooltipProvider.titleToken = titleToken;
			tooltipProvider.titleColor = titleColor;
			tooltipProvider.bodyToken = bodyToken;
			tooltipProvider.bodyColor = gray;
		}
		currentDisplayData = newDisplayData;
	}

	private void DoReminderFlash()
	{
		if (Object.op_Implicit((Object)(object)reminderFlashPanelObject))
		{
			AnimateUIAlpha component = reminderFlashPanelObject.GetComponent<AnimateUIAlpha>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.time = 0f;
			}
			reminderFlashPanelObject.SetActive(true);
		}
		equipmentReminderTimer = 5f;
	}

	private void DoStockFlash()
	{
		DoReminderFlash();
		if (Object.op_Implicit((Object)(object)stockFlashPanelObject))
		{
			AnimateUIAlpha component = stockFlashPanelObject.GetComponent<AnimateUIAlpha>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.time = 0f;
			}
			stockFlashPanelObject.SetActive(true);
		}
	}

	private DisplayData GenerateDisplayData()
	{
		DisplayData result = default(DisplayData);
		EquipmentIndex equipmentIndex = EquipmentIndex.None;
		if (Object.op_Implicit((Object)(object)targetInventory))
		{
			EquipmentState equipmentState;
			if (displayAlternateEquipment)
			{
				equipmentState = targetInventory.alternateEquipmentState;
				result.hideEntireDisplay = targetInventory.GetEquipmentSlotCount() <= 1;
			}
			else
			{
				equipmentState = targetInventory.currentEquipmentState;
				result.hideEntireDisplay = false;
			}
			_ = Run.FixedTimeStamp.now;
			Run.FixedTimeStamp chargeFinishTime = equipmentState.chargeFinishTime;
			equipmentIndex = equipmentState.equipmentIndex;
			result.cooldownValue = ((!chargeFinishTime.isInfinity) ? Mathf.CeilToInt(chargeFinishTime.timeUntilClamped) : 0);
			result.stock = equipmentState.charges;
			result.maxStock = ((!Object.op_Implicit((Object)(object)targetEquipmentSlot)) ? 1 : targetEquipmentSlot.maxStock);
		}
		else if (displayAlternateEquipment)
		{
			result.hideEntireDisplay = true;
		}
		result.equipmentDef = EquipmentCatalog.GetEquipmentDef(equipmentIndex);
		return result;
	}

	private void Update()
	{
		SetDisplayData(GenerateDisplayData());
		equipmentReminderTimer -= Time.deltaTime;
		if (currentDisplayData.isReady && equipmentReminderTimer < 0f && (Object)(object)currentDisplayData.equipmentDef != (Object)null)
		{
			DoReminderFlash();
		}
	}
}
