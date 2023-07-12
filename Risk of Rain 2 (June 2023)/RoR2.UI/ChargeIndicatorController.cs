using System.Text;
using HG;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RoR2.UI;

public class ChargeIndicatorController : MonoBehaviour
{
	[Header("Cached Values")]
	public SpriteRenderer[] iconSprites;

	public TextMeshPro chargingText;

	public HoldoutZoneController holdoutZoneController;

	[Header("Color Values")]
	public Color spriteBaseColor;

	public Color spriteFlashColor;

	public Color spriteChargingColor;

	public Color spriteChargedColor;

	public Color textBaseColor;

	public Color textChargingColor;

	[Header("Options")]
	public bool disableTextWhenNotCharging;

	public bool disableTextWhenCharged;

	public bool flashWhenNotCharging;

	public float flashFrequency;

	[Header("Control Values - Don't assign via inspector!")]
	public bool isCharging;

	public bool isCharged;

	private float flashStopwatch;

	public uint chargeValue { get; set; }

	private void Update()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)holdoutZoneController))
		{
			chargeValue = (uint)holdoutZoneController.displayChargePercent;
			isCharging = holdoutZoneController.isAnyoneCharging;
		}
		Color color = spriteBaseColor;
		Color color2 = textBaseColor;
		if (!isCharged)
		{
			if (flashWhenNotCharging)
			{
				flashStopwatch += Time.deltaTime;
				color = (((int)(flashStopwatch * flashFrequency) % 2 == 0) ? spriteFlashColor : spriteBaseColor);
			}
			if (isCharging)
			{
				color = spriteChargingColor;
				color2 = textChargingColor;
			}
			if (disableTextWhenNotCharging)
			{
				((Behaviour)chargingText).enabled = isCharging;
			}
			else
			{
				((Behaviour)chargingText).enabled = true;
			}
		}
		else
		{
			color = spriteChargedColor;
			if (disableTextWhenCharged)
			{
				((Behaviour)chargingText).enabled = false;
			}
		}
		if (((Behaviour)chargingText).enabled)
		{
			StringBuilder stringBuilder = StringBuilderPool.RentStringBuilder();
			stringBuilder.AppendUint(chargeValue, 1u, 3u).Append("%");
			((TMP_Text)chargingText).SetText(stringBuilder);
			StringBuilderPool.ReturnStringBuilder(stringBuilder);
		}
		SpriteRenderer[] array = iconSprites;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].color = color;
		}
		((Graphic)chargingText).color = color2;
	}
}
